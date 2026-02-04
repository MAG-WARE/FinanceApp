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
public class GoalController : ControllerBase
{
    private readonly IGoalService _goalService;
    private readonly ILogger<GoalController> _logger;

    public GoalController(IGoalService goalService, ILogger<GoalController> logger)
    {
        _goalService = goalService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Lista todas as metas do usuário (com suporte a ViewContext)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GoalDto>>> GetAll(
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var goals = await _goalService.GetAllGoalsAsync(userId, context, memberUserId);
        return Ok(goals);
    }

    /// <summary>
    /// Lista apenas metas ativas (não completadas)
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<GoalDto>>> GetActive(
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var goals = await _goalService.GetActiveGoalsAsync(userId, context, memberUserId);
        return Ok(goals);
    }

    /// <summary>
    /// Lista apenas metas completadas
    /// </summary>
    [HttpGet("completed")]
    public async Task<ActionResult<IEnumerable<GoalDto>>> GetCompleted(
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var goals = await _goalService.GetCompletedGoalsAsync(userId, context, memberUserId);
        return Ok(goals);
    }

    /// <summary>
    /// Lista metas compartilhadas com o usuário (onde não é owner)
    /// </summary>
    [HttpGet("shared")]
    public async Task<ActionResult<IEnumerable<GoalDto>>> GetShared()
    {
        var userId = GetUserId();
        var goals = await _goalService.GetSharedGoalsAsync(userId);
        return Ok(goals);
    }

    /// <summary>
    /// Lista metas disponíveis para transação (próprias e compartilhadas)
    /// </summary>
    [HttpGet("for-transaction")]
    public async Task<ActionResult<IEnumerable<GoalForTransactionDto>>> GetForTransaction()
    {
        var userId = GetUserId();
        var goals = await _goalService.GetGoalsForTransactionAsync(userId);
        return Ok(goals);
    }

    /// <summary>
    /// Obtém uma meta específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<GoalDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var goal = await _goalService.GetGoalByIdAsync(id, userId);

        if (goal == null)
        {
            return NotFound(new { message = "Meta não encontrada" });
        }

        return Ok(goal);
    }

    /// <summary>
    /// Cria uma nova meta
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GoalDto>> Create([FromBody] CreateGoalDto dto)
    {
        var validator = new CreateGoalDtoValidator();
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
        var goal = await _goalService.CreateGoalAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
    }

    /// <summary>
    /// Atualiza uma meta existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<GoalDto>> Update(Guid id, [FromBody] UpdateGoalDto dto)
    {
        var validator = new UpdateGoalDtoValidator();
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
        var goal = await _goalService.UpdateGoalAsync(id, dto, userId);

        return Ok(goal);
    }

    /// <summary>
    /// Deleta uma meta (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _goalService.DeleteGoalAsync(id, userId);

        return NoContent();
    }
}
