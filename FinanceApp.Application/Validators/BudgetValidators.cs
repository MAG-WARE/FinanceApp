using FinanceApp.Application.DTOs;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateBudgetDtoValidator : AbstractValidator<CreateBudgetDto>
{
    public CreateBudgetDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12");

        RuleFor(x => x.Year)
            .GreaterThanOrEqualTo(2000).WithMessage("Ano deve ser maior ou igual a 2000")
            .LessThanOrEqualTo(2100).WithMessage("Ano deve ser menor ou igual a 2100");

        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Valor limite deve ser maior que zero");
    }
}

public class UpdateBudgetDtoValidator : AbstractValidator<UpdateBudgetDto>
{
    public UpdateBudgetDtoValidator()
    {
        RuleFor(x => x.LimitAmount)
            .GreaterThan(0).WithMessage("Valor limite deve ser maior que zero");
    }
}
