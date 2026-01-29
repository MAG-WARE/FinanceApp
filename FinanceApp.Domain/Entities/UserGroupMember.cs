using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class UserGroupMember : BaseEntity
{
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public GroupRole Role { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation properties
    public UserGroup Group { get; set; } = null!;
    public User User { get; set; } = null!;
}
