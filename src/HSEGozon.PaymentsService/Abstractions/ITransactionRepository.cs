using HSEGozon.PaymentsService.Domain.Entities;

namespace HSEGozon.PaymentsService.Abstractions;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task<Transaction?> GetByMessageIdAndOrderIdAsync(string messageId, Guid orderId);
}

