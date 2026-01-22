using FinanceApp.Application.DTOs;

namespace FinanceApp.Application.Interfaces;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId);
    Task<AccountDto?> GetAccountByIdAsync(Guid accountId, Guid userId);
    Task<AccountDto> CreateAccountAsync(CreateAccountDto dto, Guid userId);
    Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto dto, Guid userId);
    Task DeleteAccountAsync(Guid accountId, Guid userId);
    Task<IEnumerable<AccountDto>> GetActiveAccountsAsync(Guid userId);
}
