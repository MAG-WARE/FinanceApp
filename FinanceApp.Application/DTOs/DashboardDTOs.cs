namespace FinanceApp.Application.DTOs;

public class DashboardSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public List<CategorySpendingDto> TopSpendingCategories { get; set; } = new();
    public List<MonthlyBalanceDto> BalanceHistory { get; set; } = new();
    public ComparisonDto Comparison { get; set; } = new();
}

public class CategorySpendingDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string? Color { get; set; }
}

public class MonthlyBalanceDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Balance { get; set; }
}

public class ComparisonDto
{
    public decimal CurrentMonthIncome { get; set; }
    public decimal PreviousMonthIncome { get; set; }
    public decimal IncomeChange { get; set; }
    public decimal IncomeChangePercentage { get; set; }

    public decimal CurrentMonthExpenses { get; set; }
    public decimal PreviousMonthExpenses { get; set; }
    public decimal ExpensesChange { get; set; }
    public decimal ExpensesChangePercentage { get; set; }
}

public class DateRangeDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
