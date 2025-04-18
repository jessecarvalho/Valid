using Application.Application.Handlers.Profile.Commands.CreateProfile;
using FluentValidation;

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommandRequest>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.Profile)
            .NotEmpty()
            .NotNull()
            .WithMessage("O perfil é obrigatório");
        
        RuleFor(x => x.Profile.ProfileName)
            .NotEmpty().WithMessage("Nome do perfil é obrigatório")
            .MaximumLength(50).WithMessage("Nome não pode exceder 50 caracteres");
        
        RuleFor(x => x.Profile.Parameters)
            .NotNull().WithMessage("Parâmetros não podem ser nulos")
            .Must(p => p.Any()).WithMessage("Deve conter pelo menos um parâmetro");
    }
}