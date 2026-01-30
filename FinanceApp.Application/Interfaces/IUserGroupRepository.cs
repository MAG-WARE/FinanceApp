using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Interfaces;

public interface IUserGroupRepository
{
    Task<IEnumerable<UserGroup>> GetUserGroupsWithMembersAsync(Guid userId);
    Task<UserGroup?> GetGroupByIdWithMembersAsync(Guid groupId, Guid userId);
    Task<UserGroup?> GetGroupByIdAsync(Guid groupId);
    Task<UserGroup?> GetGroupByInviteCodeAsync(string inviteCode);
    Task<UserGroup> AddAsync(UserGroup group);
    Task UpdateAsync(UserGroup group);
    Task<UserGroupMember?> GetMembershipAsync(Guid groupId, Guid userId);
    Task<UserGroupMember> AddMemberAsync(UserGroupMember member);
    Task UpdateMemberAsync(UserGroupMember member);
    Task<IEnumerable<UserGroupMember>> GetGroupMembersAsync(Guid groupId);
    Task<bool> IsMemberAsync(Guid groupId, Guid userId);
    Task<IEnumerable<Guid>> GetUserGroupIdsAsync(Guid userId);
    Task<IEnumerable<Guid>> GetGroupMemberUserIdsAsync(IEnumerable<Guid> groupIds);
    Task<bool> AreUsersInSameGroupAsync(Guid userId1, Guid userId2);
}
