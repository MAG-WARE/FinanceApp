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
    private readonly IRepository<Goal> _goalRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        IRepository<Transaction> transactionRepository,
        IRepository<Account> accountRepository,
        IRepository<Category> categoryRepository,
        IRepository<Goal> goalRepository,
        IMapper mapper,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _goalRepository = goalRepository;
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
        try
        {
            _logger.LogInformation(
                "Iniciando criação de transação - UserId: {UserId}, Type: {Type}, Amount: {Amount}, AccountId: {AccountId}, CategoryId: {CategoryId}",
                userId, dto.Type, dto.Amount, dto.AccountId, dto.CategoryId);

            // Validar que a conta pertence ao usuário
            var account = await _accountRepository.GetByIdAsync(dto.AccountId);
            if (account == null)
            {
                _logger.LogWarning("Conta não encontrada: {AccountId}", dto.AccountId);
                throw new KeyNotFoundException($"Conta com ID {dto.AccountId} não encontrada");
            }
            if (account.UserId != userId)
            {
                _logger.LogWarning("Tentativa de acesso não autorizado - UserId: {UserId}, AccountId: {AccountId}, AccountOwner: {OwnerId}",
                    userId, dto.AccountId, account.UserId);
                throw new UnauthorizedAccessException("Conta não pertence ao usuário");
            }

            // Validar que a categoria pertence ao usuário
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning("Categoria não encontrada: {CategoryId}", dto.CategoryId);
                throw new KeyNotFoundException($"Categoria com ID {dto.CategoryId} não encontrada");
            }
            if (category.UserId != userId)
            {
                _logger.LogWarning("Tentativa de acesso não autorizado - UserId: {UserId}, CategoryId: {CategoryId}, CategoryOwner: {OwnerId}",
                    userId, dto.CategoryId, category.UserId);
                throw new UnauthorizedAccessException("Categoria não pertence ao usuário");
            }

            // Validar tipo de categoria com tipo de transação
            if ((dto.Type == TransactionType.Income && category.Type != CategoryType.Income) ||
                (dto.Type == TransactionType.Expense && category.Type != CategoryType.Expense))
            {
                _logger.LogWarning(
                    "Tipo de categoria incompatível - TransactionType: {TransactionType}, CategoryType: {CategoryType}",
                    dto.Type, category.Type);
                throw new InvalidOperationException($"Tipo de categoria ({category.Type}) não corresponde ao tipo de transação ({dto.Type})");
            }

            // Validar conta de destino para transferências
            if (dto.Type == TransactionType.Transfer)
            {
                if (!dto.DestinationAccountId.HasValue)
                {
                    _logger.LogWarning("Tentativa de criar transferência sem conta de destino");
                    throw new InvalidOperationException("Conta de destino é obrigatória para transferências");
                }

                var destinationAccount = await _accountRepository.GetByIdAsync(dto.DestinationAccountId.Value);
                if (destinationAccount == null)
                {
                    _logger.LogWarning("Conta de destino não encontrada: {DestinationAccountId}", dto.DestinationAccountId.Value);
                    throw new KeyNotFoundException($"Conta de destino com ID {dto.DestinationAccountId.Value} não encontrada");
                }
                if (destinationAccount.UserId != userId)
                {
                    _logger.LogWarning("Conta de destino não pertence ao usuário - UserId: {UserId}, DestinationAccountId: {DestinationAccountId}",
                        userId, dto.DestinationAccountId.Value);
                    throw new UnauthorizedAccessException("Conta de destino não pertence ao usuário");
                }

                if (dto.AccountId == dto.DestinationAccountId)
                {
                    _logger.LogWarning("Tentativa de transferência para a mesma conta - AccountId: {AccountId}", dto.AccountId);
                    throw new InvalidOperationException("Conta de origem e destino não podem ser iguais");
                }
            }

            // Validar meta se fornecida
            Goal? goal = null;
            if (dto.GoalId.HasValue)
            {
                var goals = await _goalRepository.FindAsync(g => g.Id == dto.GoalId.Value && g.UserId == userId);
                goal = goals.FirstOrDefault();

                if (goal == null)
                {
                    _logger.LogWarning("Meta não encontrada ou não pertence ao usuário: {GoalId}", dto.GoalId.Value);
                    throw new KeyNotFoundException($"Meta com ID {dto.GoalId.Value} não encontrada");
                }

                _logger.LogInformation("Transação será vinculada à meta {GoalName} (ID: {GoalId})", goal.Name, goal.Id);
            }

            _logger.LogInformation("Validações concluídas. Criando entidade de transação...");

            var transaction = _mapper.Map<Transaction>(dto);
            transaction.Id = Guid.NewGuid();

            if (transaction.Date.Kind != DateTimeKind.Utc)
                transaction.Date = DateTime.SpecifyKind(transaction.Date, DateTimeKind.Utc);

            _logger.LogInformation("Salvando transação no banco de dados - TransactionId: {TransactionId}", transaction.Id);

            try
            {
                var createdTransaction = await _transactionRepository.AddAsync(transaction);

                _logger.LogInformation("Transação criada com sucesso - TransactionId: {TransactionId}, UserId: {UserId}",
                    createdTransaction.Id, userId);

                // Atualizar meta se vinculada
                if (goal != null)
                {
                    goal.CurrentAmount += dto.Amount;

                    // Verificar se a meta foi completada
                    if (goal.CurrentAmount >= goal.TargetAmount && !goal.IsCompleted)
                    {
                        goal.IsCompleted = true;
                        _logger.LogInformation("Meta {GoalName} completada! CurrentAmount: {CurrentAmount}, TargetAmount: {TargetAmount}",
                            goal.Name, goal.CurrentAmount, goal.TargetAmount);
                    }

                    await _goalRepository.UpdateAsync(goal);
                    _logger.LogInformation("Meta {GoalName} atualizada - CurrentAmount: {CurrentAmount}", goal.Name, goal.CurrentAmount);
                }

                return (await MapTransactionsWithDetails(new[] { createdTransaction })).First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao salvar transação no banco - TransactionId: {TransactionId}, UserId: {UserId}, AccountId: {AccountId}, Amount: {Amount}. Erro: {ErrorMessage}",
                    transaction.Id, userId, dto.AccountId, dto.Amount, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException &&
                                    ex is not KeyNotFoundException &&
                                    ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao criar transação - UserId: {UserId}, Type: {Type}, Amount: {Amount}",
                userId, dto.Type, dto.Amount);
            throw new InvalidOperationException($"Erro ao criar transação: {ex.Message}", ex);
        }
    }

    public async Task<TransactionDto> UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto, Guid userId)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando atualização de transação - TransactionId: {TransactionId}, UserId: {UserId}, NewAmount: {Amount}",
                transactionId, userId, dto.Amount);

            var transactions = await _transactionRepository.FindAsync(t => t.Id == transactionId);
            var transaction = transactions.FirstOrDefault();

            if (transaction == null)
            {
                _logger.LogWarning("Transação não encontrada para atualização: {TransactionId}", transactionId);
                throw new KeyNotFoundException($"Transação com ID {transactionId} não encontrada");
            }

            var accounts = await _accountRepository.FindAsync(a => a.Id == transaction.AccountId);
            var account = accounts.FirstOrDefault();

            if (account == null)
            {
                _logger.LogWarning("Conta da transação não encontrada: {AccountId}", transaction.AccountId);
                throw new KeyNotFoundException($"Conta com ID {transaction.AccountId} não encontrada");
            }
            if (account.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar transação não autorizada - UserId: {UserId}, TransactionId: {TransactionId}, AccountOwner: {OwnerId}",
                    userId, transactionId, account.UserId);
                throw new UnauthorizedAccessException("Transação não pertence ao usuário");
            }

            var categories = await _categoryRepository.FindAsync(c => c.Id == dto.CategoryId);
            var category = categories.FirstOrDefault();

            if (category == null)
            {
                _logger.LogWarning("Categoria não encontrada: {CategoryId}", dto.CategoryId);
                throw new KeyNotFoundException($"Categoria com ID {dto.CategoryId} não encontrada");
            }
            if (category.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentativa de usar categoria não autorizada - UserId: {UserId}, CategoryId: {CategoryId}, CategoryOwner: {OwnerId}",
                    userId, dto.CategoryId, category.UserId);
                throw new UnauthorizedAccessException("Categoria não pertence ao usuário");
            }

            _logger.LogInformation("Validações concluídas. Atualizando transação no banco...");

            // Armazenar valores antigos para recalcular meta
            var oldAmount = transaction.Amount;
            var oldGoalId = transaction.GoalId;

            transaction.CategoryId = dto.CategoryId;
            transaction.Amount = dto.Amount;
            transaction.Date = dto.Date;
            transaction.Description = dto.Description;
            transaction.Notes = dto.Notes;
            transaction.GoalId = dto.GoalId;

            if (transaction.Date.Kind != DateTimeKind.Utc)
                transaction.Date = DateTime.SpecifyKind(transaction.Date, DateTimeKind.Utc);

            try
            {
                await _transactionRepository.UpdateAsync(transaction);

                _logger.LogInformation("Transação atualizada com sucesso - TransactionId: {TransactionId}, UserId: {UserId}",
                    transactionId, userId);

                // Atualizar metas se necessário
                if (oldGoalId != dto.GoalId || oldAmount != dto.Amount)
                {
                    // Remover valor da meta antiga
                    if (oldGoalId.HasValue)
                    {
                        var oldGoals = await _goalRepository.FindAsync(g => g.Id == oldGoalId.Value);
                        var oldGoal = oldGoals.FirstOrDefault();
                        if (oldGoal != null)
                        {
                            oldGoal.CurrentAmount -= oldAmount;
                            if (oldGoal.CurrentAmount < oldGoal.TargetAmount && oldGoal.IsCompleted)
                            {
                                oldGoal.IsCompleted = false;
                            }
                            await _goalRepository.UpdateAsync(oldGoal);
                            _logger.LogInformation("Meta antiga {GoalName} atualizada - CurrentAmount: {CurrentAmount}",
                                oldGoal.Name, oldGoal.CurrentAmount);
                        }
                    }

                    // Adicionar valor à meta nova
                    if (dto.GoalId.HasValue)
                    {
                        var newGoals = await _goalRepository.FindAsync(g => g.Id == dto.GoalId.Value && g.UserId == userId);
                        var newGoal = newGoals.FirstOrDefault();
                        if (newGoal != null)
                        {
                            newGoal.CurrentAmount += dto.Amount;
                            if (newGoal.CurrentAmount >= newGoal.TargetAmount && !newGoal.IsCompleted)
                            {
                                newGoal.IsCompleted = true;
                                _logger.LogInformation("Meta {GoalName} completada! CurrentAmount: {CurrentAmount}, TargetAmount: {TargetAmount}",
                                    newGoal.Name, newGoal.CurrentAmount, newGoal.TargetAmount);
                            }
                            await _goalRepository.UpdateAsync(newGoal);
                            _logger.LogInformation("Meta nova {GoalName} atualizada - CurrentAmount: {CurrentAmount}",
                                newGoal.Name, newGoal.CurrentAmount);
                        }
                    }
                }

                return (await MapTransactionsWithDetails(new[] { transaction })).First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao atualizar transação no banco - TransactionId: {TransactionId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    transactionId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException &&
                                    ex is not KeyNotFoundException &&
                                    ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao atualizar transação - TransactionId: {TransactionId}, UserId: {UserId}",
                transactionId, userId);
            throw new InvalidOperationException($"Erro ao atualizar transação: {ex.Message}", ex);
        }
    }

    public async Task DeleteTransactionAsync(Guid transactionId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Iniciando exclusão de transação - TransactionId: {TransactionId}, UserId: {UserId}",
                transactionId, userId);

            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                _logger.LogWarning("Transação não encontrada para exclusão: {TransactionId}", transactionId);
                throw new KeyNotFoundException($"Transação com ID {transactionId} não encontrada");
            }

            var account = await _accountRepository.GetByIdAsync(transaction.AccountId);
            if (account == null)
            {
                _logger.LogWarning("Conta da transação não encontrada: {AccountId}", transaction.AccountId);
                throw new KeyNotFoundException($"Conta com ID {transaction.AccountId} não encontrada");
            }
            if (account.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentativa de excluir transação não autorizada - UserId: {UserId}, TransactionId: {TransactionId}, AccountOwner: {OwnerId}",
                    userId, transactionId, account.UserId);
                throw new UnauthorizedAccessException("Transação não pertence ao usuário");
            }

            _logger.LogInformation("Validações concluídas. Excluindo transação do banco...");

            try
            {
                // Atualizar meta se vinculada
                if (transaction.GoalId.HasValue)
                {
                    var goals = await _goalRepository.FindAsync(g => g.Id == transaction.GoalId.Value);
                    var goal = goals.FirstOrDefault();
                    if (goal != null)
                    {
                        goal.CurrentAmount -= transaction.Amount;
                        if (goal.CurrentAmount < goal.TargetAmount && goal.IsCompleted)
                        {
                            goal.IsCompleted = false;
                        }
                        await _goalRepository.UpdateAsync(goal);
                        _logger.LogInformation("Meta {GoalName} atualizada após exclusão de transação - CurrentAmount: {CurrentAmount}",
                            goal.Name, goal.CurrentAmount);
                    }
                }

                await _transactionRepository.DeleteAsync(transactionId);

                _logger.LogInformation("Transação excluída com sucesso - TransactionId: {TransactionId}, UserId: {UserId}",
                    transactionId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao excluir transação no banco - TransactionId: {TransactionId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    transactionId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException &&
                                    ex is not KeyNotFoundException &&
                                    ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao excluir transação - TransactionId: {TransactionId}, UserId: {UserId}",
                transactionId, userId);
            throw new InvalidOperationException($"Erro ao excluir transação: {ex.Message}", ex);
        }
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

            Goal? goal = null;
            if (transaction.GoalId.HasValue)
            {
                var goals = await _goalRepository.FindAsync(g => g.Id == transaction.GoalId.Value);
                goal = goals.FirstOrDefault();
            }

            var dto = _mapper.Map<TransactionDto>(transaction);
            dto.AccountName = account?.Name ?? "Conta removida";
            dto.CategoryName = category?.Name ?? "Categoria removida";
            dto.DestinationAccountName = destinationAccount?.Name;
            dto.GoalName = goal?.Name;

            transactionDtos.Add(dto);
        }

        return transactionDtos;
    }
}
