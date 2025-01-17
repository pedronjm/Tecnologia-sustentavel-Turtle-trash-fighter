using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lixengre : MonoBehaviour
{

    private float aux;

    public int score;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        aux = GameControler.instance.qttengrenagem;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (GameControler.instance.qttengrenagem > 0 && GameControler.instance.rstengrenagem > 0)

            {
                GameControler.instance.audioSource.PlayOneShot(GameControler.instance.jogarfora);
                GameControler.instance.qttengrenagem -= aux;
                GameControler.instance.rstengrenagem -= aux;

            }
        }
    }
}