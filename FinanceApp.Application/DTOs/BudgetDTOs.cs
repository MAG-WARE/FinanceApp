namespace FinanceApp.Application.DTOs;

public class BudgetDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBudgetDto
{
    public Guid CategoryId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }
}

public class UpdateBudgetDto
{
    public decimal LimitAmount { get; set; }
}

public class BudgetStatusDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PercentageUsed { get; set; }
    public bool IsExceeded { get; set; }
}
