using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid userId);
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid userId, ViewContext context, Guid? memberUserId = null);
    Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId, Guid userId);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId);
    Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto, Guid userId);
    Task DeleteCategoryAsync(Guid categoryId, Guid userId);
    Task<IEnumerable<CategoryDto>> GetCategoriesByTypeAsync(Guid userId, CategoryType type);
    Task<IEnumerable<CategoryDto>> GetCategoriesByTypeAsync(Guid userId, CategoryType type, ViewContext context, Guid? memberUserId = null);
    Task CreateDefaultCategoriesAsync(Guid userId);
}
