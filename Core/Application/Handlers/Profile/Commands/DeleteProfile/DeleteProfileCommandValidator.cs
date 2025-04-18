using Application.Application.Handlers.Profile.Commands.DeleteProfile;
using FluentValidation;

public class DeleteProfileCommandValidator : AbstractValidator<DeleteProfileCommandRequest>
{
    public DeleteProfileCommandValidator()
    {
        RuleFor(x => x.ProfileName)
            .NotEmpty().WithMessage("Nome do perfil é obrigatório")
            .MaximumLength(50).WithMessage("Nome não pode exceder 50 caracteres");
    }
}