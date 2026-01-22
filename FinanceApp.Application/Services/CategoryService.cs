using AutoMapper;
using FinanceApp.Application.DTOs;
using FinanceApp.Application.Interfaces;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IRepository<Category> categoryRepository,
        IMapper mapper,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(Guid userId)
    {
        var categories = await _categoryRepository.FindAsync(c => c.UserId == userId);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId, Guid userId)
    {
        var categories = await _categoryRepository.FindAsync(c => c.Id == categoryId && c.UserId == userId);
        var category = categories.FirstOrDefault();
        return category != null ? _mapper.Map<CategoryDto>(category) : null;
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, Guid userId)
    {
        _logger.LogInformation("Creating category {CategoryName} for user {UserId}", dto.Name, userId);

        var category = _mapper.Map<Category>(dto);
        category.UserId = userId;
        category.Id = Guid.NewGuid();

        var createdCategory = await _categoryRepository.AddAsync(category);
        return _mapper.Map<CategoryDto>(createdCategory);
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto, Guid userId)
    {
        var categories = await _categoryRepository.FindAsync(c => c.Id == categoryId && c.UserId == userId);
        var category = categories.FirstOrDefault();

        if (category == null)
        {
            throw new KeyNotFoundException("Categoria não encontrada");
        }

        _logger.LogInformation("Updating category {CategoryId} for user {UserId}", categoryId, userId);

        category.Name = dto.Name;
        category.Color = dto.Color;
        category.Icon = dto.Icon;

        await _categoryRepository.UpdateAsync(category);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteCategoryAsync(Guid categoryId, Guid userId)
    {
        var categories = await _categoryRepository.FindAsync(c => c.Id == categoryId && c.UserId == userId);
        var category = categories.FirstOrDefault();

        if (category == null)
        {
            throw new KeyNotFoundException("Categoria não encontrada");
        }

        _logger.LogInformation("Deleting category {CategoryId} for user {UserId}", categoryId, userId);

        await _categoryRepository.DeleteAsync(categoryId);
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesByTypeAsync(Guid userId, CategoryType type)
    {
        var categories = await _categoryRepository.FindAsync(c => c.UserId == userId && c.Type == type);
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task CreateDefaultCategoriesAsync(Guid userId)
    {
        _logger.LogInformation("Creating default categories for user {UserId}", userId);

        var defaultCategories = new List<Category>
        {
            // Categorias de Despesa
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Alimentação", Type = CategoryType.Expense, Icon = "🍔", Color = "#FF6B6B" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Transporte", Type = CategoryType.Expense, Icon = "🚗", Color = "#4ECDC4" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Moradia", Type = CategoryType.Expense, Icon = "🏠", Color = "#45B7D1" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Saúde", Type = CategoryType.Expense, Icon = "💊", Color = "#96CEB4" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Educação", Type = CategoryType.Expense, Icon = "📚", Color = "#FFEAA7" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Lazer", Type = CategoryType.Expense, Icon = "🎮", Color = "#DFE6E9" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Compras", Type = CategoryType.Expense, Icon = "🛒", Color = "#FAB1A0" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Contas", Type = CategoryType.Expense, Icon = "📄", Color = "#74B9FF" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Outros", Type = CategoryType.Expense, Icon = "📦", Color = "#A29BFE" },

            // Categorias de Receita
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Salário", Type = CategoryType.Income, Icon = "💰", Color = "#00B894" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Freelance", Type = CategoryType.Income, Icon = "💼", Color = "#6C5CE7" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Investimentos", Type = CategoryType.Income, Icon = "📈", Color = "#FDCB6E" },
            new Category { Id = Guid.NewGuid(), UserId = userId, Name = "Outras Receitas", Type = CategoryType.Income, Icon = "💵", Color = "#55EFC4" }
        };

        foreach (var category in defaultCategories)
        {
            await _categoryRepository.AddAsync(category);
        }
    }
}
