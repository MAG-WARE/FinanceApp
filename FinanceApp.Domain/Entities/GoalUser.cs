namespace FinanceApp.Domain.Entities;

public class GoalUser
{
    public Guid GoalId { get; set; }
    public Guid UserId { get; set; }
    public bool IsOwner { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public Goal Goal { get; set; } = null!;
    public User User { get; set; } = null!;
}
