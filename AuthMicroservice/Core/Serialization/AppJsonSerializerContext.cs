using System.Text.Json.Serialization;
using AuthMicroservice.Core.DTOs.Requests;
using AuthMicroservice.Core.DTOs.Responses;

namespace AuthMicroservice.Core.Serialization;

[JsonSerializable(typeof(SignupRequestDTO))]
[JsonSerializable(typeof(SigninRequestDTO))]
[JsonSerializable(typeof(SessionRequestDTO))]
[JsonSerializable(typeof(CreateRestoreCodeRequestDTO))]
[JsonSerializable(typeof(ValidateRestoreCodeRequestDTO))]
[JsonSerializable(typeof(AuthResponseDTO))]
[JsonSerializable(typeof(SessionResponseDTO))]
[JsonSerializable(typeof(CreateRestoreCodeResponseDTO))]
[JsonSerializable(typeof(EmailDTO<string>))]
[JsonSerializable(typeof(StandardResponseDTO<AuthResponseDTO>))]
[JsonSerializable(typeof(StandardResponseDTO<SessionResponseDTO>))]
[JsonSerializable(typeof(StandardResponseDTO<CreateRestoreCodeResponseDTO>))]
[JsonSerializable(typeof(StandardResponseDTO<object>))]
[JsonSerializable(typeof(StandardError))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
