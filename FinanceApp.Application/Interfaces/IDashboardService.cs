using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, ViewContext context, Guid? memberUserId = null);
    Task<DashboardSummaryDto> GetDashboardSummaryByMonthAsync(Guid userId, int month, int year);
    Task<DashboardSummaryDto> GetDashboardSummaryByMonthAsync(Guid userId, int month, int year, ViewContext context, Guid? memberUserId = null);
    Task<DashboardSummaryDto> GetDashboardSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<DashboardSummaryDto> GetDashboardSummaryByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, ViewContext context, Guid? memberUserId = null);
}
