using UnityEngine;

public class MaçãPode : Coletavel
{
    protected override void OnCollected()
    {
        if (GameControler.instance != null)
            GameControler.instance.qttmaca += score;
    }
}
