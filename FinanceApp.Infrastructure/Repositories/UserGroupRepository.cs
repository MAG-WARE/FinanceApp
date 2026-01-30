using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.Repositories;

public class UserGroupRepository : IUserGroupRepository
{
    private readonly FinanceAppDbContext _context;

    public UserGroupRepository(FinanceAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserGroup>> GetUserGroupsWithMembersAsync(Guid userId)
    {
        return await _context.UserGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Include(g => g.CreatedByUser)
            .Where(g => !g.IsDeleted && g.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .ToListAsync();
    }

    public async Task<UserGroup?> GetGroupByIdWithMembersAsync(Guid groupId, Guid userId)
    {
        return await _context.UserGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Include(g => g.CreatedByUser)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted && g.Members.Any(m => m.UserId == userId && !m.IsDeleted));
    }

    public async Task<UserGroup?> GetGroupByIdAsync(Guid groupId)
    {
        return await _context.UserGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.IsDeleted);
    }

    public async Task<UserGroup?> GetGroupByInviteCodeAsync(string inviteCode)
    {
        return await _context.UserGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == inviteCode && !g.IsDeleted);
    }

    public async Task<UserGroup> AddAsync(UserGroup group)
    {
        _context.UserGroups.Add(group);
        await _context.SaveChangesAsync();
        return group;
    }

    public async Task UpdateAsync(UserGroup group)
    {
        _context.UserGroups.Update(group);
        await _context.SaveChangesAsync();
    }

    public async Task<UserGroupMember?> GetMembershipAsync(Guid groupId, Guid userId)
    {
        return await _context.UserGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && !m.IsDeleted);
    }

    public async Task<UserGroupMember> AddMemberAsync(UserGroupMember member)
    {
        _context.UserGroupMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task UpdateMemberAsync(UserGroupMember member)
    {
        _context.UserGroupMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserGroupMember>> GetGroupMembersAsync(Guid groupId)
    {
        return await _context.UserGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId && !m.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsMemberAsync(Guid groupId, Guid userId)
    {
        return await _context.UserGroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && !m.IsDeleted);
    }

    public async Task<IEnumerable<Guid>> GetUserGroupIdsAsync(Guid userId)
    {
        return await _context.UserGroupMembers
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.GroupId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetGroupMemberUserIdsAsync(IEnumerable<Guid> groupIds)
    {
        return await _context.UserGroupMembers
            .Where(m => groupIds.Contains(m.GroupId) && !m.IsDeleted)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> AreUsersInSameGroupAsync(Guid userId1, Guid userId2)
    {
        var user1Groups = await GetUserGroupIdsAsync(userId1);
        var user2Groups = await GetUserGroupIdsAsync(userId2);
        return user1Groups.Intersect(user2Groups).Any();
    }
}
