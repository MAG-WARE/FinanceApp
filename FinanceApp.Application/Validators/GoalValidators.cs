using FinanceApp.Application.DTOs;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateGoalDtoValidator : AbstractValidator<CreateGoalDto>
{
    public CreateGoalDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da meta é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Valor alvo deve ser maior que zero");

        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Valor atual não pode ser negativo");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de início é obrigatória");

        RuleFor(x => x.TargetDate)
            .GreaterThan(x => x.StartDate).WithMessage("Data alvo deve ser posterior à data de início")
            .When(x => x.TargetDate.HasValue);
    }
}

public class UpdateGoalDtoValidator : AbstractValidator<UpdateGoalDto>
{
    public UpdateGoalDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da meta é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Valor alvo deve ser maior que zero");

        RuleFor(x => x.CurrentAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Valor atual não pode ser negativo");
    }
}
