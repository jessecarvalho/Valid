using Application.Domain.Entities;
using MediatR;

namespace Application.Application.Handlers.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandRequest(string oldProfileName, Domain.Entities.Profile newProfile) : IRequest<Domain.Entities.Profile>
{
    public string OldProfileName { get; set; } = oldProfileName;
    public Domain.Entities.Profile NewProfile { get; set; } = newProfile;
}