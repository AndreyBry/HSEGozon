using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Domain.DTOs;
using HSEGozon.PaymentsService.Infrastructure.Data;

namespace HSEGozon.PaymentsService.Application;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly PaymentsDbContext _context;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        PaymentsDbContext context,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<AccountResponse?> CreateAccountAsync(Guid userId)
    {
        var existingAccount = await _accountRepository.GetByUserIdAsync(userId);

        if (existingAccount != null)
        {
            _logger.LogWarning("Account already exists for user {UserId}", userId);
            return MapToResponse(existingAccount);
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Account created for user {UserId}", userId);
        return MapToResponse(account);
    }

    public async Task<AccountResponse?> TopUpAccountAsync(Guid userId, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be positive", nameof(amount));
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _accountRepository.GetByUserIdWithLockAsync(userId);

            if (account == null)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning("Account not found for user {UserId}", userId);
                return null;
            }

            account.Balance += amount;
            account.UpdatedAt = DateTime.UtcNow;

            var transactionRecord = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                OrderId = Guid.Empty,
                MessageId = Guid.NewGuid().ToString(),
                Amount = amount,
                Type = TransactionType.Credit,
                CreatedAt = DateTime.UtcNow
            };

            await _accountRepository.UpdateAsync(account);
            await _transactionRepository.AddAsync(transactionRecord);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Account {AccountId} topped up with {Amount}", account.Id, amount);
            return MapToResponse(account);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AccountResponse?> GetAccountAsync(Guid userId)
    {
        var account = await _accountRepository.GetByUserIdAsync(userId);

        if (account == null)
        {
            return null;
        }

        return MapToResponse(account);
    }

    private static AccountResponse MapToResponse(Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            UserId = account.UserId,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        };
    }
}

