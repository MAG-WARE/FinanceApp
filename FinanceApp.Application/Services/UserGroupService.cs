using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class UserGroupService : IUserGroupService
{
    private readonly FinanceAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserGroupService> _logger;

    public UserGroupService(
        FinanceAppDbContext context,
        IMapper mapper,
        ILogger<UserGroupService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<UserGroupDto>> GetUserGroupsAsync(Guid userId)
    {
        var groups = await _context.UserGroups
            .Include(g => g.CreatedByUser)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserGroupDto>>(groups);
    }

    public async Task<UserGroupDto?> GetGroupByIdAsync(Guid groupId, Guid userId)
    {
        var group = await _context.UserGroups
            .Include(g => g.CreatedByUser)
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.Members.Any(m => m.UserId == userId));

        return group == null ? null : _mapper.Map<UserGroupDto>(group);
    }

    public async Task<UserGroupDto> CreateGroupAsync(CreateUserGroupDto dto, Guid userId)
    {
        _logger.LogInformation("Creating group {GroupName} for user {UserId}", dto.Name, userId);

        var group = _mapper.Map<UserGroup>(dto);
        group.Id = Guid.NewGuid();
        group.CreatedByUserId = userId;
        group.InviteCode = GenerateInviteCode();

        _context.UserGroups.Add(group);

        // Add creator as owner member
        var member = new UserGroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return await GetGroupByIdAsync(group.Id, userId) ?? throw new Exception("Erro ao criar grupo");
    }

    public async Task<UserGroupDto> UpdateGroupAsync(Guid groupId, UpdateUserGroupDto dto, Guid userId)
    {
        var group = await _context.UserGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new KeyNotFoundException("Grupo não encontrado");

        var membership = group.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || membership.Role != GroupRole.Owner)
            throw new UnauthorizedAccessException("Apenas o proprietário pode editar o grupo");

        _logger.LogInformation("Updating group {GroupId} for user {UserId}", groupId, userId);

        group.Name = dto.Name;
        group.Description = dto.Description;

        await _context.SaveChangesAsync();

        return await GetGroupByIdAsync(group.Id, userId) ?? throw new Exception("Erro ao atualizar grupo");
    }

    public async Task DeleteGroupAsync(Guid groupId, Guid userId)
    {
        var group = await _context.UserGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new KeyNotFoundException("Grupo não encontrado");

        var membership = group.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || membership.Role != GroupRole.Owner)
            throw new UnauthorizedAccessException("Apenas o proprietário pode excluir o grupo");

        _logger.LogInformation("Deleting group {GroupId} for user {UserId}", groupId, userId);

        group.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<UserGroupDto> JoinGroupAsync(JoinGroupDto dto, Guid userId)
    {
        var group = await _context.UserGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == dto.InviteCode && !g.IsDeleted);

        if (group == null)
            throw new KeyNotFoundException("Código de convite inválido ou grupo não encontrado");

        if (group.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            throw new InvalidOperationException("Você já é membro deste grupo");

        _logger.LogInformation("User {UserId} joining group {GroupId}", userId, group.Id);

        var member = new UserGroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return await GetGroupByIdAsync(group.Id, userId) ?? throw new Exception("Erro ao entrar no grupo");
    }

    public async Task LeaveGroupAsync(Guid groupId, Guid userId)
    {
        var membership = await _context.UserGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && !m.IsDeleted);

        if (membership == null)
            throw new KeyNotFoundException("Você não é membro deste grupo");

        if (membership.Role == GroupRole.Owner)
            throw new InvalidOperationException("O proprietário não pode sair do grupo. Transfira a propriedade ou exclua o grupo.");

        _logger.LogInformation("User {UserId} leaving group {GroupId}", userId, groupId);

        membership.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid groupId, Guid memberUserId, Guid requestingUserId)
    {
        var requestingMembership = await _context.UserGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == requestingUserId && !m.IsDeleted);

        if (requestingMembership == null || requestingMembership.Role != GroupRole.Owner)
            throw new UnauthorizedAccessException("Apenas o proprietário pode remover membros");

        var memberToRemove = await _context.UserGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == memberUserId && !m.IsDeleted);

        if (memberToRemove == null)
            throw new KeyNotFoundException("Membro não encontrado");

        if (memberToRemove.Role == GroupRole.Owner)
            throw new InvalidOperationException("Não é possível remover o proprietário do grupo");

        _logger.LogInformation("Removing member {MemberUserId} from group {GroupId}", memberUserId, groupId);

        memberToRemove.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<string> RegenerateInviteCodeAsync(Guid groupId, Guid userId)
    {
        var group = await _context.UserGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new KeyNotFoundException("Grupo não encontrado");

        var membership = group.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null || membership.Role != GroupRole.Owner)
            throw new UnauthorizedAccessException("Apenas o proprietário pode regenerar o código de convite");

        _logger.LogInformation("Regenerating invite code for group {GroupId}", groupId);

        group.InviteCode = GenerateInviteCode();
        await _context.SaveChangesAsync();

        return group.InviteCode;
    }

    public async Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(Guid groupId, Guid userId)
    {
        var isMember = await _context.UserGroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && !m.IsDeleted);

        if (!isMember)
            throw new UnauthorizedAccessException("Você não é membro deste grupo");

        var members = await _context.UserGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId && !m.IsDeleted)
            .ToListAsync();

        return _mapper.Map<IEnumerable<GroupMemberDto>>(members);
    }

    public async Task<IEnumerable<Guid>> GetAccessibleUserIdsAsync(Guid userId, ViewContext context, Guid? memberUserId = null)
    {
        switch (context)
        {
            case ViewContext.Own:
                return new[] { userId };

            case ViewContext.Member:
                if (!memberUserId.HasValue)
                    throw new ArgumentException("MemberUserId é obrigatório para o contexto Member");

                // Verify the user has access to view this member's data
                var hasAccess = await _context.UserGroupMembers
                    .Where(m => m.UserId == userId && !m.IsDeleted)
                    .Select(m => m.GroupId)
                    .Intersect(
                        _context.UserGroupMembers
                            .Where(m => m.UserId == memberUserId.Value && !m.IsDeleted)
                            .Select(m => m.GroupId)
                    )
                    .AnyAsync();

                if (!hasAccess)
                    throw new UnauthorizedAccessException("Você não tem acesso aos dados deste usuário");

                return new[] { memberUserId.Value };

            case ViewContext.All:
                // Get all user IDs from groups the user belongs to
                var groupIds = await _context.UserGroupMembers
                    .Where(m => m.UserId == userId && !m.IsDeleted)
                    .Select(m => m.GroupId)
                    .ToListAsync();

                if (!groupIds.Any())
                    return new[] { userId };

                var userIds = await _context.UserGroupMembers
                    .Where(m => groupIds.Contains(m.GroupId) && !m.IsDeleted)
                    .Select(m => m.UserId)
                    .Distinct()
                    .ToListAsync();

                return userIds;

            default:
                return new[] { userId };
        }
    }

    public async Task ShareGoalAsync(ShareGoalDto dto, Guid userId)
    {
        var goal = await _context.Goals
            .Include(g => g.GoalUsers)
            .FirstOrDefaultAsync(g => g.Id == dto.GoalId && g.UserId == userId && !g.IsDeleted);

        if (goal == null)
            throw new KeyNotFoundException("Meta não encontrada ou você não é o proprietário");

        // Verify all users are in the same group as the owner
        var ownerGroupIds = await _context.UserGroupMembers
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.GroupId)
            .ToListAsync();

        foreach (var targetUserId in dto.UserIds)
        {
            if (targetUserId == userId)
                continue;

            var isInSameGroup = await _context.UserGroupMembers
                .AnyAsync(m => ownerGroupIds.Contains(m.GroupId) && m.UserId == targetUserId && !m.IsDeleted);

            if (!isInSameGroup)
                throw new InvalidOperationException($"Usuário não está no mesmo grupo");

            var existingShare = goal.GoalUsers.FirstOrDefault(gu => gu.UserId == targetUserId);
            if (existingShare == null)
            {
                var goalUser = new GoalUser
                {
                    GoalId = goal.Id,
                    UserId = targetUserId,
                    IsOwner = false,
                    AddedAt = DateTime.UtcNow
                };
                _context.GoalUsers.Add(goalUser);
            }
        }

        // Ensure owner is in GoalUsers
        if (!goal.GoalUsers.Any(gu => gu.UserId == userId))
        {
            var ownerGoalUser = new GoalUser
            {
                GoalId = goal.Id,
                UserId = userId,
                IsOwner = true,
                AddedAt = DateTime.UtcNow
            };
            _context.GoalUsers.Add(ownerGoalUser);
        }

        _logger.LogInformation("Sharing goal {GoalId} with users {UserIds}", dto.GoalId, string.Join(", ", dto.UserIds));
        await _context.SaveChangesAsync();
    }

    public async Task UnshareGoalAsync(UnshareGoalDto dto, Guid userId)
    {
        var goal = await _context.Goals
            .Include(g => g.GoalUsers)
            .FirstOrDefaultAsync(g => g.Id == dto.GoalId && g.UserId == userId && !g.IsDeleted);

        if (goal == null)
            throw new KeyNotFoundException("Meta não encontrada ou você não é o proprietário");

        if (dto.UserId == userId)
            throw new InvalidOperationException("Não é possível remover o proprietário da meta");

        var goalUser = goal.GoalUsers.FirstOrDefault(gu => gu.UserId == dto.UserId);
        if (goalUser != null)
        {
            _context.GoalUsers.Remove(goalUser);
            _logger.LogInformation("Unsharing goal {GoalId} with user {TargetUserId}", dto.GoalId, dto.UserId);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<GoalUserDto>> GetGoalUsersAsync(Guid goalId, Guid userId)
    {
        var goal = await _context.Goals
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);

        if (goal == null)
            throw new KeyNotFoundException("Meta não encontrada");

        // User must be owner or shared with
        var hasAccess = goal.UserId == userId || goal.GoalUsers.Any(gu => gu.UserId == userId);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Você não tem acesso a esta meta");

        return _mapper.Map<IEnumerable<GoalUserDto>>(goal.GoalUsers);
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
