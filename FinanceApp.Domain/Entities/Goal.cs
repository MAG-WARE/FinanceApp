namespace FinanceApp.Domain.Entities;

public class Goal : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public bool IsCompleted { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }

    public User User { get; set; } = null!;

    public ICollection<GoalUser> GoalUsers { get; set; } = new List<GoalUser>();
}
