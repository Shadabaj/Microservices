using Mango.MessageBus.RabbitMQExchange;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.MessageBus.RabbitMQExhange
{

    public class RabbitMQExchangeSender : IRabbitMQExchangeSender
    {

        private readonly IConfigurationSection _configuration;
        //For the Rabbit MQ Direct Exchange Declaration
        private string orderEQExhange;
        private string rewardRQExhange;

        public RabbitMQExchangeSender(IConfigurationSection configuration)
        {
            _configuration = configuration;
            //For the Rabbit MQ Direct Exchange Initialization
            orderEQExhange = _configuration["OrderUpdateExchange"];
            rewardRQExhange = _configuration["RewardUpdateExchange"];
        }

        public void SendMessage(object message, string exchangeName)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["Host"],
                    UserName = _configuration["Username"],
                    Password = _configuration["Password"]

                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declare durable queue for the Fanout
                //channel.ExchangeDeclare(exchangeName,ExchangeType.Fanout,durable:false);

                #region Declare durable queue for the Direct Exchange
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: false);

                channel.QueueDeclare(orderEQExhange, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueDeclare(rewardRQExhange, durable: true, exclusive: false, autoDelete: false, arguments: null);

                channel.QueueBind(orderEQExhange, exchangeName, "OrderUpdate");
                channel.QueueBind(rewardRQExhange, exchangeName, "RewardUpdate");

                #endregion

                var json = JsonConvert.SerializeObject(message);

                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();

                properties.Persistent = true;

                #region Declare durable queue for the Fanout
                //channel.BasicPublish(exchange: exchangeName,routingKey: "",basicProperties: properties, body: body);
                #endregion

                #region Declare durable queue for the Direct Exchange
                channel.BasicPublish(exchange: exchangeName, routingKey: "OrderUpdate", basicProperties: properties, body: body);
                channel.BasicPublish(exchange: exchangeName, routingKey: "RewardUpdate", basicProperties: properties, body: body);
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
            }
        }
    }

}

