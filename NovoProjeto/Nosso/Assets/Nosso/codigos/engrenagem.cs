using UnityEngine;

public class engrenagem : Coletavel
{
    protected override void OnCollected()
    {
        if (GameControler.instance != null)
            GameControler.instance.qttengrenagem += score;
    }
}
