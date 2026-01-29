using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Interfaces;

public interface IBudgetService
{
    Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync(Guid userId);
    Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync(Guid userId, ViewContext context, Guid? memberUserId = null);
    Task<BudgetDto?> GetBudgetByIdAsync(Guid budgetId, Guid userId);
    Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto dto, Guid userId);
    Task<BudgetDto> UpdateBudgetAsync(Guid budgetId, UpdateBudgetDto dto, Guid userId);
    Task DeleteBudgetAsync(Guid budgetId, Guid userId);
    Task<IEnumerable<BudgetStatusDto>> GetBudgetStatusAsync(Guid userId, int month, int year);
    Task<IEnumerable<BudgetStatusDto>> GetBudgetStatusAsync(Guid userId, int month, int year, ViewContext context, Guid? memberUserId = null);
    Task<IEnumerable<BudgetDto>> GetBudgetsByMonthAsync(Guid userId, int month, int year);
    Task<IEnumerable<BudgetDto>> GetBudgetsByMonthAsync(Guid userId, int month, int year, ViewContext context, Guid? memberUserId = null);
}
