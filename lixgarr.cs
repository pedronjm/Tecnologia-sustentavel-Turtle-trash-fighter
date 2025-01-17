using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lixGarrf : MonoBehaviour
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
        aux = GameControler.instance.qttgarrafa;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        
        if (col.gameObject.tag == "Player")
        {
            if (GameControler.instance.qttgarrafa > 0 && GameControler.instance.rstgarrafa > 0)
            {
                GameControler.instance.audioSource.PlayOneShot(GameControler.instance.jogarfora);
                GameControler.instance.qttgarrafa -= aux;
                GameControler.instance.rstgarrafa -= aux;

            }
        }
    }
}
