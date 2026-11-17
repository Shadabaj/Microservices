using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Mango.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private readonly string _connectionString;

        public MessageBus(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task PublishMessage(string queue_topic_Name, object message)
        {
            await using var client = new ServiceBusClient(_connectionString);

            ServiceBusSender sender = client.CreateSender(queue_topic_Name);

            var jsonMessage = JsonConvert.SerializeObject(message);

            ServiceBusMessage busMessage = new ServiceBusMessage(jsonMessage);

            await sender.SendMessageAsync(busMessage);
        }
    }
}
