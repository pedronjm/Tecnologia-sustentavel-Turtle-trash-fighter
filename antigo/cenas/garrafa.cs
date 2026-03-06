using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class garrafa : MonoBehaviour
{
      public int score; 
      private SpriteRenderer sr;
     private CircleCollider2D circle;
     private float aux = 0;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        circle = GetComponent<CircleCollider2D>();
    }

  
     void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player" && aux == 0)
        {
             aux = 1;
              sr.enabled = false;
              circle.enabled = false;

            GameControler.instance.qttgarrafa += score;
            GameControler.instance.audioSource.PlayOneShot(GameControler.instance.coletar);
            Destroy(gameObject, 0.25f);
        }
    }
}
