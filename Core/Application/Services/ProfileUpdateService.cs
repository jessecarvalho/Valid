using Application.Application.Handlers.Profile.Commands.UpdateProfile;
using Application.Application.Handlers.Profile.Queries;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Application.Services;

public class ProfileUpdateService(IMediator mediator, ILogger<ProfileUpdateService> logger, IConfiguration configuration) : BackgroundService
{
    private readonly Random _random = new();
    private readonly int  _timeToUpdateProfilesInSeconds = configuration.GetValue<int>("TimeToUpdateProfilesInSeconds", 300);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromMinutes(_timeToUpdateProfilesInSeconds);
            try
            {
                logger.LogInformation("Iniciando atualização aleatória de parâmetros de perfis");

                var profiles = await mediator.Send(new ListAllProfilesQueryRequest(), cancellationToken) ?? new List<Profile>();

                if (!profiles.Any())
                {
                    logger.LogInformation("Nenhum perfil encontrado para atualização");
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                var randomProfile = profiles[_random.Next(profiles.Count)];
                
                if (!randomProfile.Parameters.Any())
                {
                    logger.LogInformation("Perfil '{ProfileName}' não possui parâmetros para atualizar", randomProfile.ProfileName);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                var randomParameterKey = randomProfile.Parameters.Keys.ElementAt(_random.Next(randomProfile.Parameters.Count));
                
                var randomValue = _random.Next(2) == 0 ? "False" : "True";
                
                randomProfile.Parameters[randomParameterKey] = randomValue;

                logger.LogInformation("Atualizando parâmetro '{RandomParameterKey}' do perfil '{ProfileName}' para '{RandomValue}'", randomParameterKey, randomProfile.ProfileName, randomValue);

                await mediator.Send(new UpdateProfileCommandRequest(randomProfile.ProfileName, randomProfile), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante a atualização de perfis");
            }

            await Task.Delay(delay, cancellationToken);
        }
    }
}