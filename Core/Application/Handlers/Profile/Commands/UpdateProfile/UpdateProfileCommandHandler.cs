using Application.Application.Handlers.Profile.Commands.CreateProfile;
using Application.Application.Handlers.Profile.Queries;
using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Application.Handlers.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IMemoryStorage memoryStorage,
    ILogger<UpdateProfileCommandHandler> logger,
    IMediator mediator)
    : IRequestHandler<UpdateProfileCommandRequest, Domain.Entities.Profile>
{
    public Task<Domain.Entities.Profile> Handle(UpdateProfileCommandRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando atualização do perfil: {OldProfileName} para {NewProfileName}", 
        request.OldProfileName, request.NewProfile.ProfileName);

        try
        {
            logger.LogDebug("Buscando perfil antigo: {OldProfileName}", request.OldProfileName);
            var getProfileRequest = new GetProfileParametersRequest(request.OldProfileName);
            var profile = mediator.Send(getProfileRequest, cancellationToken).Result;

            if (profile == null)
            {
                logger.LogWarning("Perfil antigo {OldProfileName} não encontrado", request.OldProfileName);
                throw new KeyNotFoundException($"Perfil {request.OldProfileName} não encontrado");
            }

            logger.LogDebug("Obtendo lista completa de perfis");
            var getAllProfilesRequest = new ListAllProfilesQueryRequest();
            var profiles = mediator.Send(getAllProfilesRequest, cancellationToken).Result ?? new List<Domain.Entities.Profile>();
            
            logger.LogInformation("Substituindo perfil {OldProfileName} na lista de {ProfileCount} perfis",
                request.OldProfileName, profiles.Count);
            
            if (!profiles.Remove(profile))
            {
                logger.LogWarning("Perfil {OldProfileName} não estava na lista para remoção", request.OldProfileName);
            }

            profiles.Add(request.NewProfile);
            logger.LogDebug("Novo perfil {NewProfileName} adicionado à lista", request.NewProfile.ProfileName);

            logger.LogDebug("Atualizando storage de perfis");
            memoryStorage.Remove("profile");
            memoryStorage.Set<List<Domain.Entities.Profile>>("profile", profiles);

            logger.LogInformation("Perfil {OldProfileName} atualizado para {NewProfileName} com sucesso",
                request.OldProfileName, request.NewProfile.ProfileName);
            
            return Task.FromResult(request.NewProfile);
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao atualizar perfil {OldProfileName} para {NewProfileName}",
                request.OldProfileName, request.NewProfile.ProfileName);
            throw;
        }
    }
}