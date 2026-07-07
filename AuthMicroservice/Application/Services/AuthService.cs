using System;
using AuthMicroservice.Core.DTOs.Requests;
using AuthMicroservice.Core.DTOs.Responses;
using AuthMicroservice.Core.Interfaces;
using AuthMicroservice.Core.Exceptions;
using AuthMicroservice.Core.Dictionaries;

namespace AuthMicroservice.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICryptoHelper _cryptoHelper;
    private readonly IJwtHelper _jwtHelper;

    public AuthService(IUserRepository userRepository, ICryptoHelper cryptoHelper, IJwtHelper jwtHelper)
    {
        _userRepository = userRepository;
        _cryptoHelper = cryptoHelper;
        _jwtHelper = jwtHelper;
    }

    public StandardResponseDTO<AuthResponseDTO> Signup(SignupRequestDTO request)
    {
        try
        {
            var count = _userRepository.GetUserCount(request.UserEmail ?? string.Empty, request.UserName, request.UserIsoCountry);
            if (count > 0)
            {
                throw new CustomAuthException(409, "AUTH001", MessageDictionary.GetMessage("AUTH001"));
            }

            var hashedPassword = _cryptoHelper.HashPassword(request.UserPassword);
            var newUid = _userRepository.InsertUser(request, hashedPassword);
            
            var userFullName = $"{request.UserFirstName} {request.UserLastName}";
            var session = _jwtHelper.CreateSession(newUid, userFullName, request.UserEmail ?? request.UserName ?? "");
            
            _userRepository.UpsertSession(newUid, session.RefreshToken, session.SessionToken);

            return new StandardResponseDTO<AuthResponseDTO>
            {
                StatusCode = 200,
                Success = true,
                Message = MessageDictionary.GetMessage("OK001"),
                Data = new AuthResponseDTO
                {
                    UserFullName = userFullName,
                    UserEmail = request.UserEmail ?? string.Empty,
                    RefreshToken = session.RefreshToken,
                    SessionToken = session.SessionToken
                }
            };
        }
        catch (CustomAuthException)
        {
            throw; // Rethrow to let middleware handle it
        }
        catch (Exception ex)
        {
            // Unhandled exceptions are converted to 500
            throw new CustomAuthException(500, "AUT000", MessageDictionary.GetMessage("AUT000") + ": " + ex.Message);
        }
    }

    public StandardResponseDTO<AuthResponseDTO> Signin(SigninRequestDTO request)
    {
        try
        {
            var user = _userRepository.GetUserForAuth(request.UserEmail ?? string.Empty, request.UserName, request.UserIsoCountry);
            
            if (user == null)
            {
                throw new CustomAuthException(409, "AUTH002", MessageDictionary.GetMessage("AUTH002"));
            }

            var match = _cryptoHelper.VerifyPassword(request.UserPassword, user.Value.PasswordHash);
            if (!match)
            {
                throw new CustomAuthException(409, "AUTH002", MessageDictionary.GetMessage("AUTH002"));
            }

            var session = _jwtHelper.CreateSession(user.Value.UserUID, user.Value.UserFullName, request.UserEmail ?? request.UserName ?? "");
            _userRepository.UpsertSession(user.Value.UserUID, session.RefreshToken, session.SessionToken);

            return new StandardResponseDTO<AuthResponseDTO>
            {
                StatusCode = 200,
                Success = true,
                Message = MessageDictionary.GetMessage("OK002"),
                Data = new AuthResponseDTO
                {
                    UserFullName = user.Value.UserFullName,
                    UserEmail = request.UserEmail ?? string.Empty,
                    RefreshToken = session.RefreshToken,
                    SessionToken = session.SessionToken
                }
            };
        }
        catch (CustomAuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CustomAuthException(500, "AUT000", MessageDictionary.GetMessage("AUT000") + ": " + ex.Message);
        }
    }

    public StandardResponseDTO<SessionResponseDTO?> ValidateSession(SessionRequestDTO request)
    {
        try
        {
            var (isValid, isExpired) = _jwtHelper.ValidateToken(request.Token);

            if (!isValid)
            {
                throw new CustomAuthException(401, "AUTH003", MessageDictionary.GetMessage("AUTH003"));
            }

            if (!isExpired)
            {
                return new StandardResponseDTO<SessionResponseDTO?>
                {
                    StatusCode = 200,
                    Success = true,
                    Message = MessageDictionary.GetMessage("OK003"),
                    Data = null
                };
            }

            var sessionExpiresDate = _userRepository.GetSessionExpiresDate(request.RefreshToken);
            if (sessionExpiresDate == null)
            {
                throw new CustomAuthException(401, "AUTH004", MessageDictionary.GetMessage("AUTH004"));
            }

            if (sessionExpiresDate.Value < DateTime.UtcNow)
            {
                throw new CustomAuthException(401, "AUTH005", MessageDictionary.GetMessage("AUTH005"));
            }

            var userData = _userRepository.GetUserDataByRefreshToken(request.RefreshToken);
            if (userData == null)
            {
                throw new CustomAuthException(401, "AUTH004", MessageDictionary.GetMessage("AUTH004"));
            }

            var session = _jwtHelper.CreateSession(userData.Value.UserUID, userData.Value.UserFullName, userData.Value.UserEmail);
            
            _userRepository.UpdateSessionByRefreshToken(request.RefreshToken, session.RefreshToken, session.SessionToken);

            return new StandardResponseDTO<SessionResponseDTO?>
            {
                StatusCode = 200,
                Success = true,
                Message = MessageDictionary.GetMessage("OK003"),
                Data = new SessionResponseDTO
                {
                    Token = session.SessionToken,
                    RefreshToken = session.RefreshToken
                }
            };
        }
        catch (CustomAuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CustomAuthException(500, "AUT000", MessageDictionary.GetMessage("AUT000") + ": " + ex.Message);
        }
    }

    public StandardResponseDTO<object?> Logout(string refreshToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _userRepository.DeleteSession(refreshToken);
            }

            return new StandardResponseDTO<object?>
            {
                StatusCode = 200,
                Success = true,
                Message = MessageDictionary.GetMessage("OK004"),
                Data = null
            };
        }
        catch (Exception ex)
        {
            throw new CustomAuthException(500, "AUT000", MessageDictionary.GetMessage("AUT000") + ": " + ex.Message);
        }
    }
}
