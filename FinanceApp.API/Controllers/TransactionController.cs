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
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Lista todas as transações do usuário com paginação (com suporte a ViewContext)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var transactions = await _transactionService.GetAllTransactionsAsync(userId, context, memberUserId, pageNumber, pageSize);
        return Ok(transactions);
    }

    /// <summary>
    /// Obtém uma transação específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var transaction = await _transactionService.GetTransactionByIdAsync(id, userId);

        if (transaction == null)
        {
            return NotFound(new { message = "Transação não encontrada" });
        }

        return Ok(transaction);
    }

    /// <summary>
    /// Lista transações por conta
    /// </summary>
    [HttpGet("account/{accountId}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByAccount(Guid accountId)
    {
        var userId = GetUserId();
        var transactions = await _transactionService.GetTransactionsByAccountAsync(accountId, userId);
        return Ok(transactions);
    }

    /// <summary>
    /// Lista transações por categoria
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByCategory(Guid categoryId)
    {
        var userId = GetUserId();
        var transactions = await _transactionService.GetTransactionsByCategoryAsync(categoryId, userId);
        return Ok(transactions);
    }

    /// <summary>
    /// Lista transações por tipo (Income, Expense, Transfer)
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByType(
        TransactionType type,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var transactions = await _transactionService.GetTransactionsByTypeAsync(userId, type, context, memberUserId);
        return Ok(transactions);
    }

    /// <summary>
    /// Lista transações por período
    /// </summary>
    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] ViewContext context = ViewContext.Own,
        [FromQuery] Guid? memberUserId = null)
    {
        var userId = GetUserId();
        var transactions = await _transactionService.GetTransactionsByDateRangeAsync(userId, startDate, endDate, context, memberUserId);
        return Ok(transactions);
    }

    /// <summary>
    /// Cria uma nova transação
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create([FromBody] CreateTransactionDto dto)
    {
        var validator = new CreateTransactionDtoValidator();
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
        var transaction = await _transactionService.CreateTransactionAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    /// <summary>
    /// Atualiza uma transação existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TransactionDto>> Update(Guid id, [FromBody] UpdateTransactionDto dto)
    {
        var validator = new UpdateTransactionDtoValidator();
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
        var transaction = await _transactionService.UpdateTransactionAsync(id, dto, userId);

        return Ok(transaction);
    }

    /// <summary>
    /// Deleta uma transação (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _transactionService.DeleteTransactionAsync(id, userId);

        return NoContent();
    }
}
