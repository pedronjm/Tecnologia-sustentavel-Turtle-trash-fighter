using System.Collections.Generic;
using UnityEngine;


public class CheckpointState : MonoBehaviour
{

    public static CheckpointState instance { get; private set; }


    public string CurrentCheckpointId { get; private set; } = string.Empty;


    [Header("Checkpoints da fase")]
    [SerializeField]
    private List<CheckpointData> checkpoints = new List<CheckpointData>();



    void Awake()
    {

        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }


        instance = this;

        DontDestroyOnLoad(gameObject);

    }





    public void SetCheckpoint(string checkpointId)
    {

        if(string.IsNullOrEmpty(checkpointId))
            return;


        CurrentCheckpointId = checkpointId;

    }





    public Vector3 GetCheckpointPosition()
    {

        foreach(CheckpointData checkpoint in checkpoints)
        {

            if(checkpoint != null &&
               checkpoint.checkpointId == CurrentCheckpointId)
            {

                return checkpoint.transform.position;

            }

        }


        return Vector3.zero;

    }





    public void Restaurar(string checkpointId)
    {

        CurrentCheckpointId = checkpointId ?? string.Empty;

    }





    public bool HasCheckpoint()
    {

        return !string.IsNullOrEmpty(CurrentCheckpointId);

    }

}