using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HSEGozon.PaymentsService.Infrastructure.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly PaymentsDbContext _context;

    public OutboxMessageRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<OutboxMessage> AddAsync(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
        return message;
    }

    public async Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync(int limit = 100)
    {
        return await _context.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpdateAsync(OutboxMessage message)
    {
        _context.OutboxMessages.Update(message);
    }
}

