using HSEGozon.PaymentsService.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace HSEGozon.PaymentsService.Infrastructure.Messaging;

public class RabbitMqMessageConsumer : IMessageConsumer
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger<RabbitMqMessageConsumer> _logger;
    private IModel? _channel;
    private EventingBasicConsumer? _consumer;

    public RabbitMqMessageConsumer(IRabbitMqConnection connection, ILogger<RabbitMqMessageConsumer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task StartConsumingAsync(string queueName, Func<string, string, Task> onMessageReceived)
    {
        try
        {
            _channel = _connection.CreateConnection().CreateModel();
            
            var exchange = queueName.Contains("payment") ? "payments" : "orders";
            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            
            var routingKey = queueName.Contains("process-payment") ? "process.payment" : "payment.status";
            _channel.QueueBind(queue: queueName, exchange: exchange, routingKey: routingKey);

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
                var routingKey = ea.RoutingKey;

                try
                {
                    await onMessageReceived(message, messageId);
                    _channel?.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Message processed successfully: {MessageId}", messageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {MessageId}", messageId);
                    _channel?.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);
            _logger.LogInformation("Started consuming from queue: {QueueName}", queueName);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting consumer for queue: {QueueName}", queueName);
            throw;
        }
    }

    public void StopConsuming()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}

