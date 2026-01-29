using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(Guid userId, int pageNumber = 1, int pageSize = 50);
    Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync(Guid userId, ViewContext context, Guid? memberUserId = null, int pageNumber = 1, int pageSize = 50);
    Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId);
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto, Guid userId);
    Task<TransactionDto> UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto, Guid userId);
    Task DeleteTransactionAsync(Guid transactionId, Guid userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByAccountAsync(Guid accountId, Guid userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByCategoryAsync(Guid categoryId, Guid userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByTypeAsync(Guid userId, TransactionType type);
    Task<IEnumerable<TransactionDto>> GetTransactionsByTypeAsync(Guid userId, TransactionType type, ViewContext context, Guid? memberUserId = null);
    Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, ViewContext context, Guid? memberUserId = null);
}
