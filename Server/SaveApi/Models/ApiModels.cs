using System.Text.Json;

namespace SaveApi.Models;

public sealed record RegisterRequest(string Login, string Password, string Nome);
public sealed record LoginRequest(string Login, string Password);
public sealed record AuthResponse(string AccessToken, string Login, string Nome);
public sealed record ApiError(string Message);

public sealed record UserAccount
{
    public long Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string PasswordSalt { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}

public sealed record UserConfig
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public float VolumeMaster { get; init; }
    public float VolumeMusic { get; init; }
    public float VolumeSfx { get; init; }
    public string KeybindsJson { get; init; } = "{}";
    public DateTime UpdatedAtUtc { get; init; }
}

public sealed record SaveSlotRecord
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public int SlotIndex { get; init; }
    public string SlotName { get; init; } = string.Empty;
    public string SelectedCharacter { get; init; } = string.Empty;
    public bool PlayTutorial { get; init; }
    public string Difficulty { get; init; } = string.Empty;
    public string SceneName { get; init; } = string.Empty;
    public string CheckpointId { get; init; } = string.Empty;
    public float CheckpointX { get; init; }
    public float CheckpointY { get; init; }
    public float CheckpointZ { get; init; }
    public string CollectedIdsJson { get; init; } = "[]";
    public string DeadEnemyIdsJson { get; init; } = "[]";
    public float CompletionPercent { get; init; }
    public DateTime LastSavedAtUtc { get; init; }
}

public sealed record UserConfigResponse
{
    public float VolumeMaster { get; init; }
    public float VolumeMusic { get; init; }
    public float VolumeSfx { get; init; }
    public JsonElement Keybinds { get; init; }
}

public sealed record UserConfigUpsertRequest(
    float VolumeMaster,
    float VolumeMusic,
    float VolumeSfx,
    JsonElement Keybinds
);

public sealed record SaveUpsertRequest(
    int SlotIndex,
    string SlotName,
    string SelectedCharacter,
    bool PlayTutorial,
    string Difficulty,
    string SceneName,
    string CheckpointId,
    float CheckpointX,
    float CheckpointY,
    float CheckpointZ,
    JsonElement CollectedIds,
    JsonElement DeadEnemyIds,
    float CompletionPercent
);
