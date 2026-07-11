namespace AuthMicroservice.Core.DTOs.Responses;

public class CreateRestoreCodeResponseDTO
{
    public EmailDTO<string> Email { get; set; } = new();
}
