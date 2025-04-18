using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Application.Handlers.Profile.Queries.ValidateProfileParameter;

public class ValidateProfileParameterHandler(
    ILogger<ValidateProfileParameterHandler> logger,
    IMediator mediator)
    : IRequestHandler<ValidateProfileParameterRequest, bool>
{
    public Task<bool> Handle(ValidateProfileParameterRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando validação do parâmetro {Parameter} para o perfil {ProfileName}", 
            request.Parameter, request.ProfileName);

        try
        {
            logger.LogDebug("Buscando parâmetros do perfil {ProfileName}", request.ProfileName);
            var getProfileRequest = new GetProfileParametersRequest(request.ProfileName);
            var profile = mediator.Send(getProfileRequest, cancellationToken).Result;

            if (profile.Parameters.TryGetValue(request.Parameter, out var value))
            {
                bool isValid = value.ToLower() == "true";
                logger.LogDebug("Parâmetro {Parameter} encontrado com valor {Value} (válido: {IsValid})",
                    request.Parameter, value, isValid);
                return Task.FromResult(isValid);
            }

            logger.LogDebug("Parâmetro {Parameter} não encontrado no perfil {ProfileName}", 
                request.Parameter, request.ProfileName);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao validar parâmetro {Parameter} para o perfil {ProfileName}", 
                request.Parameter, request.ProfileName);
            throw;
        }
        
        
    }
}