using System.Data;
using Application.Application.Handlers.Profile.Queries;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Application.Handlers.Profile.Commands.CreateProfile;

public class CreateProfileCommandHandler(
    IMemoryStorage memoryStorage,
    ILogger<CreateProfileCommandHandler> logger,
    IMediator mediator)
    : IRequestHandler<CreateProfileCommandRequest, Domain.Entities.Profile>
{
    public Task<Domain.Entities.Profile> Handle(CreateProfileCommandRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando criação do perfil {ProfileName}", request.Profile.ProfileName);

        try
        {
            logger.LogDebug("Obtendo lista atual de perfis");
            var listAllProfilesRequest = new ListAllProfilesQueryRequest();
            var profiles = mediator.Send(listAllProfilesRequest, cancellationToken).Result ?? new List<Domain.Entities.Profile>();

            CheckIfProfileAlreadyExists();

            logger.LogInformation("Adicionando novo perfil à lista existente de {ProfileCount} perfis", profiles.Count);
            profiles.Add(request.Profile);
            
            logger.LogDebug("Atualizando storage de perfis");
            memoryStorage.Remove("profiles");
            memoryStorage.Set<List<Domain.Entities.Profile>>("profiles", profiles);

            logger.LogInformation("Perfil {ProfileName} criado com sucesso", request.Profile.ProfileName);
            return Task.FromResult(request.Profile);
            
            void CheckIfProfileAlreadyExists()
            {
                var profileExists = profiles.Any(x => 
                    string.Equals(x.ProfileName.Trim(), request.Profile.ProfileName.Trim()));

                if (profileExists)
                    throw new ArgumentException($"Já existe um perfil com o nome '{request.Profile.ProfileName}'");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao criar perfil {ProfileName}", request.Profile.ProfileName);
            throw;
        }
    }
}