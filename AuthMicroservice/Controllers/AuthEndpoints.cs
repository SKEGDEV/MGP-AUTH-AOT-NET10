using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthMicroservice.Core.Interfaces;
using AuthMicroservice.Core.DTOs.Requests;

namespace AuthMicroservice.Controllers;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/signup", ([FromBody] SignupRequestDTO request, IAuthService authService) =>
        {
            var result = authService.Signup(request);
            return Results.Ok(result);
        });

        app.MapPost("/signin", ([FromBody] SigninRequestDTO request, IAuthService authService) =>
        {
            var result = authService.Signin(request);
            return Results.Ok(result);
        });

        app.MapPost("/validate-session", ([FromBody] SessionRequestDTO request, IAuthService authService) =>
        {
            var result = authService.ValidateSession(request);
            return Results.Ok(result);
        });

        app.MapDelete("/logout", ([FromQuery] string refreshToken, IAuthService authService) =>
        {
            var result = authService.Logout(refreshToken);
            return Results.Ok(result);
        });

        app.MapPost("/create-restore-code", ([FromBody] CreateRestoreCodeRequestDTO request, IAuthService authService) =>
        {
            var result = authService.CreateRestoreCode(request);
            return Results.Ok(result);
        });

        app.MapPost("/validate-restore-code", ([FromBody] ValidateRestoreCodeRequestDTO request, IAuthService authService) =>
        {
            var result = authService.ValidateRestoreCode(request);
            return Results.Ok(result);
        });
    }
}
