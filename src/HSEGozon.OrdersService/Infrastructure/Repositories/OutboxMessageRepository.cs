using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Domain.Entities;
using HSEGozon.OrdersService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HSEGozon.OrdersService.Infrastructure.Repositories;

public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly OrdersDbContext _context;

    public OutboxMessageRepository(OrdersDbContext context)
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

