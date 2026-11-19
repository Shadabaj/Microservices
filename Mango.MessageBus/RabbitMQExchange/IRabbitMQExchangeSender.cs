namespace Mango.MessageBus.RabbitMQExchange
{
    public interface IRabbitMQExchangeSender
    {
        void SendMessage(object message, string exchangeName);
    }
}
