using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HSEGozon.PaymentsService.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly PaymentsDbContext _context;

    public AccountRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<Account?> GetByUserIdWithLockAsync(Guid userId)
    {
        return await _context.Accounts
            .FromSqlRaw("SELECT * FROM \"Accounts\" WHERE \"UserId\" = {0} FOR UPDATE", userId)
            .FirstOrDefaultAsync();
    }

    public async Task<Account> AddAsync(Account account)
    {
        _context.Accounts.Add(account);
        return account;
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await _context.Accounts
            .AnyAsync(a => a.UserId == userId);
    }
}

