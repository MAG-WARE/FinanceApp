using FinanceApp.Application.DTOs;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateUserGroupDtoValidator : AbstractValidator<CreateUserGroupDto>
{
    public CreateUserGroupDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do grupo é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
    }
}

public class UpdateUserGroupDtoValidator : AbstractValidator<UpdateUserGroupDto>
{
    public UpdateUserGroupDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do grupo é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
    }
}

public class JoinGroupDtoValidator : AbstractValidator<JoinGroupDto>
{
    public JoinGroupDtoValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty().WithMessage("Código de convite é obrigatório")
            .MaximumLength(20).WithMessage("Código de convite inválido");
    }
}

public class ShareGoalDtoValidator : AbstractValidator<ShareGoalDto>
{
    public ShareGoalDtoValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("ID da meta é obrigatório");

        RuleFor(x => x.UserIds)
            .NotEmpty().WithMessage("É necessário informar ao menos um usuário para compartilhar");
    }
}

public class UnshareGoalDtoValidator : AbstractValidator<UnshareGoalDto>
{
    public UnshareGoalDtoValidator()
    {
        RuleFor(x => x.GoalId)
            .NotEmpty().WithMessage("ID da meta é obrigatório");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID do usuário é obrigatório");
    }
}
