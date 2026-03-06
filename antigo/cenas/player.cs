using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Movimentação
    public float Speed;
    public float JumpForce;
    public bool isJumping;
    public bool doubleJump;
    public float auxDashCooldown = 5f; // Tempo de recarga do Dash
    private float dashCooldown;       // Temporizador do cooldown
    public float dashforce;           // Força aplicada no Dash
    private bool isDashing;           // Controla se o jogador está no Dash

    // Áudio
    public AudioSource audioSource;
    public AudioClip walkinggrass;
    public AudioClip runningstone;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip deathSound;

    // Componentes
    private Rigidbody2D rig;
    public Animator anim;
    private SpriteRenderer sprite;
    private GameControler gameControl;

    // Variáveis globais para rastreamento
    public static Player player;

    // Contadores de ações (tutorial)
    public float drt, esquerda, pulo, pulodlp, dsh;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        player = this;
        gameControl = GameControler.instance;
    }

    void Update()
    {   if (GameControler.instance.inaux != 0 || GameControler.instance.fase != 1){
        Move();  // Movimentação do jogador
        Jump();  // Pulo do jogador
        Dash();  // Dash do jogador
        }
    }

    void Move()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        transform.position += movement * Time.deltaTime * Speed;

        if (movement.x > 0f)
        {
            anim.SetBool("walk", true);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            drt += Time.deltaTime; // Incrementa o contador com base no tempo
            PlayWalkingSound();
        }
        else if (movement.x < 0f)
        {
            anim.SetBool("walk", true);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            esquerda += Time.deltaTime;
            PlayWalkingSound();
        }
        else
        {
            anim.SetBool("walk", false);
            StopWalkingSound();
        }
    }

    void PlayWalkingSound()
    {
        if (!audioSource.isPlaying && !isJumping)
        {
            if (GameControler.instance.fase == 1 || GameControler.instance.fase == 3)
            {
                audioSource.clip = walkinggrass;
            }
            else
            {
                audioSource.clip = runningstone;
            }
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void StopWalkingSound()
    {
        if (audioSource.isPlaying )
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (!isJumping)
            {
                rig.velocity = new Vector2(rig.velocity.x, JumpForce);
                doubleJump = true;
                anim.SetBool("jump", true);
                pulo++;
                PlayJumpSound();
            }
            else if (doubleJump)
            {
                
                rig.velocity = new Vector2(rig.velocity.x, JumpForce);
                doubleJump = false;
                pulodlp++;
                anim.SetBool("jump", true);
                
                PlayJumpSound();
                StartCoroutine(jumpanima());
            }
        }
    }

    void PlayJumpSound()
    {
        // Reproduz o som de pulo sem interferir nos sons de caminhada ou Dash
        if (audioSource.clip != jumpSound || !audioSource.isPlaying)
        {
            audioSource.Stop(); // Interrompe o som atual para evitar sobreposição
            audioSource.clip = jumpSound;
            audioSource.loop = false; // Som de pulo não deve ser em loop
            audioSource.Play(); // Reproduz o som de pulo
        }else if (audioSource.clip == jumpSound){
             audioSource.Play(); // Reproduz o som de pulo
        }

    }

    void Dash()
    {
        if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
            return;
        }

        if (Input.GetButtonDown("dash") && !isDashing)
        {
            audioSource.PlayOneShot(dashSound);
            dsh++; // Incrementa o contador de Dash
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldown = auxDashCooldown;

        float dashDirection = transform.localScale.x > 0 ? 1 : -1;

        rig.velocity = new Vector2(dashDirection * dashforce, 0f);

        anim.SetBool("dasht", true);
        sprite.flipX = true;

        yield return new WaitForSeconds(0.2f);

        sprite.flipX = false;
        anim.SetBool("dasht", false);

        rig.velocity = new Vector2(0f, rig.velocity.y);

        isDashing = false;
    }
    IEnumerator jumpanima()
    {
       
        

         anim.SetBool("duoublejump", true);
        

        yield return new WaitForSeconds(0.5f);

        
         anim.SetBool("duoublejump", false);

       
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isJumping = false;
            anim.SetBool("jump", false);
            anim.SetBool("duoublejump", false);
        }
        else if (collision.gameObject.CompareTag("Spike"))
        {
            gameControl.health = 0;
            
        }
        if (collision.gameObject.CompareTag("Finish"))
        {
            if (gameControl.rstgarrafa == 0 && gameControl.rstengrenagem == 0 && gameControl.rstmaca == 0 && gameControl.rstcircuito == 0 )

                gameControl.PassarDeFase();
                
            else
                gameControl.NotCollected();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isJumping = true;
        }
    }
}
