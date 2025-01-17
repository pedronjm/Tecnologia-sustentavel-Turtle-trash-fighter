using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lixmaca : MonoBehaviour
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
       aux = GameControler.instance.qttmaca;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (GameControler.instance.qttmaca > 0 && GameControler.instance.rstmaca > 0 )
            {
                GameControler.instance.audioSource.PlayOneShot(GameControler.instance.jogarfora);
                GameControler.instance.qttmaca -= aux;
                GameControler.instance.rstmaca -= aux;
              
           }
        }
    }
}