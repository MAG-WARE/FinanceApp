using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Interfaces;

public interface IUserGroupService
{
    // Group management
    Task<IEnumerable<UserGroupDto>> GetUserGroupsAsync(Guid userId);
    Task<UserGroupDto?> GetGroupByIdAsync(Guid groupId, Guid userId);
    Task<UserGroupDto> CreateGroupAsync(CreateUserGroupDto dto, Guid userId);
    Task<UserGroupDto> UpdateGroupAsync(Guid groupId, UpdateUserGroupDto dto, Guid userId);
    Task DeleteGroupAsync(Guid groupId, Guid userId);

    // Membership management
    Task<UserGroupDto> JoinGroupAsync(JoinGroupDto dto, Guid userId);
    Task LeaveGroupAsync(Guid groupId, Guid userId);
    Task RemoveMemberAsync(Guid groupId, Guid memberUserId, Guid requestingUserId);
    Task<string> RegenerateInviteCodeAsync(Guid groupId, Guid userId);

    // Group members for context
    Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(Guid groupId, Guid userId);
    Task<IEnumerable<Guid>> GetAccessibleUserIdsAsync(Guid userId, ViewContext context, Guid? memberUserId = null);

    // Goal sharing
    Task ShareGoalAsync(ShareGoalDto dto, Guid userId);
    Task UnshareGoalAsync(UnshareGoalDto dto, Guid userId);
    Task<IEnumerable<GoalUserDto>> GetGoalUsersAsync(Guid goalId, Guid userId);
}
