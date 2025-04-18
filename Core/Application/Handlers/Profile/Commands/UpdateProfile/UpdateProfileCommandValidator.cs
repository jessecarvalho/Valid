using Application.Application.Handlers.Profile.Commands.UpdateProfile;
using FluentValidation;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommandRequest>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.OldProfileName)
            .NotEmpty().WithMessage("Nome do perfil é obrigatório");
        RuleFor(x => x.NewProfile)
            .NotEmpty().WithMessage("Novo perfil é obrigatório");
        RuleFor(x => x.NewProfile.ProfileName)
            .NotEmpty().WithMessage("O Nome atualizado do perfil é obrigatório")
            .MaximumLength(50).WithMessage("Nome não pode exceder 50 caracteres");
        RuleFor(x => x.NewProfile.Parameters)
            .NotEmpty().WithMessage("Ao menos um parâmetro para o perfil novo é obrigatório");

    }
}