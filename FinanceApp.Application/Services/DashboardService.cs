using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUserGroupService _userGroupService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IRepository<Transaction> transactionRepository,
        IRepository<Account> accountRepository,
        IRepository<Category> categoryRepository,
        IRepository<User> userRepository,
        IUserGroupService userGroupService,
        ILogger<DashboardService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _userGroupService = userGroupService;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        return await GetDashboardSummaryAsync(userId, ViewContext.Own, null);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, ViewContext context, Guid? memberUserId = null)
    {
        var now = DateTime.UtcNow;
        return await GetDashboardSummaryByMonthAsync(userId, now.Month, now.Year, context, memberUserId);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryByMonthAsync(Guid userId, int month, int year)
    {
        return await GetDashboardSummaryByMonthAsync(userId, month, year, ViewContext.Own, null);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryByMonthAsync(Guid userId, int month, int year, ViewContext context, Guid? memberUserId = null)
    {
        var startDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

        return await GetDashboardSummaryByDateRangeAsync(userId, startDate, endDate, context, memberUserId);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await GetDashboardSummaryByDateRangeAsync(userId, startDate, endDate, ViewContext.Own, null);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, ViewContext context, Guid? memberUserId = null)
    {
        var accessibleUserIds = await _userGroupService.GetAccessibleUserIdsAsync(userId, context, memberUserId);

        var startDateUtc = startDate.Kind == DateTimeKind.Utc
            ? startDate
            : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var endDateUtc = endDate.Kind == DateTimeKind.Utc
            ? endDate
            : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        _logger.LogInformation("Getting dashboard summary for user {UserId} with context {Context} from {StartDate} to {EndDate}",
            userId, context, startDateUtc, endDateUtc);

        var userAccounts = await _accountRepository.FindAsync(a => accessibleUserIds.Contains(a.UserId));
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var transactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.Date >= startDateUtc &&
            t.Date <= endDateUtc);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var balance = totalIncome - totalExpenses;

        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .Take(5)
            .ToList();

        var topSpendingCategories = new List<CategorySpendingDto>();
        foreach (var item in expensesByCategory)
        {
            var category = await _categoryRepository.GetByIdAsync(item.CategoryId);
            if (category != null)
            {
                var user = await _userRepository.GetByIdAsync(category.UserId);
                topSpendingCategories.Add(new CategorySpendingDto
                {
                    CategoryId = item.CategoryId,
                    CategoryName = category.Name,
                    UserId = category.UserId,
                    UserName = user?.Name ?? "Usuário removido",
                    Amount = item.Amount,
                    Percentage = totalExpenses > 0 ? (item.Amount / totalExpenses) * 100 : 0,
                    Color = category.Color
                });
            }
        }

        var balanceHistory = await GetBalanceHistoryAsync(accessibleUserIds, 6);

        var comparison = await GetComparisonWithPreviousMonthAsync(accessibleUserIds, startDateUtc);

        return new DashboardSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            Balance = balance,
            Month = startDateUtc.Month,
            Year = startDateUtc.Year,
            TopSpendingCategories = topSpendingCategories,
            BalanceHistory = balanceHistory,
            Comparison = comparison
        };
    }

    private async Task<List<MonthlyBalanceDto>> GetBalanceHistoryAsync(IEnumerable<Guid> userIds, int numberOfMonths)
    {
        var userAccounts = await _accountRepository.FindAsync(a => userIds.Contains(a.UserId));
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var balanceHistory = new List<MonthlyBalanceDto>();
        var currentDate = DateTime.UtcNow;

        for (int i = numberOfMonths - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var startDate = DateTime.SpecifyKind(new DateTime(targetDate.Year, targetDate.Month, 1), DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(startDate.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var transactions = await _transactionRepository.FindAsync(t =>
                accountIds.Contains(t.AccountId) &&
                t.Date >= startDate &&
                t.Date <= endDate);

            var income = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            balanceHistory.Add(new MonthlyBalanceDto
            {
                Month = targetDate.Month,
                Year = targetDate.Year,
                Income = income,
                Expenses = expenses,
                Balance = income - expenses
            });
        }

        return balanceHistory;
    }

    private async Task<ComparisonDto> GetComparisonWithPreviousMonthAsync(IEnumerable<Guid> userIds, DateTime currentMonthStart)
    {
        var userAccounts = await _accountRepository.FindAsync(a => userIds.Contains(a.UserId));
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var currentMonthStartUtc = currentMonthStart.Kind == DateTimeKind.Utc
            ? currentMonthStart
            : DateTime.SpecifyKind(currentMonthStart, DateTimeKind.Utc);

        var currentMonthEnd = DateTime.SpecifyKind(currentMonthStartUtc.AddMonths(1).AddDays(-1), DateTimeKind.Utc);
        var currentTransactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.Date >= currentMonthStartUtc &&
            t.Date <= currentMonthEnd);

        var currentIncome = currentTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var currentExpenses = currentTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var previousMonthStart = DateTime.SpecifyKind(currentMonthStartUtc.AddMonths(-1), DateTimeKind.Utc);
        var previousMonthEnd = DateTime.SpecifyKind(previousMonthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);
        var previousTransactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.Date >= previousMonthStart &&
            t.Date <= previousMonthEnd);

        var previousIncome = previousTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var previousExpenses = previousTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var incomeChange = currentIncome - previousIncome;
        var incomeChangePercentage = previousIncome > 0
            ? (incomeChange / previousIncome) * 100
            : 0;

        var expensesChange = currentExpenses - previousExpenses;
        var expensesChangePercentage = previousExpenses > 0
            ? (expensesChange / previousExpenses) * 100
            : 0;

        return new ComparisonDto
        {
            CurrentMonthIncome = currentIncome,
            PreviousMonthIncome = previousIncome,
            IncomeChange = incomeChange,
            IncomeChangePercentage = incomeChangePercentage,
            CurrentMonthExpenses = currentExpenses,
            PreviousMonthExpenses = previousExpenses,
            ExpensesChange = expensesChange,
            ExpensesChangePercentage = expensesChangePercentage
        };
    }
}
