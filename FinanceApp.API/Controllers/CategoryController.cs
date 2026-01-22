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
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Lista todas as categorias do usuário
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var userId = GetUserId();
        var categories = await _categoryService.GetAllCategoriesAsync(userId);
        return Ok(categories);
    }

    /// <summary>
    /// Lista categorias por tipo (Income ou Expense)
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetByType(CategoryType type)
    {
        var userId = GetUserId();
        var categories = await _categoryService.GetCategoriesByTypeAsync(userId, type);
        return Ok(categories);
    }

    /// <summary>
    /// Obtém uma categoria específica por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var category = await _categoryService.GetCategoryByIdAsync(id, userId);

        if (category == null)
        {
            return NotFound(new { message = "Categoria não encontrada" });
        }

        return Ok(category);
    }

    /// <summary>
    /// Cria uma nova categoria
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var validator = new CreateCategoryDtoValidator();
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
        var category = await _categoryService.CreateCategoryAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Atualiza uma categoria existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var validator = new UpdateCategoryDtoValidator();
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
        var category = await _categoryService.UpdateCategoryAsync(id, dto, userId);

        return Ok(category);
    }

    /// <summary>
    /// Deleta uma categoria (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        await _categoryService.DeleteCategoryAsync(id, userId);

        return NoContent();
    }
}
