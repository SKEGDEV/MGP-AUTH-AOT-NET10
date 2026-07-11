using System;
using Microsoft.Data.Sqlite;
using AuthMicroservice.Core.Interfaces;
using AuthMicroservice.Core.DTOs.Requests;

namespace AuthMicroservice.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(ISettings settings)
    {
        _connectionString = settings.ConnectionString;
    }

    public int GetUserCount(string email, string? username, string isoCountry)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT COUNT(1) 
            FROM user 
            WHERE (userEmail = @email OR (userName IS NOT NULL AND userName = @username))
              AND userIsoCountry = @isoCountry";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@email", email ?? string.Empty);
        command.Parameters.AddWithValue("@username", username ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@isoCountry", isoCountry);

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public string InsertUser(SignupRequestDTO request, string hashedPassword)
    {
        var userUID = Guid.NewGuid().ToString();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            INSERT INTO user (userUID, userFirstName, userLastName, userName, userEmail, userPassword, userIsoCountry, userStatusId)
            VALUES (@userUID, @firstName, @lastName, @userName, @email, @password, @isoCountry, 1)";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@userUID", userUID);
        command.Parameters.AddWithValue("@firstName", request.UserFirstName);
        command.Parameters.AddWithValue("@lastName", request.UserLastName);
        command.Parameters.AddWithValue("@userName", request.UserName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@email", request.UserEmail ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@password", hashedPassword);
        command.Parameters.AddWithValue("@isoCountry", request.UserIsoCountry);

        command.ExecuteNonQuery();

        return userUID;
    }

    public void UpsertSession(string userUID, string refreshToken, string sessionToken)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Simple approach: Delete existing for user, insert new
        var deleteQuery = "DELETE FROM userSession WHERE userSessionUserUID = @userUID";
        using var deleteCommand = new SqliteCommand(deleteQuery, connection);
        deleteCommand.Parameters.AddWithValue("@userUID", userUID);
        deleteCommand.ExecuteNonQuery();

        var insertQuery = @"
            INSERT INTO userSession (userSessionUID, userSessionUserUID, userSessionRefreshToken, userSessionExpiresDate)
            VALUES (@sessionUID, @userUID, @refreshToken, @expiresDate)";

        using var insertCommand = new SqliteCommand(insertQuery, connection);
        insertCommand.Parameters.AddWithValue("@sessionUID", Guid.NewGuid().ToString());
        insertCommand.Parameters.AddWithValue("@userUID", userUID);
        insertCommand.Parameters.AddWithValue("@refreshToken", refreshToken);
        insertCommand.Parameters.AddWithValue("@expiresDate", DateTime.UtcNow.AddDays(7).ToString("O"));

        insertCommand.ExecuteNonQuery();
    }

    public (string PasswordHash, string UserUID, string UserFullName)? GetUserForAuth(string email, string? username, string isoCountry)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT userPassword, userUID, userFirstName || ' ' || userLastName AS userFullName
            FROM user 
            WHERE (userEmail = @email OR (userName IS NOT NULL AND userName = @username))
              AND userIsoCountry = @isoCountry
              AND userStatusId = 1"; // Rule 6

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@email", email ?? string.Empty);
        command.Parameters.AddWithValue("@username", username ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@isoCountry", isoCountry);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var hash = reader.GetString(0);
            var uid = reader.GetString(1);
            var fullName = reader.GetString(2);
            return (hash, uid, fullName);
        }

        return null;
    }

    public DateTime? GetSessionExpiresDate(string refreshToken)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = "SELECT userSessionExpiresDate FROM userSession WHERE userSessionRefreshToken = @refreshToken";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@refreshToken", refreshToken);

        var result = command.ExecuteScalar();
        if (result != null && result != DBNull.Value && DateTime.TryParse(result.ToString(), out DateTime date))
        {
            return date;
        }

        return null;
    }

    public (string UserUID, string UserFullName, string UserEmail)? GetUserDataByRefreshToken(string refreshToken)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT u.userUID, u.userFirstName || ' ' || u.userLastName AS userFullName, u.userEmail
            FROM userSession s
            JOIN user u ON s.userSessionUserUID = u.userUID
            WHERE s.userSessionRefreshToken = @refreshToken";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@refreshToken", refreshToken);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var uid = reader.GetString(0);
            var fullName = reader.GetString(1);
            var email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            return (uid, fullName, email);
        }

        return null;
    }

    public void UpdateSessionByRefreshToken(string oldRefreshToken, string newRefreshToken, string newSessionToken)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            UPDATE userSession 
            SET userSessionRefreshToken = @newRefreshToken, 
                userSessionExpiresDate = @expiresDate
            WHERE userSessionRefreshToken = @oldRefreshToken";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@newRefreshToken", newRefreshToken);
        command.Parameters.AddWithValue("@expiresDate", DateTime.UtcNow.AddDays(7).ToString("O"));
        command.Parameters.AddWithValue("@oldRefreshToken", oldRefreshToken);

        command.ExecuteNonQuery();
    }

    public void DeleteSession(string refreshToken)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = "DELETE FROM userSession WHERE userSessionRefreshToken = @refreshToken";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@refreshToken", refreshToken);
        
        command.ExecuteNonQuery();
    }

    public string? GetUserUidByEmailAndCountry(string email, string isoCountry)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = "SELECT userUID FROM user WHERE userEmail = @email AND userIsoCountry = @isoCountry";
        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@isoCountry", isoCountry);

        var result = command.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
            return result.ToString();
        }

        return null;
    }

    public void InsertRestoreCode(string userUID, string restoreCode, string dateCreated)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            INSERT INTO userRestoreCode (userRestoreCode, userRestoreCodeUserUID, userRestoreCodeDateCreated)
            VALUES (@code, @userUID, @dateCreated)";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@code", restoreCode);
        command.Parameters.AddWithValue("@userUID", userUID);
        command.Parameters.AddWithValue("@dateCreated", dateCreated);

        command.ExecuteNonQuery();
    }

    public string? GetRestoreCodeDate(string restoreCode, string email, string isoCountry)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            SELECT urc.userRestoreCodeDateCreated
            FROM userRestoreCode urc
            INNER JOIN user u ON u.userUID = urc.userRestoreCodeUserUID
            WHERE urc.userRestoreCode = @code
              AND u.userEmail = @email
              AND u.userIsoCountry = @isoCountry
              AND urc.userRestoreCodeIsUsed = 0";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@code", restoreCode);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@isoCountry", isoCountry);

        var result = command.ExecuteScalar();
        if (result != null && result != DBNull.Value)
        {
            return result.ToString();
        }

        return null;
    }

    public void MarkRestoreCodeAsUsed(string restoreCode, string email, string isoCountry)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var query = @"
            UPDATE userRestoreCode
            SET userRestoreCodeIsUsed = 1
            WHERE userRestoreCode = @code
              AND userRestoreCodeUserUID IN (SELECT userUID FROM user WHERE userEmail = @email AND userIsoCountry = @isoCountry)
              AND userRestoreCodeIsUsed = 0";

        using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@code", restoreCode);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@isoCountry", isoCountry);

        command.ExecuteNonQuery();
    }
}
