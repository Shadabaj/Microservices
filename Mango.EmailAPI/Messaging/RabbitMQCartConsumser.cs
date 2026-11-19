
using Mango.EmailAPI.Services.IServices;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;
using Mango.EmailAPI.Models.Dto;

namespace Mango.EmailAPI.Messaging
{
    public class RabbitMQCartConsumser : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQCartConsumser(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _configuration["TopicAndQueueNames:EmailShoppingCartQueue"], durable: true, exclusive: false, autoDelete: false, arguments: null);
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
                CartDto cart = JsonConvert.DeserializeObject<CartDto>(content);
                // Process the message (e.g., send an email)
                HandleMessageAsync(cart).GetAwaiter().GetResult();
                // Acknowledge the message
                _channel.BasicAck(ea.DeliveryTag, false);

            };
            _channel.BasicConsume(queue: _configuration["TopicAndQueueNames:EmailShoppingCartQueue"], autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public async Task HandleMessageAsync(CartDto cart)
        {
            // Call the email service to send the email
            await _emailService.EmailCartAndLog(cart);
        }
    }
}
