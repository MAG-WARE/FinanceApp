using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Interfaces;

public interface IGoalRepository
{
    Task<IEnumerable<Goal>> GetGoalsWithUsersAsync(IEnumerable<Guid> userIds, bool? isCompleted = null);
    Task<IEnumerable<Goal>> GetGoalsByAccessibleUsersAsync(IEnumerable<Guid> accessibleUserIds, bool? isCompleted = null);
    Task<IEnumerable<Goal>> GetSharedGoalsAsync(Guid userId);
    Task<IEnumerable<Goal>> GetNonOwnerSharedGoalsAsync(Guid userId);
    Task<Goal?> GetGoalByIdWithUsersAsync(Guid goalId);
    Task<Goal?> GetGoalByIdAsync(Guid goalId);
    Task<Goal> AddAsync(Goal goal);
    Task<Goal> AddWithGoalUserAsync(Goal goal, GoalUser goalUser);
    Task UpdateAsync(Goal goal);
    Task<GoalUser> AddGoalUserAsync(GoalUser goalUser);
    Task RemoveGoalUserAsync(GoalUser goalUser);
    Task<IEnumerable<GoalUser>> GetGoalUsersAsync(Guid goalId);
    Task<bool> IsGoalSharedWithUserAsync(Guid goalId, Guid userId);
}
