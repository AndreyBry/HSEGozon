using HSEGozon.OrdersService.Domain.Entities;

namespace HSEGozon.OrdersService.Abstractions;

public interface IOutboxMessageRepository
{
    Task<OutboxMessage> AddAsync(OutboxMessage message);
    Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int limit = 100);
    Task UpdateAsync(OutboxMessage message);
}

