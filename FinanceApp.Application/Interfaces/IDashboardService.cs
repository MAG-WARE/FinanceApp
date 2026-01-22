using FinanceApp.Application.DTOs;

namespace FinanceApp.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
    Task<DashboardSummaryDto> GetDashboardSummaryByMonthAsync(Guid userId, int month, int year);
    Task<DashboardSummaryDto> GetDashboardSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
}
