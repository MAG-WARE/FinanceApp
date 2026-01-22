using FinanceApp.Application.DTOs;

namespace FinanceApp.Application.Interfaces;

public interface IGoalService
{
    Task<IEnumerable<GoalDto>> GetAllGoalsAsync(Guid userId);
    Task<GoalDto?> GetGoalByIdAsync(Guid goalId, Guid userId);
    Task<GoalDto> CreateGoalAsync(CreateGoalDto dto, Guid userId);
    Task<GoalDto> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, Guid userId);
    Task DeleteGoalAsync(Guid goalId, Guid userId);
    Task<IEnumerable<GoalDto>> GetActiveGoalsAsync(Guid userId);
    Task<IEnumerable<GoalDto>> GetCompletedGoalsAsync(Guid userId);
}
