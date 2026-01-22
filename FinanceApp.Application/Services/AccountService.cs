using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IRepository<Account> accountRepository,
        IRepository<Transaction> transactionRepository,
        IMapper mapper,
        ILogger<AccountService> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync(Guid userId)
    {
        var accounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        return await MapAccountsWithBalance(accounts);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(Guid accountId, Guid userId)
    {
        var accounts = await _accountRepository.FindAsync(a => a.Id == accountId && a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account == null)
            return null;

        var accountDto = _mapper.Map<AccountDto>(account);
        accountDto.CurrentBalance = await CalculateAccountBalance(accountId);
        return accountDto;
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto, Guid userId)
    {
        _logger.LogInformation("Creating account {AccountName} for user {UserId}", dto.Name, userId);

        var account = _mapper.Map<Account>(dto);
        account.UserId = userId;
        account.Id = Guid.NewGuid();
        account.IsActive = true;

        var createdAccount = await _accountRepository.AddAsync(account);

        var accountDto = _mapper.Map<AccountDto>(createdAccount);
        accountDto.CurrentBalance = createdAccount.InitialBalance;
        return accountDto;
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto dto, Guid userId)
    {
        var accounts = await _accountRepository.FindAsync(a => a.Id == accountId && a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account == null)
        {
            throw new KeyNotFoundException("Conta não encontrada");
        }

        _logger.LogInformation("Updating account {AccountId} for user {UserId}", accountId, userId);

        account.Name = dto.Name;
        account.IsActive = dto.IsActive;
        account.Color = dto.Color;
        account.Icon = dto.Icon;

        await _accountRepository.UpdateAsync(account);

        var accountDto = _mapper.Map<AccountDto>(account);
        accountDto.CurrentBalance = await CalculateAccountBalance(accountId);
        return accountDto;
    }

    public async Task DeleteAccountAsync(Guid accountId, Guid userId)
    {
        var accounts = await _accountRepository.FindAsync(a => a.Id == accountId && a.UserId == userId);
        var account = accounts.FirstOrDefault();

        if (account == null)
        {
            throw new KeyNotFoundException("Conta não encontrada");
        }

        _logger.LogInformation("Deleting account {AccountId} for user {UserId}", accountId, userId);

        await _accountRepository.DeleteAsync(accountId);
    }

    public async Task<IEnumerable<AccountDto>> GetActiveAccountsAsync(Guid userId)
    {
        var accounts = await _accountRepository.FindAsync(a => a.UserId == userId && a.IsActive);
        return await MapAccountsWithBalance(accounts);
    }

    private async Task<decimal> CalculateAccountBalance(Guid accountId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
            return 0;

        var transactions = await _transactionRepository.FindAsync(t =>
            t.AccountId == accountId || t.DestinationAccountId == accountId);

        var balance = account.InitialBalance;

        foreach (var transaction in transactions)
        {
            if (transaction.AccountId == accountId)
            {
                // Transação de origem desta conta
                if (transaction.Type == TransactionType.Income)
                {
                    balance += transaction.Amount;
                }
                else if (transaction.Type == TransactionType.Expense || transaction.Type == TransactionType.Transfer)
                {
                    balance -= transaction.Amount;
                }
            }

            if (transaction.DestinationAccountId == accountId && transaction.Type == TransactionType.Transfer)
            {
                // Transferência recebida nesta conta
                balance += transaction.Amount;
            }
        }

        return balance;
    }

    private async Task<IEnumerable<AccountDto>> MapAccountsWithBalance(IEnumerable<Account> accounts)
    {
        var accountDtos = new List<AccountDto>();

        foreach (var account in accounts)
        {
            var accountDto = _mapper.Map<AccountDto>(account);
            accountDto.CurrentBalance = await CalculateAccountBalance(account.Id);
            accountDtos.Add(accountDto);
        }

        return accountDtos;
    }
}
