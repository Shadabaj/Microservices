using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.MessageBus.RabbitMQSender
{

    public class RabbitMQAuthMessageSender : IRabbitMQExchangeSender
    {

        private readonly IConfigurationSection _configuration;

        public RabbitMQAuthMessageSender(IConfigurationSection configuration)
        {
            _configuration = configuration;
        }

        public void SendMessage(object message, string queueName)
        {
            try
            {
                var factory = new ConnectionFactory()
                {

                    //HostName = _configuration["RabbitMQ:Host"],
                    //UserName = _configuration["RabbitMQ:Username"],
                    //Password = _configuration["RabbitMQ:Password"]
                    HostName = _configuration["Host"],
                    UserName = _configuration["Username"],
                    Password = _configuration["Password"]

                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                // Declare durable queue
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var json = JsonConvert.SerializeObject(message);

                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
            }
        }
    }



    //public void SendMessage(object message, string queueName)
    //{
    //    if (!ConnectionExists())
    //    {
    //        return;
    //    }

    //    using var channel = _connection!.CreateModel();
    //    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

    //    var json = JsonConvert.SerializeObject(message);
    //    var body = Encoding.UTF8.GetBytes(json);

    //    channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: body);
    //}

    //private void CreateConnection()
    //{
    //    try
    //    {
    //        var factory = new ConnectionFactory
    //        {
    //            HostName = _hostName
    //        };
    //        _connection = factory.CreateConnection();
    //    }
    //    catch (Exception)
    //    {
    //        // Connection failed — caller will skip publish when _connection is null.
    //    }
    //}

    //private bool ConnectionExists()
    //{
    //    if (_connection != null)
    //    {
    //        return true;
    //    }

    //    CreateConnection();
    //    return _connection != null;
    //}
}

