using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Movimentação
    public float Speed;
    public float JumpForce;
    public bool isJumping;
    public bool doubleJump;
    public float auxDashCooldown = 5f; // Tempo de recarga do Dash
    private float dashCooldown; // Temporizador do cooldown
    public float dashforce; // Força aplicada no Dash
    private bool isDashing; // Controla se o jogador está no Dash
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    public bool isWallSliding;

    [Header("Combat")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    public float attackCooldown = 0.3f;
    private float attackTimer;

    public LayerMask enemyLayer;

    // // Áudio
    // public AudioSource audioSource;
    // public AudioClip walkinggrass;
    // public AudioClip runningstone;
    // public AudioClip jumpSound;
    // public AudioClip dashSound;
    // public AudioClip deathSound;

    // Componentes
    private Rigidbody2D rig;
    public Animator anim;
    private SpriteRenderer sprite;
    private GameControler gameControl;

    // Variáveis globais para rastreamento
    public static Player player;

    // Contadores de ações (tutorial)
    public float drt,
        esquerda,
        pulo,
        pulodlp,
        dsh;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        player = this;
        gameControl = GameControler.instance;
    }

    void Update()
    { // if (GameControler.instance.inaux != 0 || GameControler.instance.fase != 1){
        Move(); // Movimentação do jogador
        Jump(); // Pulo do jogador
        Dash(); // Dash do jogador
        //  }
        Attack();
        WallSlide();
        CheckFall();
    }
    void CheckFall()
    {
        // Se a velocidade vertical for negativa e não estiver no chão nem na parede
        if (rig.linearVelocity.y < -0.1f && isJumping && !isWallSliding)
        {
            anim.SetBool("fall", true);
            anim.SetBool("jump", false); // Desliga o pulo para entrar na queda
            anim.SetBool("doublejump", false);
        }
        else
        {
            anim.SetBool("fall", false);
        }
    }

    void Attack()
    {
        attackTimer -= Time.deltaTime;

        if (Mouse.current.leftButton.wasPressedThisFrame && attackTimer <= 0)
        {
            anim.SetTrigger("attack");

            Collider2D[] enemies = Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRange,
                enemyLayer
            );

            foreach (Collider2D enemy in enemies)
            {
                enemy.GetComponent<Damageable>()?.TakeDamage(attackDamage, transform.position);
            }

            attackTimer = attackCooldown;
        }
    }

    void Move()
    {
        float horizontal = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;
        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;

        Vector3 movement = new Vector3(horizontal, 0f, 0f);
        transform.position += movement * Time.deltaTime * Speed;

        if (horizontal > 0f)
        {
            anim.SetBool("walk", true);
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
            drt += Time.deltaTime;
        }
        else if (horizontal < 0f)
        {
            anim.SetBool("walk", true);
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
            esquerda += Time.deltaTime;
        }
        else
        {
            anim.SetBool("walk", false);
        }
    }

    // void PlayWalkingSound()
    // {
    //     if (!audioSource.isPlaying && !isJumping)
    //     {
    //         if (GameControler.instance.fase == 1 || GameControler.instance.fase == 3)
    //         {
    //             audioSource.clip = walkinggrass;
    //         }
    //         else
    //         {
    //             audioSource.clip = runningstone;
    //         }
    //         audioSource.loop = true;
    //         audioSource.Play();
    //     }
    // }

    // void StopWalkingSound()
    // {
    //     if (audioSource.isPlaying )
    //     {
    //         audioSource.loop = false;
    //         audioSource.Stop();
    //     }
    // }

    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isJumping)
            {
                rig.linearVelocity = new Vector2(rig.linearVelocity.x, JumpForce);
                doubleJump = true;
                anim.SetBool("jump", true);
                pulo++;
            }
            else if (doubleJump)
            {
                rig.linearVelocity = new Vector2(rig.linearVelocity.x, JumpForce);
                doubleJump = false;
                pulodlp++;

                // Desliga o pulo normal e liga o duplo
                anim.SetBool("jump", false);
                anim.SetBool("doublejump", true);

                StopCoroutine(jumpanima()); // Para a anterior se você pulou muito rápido
                StartCoroutine(jumpanima());
            }
        }
    }

    // void PlayJumpSound()
    // {
    //     // Reproduz o som de pulo sem interferir nos sons de caminhada ou Dash
    //     if (audioSource.clip != jumpSound || !audioSource.isPlaying)
    //     {
    //         audioSource.Stop(); // Interrompe o som atual para evitar sobreposição
    //         audioSource.clip = jumpSound;
    //         audioSource.loop = false; // Som de pulo não deve ser em loop
    //         audioSource.Play(); // Reproduz o som de pulo
    //     }else if (audioSource.clip == jumpSound){
    //          audioSource.Play(); // Reproduz o som de pulo
    //     }

    // }

    void Dash()
    {
        if (dashCooldown > 0)
        {
            dashCooldown -= Time.deltaTime;
            return;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && !isDashing)
        {
            // audioSource.PlayOneShot(dashSound);
            dsh++; // Incrementa o contador de Dash
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldown = auxDashCooldown;

        float dashDirection = transform.localScale.x > 0 ? 1 : -1;

        rig.linearVelocity = new Vector2(dashDirection * dashforce, 0f);

        anim.SetBool("dash", true);

        yield return new WaitForSeconds(0.2f);

        anim.SetBool("dash", false);

        rig.linearVelocity = new Vector2(0f, rig.linearVelocity.y);

        isDashing = false;
    }

    IEnumerator jumpanima()
    {
        yield return new WaitForSeconds(0.5f);

        // Só desliga se ainda estiver no ar, caso contrário o OnCollision já cuidou disso
        if (isJumping)
        { 
            anim.SetBool("doublejump", false);
        }
    }

    void WallSlide()
    {
        float direction = transform.localScale.x > 0 ? 1 : -1;
        Vector2 rayDirection = new Vector2(direction, 0);
        float raioComprimento = 0.8f;

        // Raio começa um pouco fora do centro para não bater no próprio corpo
        Vector2 startingPoint = new Vector2(
            transform.position.x + (direction * 0.1f),
            transform.position.y
        );

        RaycastHit2D wallCheck = Physics2D.Raycast(
            startingPoint,
            rayDirection,
            raioComprimento,
            wallLayer
        );
        Debug.DrawRay(startingPoint, rayDirection * raioComprimento, Color.yellow);

        // Se o raio bateu em algo que não é o próprio player
        if (wallCheck.collider != null && wallCheck.collider.gameObject != gameObject)
        {
            // Se estiver caindo e encostado na parede
            if (rig.linearVelocity.y < 0)
            {
                isWallSliding = true;
                rig.linearVelocity = new Vector2(rig.linearVelocity.x, -wallSlideSpeed);
                anim.SetBool("wall", true);
            }
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("wall", false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isJumping = false;
            doubleJump = false;
            isWallSliding = false;

            // Reset de todos os parâmetros, incluindo o novo "fall"
            anim.SetBool("jump", false);
            anim.SetBool("doublejump", false);
            anim.SetBool("wall", false);
            anim.SetBool("fall", false); // Garante que parou de cair ao tocar o chão
        }
    

        /* else if (collision.gameObject.CompareTag("Spike"))
        {
            gameControl.health = 0;
        } */
        if (collision.gameObject.CompareTag("Finish"))
        {
            //if (gameControl.rstgarrafa == 0 && gameControl.rstengrenagem == 0 && gameControl.rstmaca == 0 && gameControl.rstcircuito == 0 )

            //      gameControl.PassarDeFase();

            // else
            //     gameControl.NotCollected();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isJumping = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
