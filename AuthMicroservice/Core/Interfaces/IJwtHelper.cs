namespace AuthMicroservice.Core.Interfaces;

public interface IJwtHelper
{
    (string SessionToken, string RefreshToken) CreateSession(string userUID, string userFullName, string userEmail);
    (bool IsValid, bool IsExpired) ValidateToken(string token);
}
