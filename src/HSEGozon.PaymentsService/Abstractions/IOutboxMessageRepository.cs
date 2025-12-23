using HSEGozon.PaymentsService.Domain.Entities;

namespace HSEGozon.PaymentsService.Abstractions;

public interface IOutboxMessageRepository
{
    Task<OutboxMessage> AddAsync(OutboxMessage message);
    Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int limit = 100);
    Task UpdateAsync(OutboxMessage message);
}

