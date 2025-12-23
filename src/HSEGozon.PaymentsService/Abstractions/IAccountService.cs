using HSEGozon.PaymentsService.Domain.DTOs;

namespace HSEGozon.PaymentsService.Abstractions;

public interface IAccountService
{
    Task<AccountResponse?> CreateAccountAsync(Guid userId);
    Task<AccountResponse?> TopUpAccountAsync(Guid userId, decimal amount);
    Task<AccountResponse?> GetAccountAsync(Guid userId);
}

