namespace FinanceApp.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    // User groups
    public ICollection<UserGroup> CreatedGroups { get; set; } = new List<UserGroup>();
    public ICollection<UserGroupMember> GroupMemberships { get; set; } = new List<UserGroupMember>();

    // Shared goals - N:N relationship
    public ICollection<GoalUser> SharedGoals { get; set; } = new List<GoalUser>();
}
