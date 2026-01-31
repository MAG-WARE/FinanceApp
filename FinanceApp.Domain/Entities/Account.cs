using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
