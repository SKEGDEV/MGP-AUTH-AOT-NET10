# Implementation of Session Validation and Logout Flows (spect-0002)

This plan details the implementation of the new Session Validation and Logout features as defined in `spect-0002-version-1.md`, while strictly adhering to the architectural rules defined in `micro_development_roll.md`.

## User Review Required
- **JWT Expiration Change:** The JWT expiration time will be reduced from 2 hours to 15 minutes as per spec 1.3. This will affect all future sign-ins.
- **Endpoint Routes:** I propose using `/validate-session` for the POST method and `/logout` for the DELETE method. Please confirm if these route paths are acceptable.

## Open Questions
- None at this time, the specification is clear.

## Proposed Changes

### Core (AOT Optimized, DTOs, Interfaces, Dictionaries)
- Add new request and response DTOs for the session validation flow.
- Add specific exceptions mapping to the new error codes.
- Register new DTOs in the AOT JSON Serializer Context.

#### [NEW] [SessionRequestDTO.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/DTOs/Requests/SessionRequestDTO.cs)
```csharp
namespace AuthMicroservice.Core.DTOs.Requests;

public class SessionRequestDTO
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

#### [NEW] [SessionResponseDTO.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/DTOs/Responses/SessionResponseDTO.cs)
```csharp
namespace AuthMicroservice.Core.DTOs.Responses;

public class SessionResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
```

#### [MODIFY] [AppJsonSerializerContext.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Serialization/AppJsonSerializerContext.cs)
- Add `[JsonSerializable(typeof(SessionRequestDTO))]`
- Add `[JsonSerializable(typeof(SessionResponseDTO))]`
- Add `[JsonSerializable(typeof(StandardResponseDTO<SessionResponseDTO>))]`

#### [MODIFY] [MessageDictionary.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Dictionaries/MessageDictionary.cs)
- Add entries for:
  - `"AUTH003"`: "token malformed, please re-signin"
  - `"AUTH004"`: "session doesn't exist, please re-signin"
  - `"AUTH005"`: "session expired, please re-signin"
  - `"OK003"`: "Session valid" (or similar success message for refresh/validate)
  - `"OK004"`: "Have a good day i hope see you again"

#### [MODIFY] [IJwtHelper.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Interfaces/IJwtHelper.cs)
- Add `(bool IsValid, bool IsExpired) ValidateToken(string token);`

#### [MODIFY] [IUserRepository.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Interfaces/IUserRepository.cs)
- Add `DateTime? GetSessionExpiresDate(string refreshToken);`
- Add `(string UserUID, string UserFullName, string UserEmail)? GetUserDataByRefreshToken(string refreshToken);`
- Add `void UpdateSessionByRefreshToken(string oldRefreshToken, string newRefreshToken, string newSessionToken);`
- Add `void DeleteSession(string refreshToken);`

#### [MODIFY] [IAuthService.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Core/Interfaces/IAuthService.cs)
- Add `StandardResponseDTO<SessionResponseDTO> ValidateSession(SessionRequestDTO request);`
- Add `StandardResponseDTO<object> Logout(string refreshToken);`

---

### Infrastructure (Repositories)

#### [MODIFY] [UserRepository.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Infrastructure/Repositories/UserRepository.cs)
- Implement `GetSessionExpiresDate` to query `userSessionExpiresDate` from `userSession` using `userSessionRefreshToken`.
- Implement `GetUserDataByRefreshToken` to JOIN `userSession` and `user` to fetch data required for a new JWT (UID, FullName, Email).
- Implement `UpdateSessionByRefreshToken` to update `userSessionRefreshToken`, `userSessionExpiresDate` where `userSessionRefreshToken` matches the old one.
- Implement `DeleteSession` to delete the session record where `userSessionRefreshToken` matches.

---

### Application (Services & Helpers)

#### [MODIFY] [JwtHelper.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Application/Helpers/JwtHelper.cs)
- Modify `CreateSession` to set the expiration time of the JWT to **15 minutes** instead of 2 hours.
- Implement `ValidateToken(string token)` to use `JwtSecurityTokenHandler` with `ValidateLifetime = false`. It will manually check if the signature is valid, and then return whether it's valid and if it's expired based on the token's `ValidTo` claim.

#### [MODIFY] [AuthService.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Application/Services/AuthService.cs)
- Implement `ValidateSession` orchestrating the flowchart logic:
  - Call `_jwtHelper.ValidateToken(request.Token)`
  - Handle signature failure (`AUTH003`, 401).
  - Handle valid & not expired (Return 200, null data).
  - Handle expired -> DB checks for refresh token (`AUTH004`, `AUTH005`).
  - Generate new tokens and update DB.
- Implement `Logout` orchestrating the deletion logic.

---

### Controller (Minimal APIs)

#### [MODIFY] [AuthEndpoints.cs](file:///home/skegdeveloper/Documents/MGP-MICROSRV-AOT-NET10/AUTH/AuthMicroservice/Controllers/AuthEndpoints.cs)
- Map `POST /validate-session` receiving `SessionRequestDTO` from body and calling `authService.ValidateSession`.
- Map `DELETE /logout` receiving `refreshToken` from query parameters (`[FromQuery]`) and calling `authService.Logout`.

## Verification Plan

### Automated Tests
- No automated test commands were found in the current workspace, so manual verification will be prioritized, and we can check if it builds using `.NET` CLI.
- Run `dotnet build` to ensure Native AOT compatibility and zero compilation errors.

### Manual Verification
- Review code visually to ensure no `try/catch` inside repositories, no business logic in controllers, and strict AOT serialization compliance.
