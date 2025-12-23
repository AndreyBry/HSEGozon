using HSEGozon.PaymentsService.Domain.Entities;

namespace HSEGozon.PaymentsService.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByUserIdAsync(Guid userId);
    Task<Account?> GetByUserIdWithLockAsync(Guid userId);
    Task<Account> AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task<bool> ExistsByUserIdAsync(Guid userId);
}

