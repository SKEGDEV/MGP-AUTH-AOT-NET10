namespace AuthMicroservice.Core.DTOs.Responses;

public class AuthResponseDTO
{
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
}
