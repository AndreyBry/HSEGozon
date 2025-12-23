using HSEGozon.OrdersService.Abstractions;
using RabbitMQ.Client;
using System.Text;

namespace HSEGozon.OrdersService.Infrastructure.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger<RabbitMqMessagePublisher> _logger;

    public RabbitMqMessagePublisher(IRabbitMqConnection connection, ILogger<RabbitMqMessagePublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishAsync(string exchange, string routingKey, string messageType, string payload)
    {
        try
        {
            using var channel = _connection.CreateConnection().CreateModel();
            
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
            
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Type = messageType;
            properties.MessageId = Guid.NewGuid().ToString();

            var body = Encoding.UTF8.GetBytes(payload);

            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Message published to {Exchange}/{RoutingKey}, type: {MessageType}",
                exchange, routingKey, messageType);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to {Exchange}/{RoutingKey}", exchange, routingKey);
            throw;
        }
    }
}

