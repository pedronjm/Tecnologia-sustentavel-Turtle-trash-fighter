using System.Text.Json;
using MySqlConnector;
using SaveApi.Data;
using SaveApi.Models;

namespace SaveApi.Repositories;

public sealed class MySqlSaveRepository : ISaveRepository
{
    private readonly MySqlDb _db;

    public MySqlSaveRepository(MySqlDb db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SaveSlotRecord>> GetAllAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, user_id, slot_index, slot_name, selected_character, play_tutorial, difficulty, scene_name,
       checkpoint_id, checkpoint_x, checkpoint_y, checkpoint_z, collected_ids_json, dead_enemy_ids_json,
       completion_percent, last_saved_at_utc
FROM saves
WHERE user_id = @user_id
ORDER BY slot_index;";

        var results = new List<SaveSlotRecord>();

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user_id", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            results.Add(MapSave(reader));

        return results;
    }

    public async Task<SaveSlotRecord?> GetAsync(long userId, int slotIndex, CancellationToken cancellationToken = default)
    {
        const string sql = @"
SELECT id, user_id, slot_index, slot_name, selected_character, play_tutorial, difficulty, scene_name,
       checkpoint_id, checkpoint_x, checkpoint_y, checkpoint_z, collected_ids_json, dead_enemy_ids_json,
       completion_percent, last_saved_at_utc
FROM saves
WHERE user_id = @user_id AND slot_index = @slot_index
LIMIT 1;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@slot_index", slotIndex);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapSave(reader);
    }

    public async Task<SaveSlotRecord> UpsertAsync(long userId, SaveUpsertRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO saves (
    user_id, slot_index, slot_name, selected_character, play_tutorial, difficulty, scene_name,
    checkpoint_id, checkpoint_x, checkpoint_y, checkpoint_z, collected_ids_json, dead_enemy_ids_json,
    completion_percent, last_saved_at_utc
)
VALUES (
    @user_id, @slot_index, @slot_name, @selected_character, @play_tutorial, @difficulty, @scene_name,
    @checkpoint_id, @checkpoint_x, @checkpoint_y, @checkpoint_z, @collected_ids_json, @dead_enemy_ids_json,
    @completion_percent, UTC_TIMESTAMP()
)
ON DUPLICATE KEY UPDATE
    slot_name = VALUES(slot_name),
    selected_character = VALUES(selected_character),
    play_tutorial = VALUES(play_tutorial),
    difficulty = VALUES(difficulty),
    scene_name = VALUES(scene_name),
    checkpoint_id = VALUES(checkpoint_id),
    checkpoint_x = VALUES(checkpoint_x),
    checkpoint_y = VALUES(checkpoint_y),
    checkpoint_z = VALUES(checkpoint_z),
    collected_ids_json = VALUES(collected_ids_json),
    dead_enemy_ids_json = VALUES(dead_enemy_ids_json),
    completion_percent = VALUES(completion_percent),
    last_saved_at_utc = UTC_TIMESTAMP();

SELECT id, user_id, slot_index, slot_name, selected_character, play_tutorial, difficulty, scene_name,
       checkpoint_id, checkpoint_x, checkpoint_y, checkpoint_z, collected_ids_json, dead_enemy_ids_json,
       completion_percent, last_saved_at_utc
FROM saves
WHERE user_id = @user_id AND slot_index = @slot_index
LIMIT 1;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@slot_index", request.SlotIndex);
        command.Parameters.AddWithValue("@slot_name", request.SlotName);
        command.Parameters.AddWithValue("@selected_character", request.SelectedCharacter);
        command.Parameters.AddWithValue("@play_tutorial", request.PlayTutorial);
        command.Parameters.AddWithValue("@difficulty", request.Difficulty);
        command.Parameters.AddWithValue("@scene_name", request.SceneName);
        command.Parameters.AddWithValue("@checkpoint_id", request.CheckpointId);
        command.Parameters.AddWithValue("@checkpoint_x", request.CheckpointX);
        command.Parameters.AddWithValue("@checkpoint_y", request.CheckpointY);
        command.Parameters.AddWithValue("@checkpoint_z", request.CheckpointZ);
        command.Parameters.AddWithValue("@collected_ids_json", request.CollectedIds.GetRawText());
        command.Parameters.AddWithValue("@dead_enemy_ids_json", request.DeadEnemyIds.GetRawText());
        command.Parameters.AddWithValue("@completion_percent", request.CompletionPercent);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return (await GetAsync(userId, request.SlotIndex, cancellationToken))!;
    }

    public async Task<bool> DeleteAsync(long userId, int slotIndex, CancellationToken cancellationToken = default)
    {
        const string sql = @"
DELETE FROM saves
WHERE user_id = @user_id AND slot_index = @slot_index;";

        await using var connection = _db.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithValue("@slot_index", slotIndex);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    private static SaveSlotRecord MapSave(MySqlDataReader reader)
    {
        return new SaveSlotRecord
        {
            Id = reader.GetInt64("id"),
            UserId = reader.GetInt64("user_id"),
            SlotIndex = reader.GetInt32("slot_index"),
            SlotName = reader.GetString("slot_name"),
            SelectedCharacter = reader.GetString("selected_character"),
            PlayTutorial = reader.GetBoolean("play_tutorial"),
            Difficulty = reader.GetString("difficulty"),
            SceneName = reader.GetString("scene_name"),
            CheckpointId = reader.GetString("checkpoint_id"),
            CheckpointX = reader.GetFloat("checkpoint_x"),
            CheckpointY = reader.GetFloat("checkpoint_y"),
            CheckpointZ = reader.GetFloat("checkpoint_z"),
            CollectedIdsJson = reader.GetString("collected_ids_json"),
            DeadEnemyIdsJson = reader.GetString("dead_enemy_ids_json"),
            CompletionPercent = reader.GetFloat("completion_percent"),
            LastSavedAtUtc = reader.GetDateTime("last_saved_at_utc"),
        };
    }
}
