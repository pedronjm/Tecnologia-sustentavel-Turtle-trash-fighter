using System.Collections;
using UnityEngine;

public class sacola : enemy
{
    public LayerMask collisionLayer;
    public Transform groundCheck; // Use apenas um ponto na frente para detectar abismos
    public Transform wallCheck; // Use um ponto na frente para detectar paredes

    private float swapCooldown = 0.5f;
    private float lastSwapTime;

    /*  [Header("Áudio")]
     public AudioSource audioSource;
     public AudioClip hitClip; // Som dele batendo no player
     public AudioClip dieClip; // Som dele morrendo */

    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
        if (isDead)
            return;

        Move();
        CheckObstacles();
    }

    void Move()
    {
        rig.linearVelocity = new Vector2(speed, rig.linearVelocity.y);
    }

    void CheckObstacles()
    {
        // Detecta parede na frente
        bool wallHit = Physics2D.Raycast(
            wallCheck.position,
            transform.right * Mathf.Sign(speed),
            0.2f,
            collisionLayer
        );

        // Detecta se há chão na frente (para não cair de plataformas)
        bool groundAhead = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            0.5f,
            collisionLayer
        );

        if ((wallHit || !groundAhead) && Time.time > lastSwapTime + swapCooldown)
        {
            SwapDirection();
        }
    }

    void SwapDirection()
    {
        speed = -speed;
        transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
        lastSwapTime = Time.time;
    }

    // Dano ao encostar no player (Melee do Inimigo)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            // Tenta usar o sistema de vida genérico (Damageable) do player
            Damageable dmg = collision.gameObject.GetComponent<Damageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(1, transform.position);
            }
            else if (GameControler.instance != null)
            {
                // Fallback para lógica antiga, se ainda existir GameControler
                GameControler.instance.health--;
            }

            // Inicia a animação de hit no player
            StartCoroutine(PlayPlayerHitAnimation());
        }
    }

    IEnumerator PlayPlayerHitAnimation()
    {
        if (Player.player != null)
        {
            Player.player.anim.SetBool("hitp", true);
            yield return new WaitForSeconds(0.5f);
            Player.player.anim.SetBool("hitp", false);
        }
    }
}
