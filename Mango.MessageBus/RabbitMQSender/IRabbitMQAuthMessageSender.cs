namespace Mango.MessageBus.RabbitMQSender
{
    public interface IRabbitMQExchangeSender
    {
        void SendMessage(object message, string queueName);
    }
}
