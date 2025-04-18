using Application.Domain.Entities;
using MediatR;

namespace Application.Application.Handlers.Profile.Queries.GetProfileParameters;

public class GetProfileParametersRequest (string profileName) : IRequest<Domain.Entities.Profile>
{
    public string ProfileName { get; set; } = profileName;
}