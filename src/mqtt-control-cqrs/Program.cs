using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using System.Text.Json;

Random r = new Random();

var mqttFactory = new MqttFactory();
using (var mqttClient = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithTcpServer("broker.hivemq.com")
        .Build();

    // Setup message handling before connecting so that queued messages
    // are also handled properly. When there is no event handler attached all
    // received messages get lost.
    mqttClient.ApplicationMessageReceivedAsync += e =>
    {
        string payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload, 0, e.ApplicationMessage.Payload.Length);

        Console.WriteLine(e.ClientId + " " + e.ApplicationMessage.Topic + " " + " " + payload);

        return Task.CompletedTask;
    };

    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(f => { f.WithTopic("samples/#"); })
        .Build();

    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

    Console.WriteLine("MQTT client subscribed to topic.");

    for (int i = 0; i < 100; i++)
    {
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic("samples/temperature/living_room")
            .WithPayload(r.Next(-40,50).ToString())
            .Build();

        await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
    }

    Console.WriteLine("Press enter to exit.");
    Console.ReadLine();
}
