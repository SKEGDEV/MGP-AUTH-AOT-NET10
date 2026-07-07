namespace AuthMicroservice.Core.Interfaces;

public interface ICryptoHelper
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string hashedPassword);
}
