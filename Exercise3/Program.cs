using System.Text.Json;
using Exercise1;
using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;

var builder = WebApplication.CreateBuilder(args);



var appOptions = builder.Services.AddAppOptions(builder.Configuration);
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

builder.Services.AddSingleton(client);
builder.Services.AddScoped<IMqttPublisher, MqttPublisher>();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();

app.Run();

public class MqttPublisher(HiveMQClient mqttClient) : IMqttPublisher
{
    public Task Publish(string topic, string message)
    {
        mqttClient.PublishAsync(topic, message);
        return Task.CompletedTask;
    }
}

public interface IMqttPublisher
{
    public Task Publish(string topic, string message);
}