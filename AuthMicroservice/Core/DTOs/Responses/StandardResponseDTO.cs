using System;
using System.Collections.Generic;

namespace AuthMicroservice.Core.DTOs.Responses;

public class StandardError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorComponent { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class StandardResponseDTO<T>
{
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<StandardError> Errors { get; set; } = new();
    public string UserErrorMessage { get; set; } = string.Empty;
    public string ErrorTraceId { get; set; } = string.Empty;
    public DateTime ErrorDateTimeEvent { get; set; } = DateTime.UtcNow;
}
