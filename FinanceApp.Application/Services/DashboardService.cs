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
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IRepository<Transaction> transactionRepository,
        IRepository<Account> accountRepository,
        IRepository<Category> categoryRepository,
        ILogger<DashboardService> logger)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await GetDashboardSummaryByMonthAsync(userId, now.Month, now.Year);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryByMonthAsync(Guid userId, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await GetDashboardSummaryByDateRangeAsync(userId, startDate, endDate);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Getting dashboard summary for user {UserId} from {StartDate} to {EndDate}",
            userId, startDate, endDate);

        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var transactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.Date >= startDate &&
            t.Date <= endDate);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var balance = totalIncome - totalExpenses;

        // Top 5 categorias de gastos
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
                topSpendingCategories.Add(new CategorySpendingDto
                {
                    CategoryId = item.CategoryId,
                    CategoryName = category.Name,
                    Amount = item.Amount,
                    Percentage = totalExpenses > 0 ? (item.Amount / totalExpenses) * 100 : 0,
                    Color = category.Color
                });
            }
        }

        // Evolução do saldo nos últimos 6 meses
        var balanceHistory = await GetBalanceHistoryAsync(userId, 6);

        // Comparativo com mês anterior
        var comparison = await GetComparisonWithPreviousMonthAsync(userId, startDate);

        return new DashboardSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            Balance = balance,
            Month = startDate.Month,
            Year = startDate.Year,
            TopSpendingCategories = topSpendingCategories,
            BalanceHistory = balanceHistory,
            Comparison = comparison
        };
    }

    private async Task<List<MonthlyBalanceDto>> GetBalanceHistoryAsync(Guid userId, int numberOfMonths)
    {
        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        var balanceHistory = new List<MonthlyBalanceDto>();
        var currentDate = DateTime.UtcNow;

        for (int i = numberOfMonths - 1; i >= 0; i--)
        {
            var targetDate = currentDate.AddMonths(-i);
            var startDate = new DateTime(targetDate.Year, targetDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

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

    private async Task<ComparisonDto> GetComparisonWithPreviousMonthAsync(Guid userId, DateTime currentMonthStart)
    {
        var userAccounts = await _accountRepository.FindAsync(a => a.UserId == userId);
        var accountIds = userAccounts.Select(a => a.Id).ToList();

        // Mês atual
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
        var currentTransactions = await _transactionRepository.FindAsync(t =>
            accountIds.Contains(t.AccountId) &&
            t.Date >= currentMonthStart &&
            t.Date <= currentMonthEnd);

        var currentIncome = currentTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var currentExpenses = currentTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        // Mês anterior
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
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
