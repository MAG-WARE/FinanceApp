namespace FinanceApp.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal LimitAmount { get; set; }
    
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
