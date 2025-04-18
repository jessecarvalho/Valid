using Application.Application.Handlers.Profile.Commands.CreateProfile;
using Application.Application.Handlers.Profile.Queries;
using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Application.Handlers.Profile.Commands.DeleteProfile;

public class DeleteProfileCommandHandler(
    IMemoryStorage memoryStorage,
    ILogger<DeleteProfileCommandHandler> logger,
    IMediator mediator)
    : IRequestHandler<DeleteProfileCommandRequest, bool>
{
    public Task<bool> Handle(DeleteProfileCommandRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando exclusão do perfil: {ProfileName}", request.ProfileName);

        try
        {
            logger.LogDebug("Buscando parâmetros do perfil {ProfileName}", request.ProfileName);
            var getProfileRequest = new GetProfileParametersRequest(request.ProfileName);
            var profile = mediator.Send(getProfileRequest, cancellationToken).Result;

            logger.LogDebug("Obtendo lista completa de perfis");
            var getAllProfilesRequest = new ListAllProfilesQueryRequest();
            var profiles = mediator.Send(getAllProfilesRequest, cancellationToken).Result ?? new List<Domain.Entities.Profile>();

            logger.LogInformation("Removendo perfil {ProfileName} da lista de {ProfileCount} perfis", 
                request.ProfileName, profiles.Count);
            
            bool removed = profiles.Remove(profile);
            if (!removed)
            {
                logger.LogWarning("Perfil {ProfileName} não encontrado na lista durante a remoção", request.ProfileName);
                return Task.FromResult(false);
            }
            
            logger.LogDebug("Atualizando perfis");
            memoryStorage.Remove("profile");
            memoryStorage.Set<List<Domain.Entities.Profile>>("profile", profiles);

            logger.LogInformation("Perfil {ProfileName} excluído com sucesso", request.ProfileName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir perfil {ProfileName}", request.ProfileName);
            throw;
        }
    }
}