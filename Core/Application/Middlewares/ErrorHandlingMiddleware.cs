using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Application.Middlewares;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorMessage) = DetermineStatusCodeAndMessage(exception);
            
        var errorId = Guid.NewGuid().ToString();
            
        logger.LogError(exception, "Erro {ErrorId}: {Message}", errorId, exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new ErrorResponse
        {
            ErrorId = errorId,
            Message = errorMessage,
            Details = environment.IsDevelopment() ? exception.Message : null,
            StackTrace = environment.IsDevelopment() ? exception.StackTrace : null,
            Timestamp = DateTimeOffset.UtcNow
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = environment.IsDevelopment()
        };

        var jsonString = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(jsonString);
    }

    private static (HttpStatusCode StatusCode, string Message) DetermineStatusCodeAndMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Não autorizado"),
            
            ArgumentException or 
                ArgumentNullException or 
                ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, exception.Message),
            
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "A operação expirou"),
            
            NotImplementedException => (HttpStatusCode.NotImplemented, "Funcionalidade não implementada"),
            
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            
            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro interno. Por favor, tente novamente mais tarde.")
        };
    }
}

public class ErrorResponse
{
    public string ErrorId { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}