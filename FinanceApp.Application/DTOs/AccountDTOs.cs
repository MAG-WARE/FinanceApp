using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal InitialBalance { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}

public class UpdateAccountDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
