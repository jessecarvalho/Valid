namespace Application.Domain.Entities;

public class Profile (string profileName, Dictionary<string, string>? parameters)
{
    public string ProfileName { get; } = profileName;
    public Dictionary<string, string> Parameters { get; } = parameters ?? new Dictionary<string, string>();
}