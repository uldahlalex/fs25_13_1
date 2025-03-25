using System.Text.Json;
using Exercise1;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var appOptions = builder.Services.AddAppOptions(builder.Configuration);

builder.Services.AddDbContext<Db>((provider, optionsBuilder) =>
{
    optionsBuilder.UseSqlite("Data Source=db.sqlite");
} );


var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<string>>();

var options = new HiveMQClientOptionsBuilder()
    .WithWebSocketServer($"wss://{appOptions.MQTT_BROKER_HOST}:8884/mqtt")  // Using WSS (secure WebSocket)
    .WithClientId($"myClientId_{Guid.NewGuid()}")
    .WithCleanStart(true)
    .WithKeepAlive(30)
    .WithAutomaticReconnect(true)
    .WithMaximumPacketSize(1024)
    .WithReceiveMaximum(100)
    .WithSessionExpiryInterval(3600)
    .WithUserName(appOptions.MQTT_USERNAME)
    .WithPassword(appOptions.MQTT_PASSWORD)
    .WithRequestProblemInformation(true)
    .WithRequestResponseInformation(true)
    .WithAllowInvalidBrokerCertificates(true)
    .Build();
var client = new HiveMQClient(options);

client.OnMessageReceived += (sender, args) =>
{
    Console.WriteLine("Message Received: {}", args.PublishMessage.PayloadAsString);
};

await client.ConnectAsync().ConfigureAwait(false);

var subscribeOptionsBuilder = new SubscribeOptionsBuilder()
    .WithSubscription(new TopicFilter("device/+/timeseries", QualityOfService.ExactlyOnceDelivery),
        (obj, e) =>
        {
            logger.LogInformation(JsonSerializer.Serialize(e.PublishMessage.PayloadAsString));
            var data = JsonSerializer.Deserialize<TimeseriesData>(e.PublishMessage.PayloadAsString);
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Db>();
               db .TimeseriesData.Add(data);
                db.SaveChanges();
                logger.LogInformation("Now the database has the follwing data in the timeseries table: ");
                foreach (var d in db.TimeseriesData)
                {
                    logger.LogInformation(JsonSerializer.Serialize(d));
                }
            }
            
        });
var subscribeOptions = subscribeOptionsBuilder.Build();
await client.SubscribeAsync(subscribeOptions);
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<Db>().Database.EnsureCreated();
}
app.Run();