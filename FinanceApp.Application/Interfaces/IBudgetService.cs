using FinanceApp.Application.DTOs;

namespace FinanceApp.Application.Interfaces;

public interface IBudgetService
{
    Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync(Guid userId);
    Task<BudgetDto?> GetBudgetByIdAsync(Guid budgetId, Guid userId);
    Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto dto, Guid userId);
    Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetDto dto, Guid userId);
    Task DeleteBudgetAsync(Guid budgetId, Guid userId);
    Task<IEnumerable<BudgetStatusDto>> GetBudgetStatusAsync(Guid userId, int month, int year);
    Task<IEnumerable<BudgetDto>> GetBudgetsByMonthAsync(Guid userId, int month, int year);
}
