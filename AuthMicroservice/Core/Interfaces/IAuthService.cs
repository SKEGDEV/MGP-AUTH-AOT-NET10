using AuthMicroservice.Core.DTOs.Requests;
using AuthMicroservice.Core.DTOs.Responses;

namespace AuthMicroservice.Core.Interfaces;

public interface IAuthService
{
    StandardResponseDTO<AuthResponseDTO> Signup(SignupRequestDTO request);
    StandardResponseDTO<AuthResponseDTO> Signin(SigninRequestDTO request);
    StandardResponseDTO<SessionResponseDTO?> ValidateSession(SessionRequestDTO request);
    StandardResponseDTO<object?> Logout(string refreshToken);
}
