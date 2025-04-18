using Application.Interfaces;
using FluentValidation;
using Infra;

namespace Presentation.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddProfileServices(this IServiceCollection services)
    {
        services.AddSingleton<IMemoryStorage, MemoryStorage>();
        services.AddValidatorsFromAssemblyContaining<CreateProfileCommandValidator>();
        return services;
    }
}