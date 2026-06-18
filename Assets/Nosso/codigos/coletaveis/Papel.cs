public class Papel : Coletavel
{
    protected override void OnCollected()
    {
        if (GameControler.instance != null)
            GameControler.instance.qttPapel += score;
    }
}
