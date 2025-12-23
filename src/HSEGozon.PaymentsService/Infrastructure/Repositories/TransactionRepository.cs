using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HSEGozon.PaymentsService.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly PaymentsDbContext _context;

    public TransactionRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
    }

    public async Task<Transaction?> GetByMessageIdAndOrderIdAsync(string messageId, Guid orderId)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.MessageId == messageId && t.OrderId == orderId);
    }
}

