using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Category : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryType Type { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
