using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthMicroservice.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthMicroservice.Application.Helpers;

public class JwtHelper : IJwtHelper
{
    private readonly string _secret;

    public JwtHelper(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"] ?? "SuperSecretKeyForDevelopmentPurposesOnly123!";
    }

    public (string SessionToken, string RefreshToken) CreateSession(string userUID, string userFullName, string userEmail)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userUID),
                new Claim(ClaimTypes.Name, userFullName),
                new Claim(ClaimTypes.Email, userEmail)
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var sessionToken = tokenHandler.WriteToken(token);

        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        return (sessionToken, refreshToken);
    }

    public (bool IsValid, bool IsExpired) ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // We check lifetime manually below
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var isExpired = jwtToken.ValidTo < DateTime.UtcNow;

            return (true, isExpired);
        }
        catch
        {
            return (false, false);
        }
    }
}
