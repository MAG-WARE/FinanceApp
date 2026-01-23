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
        try
        {
            _logger.LogInformation(
                "Iniciando criação de conta - UserId: {UserId}, Name: {Name}, InitialBalance: {Balance}",
                userId, dto.Name, dto.InitialBalance);

            var account = _mapper.Map<Account>(dto);
            account.UserId = userId;
            account.Id = Guid.NewGuid();
            account.IsActive = true;

            _logger.LogInformation("Salvando conta no banco de dados - AccountId: {AccountId}", account.Id);

            try
            {
                var createdAccount = await _accountRepository.AddAsync(account);

                _logger.LogInformation("Conta criada com sucesso - AccountId: {AccountId}, UserId: {UserId}",
                    createdAccount.Id, userId);

                var accountDto = _mapper.Map<AccountDto>(createdAccount);
                accountDto.CurrentBalance = createdAccount.InitialBalance;
                return accountDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao salvar conta no banco - AccountId: {AccountId}, UserId: {UserId}, Name: {Name}. Erro: {ErrorMessage}",
                    account.Id, userId, dto.Name, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao criar conta - UserId: {UserId}, Name: {Name}",
                userId, dto.Name);
            throw new InvalidOperationException($"Erro ao criar conta: {ex.Message}", ex);
        }
    }

    public async Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto dto, Guid userId)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando atualização de conta - AccountId: {AccountId}, UserId: {UserId}, NewName: {Name}",
                accountId, userId, dto.Name);

            var accounts = await _accountRepository.FindAsync(a => a.Id == accountId && a.UserId == userId);
            var account = accounts.FirstOrDefault();

            if (account == null)
            {
                _logger.LogWarning("Conta não encontrada ou não pertence ao usuário - AccountId: {AccountId}, UserId: {UserId}",
                    accountId, userId);
                throw new KeyNotFoundException($"Conta com ID {accountId} não encontrada");
            }

            _logger.LogInformation("Validações concluídas. Atualizando conta no banco...");

            account.Name = dto.Name;
            account.IsActive = dto.IsActive;
            account.Color = dto.Color;
            account.Icon = dto.Icon;

            try
            {
                await _accountRepository.UpdateAsync(account);

                _logger.LogInformation("Conta atualizada com sucesso - AccountId: {AccountId}, UserId: {UserId}",
                    accountId, userId);

                var accountDto = _mapper.Map<AccountDto>(account);
                accountDto.CurrentBalance = await CalculateAccountBalance(accountId);
                return accountDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao atualizar conta no banco - AccountId: {AccountId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    accountId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao atualizar conta - AccountId: {AccountId}, UserId: {UserId}",
                accountId, userId);
            throw new InvalidOperationException($"Erro ao atualizar conta: {ex.Message}", ex);
        }
    }

    public async Task DeleteAccountAsync(Guid accountId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Iniciando exclusão de conta - AccountId: {AccountId}, UserId: {UserId}",
                accountId, userId);

            var accounts = await _accountRepository.FindAsync(a => a.Id == accountId && a.UserId == userId);
            var account = accounts.FirstOrDefault();

            if (account == null)
            {
                _logger.LogWarning("Conta não encontrada ou não pertence ao usuário - AccountId: {AccountId}, UserId: {UserId}",
                    accountId, userId);
                throw new KeyNotFoundException($"Conta com ID {accountId} não encontrada");
            }

            // Verificar se há transações associadas
            var transactions = await _transactionRepository.FindAsync(t =>
                t.AccountId == accountId || t.DestinationAccountId == accountId);

            if (transactions.Any())
            {
                _logger.LogWarning("Tentativa de excluir conta com transações - AccountId: {AccountId}, TransactionCount: {Count}",
                    accountId, transactions.Count());
                throw new InvalidOperationException(
                    $"Não é possível excluir a conta pois existem {transactions.Count()} transação(ões) associada(s). Desative a conta em vez de excluí-la.");
            }

            _logger.LogInformation("Validações concluídas. Excluindo conta do banco...");

            try
            {
                await _accountRepository.DeleteAsync(accountId);

                _logger.LogInformation("Conta excluída com sucesso - AccountId: {AccountId}, UserId: {UserId}",
                    accountId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao excluir conta no banco - AccountId: {AccountId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    accountId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao excluir conta - AccountId: {AccountId}, UserId: {UserId}",
                accountId, userId);
            throw new InvalidOperationException($"Erro ao excluir conta: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AccountDto>> GetActiveAccountsAsync(Guid userId)
    {
        var accounts = await _accountRepository.FindAsync(a => a.UserId == userId && a.IsActive);
        return await MapAccountsWithBalance(accounts);
    }

    public async Task<AccountDto> ToggleActiveStatusAsync(Guid accountId, Guid userId)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando toggle de status da conta - AccountId: {AccountId}, UserId: {UserId}",
                accountId, userId);

            var accounts = await _accountRepository.FindAsync(a => a.Id == accountId && a.UserId == userId);
            var account = accounts.FirstOrDefault();

            if (account == null)
            {
                _logger.LogWarning("Conta não encontrada ou não pertence ao usuário - AccountId: {AccountId}, UserId: {UserId}",
                    accountId, userId);
                throw new KeyNotFoundException($"Conta com ID {accountId} não encontrada");
            }

            _logger.LogInformation("Alternando status - CurrentStatus: {CurrentStatus}", account.IsActive);

            account.IsActive = !account.IsActive;

            try
            {
                await _accountRepository.UpdateAsync(account);

                _logger.LogInformation("Status da conta alternado com sucesso - AccountId: {AccountId}, NewStatus: {NewStatus}",
                    accountId, account.IsActive);

                var accountDto = _mapper.Map<AccountDto>(account);
                accountDto.CurrentBalance = await CalculateAccountBalance(accountId);
                return accountDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao alternar status da conta no banco - AccountId: {AccountId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    accountId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao alternar status da conta - AccountId: {AccountId}, UserId: {UserId}",
                accountId, userId);
            throw new InvalidOperationException($"Erro ao alternar status da conta: {ex.Message}", ex);
        }
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
                if (transaction.Type == TransactionType.Income)
                    balance += transaction.Amount;
                else if (transaction.Type == TransactionType.Expense || transaction.Type == TransactionType.Transfer)
                    balance -= transaction.Amount;
            }

            if (transaction.DestinationAccountId == accountId && transaction.Type == TransactionType.Transfer)
                balance += transaction.Amount;
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
