using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class GoalService : IGoalService
{
    private readonly IRepository<Goal> _goalRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GoalService> _logger;

    public GoalService(
        IRepository<Goal> goalRepository,
        IMapper mapper,
        ILogger<GoalService> logger)
    {
        _goalRepository = goalRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<GoalDto>> GetAllGoalsAsync(Guid userId)
    {
        var goals = await _goalRepository.FindAsync(g => g.UserId == userId);
        return MapGoalsWithProgress(goals);
    }

    public async Task<GoalDto?> GetGoalByIdAsync(Guid goalId, Guid userId)
    {
        var goals = await _goalRepository.FindAsync(g => g.Id == goalId && g.UserId == userId);
        var goal = goals.FirstOrDefault();

        if (goal == null)
            return null;

        return MapGoalsWithProgress(new[] { goal }).FirstOrDefault();
    }

    public async Task<GoalDto> CreateGoalAsync(CreateGoalDto dto, Guid userId)
    {
        _logger.LogInformation("Creating goal {GoalName} for user {UserId}", dto.Name, userId);

        var goal = _mapper.Map<Goal>(dto);
        goal.UserId = userId;
        goal.Id = Guid.NewGuid();
        goal.IsCompleted = false;

        // Verificar se já atingiu a meta ao criar
        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.IsCompleted = true;
        }

        var createdGoal = await _goalRepository.AddAsync(goal);

        return MapGoalsWithProgress(new[] { createdGoal }).First();
    }

    public async Task<GoalDto> UpdateGoalAsync(Guid goalId, UpdateGoalDto dto, Guid userId)
    {
        var goals = await _goalRepository.FindAsync(g => g.Id == goalId && g.UserId == userId);
        var goal = goals.FirstOrDefault();

        if (goal == null)
        {
            throw new KeyNotFoundException("Meta não encontrada");
        }

        _logger.LogInformation("Updating goal {GoalId} for user {UserId}", goalId, userId);

        goal.Name = dto.Name;
        goal.Description = dto.Description;
        goal.TargetAmount = dto.TargetAmount;
        goal.CurrentAmount = dto.CurrentAmount;
        goal.TargetDate = dto.TargetDate;
        goal.Color = dto.Color;
        goal.Icon = dto.Icon;

        // Verificar se atingiu a meta
        if (goal.CurrentAmount >= goal.TargetAmount && !goal.IsCompleted)
        {
            goal.IsCompleted = true;
            _logger.LogInformation("Goal {GoalId} completed for user {UserId}", goalId, userId);
        }
        else if (goal.CurrentAmount < goal.TargetAmount && goal.IsCompleted)
        {
            goal.IsCompleted = false;
        }

        await _goalRepository.UpdateAsync(goal);

        return MapGoalsWithProgress(new[] { goal }).First();
    }

    public async Task DeleteGoalAsync(Guid goalId, Guid userId)
    {
        var goals = await _goalRepository.FindAsync(g => g.Id == goalId && g.UserId == userId);
        var goal = goals.FirstOrDefault();

        if (goal == null)
        {
            throw new KeyNotFoundException("Meta não encontrada");
        }

        _logger.LogInformation("Deleting goal {GoalId} for user {UserId}", goalId, userId);

        await _goalRepository.DeleteAsync(goalId);
    }

    public async Task<IEnumerable<GoalDto>> GetActiveGoalsAsync(Guid userId)
    {
        var goals = await _goalRepository.FindAsync(g => g.UserId == userId && !g.IsCompleted);
        return MapGoalsWithProgress(goals);
    }

    public async Task<IEnumerable<GoalDto>> GetCompletedGoalsAsync(Guid userId)
    {
        var goals = await _goalRepository.FindAsync(g => g.UserId == userId && g.IsCompleted);
        return MapGoalsWithProgress(goals);
    }

    private IEnumerable<GoalDto> MapGoalsWithProgress(IEnumerable<Goal> goals)
    {
        var goalDtos = new List<GoalDto>();

        foreach (var goal in goals)
        {
            var dto = _mapper.Map<GoalDto>(goal);
            dto.ProgressPercentage = goal.TargetAmount > 0
                ? Math.Min((goal.CurrentAmount / goal.TargetAmount) * 100, 100)
                : 0;
            dto.RemainingAmount = Math.Max(goal.TargetAmount - goal.CurrentAmount, 0);

            goalDtos.Add(dto);
        }

        return goalDtos;
    }
}
