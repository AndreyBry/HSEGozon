namespace HSEGozon.PaymentsService.Abstractions;

public interface IPaymentProcessingService
{
    Task ProcessPaymentAsync(string messageId, Guid orderId, Guid userId, decimal amount);
}

