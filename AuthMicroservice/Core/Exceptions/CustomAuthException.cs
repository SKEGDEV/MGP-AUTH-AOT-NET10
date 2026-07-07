using System;
using System.Collections.Generic;
using AuthMicroservice.Core.DTOs.Responses;

namespace AuthMicroservice.Core.Exceptions;

public class CustomAuthException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public List<StandardError>? Errors { get; }
    public string UserErrorMessage { get; }

    public CustomAuthException(int statusCode, string errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        UserErrorMessage = message;
    }

    public CustomAuthException(int statusCode, string errorCode, string userErrorMessage, List<StandardError> errors) : base(userErrorMessage)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Errors = errors;
        UserErrorMessage = userErrorMessage;
    }
}
