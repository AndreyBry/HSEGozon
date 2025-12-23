namespace HSEGozon.PaymentsService.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync(string exchange, string routingKey, string messageType, string payload);
}

