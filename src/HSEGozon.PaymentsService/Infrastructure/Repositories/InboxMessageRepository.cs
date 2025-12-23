using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HSEGozon.PaymentsService.Infrastructure.Repositories;

public class InboxMessageRepository : IInboxMessageRepository
{
    private readonly PaymentsDbContext _context;

    public InboxMessageRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<InboxMessage?> GetByMessageIdAsync(string messageId)
    {
        return await _context.InboxMessages
            .FirstOrDefaultAsync(m => m.MessageId == messageId);
    }

    public async Task<InboxMessage> AddAsync(InboxMessage message)
    {
        _context.InboxMessages.Add(message);
        return message;
    }

    public async Task UpdateAsync(InboxMessage message)
    {
        _context.InboxMessages.Update(message);
    }
}

