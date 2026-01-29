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
public class UserGroupController : ControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly ILogger<UserGroupController> _logger;

    public UserGroupController(IUserGroupService userGroupService, ILogger<UserGroupController> logger)
    {
        _userGroupService = userGroupService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Lista todos os grupos do usuário
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserGroupDto>>> GetAll()
    {
        var userId = GetUserId();
        var groups = await _userGroupService.GetUserGroupsAsync(userId);
        return Ok(groups);
    }

    /// <summary>
    /// Obtém um grupo específico por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserGroupDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var group = await _userGroupService.GetGroupByIdAsync(id, userId);

        if (group == null)
        {
            return NotFound(new { message = "Grupo não encontrado" });
        }

        return Ok(group);
    }

    /// <summary>
    /// Cria um novo grupo
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserGroupDto>> Create([FromBody] CreateUserGroupDto dto)
    {
        var validator = new CreateUserGroupDtoValidator();
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
        var group = await _userGroupService.CreateGroupAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    /// <summary>
    /// Atualiza um grupo existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserGroupDto>> Update(Guid id, [FromBody] UpdateUserGroupDto dto)
    {
        var validator = new UpdateUserGroupDtoValidator();
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
        var group = await _userGroupService.UpdateGroupAsync(id, dto, userId);

        return Ok(group);
    }

    /// <summary>
    /// Deleta um grupo (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _userGroupService.DeleteGroupAsync(id, userId);

        return NoContent();
    }

    /// <summary>
    /// Entrar em um grupo usando código de convite
    /// </summary>
    [HttpPost("join")]
    public async Task<ActionResult<UserGroupDto>> Join([FromBody] JoinGroupDto dto)
    {
        var validator = new JoinGroupDtoValidator();
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
        var group = await _userGroupService.JoinGroupAsync(dto, userId);

        return Ok(group);
    }

    /// <summary>
    /// Sair de um grupo
    /// </summary>
    [HttpPost("{id}/leave")]
    public async Task<ActionResult> Leave(Guid id)
    {
        var userId = GetUserId();
        await _userGroupService.LeaveGroupAsync(id, userId);

        return NoContent();
    }

    /// <summary>
    /// Remover um membro do grupo (apenas owner)
    /// </summary>
    [HttpDelete("{groupId}/members/{memberUserId}")]
    public async Task<ActionResult> RemoveMember(Guid groupId, Guid memberUserId)
    {
        var userId = GetUserId();
        await _userGroupService.RemoveMemberAsync(groupId, memberUserId, userId);

        return NoContent();
    }

    /// <summary>
    /// Regenerar código de convite do grupo (apenas owner)
    /// </summary>
    [HttpPost("{id}/regenerate-invite")]
    public async Task<ActionResult<object>> RegenerateInviteCode(Guid id)
    {
        var userId = GetUserId();
        var newCode = await _userGroupService.RegenerateInviteCodeAsync(id, userId);

        return Ok(new { inviteCode = newCode });
    }

    /// <summary>
    /// Lista membros de um grupo
    /// </summary>
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetMembers(Guid id)
    {
        var userId = GetUserId();
        var members = await _userGroupService.GetGroupMembersAsync(id, userId);

        return Ok(members);
    }

    /// <summary>
    /// Compartilhar uma meta com membros do grupo
    /// </summary>
    [HttpPost("share-goal")]
    public async Task<ActionResult> ShareGoal([FromBody] ShareGoalDto dto)
    {
        var validator = new ShareGoalDtoValidator();
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
        await _userGroupService.ShareGoalAsync(dto, userId);

        return Ok(new { message = "Meta compartilhada com sucesso" });
    }

    /// <summary>
    /// Remover compartilhamento de uma meta
    /// </summary>
    [HttpPost("unshare-goal")]
    public async Task<ActionResult> UnshareGoal([FromBody] UnshareGoalDto dto)
    {
        var validator = new UnshareGoalDtoValidator();
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
        await _userGroupService.UnshareGoalAsync(dto, userId);

        return Ok(new { message = "Compartilhamento removido com sucesso" });
    }

    /// <summary>
    /// Lista usuários que têm acesso a uma meta
    /// </summary>
    [HttpGet("goal/{goalId}/users")]
    public async Task<ActionResult<IEnumerable<GoalUserDto>>> GetGoalUsers(Guid goalId)
    {
        var userId = GetUserId();
        var goalUsers = await _userGroupService.GetGoalUsersAsync(goalId, userId);

        return Ok(goalUsers);
    }
}
