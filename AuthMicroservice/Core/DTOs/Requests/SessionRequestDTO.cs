namespace AuthMicroservice.Core.DTOs.Requests;

public class SessionRequestDTO
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
