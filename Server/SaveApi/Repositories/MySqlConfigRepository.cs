using System.Text.Json;
using MySqlConnector;
using SaveApi.Data;
using SaveApi.Models;

namespace SaveApi.Repositories;

public sealed class MySqlConfigRepository : IConfigRepository
{
    private readonly MySqlDb _db;

    public MySqlConfigRepository(MySqlDb db)
    {
        _db = db;
    }

    public async Task<UserConfig?> GetAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, user_id, volume_master, volume_music, volume_sfx, keybinds_json, updated_at_utc
FROM user_configs
WHERE user_id = @user_id
LIMIT 1;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user_id", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new UserConfig
        {
            Id = reader.GetInt64("id"),
            UserId = reader.GetInt64("user_id"),
            VolumeMaster = reader.GetFloat("volume_master"),
            VolumeMusic = reader.GetFloat("volume_music"),
            VolumeSfx = reader.GetFloat("volume_sfx"),
            KeybindsJson = reader.GetString("keybinds_json"),
            UpdatedAtUtc = reader.GetDateTime("updated_at_utc"),
        };
    }

    public async Task<UserConfig> UpsertAsync(long userId, UserConfigUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO user_configs (user_id, volume_master, volume_music, volume_sfx, keybinds_json, updated_at_utc)
VALUES (@user_id, @volume_master, @volume_music, @volume_sfx, @keybinds_json, UTC_TIMESTAMP())
ON DUPLICATE KEY UPDATE
    volume_master = VALUES(volume_master),
    volume_music = VALUES(volume_music),
    volume_sfx = VALUES(volume_sfx),
    keybinds_json = VALUES(keybinds_json),
    updated_at_utc = UTC_TIMESTAMP();

SELECT id, user_id, volume_master, volume_music, volume_sfx, keybinds_json, updated_at_utc
FROM user_configs
WHERE user_id = @user_id
LIMIT 1;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@volume_master", request.VolumeMaster);
        command.Parameters.AddWithValue("@volume_music", request.VolumeMusic);
        command.Parameters.AddWithValue("@volume_sfx", request.VolumeSfx);
        command.Parameters.AddWithValue("@keybinds_json", request.Keybinds.GetRawText());

        await command.ExecuteNonQueryAsync(cancellationToken);

        return (await GetAsync(userId, cancellationToken))!;
    }
}
