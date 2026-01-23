using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IRepository<Budget> _budgetRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        IRepository<Budget> budgetRepository,
        IRepository<Category> categoryRepository,
        IRepository<Transaction> transactionRepository,
        IRepository<Account> accountRepository,
        IMapper mapper,
        ILogger<BudgetService> logger)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync(Guid userId)
    {
        var budgets = await _budgetRepository.FindAsync(b => b.UserId == userId);
        return await MapBudgetsWithSpending(budgets, userId);
    }

    public async Task<BudgetDto?> GetBudgetByIdAsync(Guid budgetId, Guid userId)
    {
        var budgets = await _budgetRepository.FindAsync(b => b.Id == budgetId && b.UserId == userId);
        var budget = budgets.FirstOrDefault();

        if (budget == null)
            return null;

        var budgetDtos = await MapBudgetsWithSpending(new[] { budget }, userId);
        return budgetDtos.FirstOrDefault();
    }

    public async Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto dto, Guid userId)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando criação de orçamento - UserId: {UserId}, CategoryId: {CategoryId}, Month: {Month}/{Year}, Limit: {Limit}",
                userId, dto.CategoryId, dto.Month, dto.Year, dto.LimitAmount);

            // Validar que a categoria pertence ao usuário
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                _logger.LogWarning("Categoria não encontrada: {CategoryId}", dto.CategoryId);
                throw new KeyNotFoundException($"Categoria com ID {dto.CategoryId} não encontrada");
            }
            if (category.UserId != userId)
            {
                _logger.LogWarning(
                    "Tentativa de criar orçamento com categoria não autorizada - UserId: {UserId}, CategoryId: {CategoryId}, CategoryOwner: {OwnerId}",
                    userId, dto.CategoryId, category.UserId);
                throw new UnauthorizedAccessException("Categoria não pertence ao usuário");
            }

            // Verificar se já existe um orçamento para essa categoria e período
            var existingBudgets = await _budgetRepository.FindAsync(b =>
                b.UserId == userId &&
                b.CategoryId == dto.CategoryId &&
                b.Month == dto.Month &&
                b.Year == dto.Year);

            if (existingBudgets.Any())
            {
                _logger.LogWarning(
                    "Tentativa de criar orçamento duplicado - UserId: {UserId}, CategoryId: {CategoryId}, Month: {Month}/{Year}",
                    userId, dto.CategoryId, dto.Month, dto.Year);
                throw new InvalidOperationException($"Já existe um orçamento para esta categoria no período {dto.Month:D2}/{dto.Year}");
            }

            _logger.LogInformation("Validações concluídas. Criando orçamento no banco...");

            var budget = _mapper.Map<Budget>(dto);
            budget.UserId = userId;
            budget.Id = Guid.NewGuid();

            try
            {
                var createdBudget = await _budgetRepository.AddAsync(budget);

                _logger.LogInformation("Orçamento criado com sucesso - BudgetId: {BudgetId}, UserId: {UserId}",
                    createdBudget.Id, userId);

                var budgetDtos = await MapBudgetsWithSpending(new[] { createdBudget }, userId);
                return budgetDtos.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao salvar orçamento no banco - UserId: {UserId}, CategoryId: {CategoryId}. Erro: {ErrorMessage}",
                    userId, dto.CategoryId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not UnauthorizedAccessException &&
                                    ex is not KeyNotFoundException &&
                                    ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao criar orçamento - UserId: {UserId}, CategoryId: {CategoryId}",
                userId, dto.CategoryId);
            throw new InvalidOperationException($"Erro ao criar orçamento: {ex.Message}", ex);
        }
    }

    public async Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetDto dto, Guid userId)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando atualização de orçamento - BudgetId: {BudgetId}, UserId: {UserId}, NewLimit: {Limit}",
                budgetId, userId, dto.LimitAmount);

            var budgets = await _budgetRepository.FindAsync(b => b.Id == budgetId && b.UserId == userId);
            var budget = budgets.FirstOrDefault();

            if (budget == null)
            {
                _logger.LogWarning("Orçamento não encontrado ou não pertence ao usuário - BudgetId: {BudgetId}, UserId: {UserId}",
                    budgetId, userId);
                throw new KeyNotFoundException($"Orçamento com ID {budgetId} não encontrado");
            }

            _logger.LogInformation("Validações concluídas. Atualizando orçamento no banco...");

            budget.LimitAmount = dto.LimitAmount;

            try
            {
                await _budgetRepository.UpdateAsync(budget);

                _logger.LogInformation("Orçamento atualizado com sucesso - BudgetId: {BudgetId}, UserId: {UserId}",
                    budgetId, userId);

                var budgetDtos = await MapBudgetsWithSpending(new[] { budget }, userId);
                return budgetDtos.First();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao atualizar orçamento no banco - BudgetId: {BudgetId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    budgetId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao atualizar orçamento - BudgetId: {BudgetId}, UserId: {UserId}",
                budgetId, userId);
            throw new InvalidOperationException($"Erro ao atualizar orçamento: {ex.Message}", ex);
        }
    }

    public async Task DeleteBudgetAsync(Guid budgetId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Iniciando exclusão de orçamento - BudgetId: {BudgetId}, UserId: {UserId}",
                budgetId, userId);

            var budgets = await _budgetRepository.FindAsync(b => b.Id == budgetId && b.UserId == userId);
            var budget = budgets.FirstOrDefault();

            if (budget == null)
            {
                _logger.LogWarning("Orçamento não encontrado ou não pertence ao usuário - BudgetId: {BudgetId}, UserId: {UserId}",
                    budgetId, userId);
                throw new KeyNotFoundException($"Orçamento com ID {budgetId} não encontrado");
            }

            _logger.LogInformation("Validações concluídas. Excluindo orçamento do banco...");

            try
            {
                await _budgetRepository.DeleteAsync(budgetId);

                _logger.LogInformation("Orçamento excluído com sucesso - BudgetId: {BudgetId}, UserId: {UserId}",
                    budgetId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao excluir orçamento no banco - BudgetId: {BudgetId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    budgetId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao excluir orçamento - BudgetId: {BudgetId}, UserId: {UserId}",
                budgetId, userId);
            throw new InvalidOperationException($"Erro ao excluir orçamento: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<BudgetStatusDto>> GetBudgetStatusAsync(Guid userId, int month, int year)
    {
        var budgets = await _budgetRepository.FindAsync(b =>
            b.UserId == userId &&
            b.Month == month &&
            b.Year == year);

        var budgetStatuses = new List<BudgetStatusDto>();

        foreach (var budget in budgets)
        {
            var category = await _categoryRepository.GetByIdAsync(budget.CategoryId);
            var spentAmount = await CalculateSpentAmount(userId, budget.CategoryId, month, year);
            var remainingAmount = budget.LimitAmount - spentAmount;
            var percentageUsed = budget.LimitAmount > 0 ? (spentAmount / budget.LimitAmount) * 100 : 0;

            budgetStatuses.Add(new BudgetStatusDto
            {
                Id = budget.Id,
                CategoryName = category?.Name ?? "Categoria removida",
                Month = month,
                Year = year,
                LimitAmount = budget.LimitAmount,
                SpentAmount = spentAmount,
                RemainingAmount = remainingAmount,
                PercentageUsed = percentageUsed,
                IsExceeded = spentAmount > budget.LimitAmount
            });
        }

        return budgetStatuses.OrderByDescending(b => b.PercentageUsed);
    }

    public async Task<IEnumerable<BudgetDto>> GetBudgetsByMonthAsync(Guid userId, int month, int year)
    {
        var budgets = await _budgetRepository.FindAsync(b =>
            b.UserId == userId &&
            b.Month == month &&
            b.Year == year);

        return await MapBudgetsWithSpending(budgets, userId);
    }

    private async Task<decimal> CalculateSpentAmount(Guid userId, Guid categoryId, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var transactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.CategoryId == categoryId &&
            t.Type == TransactionType.Expense &&
            t.Date >= startDate &&
            t.Date <= endDate);

        return transactions.Sum(t => t.Amount);
    }

    private async Task<IEnumerable<BudgetDto>> MapBudgetsWithSpending(IEnumerable<Budget> budgets, Guid userId)
    {
        var budgetDtos = new List<BudgetDto>();

        foreach (var budget in budgets)
        {
            var category = await _categoryRepository.GetByIdAsync(budget.CategoryId);
            var spentAmount = await CalculateSpentAmount(userId, budget.CategoryId, budget.Month, budget.Year);
            var remainingAmount = budget.LimitAmount - spentAmount;
            var percentageUsed = budget.LimitAmount > 0 ? (spentAmount / budget.LimitAmount) * 100 : 0;

            var dto = _mapper.Map<BudgetDto>(budget);
            dto.CategoryName = category?.Name ?? "Categoria removida";
            dto.SpentAmount = spentAmount;
            dto.RemainingAmount = remainingAmount;
            dto.PercentageUsed = percentageUsed;

            budgetDtos.Add(dto);
        }

        return budgetDtos;
    }
}
