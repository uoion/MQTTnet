using MQTTnet;
// We do NOT need 'MQTTnet.Client'
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MQTTnet.Packets; // This is for MqttTopicFilter

namespace CR310_Subscriber_App
{
    /// <summary>
    /// A reusable class to manage the connection and subscription
    /// to the private HiveMQ Cloud broker.
    /// (Corrected for MQTTnet v5)
    /// </summary>
    public class HiveMqSubscriber : IAsyncDisposable
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientFactory _mqttFactory;

        public event Action<string, string>? MessageReceived;

        // --- Your Hard-Coded Credentials ---
        private const string ClusterUrl = "wss://70a1960deacd43f1807bfe830d8f25b3.s1.eu.hivemq.cloud:8884/mqtt";
        private const string Username = "csharp_app";
        // !! Remember to put your working password here !!
        private const string Password = "xbpHy_QF3bG8kLe"; 
        private const string TopicToSubscribeTo = "#"; 
        // ------------------------------------

        public HiveMqSubscriber()
        {
            // Instantiate the factory and use it to create the client
            _mqttFactory = new MqttClientFactory();
            _mqttClient = _mqttFactory.CreateMqttClient(); 

            // Setup the message handler
            _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
        }

        /// <summary>
        /// This method is called by the MQTTnet library when a message arrives.
        /// </summary>
        private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            // Convert the payload from bytes to a string
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            
            // Fire our public event to notify the main application
            MessageReceived?.Invoke(topic, payload);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Connects to the broker and subscribes to the topic.
        /// </summary>
        public async Task ConnectAndSubscribeAsync(CancellationToken cancellationToken = default)
        {
            if (Password.Contains("YOUR_"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Please open 'HiveMqSubscriber.cs' and update the 'Password' field.");
                Console.ResetColor();
                return;
            }

            // Build the options (this was correct)
            var mqttClientOptions = new MqttClientOptionsBuilder() 
                .WithWebSocketServer(o => o.WithUri(ClusterUrl))
                .WithCredentials(Username, Password)
                .WithTlsOptions(o => { })
                .Build();

            // Connect
            try
            {
                var connectResult = await _mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
                
                if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR connecting: {connectResult.ReasonString}");
                    Console.ResetColor();
                    return;
                }
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("MQTT client connected successfully.");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR connecting: {e.Message}");
                Console.ResetColor();
                return;
            }
            
            // Subscribe
            // This is the correct v5 pattern using the builder
            var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(TopicToSubscribeTo))
                .Build();

            var subscribeResult = await _mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"MQTT client successfully subscribed to topic '{TopicToSubscribeTo}'.");
            Console.ResetColor();
        }

        /// <summary>
        /// Disconnects from the broker cleanly.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_mqttClient.IsConnected)
            {
                var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection) 
                    .Build();
                await _mqttClient.DisconnectAsync(disconnectOptions);
            }
            _mqttClient.Dispose();
            
            // This was correct
            GC.SuppressFinalize(this);
        }
    }
}