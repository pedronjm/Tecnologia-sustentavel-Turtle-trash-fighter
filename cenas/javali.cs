using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class javali : MonoBehaviour
{
    private Rigidbody2D rig;
    private Animator anim;

    private BoxCollider2D box;
    private CircleCollider2D circ;

    public float speed;

    private float aux;

    public Transform rightcol;
    public Transform leftcol;

    public Transform under1;
    public Transform under2;
    public Transform playerCheck1;
    public Transform playerCheck2;

    public LayerMask collisionLayer; // Camadas para detectar chão e paredes

    private bool chao;
    private bool colliding;
    private bool wasOnGround;
    private bool wasColliding;
    private float swapCooldown = 0.5f; // Tempo mínimo entre trocas
    private float lastSwapTime;       // Armazena o tempo da última troca

    private float hittime;
    private float givehittime;
    //audio
    public AudioSource audioSource;
    public AudioClip reciveClip;
    public AudioClip hitClip;


    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        box = GetComponent<BoxCollider2D>();
        circ = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {


        // Impede atualizações adicionais após a destruição
        if (speed == 1)
            return;

        // Lógica existente...


        // Movimento contínuo na direção atual
        rig.velocity = new Vector2(speed, rig.velocity.y);

        // Verificar colisão com chão
        bool underHit1 = Physics2D.Linecast(under1.position, under2.position, collisionLayer).collider?.CompareTag("Ground") == true;
        chao = underHit1;

        // Verificar colisão com paredes
        RaycastHit2D hit = Physics2D.Linecast(rightcol.position, leftcol.position, collisionLayer);
        colliding = hit.collider != null && hit.collider.CompareTag("Ground");

        // Depuração
        Debug.DrawLine(rightcol.position, leftcol.position, Color.red);
        Debug.DrawLine(under1.position, under2.position, Color.blue);

        // Trocar de direção apenas quando necessário
        if ((!wasOnGround && !chao) || (!wasColliding && colliding))
        {
            if (Time.time > lastSwapTime + swapCooldown)
            {
                swap();
                lastSwapTime = Time.time;
                Debug.Log($"Chao: {chao}, Colliding: {colliding}");
            }
        }

        // Atualizar estados anteriores
        wasOnGround = chao;
        wasColliding = colliding;
        RaycastHit2D playerHit = Physics2D.Linecast(playerCheck1.position, playerCheck2.position, collisionLayer);
        if (playerHit.collider != null && playerHit.collider.CompareTag("Player") && Time.time > hittime + swapCooldown)
        {
            if (aux == 0)
            {
                StartCoroutine(PlayHit());
                aux = 1;
                speed = speed * 2;
                hittime = Time.time + 1f;
                anim.SetBool("run", true);
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(reciveClip);
                }

            }
            else
            {
                box.isTrigger = true;
                StartCoroutine(PlayHit());
                Debug.Log("Inimigo destruído!");
                Destroyitself();
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(reciveClip);
                }
            }

        }
        if (hit.collider != null && hit.collider.CompareTag("Player") && Time.time > givehittime + swapCooldown)
        {
            StartCoroutine(PlayHitAnimation());
            hitPlayer(hit.collider.gameObject);
            givehittime = Time.time + 1f;
        }

    }

    void hitPlayer(GameObject player)
    {
        // Adicionar lógica para destruir ou danificar o jogador
        Debug.Log("Jogador recebeu dano!");
        // Por exemplo, destruir o jogador:
        GameControler.instance.health--;
        audioSource.PlayOneShot(hitClip);
    }

    void Destroyitself()
    {
        // Adicionar lógica para destruir o inimigo
        Debug.Log("Inimigo destruído!");
        speed = 0;
        Destroy(gameObject, 0.12f);
    }
    void swap()
    {
        transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
        speed = -speed;
    }
    IEnumerator PlayHit()
    {
        // Ativa a animação de impacto
        anim.SetBool("hit", true); // Certifique-se de que o Animator tem o parâmetro "hit"
        Debug.Log("Tocou em um inimigo! Animação ativada.");

        // Aguarda 5 segundos
        yield return new WaitForSeconds(0.5f);
        // Desativa a animação de impacto
        anim.SetBool("hit", false);
        Debug.Log("Animação de impacto desativada.");
    }
    IEnumerator PlayHitAnimation()
    {
        // Ativa a animação de impacto
        Player.player.anim.SetBool("hitp", true); // Certifique-se de que o Animator tem o parâmetro "hit"
        Debug.Log("Tocou em um inimigo! Animação ativada.");
        // Aguarda 5 segundos
        yield return new WaitForSeconds(1f);
        // Desativa a animação de impacto

        if (GameControler.instance.health > 0)
        {
            Player.player.anim.SetBool("hitp", false);
            Debug.Log("Animação de impacto desativada.");
        }
    }
}
