namespace FinanceApp.Domain.Entities;

public class UserGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    // Navigation properties
    public User CreatedByUser { get; set; } = null!;
    public ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();
}
