using Microsoft.Data.Sqlite;
using AuthMicroservice.Core.Interfaces;

namespace AuthMicroservice.Infrastructure;

public class DbInitializer : IDbInitializer
{
    private readonly string _connectionString;
    private readonly string _dataSource;

    public DbInitializer(ISettings settings)
    {
        _connectionString = settings.ConnectionString;
        _dataSource = ParseDataSource(_connectionString);
    }

    public void Initialize()
    {
        var directory = Path.GetDirectoryName(_dataSource);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(_dataSource)) return;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS userStatus (
                userStatusId INTEGER PRIMARY KEY AUTOINCREMENT,
                userStatusName TEXT NOT NULL,
                userStatusDescription TEXT NOT NULL
            );

            INSERT OR IGNORE INTO userStatus (userStatusId, userStatusName, userStatusDescription) VALUES (1, 'ACTIVE', 'Full access');
            INSERT OR IGNORE INTO userStatus (userStatusId, userStatusName, userStatusDescription) VALUES (2, 'LOCKED', 'Dev intervention required');
            INSERT OR IGNORE INTO userStatus (userStatusId, userStatusName, userStatusDescription) VALUES (3, 'DELETED', 'Irreversible');

            CREATE TABLE IF NOT EXISTS user (
                userUID TEXT PRIMARY KEY,
                userFirstName TEXT NOT NULL,
                userLastName TEXT NOT NULL,
                userName TEXT,
                userEmail TEXT,
                userPassword TEXT NOT NULL,
                userIsoCountry TEXT NOT NULL,
                userStatusId INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS userSession (
                userSessionUID TEXT PRIMARY KEY,
                userSessionUserUID TEXT NOT NULL,
                userSessionRefreshToken TEXT,
                userSessionExpiresDate TEXT
            );

            CREATE TABLE IF NOT EXISTS userRestoreCode (
                userRestoreCodeId INTEGER PRIMARY KEY AUTOINCREMENT,
                userRestoreCode TEXT NOT NULL,
                userRestoreCodeUserUID TEXT NOT NULL,
                userRestoreCodeIsUsed INTEGER NOT NULL DEFAULT 0,
                userRestoreCodeDateCreated TEXT NOT NULL
            );";

        command.ExecuteNonQuery();
    }

    private static string ParseDataSource(string connectionString)
    {
        var parts = connectionString.Split(';')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2)
            .FirstOrDefault(p => p[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase));

        return parts?[1].Trim() ?? "auth.db";
    }
}
