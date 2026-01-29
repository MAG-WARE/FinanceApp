using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.DTOs;

public class UserGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
}

public class GroupMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public GroupRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class CreateUserGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateUserGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class JoinGroupDto
{
    public string InviteCode { get; set; } = string.Empty;
}

public class RegenerateInviteCodeDto
{
    public Guid GroupId { get; set; }
}

public class ShareGoalDto
{
    public Guid GoalId { get; set; }
    public List<Guid> UserIds { get; set; } = new();
}

public class UnshareGoalDto
{
    public Guid GoalId { get; set; }
    public Guid UserId { get; set; }
}

public class GoalUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public DateTime AddedAt { get; set; }
}

public class ViewContextDto
{
    public ViewContext Context { get; set; } = ViewContext.Own;
    public Guid? MemberUserId { get; set; }
}
