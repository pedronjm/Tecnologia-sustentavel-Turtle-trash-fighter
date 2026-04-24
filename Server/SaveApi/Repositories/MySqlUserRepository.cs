using System.Data;
using MySqlConnector;
using SaveApi.Data;
using SaveApi.Models;

namespace SaveApi.Repositories;

public sealed class MySqlUserRepository : IUserRepository
{
    private readonly MySqlDb _db;

    public MySqlUserRepository(MySqlDb db)
    {
        _db = db;
    }

    public async Task<UserAccount?> GetByLoginAsync(
        string login,
        CancellationToken cancellationToken = default
    )
    {
        const string sql =
            @"
SELECT id, login, password_hash, password_salt, nome, created_at_utc
FROM users
WHERE login = @login
LIMIT 1;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@login", login);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapUser(reader);
    }

    public async Task<UserAccount?> GetByIdAsync(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        const string sql =
            @"
SELECT id, login, password_hash, password_salt, nome, created_at_utc
FROM users
WHERE id = @id
LIMIT 1;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapUser(reader);
    }

    public async Task<UserAccount> CreateAsync(
        string login,
        string passwordHash,
        string passwordSalt,
        string nome,
        CancellationToken cancellationToken = default
    )
    {
        const string sql =
            @"
INSERT INTO users (login, password_hash, password_salt, nome)
VALUES (@login, @password_hash, @password_salt, @nome);

SELECT LAST_INSERT_ID();";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@password_hash", passwordHash);
        command.Parameters.AddWithValue("@password_salt", passwordSalt);
        command.Parameters.AddWithValue("@nome", nome);

        var newId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        return (await GetByIdAsync(newId, cancellationToken))!;
    }

    private static UserAccount MapUser(MySqlDataReader reader)
    {
        return new UserAccount
        {
            Id = reader.GetInt64("id"),
            Login = reader.GetString("login"),
            PasswordHash = reader.GetString("password_hash"),
            PasswordSalt = reader.GetString("password_salt"),
            Nome = reader.GetString("nome"),
            CreatedAtUtc = reader.GetDateTime("created_at_utc"),
        };
    }
}
