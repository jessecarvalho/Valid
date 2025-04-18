using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Application.Handlers.Profile.Queries.ListAllProfiles;

public class ListAllProfilesQueryHandler(IMemoryStorage memoryStorage, ILogger<ListAllProfilesQueryHandler> logger)
    : IRequestHandler<ListAllProfilesQueryRequest, List<Domain.Entities.Profile>?>
{
    public Task<List<Domain.Entities.Profile>?> Handle(ListAllProfilesQueryRequest request, CancellationToken cancellationToken)
    {
        var profiles = memoryStorage.Get<List<Domain.Entities.Profile>?>("profiles");
        
        if (profiles == null || profiles.Count == 0)
            logger.LogInformation("Nenhum perfil encontrado na memoria após busca todos os perfils");

        return Task.FromResult(profiles);
    }
}