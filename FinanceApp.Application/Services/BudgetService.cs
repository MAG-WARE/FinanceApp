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
        // Validar que a categoria pertence ao usuário
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category == null || category.UserId != userId)
        {
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
            throw new InvalidOperationException("Já existe um orçamento para esta categoria e período");
        }

        _logger.LogInformation("Creating budget for category {CategoryId}, month {Month}/{Year}, user {UserId}",
            dto.CategoryId, dto.Month, dto.Year, userId);

        var budget = _mapper.Map<Budget>(dto);
        budget.UserId = userId;
        budget.Id = Guid.NewGuid();

        var createdBudget = await _budgetRepository.AddAsync(budget);

        var budgetDtos = await MapBudgetsWithSpending(new[] { createdBudget }, userId);
        return budgetDtos.First();
    }

    public async Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetDto dto, Guid userId)
    {
        var budgets = await _budgetRepository.FindAsync(b => b.Id == budgetId && b.UserId == userId);
        var budget = budgets.FirstOrDefault();

        if (budget == null)
        {
            throw new KeyNotFoundException("Orçamento não encontrado");
        }

        _logger.LogInformation("Updating budget {BudgetId} for user {UserId}", budgetId, userId);

        budget.LimitAmount = dto.LimitAmount;

        await _budgetRepository.UpdateAsync(budget);

        var budgetDtos = await MapBudgetsWithSpending(new[] { budget }, userId);
        return budgetDtos.First();
    }

    public async Task DeleteBudgetAsync(Guid budgetId, Guid userId)
    {
        var budgets = await _budgetRepository.FindAsync(b => b.Id == budgetId && b.UserId == userId);
        var budget = budgets.FirstOrDefault();

        if (budget == null)
        {
            throw new KeyNotFoundException("Orçamento não encontrado");
        }

        _logger.LogInformation("Deleting budget {BudgetId} for user {UserId}", budgetId, userId);

        await _budgetRepository.DeleteAsync(budgetId);
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
