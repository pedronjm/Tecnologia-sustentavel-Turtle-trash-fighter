using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vida : MonoBehaviour
{
    private SpriteRenderer sr;
    private CircleCollider2D circle;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        circle = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
   
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {

            sr.enabled = false;
            circle.enabled = false;
            if ( GameControler.instance.health < 5){
            GameControler.instance.health += 1;
            }
            GameControler.instance.audioSource.PlayOneShot(GameControler.instance.heal);
            Destroy(gameObject);
        }
    }
}
