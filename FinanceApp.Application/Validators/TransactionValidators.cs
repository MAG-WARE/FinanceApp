using FinanceApp.Application.DTOs;
using FinanceApp.Domain.Enums;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Conta é obrigatória");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de transação inválido");

        RuleFor(x => x.DestinationAccountId)
            .NotEqual(x => x.AccountId).WithMessage("Conta de destino deve ser diferente da conta de origem")
            .When(x => x.DestinationAccountId.HasValue);

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty().WithMessage("Conta de destino é obrigatória para transferências")
            .When(x => x.Type == TransactionType.Transfer);
    }
}

public class UpdateTransactionDtoValidator : AbstractValidator<UpdateTransactionDto>
{
    public UpdateTransactionDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
    }
}
