using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Application.Validators;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ILogger<BudgetController> _logger;

    public BudgetController(IBudgetService budgetService, ILogger<BudgetController> logger)
    {
        _budgetService = budgetService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Lista todos os orçamentos do usuário (com suporte a ViewContext)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAll(
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var budgets = await _budgetService.GetAllBudgetsAsync(userId, context, memberUserId);
        return Ok(budgets);
    }

    /// <summary>
    /// Obtém um orçamento específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var budget = await _budgetService.GetBudgetByIdAsync(id, userId);

        if (budget == null)
        {
            return NotFound(new { message = "Orçamento não encontrado" });
        }

        return Ok(budget);
    }

    /// <summary>
    /// Lista orçamentos por mês/ano
    /// </summary>
    [HttpGet("month/{year}/{month}")]
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetByMonth(
        int year,
        int month,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var budgets = await _budgetService.GetBudgetsByMonthAsync(userId, month, year, context, memberUserId);
        return Ok(budgets);
    }

    /// <summary>
    /// Obtém o status dos orçamentos do mês (gastos vs limites)
    /// </summary>
    [HttpGet("status/{year}/{month}")]
    public async Task<ActionResult<IEnumerable<BudgetStatusDto>>> GetStatus(
        int year,
        int month,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var status = await _budgetService.GetBudgetStatusAsync(userId, month, year, context, memberUserId);
        return Ok(status);
    }

    /// <summary>
    /// Cria um novo orçamento
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create([FromBody] CreateBudgetDto dto)
    {
        var validator = new CreateBudgetDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Erro de validação",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var userId = GetUserId();
        var budget = await _budgetService.CreateBudgetAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget);
    }

    /// <summary>
    /// Atualiza um orçamento existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<BudgetDto>> Update(Guid id, [FromBody] UpdateBudgetDto dto)
    {
        var validator = new UpdateBudgetDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Erro de validação",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var userId = GetUserId();
        var budget = await _budgetService.UpdateBudgetAsync(id, dto, userId);

        return Ok(budget);
    }

    /// <summary>
    /// Deleta um orçamento (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _budgetService.DeleteBudgetAsync(id, userId);

        return NoContent();
    }
}
