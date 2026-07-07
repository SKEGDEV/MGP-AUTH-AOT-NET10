using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AuthMicroservice.Core.Exceptions;
using AuthMicroservice.Core.DTOs.Responses;
using AuthMicroservice.Core.Dictionaries;
using AuthMicroservice.Core.Serialization;

namespace AuthMicroservice.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomAuthException ex)
        {
            await HandleExceptionAsync(context, ex.StatusCode, ex.ErrorCode, ex.UserErrorMessage, ex.Errors);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, 500, "AUT000", MessageDictionary.GetMessage("AUT000") + ": " + ex.Message, null);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, int statusCode, string errorCode, string userErrorMessage, System.Collections.Generic.List<StandardError>? errors)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var traceId = context.TraceIdentifier;

        var response = new StandardResponseDTO<object>
        {
            StatusCode = statusCode,
            Success = false,
            Message = MessageDictionary.GetMessage(errorCode) ?? "Error",
            UserErrorMessage = userErrorMessage,
            ErrorTraceId = traceId,
            ErrorDateTimeEvent = DateTime.UtcNow,
            Errors = errors ?? new System.Collections.Generic.List<StandardError>
            {
                new StandardError
                {
                    ErrorCode = errorCode,
                    ErrorComponent = "AuthMicroservice",
                    ErrorMessage = userErrorMessage
                }
            }
        };

        var json = JsonSerializer.Serialize(response, AppJsonSerializerContext.Default.StandardResponseDTOObject);
        return context.Response.WriteAsync(json);
    }
}
