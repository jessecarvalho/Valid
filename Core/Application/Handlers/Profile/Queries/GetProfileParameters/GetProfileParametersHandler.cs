using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Application.Handlers.Profile.Queries.GetProfileParameters;

public class GetProfileParametersHandler(ILogger<GetProfileParametersHandler> logger, IMediator mediator)
    : IRequestHandler<GetProfileParametersRequest, Domain.Entities.Profile>
{
    public Task<Domain.Entities.Profile> Handle(GetProfileParametersRequest request, CancellationToken cancellationToken)
    {
        var listAllProfilesRequest = new ListAllProfilesQueryRequest();
        var profiles = mediator.Send(listAllProfilesRequest, cancellationToken).Result ?? new List<Domain.Entities.Profile>();

        var profile = profiles.FirstOrDefault(x => x.ProfileName == request.ProfileName);

        if (profile == null)
        {
            logger.LogInformation("Perfil {RequestProfileName} foi procurado mas não foi encontrado", request.ProfileName);
            throw new KeyNotFoundException("Perfil não foi encontrado");
        }

        logger.LogInformation("Perfil {RequestProfileName} foi procurado e encontrado", request.ProfileName);

        return Task.FromResult(profile);
    }
}