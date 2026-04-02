using UnityEngine;

public class garrafa : Coletavel
{
    protected override void OnCollected()
    {
        if (GameControler.instance != null)
            GameControler.instance.qttgarrafa += score;
    }
}
