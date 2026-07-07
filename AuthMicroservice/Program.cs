using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AuthMicroservice.Core.Interfaces;
using AuthMicroservice.Application.Services;
using AuthMicroservice.Application.Helpers;
using AuthMicroservice.Infrastructure.Repositories;
using AuthMicroservice.Controllers;
using AuthMicroservice.Middleware;
using AuthMicroservice.Core.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Register Dependencies
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ICryptoHelper, CryptoHelper>();
builder.Services.AddSingleton<IJwtHelper, JwtHelper>();
builder.Services.AddSingleton<IAuthService, AuthService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

var apiGroup = app.MapGroup("/api/v1/auth")
                  .AddEndpointFilter<ValidationFilter>();

apiGroup.MapAuthEndpoints();

app.Run();
