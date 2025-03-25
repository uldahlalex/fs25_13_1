using System.Text.Json;
using Exercise1;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

var builder = WebApplication.CreateBuilder(args);

var appOptions = builder.Services.AddAppOptions(builder.Configuration);
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
    .WithSubscription(new TopicFilter("topic1", QualityOfService.ExactlyOnceDelivery),
        (obj,e ) =>
    {
        logger.LogInformation(JsonSerializer.Serialize(e.PublishMessage.PayloadAsString));
    })
    .WithSubscription(new TopicFilter("topic2", QualityOfService.AtMostOnceDelivery),
        (sender, eventArgs) =>
        {
            
        } );
var subscribeOptions = subscribeOptionsBuilder.Build();
await client.SubscribeAsync(subscribeOptions);
app.Run();