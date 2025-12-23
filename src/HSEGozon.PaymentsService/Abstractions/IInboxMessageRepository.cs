using HSEGozon.PaymentsService.Domain.Entities;

namespace HSEGozon.PaymentsService.Abstractions;

public interface IInboxMessageRepository
{
    Task<InboxMessage?> GetByMessageIdAsync(string messageId);
    Task<InboxMessage> AddAsync(InboxMessage message);
    Task UpdateAsync(InboxMessage message);
}

