using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.Repositories;

public class GoalRepository : IGoalRepository
{
    private readonly FinanceAppDbContext _context;

    public GoalRepository(FinanceAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Goal>> GetGoalsWithUsersAsync(IEnumerable<Guid> userIds, bool? isCompleted = null)
    {
        var query = _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => !g.IsDeleted && g.GoalUsers.Any(gu => userIds.Contains(gu.UserId)));

        if (isCompleted.HasValue)
        {
            query = query.Where(g => g.IsCompleted == isCompleted.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Goal>> GetGoalsByAccessibleUsersAsync(IEnumerable<Guid> accessibleUserIds, bool? isCompleted = null)
    {
        var query = _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => !g.IsDeleted &&
                        (accessibleUserIds.Contains(g.UserId) || g.GoalUsers.Any(gu => accessibleUserIds.Contains(gu.UserId))));

        if (isCompleted.HasValue)
        {
            query = query.Where(g => g.IsCompleted == isCompleted.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Goal>> GetSharedGoalsAsync(Guid userId)
    {
        return await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => !g.IsDeleted && g.GoalUsers.Any(gu => gu.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Goal>> GetNonOwnerSharedGoalsAsync(Guid userId)
    {
        return await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .Where(g => !g.IsDeleted && g.GoalUsers.Any(gu => gu.UserId == userId && !gu.IsOwner))
            .ToListAsync();
    }

    public async Task<Goal?> GetGoalByIdWithUsersAsync(Guid goalId)
    {
        return await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);
    }

    public async Task<Goal?> GetGoalByIdAsync(Guid goalId)
    {
        return await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && !g.IsDeleted);
    }

    public async Task<Goal> AddAsync(Goal goal)
    {
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    public async Task<Goal> AddWithGoalUserAsync(Goal goal, GoalUser goalUser)
    {
        _context.Goals.Add(goal);
        _context.GoalUsers.Add(goalUser);
        await _context.SaveChangesAsync();

        var createdGoal = await _context.Goals
            .Include(g => g.User)
            .Include(g => g.GoalUsers)
                .ThenInclude(gu => gu.User)
            .FirstAsync(g => g.Id == goal.Id);

        return createdGoal;
    }

    public async Task UpdateAsync(Goal goal)
    {
        _context.Goals.Update(goal);
        await _context.SaveChangesAsync();
    }

    public async Task<GoalUser> AddGoalUserAsync(GoalUser goalUser)
    {
        _context.GoalUsers.Add(goalUser);
        await _context.SaveChangesAsync();
        return goalUser;
    }

    public async Task RemoveGoalUserAsync(GoalUser goalUser)
    {
        _context.GoalUsers.Remove(goalUser);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<GoalUser>> GetGoalUsersAsync(Guid goalId)
    {
        return await _context.GoalUsers
            .Include(gu => gu.User)
            .Where(gu => gu.GoalId == goalId)
            .ToListAsync();
    }

    public async Task<bool> IsGoalSharedWithUserAsync(Guid goalId, Guid userId)
    {
        return await _context.GoalUsers
            .AnyAsync(gu => gu.GoalId == goalId && gu.UserId == userId);
    }
}
