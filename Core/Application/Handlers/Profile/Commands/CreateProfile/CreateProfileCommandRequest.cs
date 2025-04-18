using MediatR;

namespace Application.Application.Handlers.Profile.Commands.CreateProfile;

public class CreateProfileCommandRequest(Domain.Entities.Profile profile) : IRequest<Domain.Entities.Profile>
{
    public Domain.Entities.Profile Profile { get; set; } = profile;
}