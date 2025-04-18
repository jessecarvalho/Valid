using System.Reflection;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Application.Middlewares;
using Application.Application.Services;
using Application.Interfaces;
using FluentValidation;
using Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssemblyContaining<ListAllProfilesQueryRequest>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProfileServices();
builder.Services.AddSwagger();
builder.Services.AddHealthChecks();

builder.Services.Configure<ApiBehaviorOptions>(options => 
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddHostedService<ProfileParameterInitializeService>();
builder.Services.AddHostedService<ProfileUpdateService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UsePathBase("/api");
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Valid Test v1");
    c.RoutePrefix = "docs";
});
app.MapHealthChecks("/health");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();