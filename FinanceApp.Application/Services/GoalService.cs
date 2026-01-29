using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class GoalService : IGoalService
{
    private readonly FinanceAppDbContext _context;
    private readonly IUserGroupService _userGroupService;
    private readonly IMapper _mapper;
    private readonly ILogger<GoalService> _logger;

    public GoalService(
        FinanceAppDbContext context,
        IUserGroupService userGroupService,
        IMapper mapper,
        ILogger<GoalService> logger)
    {
        _context = context;
        _userGroupService = userGroupService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<GoalDto>> GetAllGoalsAsync(Guid userId)
    {
        return await GetAllGoalsAsync(userId, ViewContext.Own, null);
    }

    public async Task<IEnumerable<GoalDto>> GetAllGoalsAsync(Guid userId, ViewContext context, Guid? memberUserId = null)
    {
        var accessibleUserIds = await _userGroupService.GetAccessibleUserIdsAsync(userId, context, memberUserId);

        var goals = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => accessibleUserIds.Contains(g.UserId) || g.GoalUsers.Any(gu => accessibleUserIds.Contains(gu.UserId)))
            .Where(g => !g.IsDeleted)
            .ToListAsync();

        return MapGoalsWithProgress(goals, userId);
    }

    public async Task<GoalDto?> GetGoalByIdAsync(Guid goalId, Guid userId)
    {
        var goal = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);

        if (goal == null)
            return null;

        // Check access: user is owner or goal is shared with user
        var hasAccess = goal.UserId == userId || goal.GoalUsers.Any(gu => gu.UserId == userId);
        if (!hasAccess)
            return null;

        return MapGoalsWithProgress(new[] { goal }, userId).FirstOrDefault();
    }

    public async Task<GoalDto> CreateGoalAsync(CreateGoalDto dto, Guid userId)
    {
        _logger.LogInformation("Creating goal {GoalName} for user {UserId}", dto.Name, userId);

        var goal = _mapper.Map<Goal>(dto);
        goal.UserId = userId;
        goal.Id = Guid.NewGuid();
        goal.IsCompleted = false;

        if (goal.StartDate.Kind != DateTimeKind.Utc)
            goal.StartDate = DateTime.SpecifyKind(goal.StartDate, DateTimeKind.Utc);

        if (goal.TargetDate.HasValue && goal.TargetDate.Value.Kind != DateTimeKind.Utc)
            goal.TargetDate = DateTime.SpecifyKind(goal.TargetDate.Value, DateTimeKind.Utc);

        if (goal.CurrentAmount >= goal.TargetAmount)
            goal.IsCompleted = true;

        _context.Goals.Add(goal);

        // Add owner to GoalUsers table
        var goalUser = new GoalUser
        {
            GoalId = goal.Id,
            UserId = userId,
            IsOwner = true,
            AddedAt = DateTime.UtcNow
        };
        _context.GoalUsers.Add(goalUser);

        await _context.SaveChangesAsync();

        // Reload with includes
        var createdGoal = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .FirstAsync(g => g.Id == goal.Id);

        return MapGoalsWithProgress(new[] { createdGoal }, userId).First();
    }

    public async Task<GoalDto> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, Guid userId)
    {
        var goal = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);

        if (goal == null)
            throw new KeyNotFoundException("Meta não encontrada");

        // Only owner can update
        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("Apenas o proprietário pode editar a meta");

        _logger.LogInformation("Updating goal {GoalId} for user {UserId}", goalId, userId);

        goal.Name = dto.Name;
        goal.Description = dto.Description;
        goal.TargetAmount = dto.TargetAmount;
        goal.CurrentAmount = dto.CurrentAmount;
        goal.TargetDate = dto.TargetDate;
        goal.Color = dto.Color;
        goal.Icon = dto.Icon;

        if (goal.TargetDate.HasValue && goal.TargetDate.Value.Kind != DateTimeKind.Utc)
            goal.TargetDate = DateTime.SpecifyKind(goal.TargetDate.Value, DateTimeKind.Utc);

        if (goal.CurrentAmount >= goal.TargetAmount && !goal.IsCompleted)
        {
            goal.IsCompleted = true;
            _logger.LogInformation("Goal {GoalId} completed for user {UserId}", goalId, userId);
        }
        else if (goal.CurrentAmount < goal.TargetAmount && goal.IsCompleted)
            goal.IsCompleted = false;

        await _context.SaveChangesAsync();

        return MapGoalsWithProgress(new[] { goal }, userId).First();
    }

    public async Task DeleteGoalAsync(Guid goalId, Guid userId)
    {
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);

        if (goal == null)
            throw new KeyNotFoundException("Meta não encontrada");

        // Only owner can delete
        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("Apenas o proprietário pode excluir a meta");

        _logger.LogInformation("Deleting goal {GoalId} for user {UserId}", goalId, userId);

        goal.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GoalDto>> GetActiveGoalsAsync(Guid userId)
    {
        return await GetActiveGoalsAsync(userId, ViewContext.Own, null);
    }

    public async Task<IEnumerable<GoalDto>> GetActiveGoalsAsync(Guid userId, ViewContext context, Guid? memberUserId = null)
    {
        var accessibleUserIds = await _userGroupService.GetAccessibleUserIdsAsync(userId, context, memberUserId);

        var goals = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => (accessibleUserIds.Contains(g.UserId) || g.GoalUsers.Any(gu => accessibleUserIds.Contains(gu.UserId)))
                        && !g.IsCompleted && !g.IsDeleted)
            .ToListAsync();

        return MapGoalsWithProgress(goals, userId);
    }

    public async Task<IEnumerable<GoalDto>> GetCompletedGoalsAsync(Guid userId)
    {
        return await GetCompletedGoalsAsync(userId, ViewContext.Own, null);
    }

    public async Task<IEnumerable<GoalDto>> GetCompletedGoalsAsync(Guid userId, ViewContext context, Guid? memberUserId = null)
    {
        var accessibleUserIds = await _userGroupService.GetAccessibleUserIdsAsync(userId, context, memberUserId);

        var goals = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => (accessibleUserIds.Contains(g.UserId) || g.GoalUsers.Any(gu => accessibleUserIds.Contains(gu.UserId)))
                        && g.IsCompleted && !g.IsDeleted)
            .ToListAsync();

        return MapGoalsWithProgress(goals, userId);
    }

    public async Task<IEnumerable<GoalDto>> GetSharedGoalsAsync(Guid userId)
    {
        var goals = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => g.GoalUsers.Any(gu => gu.UserId == userId && !gu.IsOwner) && !g.IsDeleted)
            .ToListAsync();

        return MapGoalsWithProgress(goals, userId);
    }

    private IEnumerable<GoalDto> MapGoalsWithProgress(IEnumerable<Goal> goals, Guid currentUserId)
    {
        var goalDtos = new List<GoalDto>();

        foreach (var goal in goals)
        {
            var dto = _mapper.Map<GoalDto>(goal);
            dto.ProgressPercentage = goal.TargetAmount > 0
                ? Math.Min((goal.CurrentAmount / goal.TargetAmount) * 100, 100)
                : 0;
            dto.RemainingAmount = Math.Max(goal.TargetAmount - goal.CurrentAmount, 0);
            dto.IsOwner = goal.UserId == currentUserId;
            dto.SharedWith = goal.GoalUsers
                .Select(gu => new GoalUserDto
                {
                    UserId = gu.UserId,
                    UserName = gu.User?.Name ?? "",
                    IsOwner = gu.IsOwner,
                    AddedAt = gu.AddedAt
                })
                .ToList();

            goalDtos.Add(dto);
        }

        return goalDtos;
    }
}
