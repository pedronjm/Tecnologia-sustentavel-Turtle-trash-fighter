using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lixcirc : MonoBehaviour
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
        aux = GameControler.instance.qttcircuito;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (GameControler.instance.qttcircuito > 0 && GameControler.instance.rstcircuito > 0)
            {
                GameControler.instance.audioSource.PlayOneShot(GameControler.instance.jogarfora);
                GameControler.instance.qttcircuito -= aux;
                GameControler.instance.rstcircuito -= aux;

            }
        }
    }
}
