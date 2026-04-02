using UnityEngine;

public class circuito : Coletavel
{
    protected override void OnCollected()
    {
        if (GameControler.instance != null)
            GameControler.instance.qttcircuito += score;
    }
}
