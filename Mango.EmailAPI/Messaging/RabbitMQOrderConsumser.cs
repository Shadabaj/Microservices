
using Mango.EmailAPI.Models.Dto;
using Mango.EmailAPI.Services.IServices;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.EmailAPI.Messaging
{

    //Rabbit MQ Exchange Consumer 
    public class RabbitMQOrderConsumser : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private IConnection _connection;
        private IModel _channel;
        string QueueName =string.Empty;
        private string orderEQExhange;
 

        public RabbitMQOrderConsumser(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;

            orderEQExhange = _configuration["RabbitMQ:OrderUpdateExchange"];

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };
            string ExchangeName = _configuration["TopicAndQueueNames:OrderCreatedTopic"];

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            //Declare durable queue for the Fanout
            //_channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);

            //Declare durable queue for the Direct
            _channel.ExchangeDeclare(ExchangeName,ExchangeType.Direct);

            //Declare durable queue for the Fanout
            //QueueName = _channel.QueueDeclare().QueueName;
            //_channel.QueueBind(QueueName, ExchangeName,routingKey:"");

            //Declare durable queue for the Direct
            _channel.QueueDeclare(orderEQExhange, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(orderEQExhange, ExchangeName, routingKey: "OrderUpdate");

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // Handle the received message
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                // Assuming the message contains the email address
                OrderHeaderDto order = JsonConvert.DeserializeObject<OrderHeaderDto>(content);
                // Process the message (e.g., send an email)
                HandleMessageAsync(order).GetAwaiter().GetResult();
                // Acknowledge the message
                _channel.BasicAck(ea.DeliveryTag, false);

            };
            //Listening to the New Queue for the fanout Exchange
            //_channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            //Listening to the New Queue for the Direct Exchange
            _channel.BasicConsume(queue: orderEQExhange, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }
        private async Task HandleMessageAsync(OrderHeaderDto order)
        {
            // Call the email service to send the email
            _emailService.OrderCreatedEmailAndLog(order).GetAwaiter().GetResult();
        }
    }
}
