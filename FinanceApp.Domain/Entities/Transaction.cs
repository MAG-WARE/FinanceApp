using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public bool IsRecurring { get; set; }
    public string? Notes { get; set; }
    
    // Para transferências entre contas
    public Guid? DestinationAccountId { get; set; }
    
    // Navigation properties
    public Account Account { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Account? DestinationAccount { get; set; }
}
