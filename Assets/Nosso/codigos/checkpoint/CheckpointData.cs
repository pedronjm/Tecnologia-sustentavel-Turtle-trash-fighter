using UnityEngine;

public class CheckpointData : MonoBehaviour
{
    public string checkpointId;

    public Vector3 Position
    {
        get { return transform.position; }
    }
}
