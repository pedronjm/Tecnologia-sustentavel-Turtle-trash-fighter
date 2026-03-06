using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sacola : MonoBehaviour
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
        // Movimento contínuo na direção atual
        rig.velocity = new Vector2(speed, rig.velocity.y);

        // Verificar colisão com chão usando OverlapArea (mais confiável)
        chao = Physics2D.OverlapArea(under1.position, under2.position, collisionLayer) != null;

        // Verificar colisão com paredes
        RaycastHit2D hit = Physics2D.Linecast(rightcol.position, leftcol.position, collisionLayer);
        colliding = hit.collider != null && hit.collider.CompareTag("Ground");

        // Depuração
        Debug.DrawLine(rightcol.position, leftcol.position, Color.red);
        Debug.DrawLine(under1.position, under2.position, Color.blue);
        Debug.Log($"Chao: {chao}, Colliding: {colliding}");

        // Trocar de direção apenas quando necessário
        if ((!wasOnGround && !chao) || (!wasColliding && colliding))
        {
            if (Time.time > lastSwapTime + swapCooldown)
            {
                swap();
                lastSwapTime = Time.time;
                Debug.Log($"Troca de direção: Chao: {chao}, Colliding: {colliding}");
            }
        }

        // Atualizar estados anteriores
        wasOnGround = chao;
        wasColliding = colliding;

        // Verificar colisão com o jogador
        RaycastHit2D playerHit = Physics2D.Linecast(playerCheck1.position, playerCheck2.position, collisionLayer);
        if (playerHit.collider != null && playerHit.collider.CompareTag("Player") && Time.time > hittime + swapCooldown || playerHit.collider.CompareTag("Spike"))
        {   if(!audioSource.isPlaying){
              audioSource.PlayOneShot(reciveClip);
            }
           Destroyitself(playerHit.collider.gameObject);
        }

        if (hit.collider != null && hit.collider.CompareTag("Player") && Time.time > givehittime + swapCooldown)
        {
            hitPlayer(hit.collider.gameObject);
            givehittime = Time.time + 1f;
        }
    }

    void hitPlayer(GameObject player)
    {
        // Adicionar lógica para destruir ou danificar o jogador
        StartCoroutine(PlayHitAnimation());
        if(!audioSource.isPlaying){
        audioSource.PlayOneShot(hitClip);}
        Debug.Log("Jogador recebeu dano!");
        GameControler.instance.health--;
    }

    void Destroyitself(GameObject player)
    {
        // Adicionar lógica para destruir o inimigo
        Debug.Log("Inimigo destruído!");
        anim.SetBool("dano", true);
        speed = 0;
        Destroy(gameObject, 0.75f);
    }

    void swap()
    {
        transform.localScale = new Vector2(transform.localScale.x * -1f, transform.localScale.y);
        speed = -speed;
    }
     IEnumerator PlayHitAnimation()
    {   
        if (GameControler.instance.health > 0)
        // Ativa a animação de impacto
        Player.player.anim.SetBool("hitp", true); // Certifique-se de que o Animator tem o parâmetro "hit"
        Debug.Log("Tocou em um inimigo! Animação ativada.");
        // Aguarda 5 segundos
        yield return new WaitForSeconds(0.75f);
        // Desativa a animação de impacto
       Player.player.anim.SetBool("hitp", false);
        Debug.Log("Animação de impacto desativada.");
    }
    
}
