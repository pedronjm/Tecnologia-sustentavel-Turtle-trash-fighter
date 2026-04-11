using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Tooltip("ID unico do checkpoint para save remoto.")]
    [SerializeField] private string checkpointId;

    private string GetCheckpointId()
    {
        if (!string.IsNullOrEmpty(checkpointId))
            return checkpointId;

        var p = transform.position;
        return $"{gameObject.scene.name}:{gameObject.name}:{Mathf.RoundToInt(p.x * 100f)}:{Mathf.RoundToInt(p.y * 100f)}";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CheckpointState.instance == null)
            return;

        CheckpointState.instance.SetCheckpoint(GetCheckpointId(), transform.position);
        Debug.Log($"Checkpoint ativado: {CheckpointState.instance.CurrentCheckpointId}");
    }
}
