using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Presentation.Extensions;

public static class SwaggerRegistration
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "Valid Test", 
                Version = "v1",
                Description = "Api for Valid TEst"
            });
    
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
        return services;
    }
}

