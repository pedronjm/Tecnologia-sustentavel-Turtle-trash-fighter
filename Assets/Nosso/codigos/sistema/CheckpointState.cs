using UnityEngine;

public class CheckpointState : MonoBehaviour
{
    public static CheckpointState instance { get; private set; }

    public string CurrentCheckpointId { get; private set; } = string.Empty;
    public Vector3 LastCheckpointPosition { get; private set; } = Vector3.zero;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCheckpoint(string checkpointId, Vector3 position)
    {
        if (string.IsNullOrEmpty(checkpointId))
            return;

        CurrentCheckpointId = checkpointId;
        LastCheckpointPosition = position;
    }

    public void Restaurar(string checkpointId, Vector3 position)
    {
        CurrentCheckpointId = checkpointId ?? string.Empty;
        LastCheckpointPosition = position;
    }

    public bool HasCheckpoint()
    {
        return !string.IsNullOrEmpty(CurrentCheckpointId);
    }
}
