using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountName { get; set; }
    public Guid? GoalId { get; set; }
    public string? GoalName { get; set; }
}

public class CreateTransactionDto
{
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public Guid? GoalId { get; set; }
}

public class UpdateTransactionDto
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public Guid? GoalId { get; set; }
}
