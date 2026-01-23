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
        try
        {
            _logger.LogInformation(
                "Iniciando criação de categoria - UserId: {UserId}, Name: {Name}, Type: {Type}",
                userId, dto.Name, dto.Type);

            var category = _mapper.Map<Category>(dto);
            category.UserId = userId;
            category.Id = Guid.NewGuid();

            _logger.LogInformation("Salvando categoria no banco de dados - CategoryId: {CategoryId}", category.Id);

            try
            {
                var createdCategory = await _categoryRepository.AddAsync(category);

                _logger.LogInformation("Categoria criada com sucesso - CategoryId: {CategoryId}, UserId: {UserId}",
                    createdCategory.Id, userId);

                return _mapper.Map<CategoryDto>(createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao salvar categoria no banco - CategoryId: {CategoryId}, UserId: {UserId}, Name: {Name}. Erro: {ErrorMessage}",
                    category.Id, userId, dto.Name, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao criar categoria - UserId: {UserId}, Name: {Name}",
                userId, dto.Name);
            throw new InvalidOperationException($"Erro ao criar categoria: {ex.Message}", ex);
        }
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto, Guid userId)
    {
        try
        {
            _logger.LogInformation(
                "Iniciando atualização de categoria - CategoryId: {CategoryId}, UserId: {UserId}, NewName: {Name}",
                categoryId, userId, dto.Name);

            var categories = await _categoryRepository.FindAsync(c => c.Id == categoryId && c.UserId == userId);
            var category = categories.FirstOrDefault();

            if (category == null)
            {
                _logger.LogWarning("Categoria não encontrada ou não pertence ao usuário - CategoryId: {CategoryId}, UserId: {UserId}",
                    categoryId, userId);
                throw new KeyNotFoundException($"Categoria com ID {categoryId} não encontrada");
            }

            _logger.LogInformation("Validações concluídas. Atualizando categoria no banco...");

            category.Name = dto.Name;
            category.Color = dto.Color;
            category.Icon = dto.Icon;

            try
            {
                await _categoryRepository.UpdateAsync(category);

                _logger.LogInformation("Categoria atualizada com sucesso - CategoryId: {CategoryId}, UserId: {UserId}",
                    categoryId, userId);

                return _mapper.Map<CategoryDto>(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao atualizar categoria no banco - CategoryId: {CategoryId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    categoryId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao atualizar categoria - CategoryId: {CategoryId}, UserId: {UserId}",
                categoryId, userId);
            throw new InvalidOperationException($"Erro ao atualizar categoria: {ex.Message}", ex);
        }
    }

    public async Task DeleteCategoryAsync(Guid categoryId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Iniciando exclusão de categoria - CategoryId: {CategoryId}, UserId: {UserId}",
                categoryId, userId);

            var categories = await _categoryRepository.FindAsync(c => c.Id == categoryId && c.UserId == userId);
            var category = categories.FirstOrDefault();

            if (category == null)
            {
                _logger.LogWarning("Categoria não encontrada ou não pertence ao usuário - CategoryId: {CategoryId}, UserId: {UserId}",
                    categoryId, userId);
                throw new KeyNotFoundException($"Categoria com ID {categoryId} não encontrada");
            }

            _logger.LogInformation("Validações concluídas. Excluindo categoria do banco...");

            try
            {
                await _categoryRepository.DeleteAsync(categoryId);

                _logger.LogInformation("Categoria excluída com sucesso - CategoryId: {CategoryId}, UserId: {UserId}",
                    categoryId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erro ao excluir categoria no banco - CategoryId: {CategoryId}, UserId: {UserId}. Erro: {ErrorMessage}",
                    categoryId, userId, ex.Message);
                throw;
            }
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex,
                "Erro inesperado ao excluir categoria - CategoryId: {CategoryId}, UserId: {UserId}",
                categoryId, userId);
            throw new InvalidOperationException($"Erro ao excluir categoria: {ex.Message}", ex);
        }
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
