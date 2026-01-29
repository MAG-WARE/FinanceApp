using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtém o resumo financeiro do mês atual (com suporte a ViewContext)
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var summary = await _dashboardService.GetDashboardSummaryAsync(userId, context, memberUserId);
        return Ok(summary);
    }

    /// <summary>
    /// Obtém o resumo financeiro de um mês específico
    /// </summary>
    [HttpGet("summary/{year}/{month}")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummaryByMonth(
        int year,
        int month,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var summary = await _dashboardService.GetDashboardSummaryByMonthAsync(userId, month, year, context, memberUserId);
        return Ok(summary);
    }

    /// <summary>
    /// Obtém o resumo financeiro de um período customizado
    /// </summary>
    [HttpPost("summary/custom")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummaryByDateRange(
        [FromBody] DateRangeDto dateRange,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var summary = await _dashboardService.GetDashboardSummaryByDateRangeAsync(userId, dateRange.StartDate, dateRange.EndDate, context, memberUserId);
        return Ok(summary);
    }
}
