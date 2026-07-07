namespace AuthMicroservice.Core.DTOs.Responses;

public class SessionResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
