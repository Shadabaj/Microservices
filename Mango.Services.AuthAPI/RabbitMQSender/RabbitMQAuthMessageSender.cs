using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.AuthAPI.RabbitMQSender
{
    public class RabbitMQAuthMessageSender : IRabbitMQAuthMessageSender
    {

        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private IConnection? _connection;

        public RabbitMQAuthMessageSender()
        {
            _hostName="localhost";
            _userName="guest";
            _password="guest";
        }

        public void SendMessage(object message, string queueName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password
            };
            _connection = factory.CreateConnection();

            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: body);
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

