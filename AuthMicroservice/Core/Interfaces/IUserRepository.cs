using AuthMicroservice.Core.DTOs.Responses;
using AuthMicroservice.Core.DTOs.Requests;

namespace AuthMicroservice.Core.Interfaces;

public interface IUserRepository
{
    int GetUserCount(string email, string? username, string isoCountry);
    string InsertUser(SignupRequestDTO request, string hashedPassword);
    void UpsertSession(string userUID, string refreshToken, string sessionToken);
    
    // Returns (passwordHash, userUID, userFullName) or null if not found
    (string PasswordHash, string UserUID, string UserFullName)? GetUserForAuth(string email, string? username, string isoCountry);

    DateTime? GetSessionExpiresDate(string refreshToken);
    (string UserUID, string UserFullName, string UserEmail)? GetUserDataByRefreshToken(string refreshToken);
    void UpdateSessionByRefreshToken(string oldRefreshToken, string newRefreshToken, string newSessionToken);
    void DeleteSession(string refreshToken);
}
