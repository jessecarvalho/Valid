using MediatR;

namespace Application.Application.Handlers.Profile.Commands.DeleteProfile;

public class DeleteProfileCommandRequest (string profileName) : IRequest<bool>
{
    public string ProfileName { get; set; } = profileName;
}