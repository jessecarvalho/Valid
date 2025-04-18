using Application.Application.Handlers.Profile.Commands.CreateProfile;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Application.Application.Services;

public class ProfileParameterInitializeService(
    IMediator mediator,
    ILogger<ProfileParameterInitializeService> logger,
    IConfiguration config)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando serviço de inicialização de perfis...");
        
        try
        {
            var defaultProfiles = config.GetSection("DefaultProfiles")
                                      .Get<Dictionary<string, Dictionary<string, string>>>() 
                                  ?? GetFallbackConfiguration();
            await InitializeProfilesAsync(defaultProfiles, cancellationToken);
            
            logger.LogInformation("Inicialização de perfis concluída com sucesso");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha na inicialização dos perfis padrão");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task InitializeProfilesAsync(
        Dictionary<string, Dictionary<string, string>> profilesConfig,
        CancellationToken cancellationToken)
    {
        var tasks = profilesConfig.Select(async profileConfig => 
        {
            try
            {
                await mediator.Send(
                    new CreateProfileCommandRequest(
                        new Profile(profileConfig.Key, profileConfig.Value)),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha no perfil {ProfileName}", profileConfig.Key);
            }
        });

        await Task.WhenAll(tasks);
}
    
    private Dictionary<string, Dictionary<string, string>> GetFallbackConfiguration()
    {
        return new() { /* ... */ };
    }
}