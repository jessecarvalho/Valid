using MediatR;

namespace Application.Application.Handlers.Profile.Queries.ValidateProfileParameter;

public class ValidateProfileParameterRequest(string profileName, string parameter) : IRequest<bool>
{
    public string ProfileName { get; set; } = profileName;
    public string Parameter { get; set; } = parameter;
}