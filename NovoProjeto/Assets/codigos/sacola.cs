using System.Collections;
using UnityEngine;

public class sacola : MonoBehaviour
{
    private Rigidbody2D rig;
    private Animator anim;

    [Header("Configurações de Movimento")]
    public float speed = 2f;
    public LayerMask collisionLayer;
    public Transform groundCheck; // Use apenas um ponto na frente para detectar abismos
    public Transform wallCheck; // Use um ponto na frente para detectar paredes

    private bool isDead = false;
    private float swapCooldown = 0.5f;
    private float lastSwapTime;

   /*  [Header("Áudio")]
    public AudioSource audioSource;
    public AudioClip hitClip; // Som dele batendo no player
    public AudioClip dieClip; // Som dele morrendo */

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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

    // Chamado pelo script Damageable quando o player ataca
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        speed = 0;
        rig.linearVelocity = Vector2.zero;

        anim.SetTrigger("dano"); // Use Trigger para morte/dano
      /*   if (audioSource && dieClip)
            audioSource.PlayOneShot(dieClip); */

        GetComponent<Collider2D>().enabled = false; // Desativa colisão para não dar dano após morto
        Destroy(gameObject, 0.75f);
    }

    public void ApplyKnockback(Vector2 attackPos)
    {
        // Empurra o inimigo levemente para trás ao receber dano melee
        float side = transform.position.x < attackPos.x ? -1f : 1f;
        rig.AddForce(new Vector2(side * 5f, 2f), ForceMode2D.Impulse);
    }

    // Dano ao encostar no player (Melee do Inimigo)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {/* 
            if (audioSource && hitClip)
                audioSource.PlayOneShot(hitClip);
 */
            // Chama a lógica de dano do controlador de jogo
            GameControler.instance.health--;

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
