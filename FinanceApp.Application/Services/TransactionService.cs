using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IRepository<Transaction> transactionRepository,
        IRepository<Account> accountRepository,
        IRepository<Category> categoryRepository,
        IMapper mapper,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(Guid userId, int pageNumber = 1, int pageSize = 50)
    {
        // Buscar contas do usuário
        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        // Buscar transações das contas do usuário
        var transactions = await _transactionRepository.FindAsync(t => accountIds.Contains(t.AccountId));

        var orderedTransactions = transactions
            .OrderByDescending(t => t.Date)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await MapTransactionsWithDetails(orderedTransactions);
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            return null;

        // Verificar se a transação pertence ao usuário
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId);
        if (account == null || account.UserId != userId)
            return null;

        return (await MapTransactionsWithDetails(new[] { transaction })).FirstOrDefault();
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto, Guid userId)
    {
        // Validar que a conta pertence ao usuário
        var account = await _accountRepository.GetByIdAsync(dto.AccountId);
        if (account == null || account.UserId != userId)
        {
            throw new UnauthorizedAccessException("Conta não pertence ao usuário");
        }

        // Validar que a categoria pertence ao usuário
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null || category.UserId != userId)
        {
            throw new UnauthorizedAccessException("Categoria não pertence ao usuário");
        }

        // Validar tipo de categoria com tipo de transação
        if ((dto.Type == TransactionType.Income && category.Type != CategoryType.Income) ||
            (dto.Type == TransactionType.Expense && category.Type != CategoryType.Expense))
        {
            throw new InvalidOperationException("Tipo de categoria não corresponde ao tipo de transação");
        }

        // Validar conta de destino para transferências
        if (dto.Type == TransactionType.Transfer)
        {
            if (!dto.DestinationAccountId.HasValue)
            {
                throw new InvalidOperationException("Conta de destino é obrigatória para transferências");
            }

            var destinationAccount = await _accountRepository.GetByIdAsync(dto.DestinationAccountId.Value);
            if (destinationAccount == null || destinationAccount.UserId != userId)
            {
                throw new UnauthorizedAccessException("Conta de destino não pertence ao usuário");
            }

            if (dto.AccountId == dto.DestinationAccountId)
            {
                throw new InvalidOperationException("Conta de origem e destino não podem ser iguais");
            }
        }

        _logger.LogInformation("Creating transaction for user {UserId}", userId);

        var transaction = _mapper.Map<Transaction>(dto);
        transaction.Id = Guid.NewGuid();

        var createdTransaction = await _transactionRepository.AddAsync(transaction);

        return (await MapTransactionsWithDetails(new[] { createdTransaction })).First();
    }

    public async Task<TransactionDto> UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            throw new KeyNotFoundException("Transação não encontrada");
        }

        // Verificar que a transação pertence ao usuário
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId);
        if (account == null || account.UserId != userId)
        {
            throw new UnauthorizedAccessException("Transação não pertence ao usuário");
        }

        // Validar que a categoria pertence ao usuário
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null || category.UserId != userId)
        {
            throw new UnauthorizedAccessException("Categoria não pertence ao usuário");
        }

        _logger.LogInformation("Updating transaction {TransactionId} for user {UserId}", transactionId, userId);

        transaction.CategoryId = dto.CategoryId;
        transaction.Amount = dto.Amount;
        transaction.Date = dto.Date;
        transaction.Description = dto.Description;
        transaction.Notes = dto.Notes;

        await _transactionRepository.UpdateAsync(transaction);

        return (await MapTransactionsWithDetails(new[] { transaction })).First();
    }

    public async Task DeleteTransactionAsync(Guid transactionId, Guid userId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            throw new KeyNotFoundException("Transação não encontrada");
        }

        // Verificar que a transação pertence ao usuário
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId);
        if (account == null || account.UserId != userId)
        {
            throw new UnauthorizedAccessException("Transação não pertence ao usuário");
        }

        _logger.LogInformation("Deleting transaction {TransactionId} for user {UserId}", transactionId, userId);

        await _transactionRepository.DeleteAsync(transactionId);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByAccountAsync(Guid accountId, Guid userId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.UserId != userId)
        {
            throw new UnauthorizedAccessException("Conta não pertence ao usuário");
        }

        var transactions = await _transactionRepository.FindAsync(t => t.AccountId == accountId);
        var orderedTransactions = transactions.OrderByDescending(t => t.Date);

        return await MapTransactionsWithDetails(orderedTransactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByCategoryAsync(Guid categoryId, Guid userId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null || category.UserId != userId)
        {
            throw new UnauthorizedAccessException("Categoria não pertence ao usuário");
        }

        var transactions = await _transactionRepository.FindAsync(t => t.CategoryId == categoryId);
        var orderedTransactions = transactions.OrderByDescending(t => t.Date);

        return await MapTransactionsWithDetails(orderedTransactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByTypeAsync(Guid userId, TransactionType type)
    {
        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var transactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) && t.Type == type);

        var orderedTransactions = transactions.OrderByDescending(t => t.Date);

        return await MapTransactionsWithDetails(orderedTransactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var transactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.Date >= startDate &&
            t.Date <= endDate);

        var orderedTransactions = transactions.OrderByDescending(t => t.Date);

        return await MapTransactionsWithDetails(orderedTransactions);
    }

    private async Task<IEnumerable<TransactionDto>> MapTransactionsWithDetails(IEnumerable<Transaction> transactions)
    {
        var transactionDtos = new List<TransactionDto>();

        foreach (var transaction in transactions)
        {
            var account = await _accountRepository.GetByIdAsync(transaction.AccountId);
            var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId);

            Account? destinationAccount = null;
            if (transaction.DestinationAccountId.HasValue)
            {
                destinationAccount = await _accountRepository.GetByIdAsync(transaction.DestinationAccountId.Value);
            }

            var dto = _mapper.Map<TransactionDto>(transaction);
            dto.AccountName = account?.Name ?? "Conta removida";
            dto.CategoryName = category?.Name ?? "Categoria removida";
            dto.DestinationAccountName = destinationAccount?.Name;

            transactionDtos.Add(dto);
        }

        return transactionDtos;
    }
}
