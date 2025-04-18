using Application.Application.Handlers.Profile.Commands.CreateProfile;
using Application.Application.Handlers.Profile.Commands.DeleteProfile;
using Application.Application.Handlers.Profile.Commands.UpdateProfile;
using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Application.Handlers.Profile.Queries.ValidateProfileParameter;
using Application.Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Esta API receberá requisições relacionadas ao gerenciamento de parâmetros de perfis
/// </summary>
[ApiController]
[Route("profiles")]
public class ProfileController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista todos os perfis cadastrados no sistema
    /// </summary>
    /// <param name="cancellationToken">Token para cancelamento da requisição</param>
    /// <response code="200">Retorna a lista completa de perfis</response>
    [ProducesResponseType(typeof(List<Profile>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<ActionResult<List<Profile>>> GetProfiles(
        CancellationToken cancellationToken)
    {
        var request = new ListAllProfilesQueryRequest();
        var profiles = await mediator.Send(request, cancellationToken);
        return Ok(profiles);
    }
    
    /// <summary>
    /// Obtém um perfil específico pelo nome
    /// </summary>
    /// <param name="profileName">Nome do perfil a ser consultado</param>
    /// <param name="cancellationToken">Token para cancelamento</param>
    [ProducesResponseType(typeof(Profile), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{profileName}")]
    public async Task<ActionResult<Profile>> GetProfile(
        [FromRoute] string profileName,
        CancellationToken cancellationToken)
    {
        var request = new GetProfileParametersRequest(profileName);
        var profile = await mediator.Send(request, cancellationToken);
        return Ok(profile);
    }
    
    /// <summary>
    /// Cria um novo perfil
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(typeof(Profile), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<ActionResult<Profile>> CreateProfile(
        CreateProfileCommandRequest request,
        CancellationToken cancellationToken)
    {
        var newProfile = await mediator.Send(request, cancellationToken);
        return Created(newProfile.ProfileName, newProfile);
    }

    /// <summary>
    /// Atualiza um perfil
    /// </summary>
    /// <param name="request">Dados do perfil para atualização</param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(typeof(List<Profile>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<ActionResult<List<Profile>>> UpdateProfile(UpdateProfileCommandRequest request, CancellationToken cancellationToken)
    {
        var profile = await mediator.Send(request, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Remove um perfil específico do sistema
    /// </summary>
    /// <param name="profileName">Nome do perfil a ser removido (case-sensitive)</param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    [HttpDelete("{profileName}")]
    public async Task<IActionResult> DeleteProfile(
        [FromRoute] string profileName,
        CancellationToken cancellationToken)
    {
        var request = new DeleteProfileCommandRequest(profileName);
        await mediator.Send(request, cancellationToken);
        return NoContent();
    }


    /// <summary>
    /// Valida um parâmetro específico de um perfil
    /// </summary>
    /// <param name="profileName">Nome do perfil</param>
    /// <param name="parameterName">Nome do parâmetro a ser validado</param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(typeof(Profile), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{profileName}/validate")]
    public async Task<ActionResult<Profile>> ValidatePermissionFromProfile(
        [FromRoute] string profileName,
        [FromQuery] string parameterName,
        CancellationToken cancellationToken)
    {
        var request = new ValidateProfileParameterRequest(profileName, parameterName);
        var profile = await mediator.Send(request, cancellationToken);
        return Ok(profile);
    }
}