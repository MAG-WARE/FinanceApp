using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Lista todas as contas do usuário
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetAll()
    {
        var userId = GetUserId();
        var accounts = await _accountService.GetAllAccountsAsync(userId);
        return Ok(accounts);
    }

    /// <summary>
    /// Lista apenas contas ativas do usuário
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<AccountDto>>> GetActive()
    {
        var userId = GetUserId();
        var accounts = await _accountService.GetActiveAccountsAsync(userId);
        return Ok(accounts);
    }

    /// <summary>
    /// Obtém uma conta específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var account = await _accountService.GetAccountByIdAsync(id, userId);

        if (account == null)
            return NotFound(new { message = "Conta não encontrada" });

        return Ok(account);
    }

    /// <summary>
    /// Cria uma nova conta
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountDto dto)
    {
        var validator = new CreateAccountDtoValidator();
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
        var account = await _accountService.CreateAccountAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    /// <summary>
    /// Atualiza uma conta existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        var validator = new UpdateAccountDtoValidator();
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
        var account = await _accountService.UpdateAccountAsync(id, dto, userId);

        return Ok(account);
    }

    /// <summary>
    /// Deleta uma conta (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _accountService.DeleteAccountAsync(id, userId);

        return NoContent();
    }
}
