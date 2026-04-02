using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float Speed = 8f;
    public float JumpForce = 12f;
    public bool isJumping;
    protected bool doubleJump;

    [Header("Dash")]
    public float auxDashCooldown = 1f;
    protected float dashCooldown;
    public float dashforce = 20f;
    protected bool isDashing;

    [Header("Wall Settings")]
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    protected bool isWallSliding;

    [Header("Components")]
    protected Rigidbody2D rig;
    public Animator anim;
    protected SpriteRenderer sprite;
    public static Player player;

    protected virtual void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        player = this;
    }

    protected virtual void Update()
    {
        if (isDashing)
            return; // Trava outros inputs durante o Dash

        Move();
        Jump();
        Dash();
        WallSlide();
        CheckFall();

        // As filhas vão implementar isso para decidir como atacam
        HandleCombatInput();
    }

    // Método obrigatório para as filhas
    protected abstract void HandleCombatInput();

    #region Movimentação Base
    void Move()
    {
        float horizontal = 0f;
        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;
        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;

        // Durante wall slide não aplica velocidade horizontal (evita grudar na parede) e tira "walk" para a animação "wall" rodar
        if (isWallSliding)
        {
            rig.linearVelocity = new Vector2(0f, rig.linearVelocity.y);
            anim.SetBool("walk", false);
            return;
        }

        rig.linearVelocity = new Vector2(horizontal * Speed, rig.linearVelocity.y);

        if (horizontal != 0)
        {
            anim.SetBool("walk", true);
            transform.localScale = new Vector3(
                Mathf.Sign(horizontal) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else
        {
            anim.SetBool("walk", false);
        }
    }

    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!isJumping)
            {
                rig.linearVelocity = new Vector2(rig.linearVelocity.x, JumpForce);
                doubleJump = true;
                isJumping = true;
                anim.SetBool("jump", true);
            }
            else if (doubleJump)
            {
                rig.linearVelocity = new Vector2(rig.linearVelocity.x, JumpForce);
                doubleJump = false;
                anim.SetBool("jump", false);
                anim.SetBool("doublejump", true);
                StartCoroutine(StopDoubleJumpAnim());
            }
        }
    }

    void Dash()
    {
        if (dashCooldown > 0)
            dashCooldown -= Time.deltaTime;

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && !isDashing && dashCooldown <= 0)
        {
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
        isDashing = false;
    }

    void WallSlide()
    {
        float direction = transform.localScale.x > 0 ? 1 : -1;
        RaycastHit2D wallCheck = Physics2D.Raycast(
            transform.position,
            Vector2.right * direction,
            0.8f,
            wallLayer
        );

        // Só faz wall slide se: encostou na parede, está caindo, e está segurando a tecla em direção à parede
        float horizontal = 0f;
        if (Keyboard.current.aKey.isPressed) horizontal = -1f;
        if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        bool pressingTowardWall = (horizontal * direction) > 0f;

        if (wallCheck.collider != null && rig.linearVelocity.y < 0 && pressingTowardWall)
        {
            isWallSliding = true;
            rig.linearVelocity = new Vector2(0f, -wallSlideSpeed);
            anim.SetBool("walk", false);
            anim.SetBool("wall", true);
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("wall", false);
        }
    }

    void CheckFall()
    {
        if (rig.linearVelocity.y < -0.1f && isJumping && !isWallSliding)
        {
            anim.SetBool("fall", true);
            anim.SetBool("jump", false);
        }
        else
        {
            anim.SetBool("fall", false);
        }
    }

    IEnumerator StopDoubleJumpAnim()
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("doublejump", false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isJumping = false;
            doubleJump = false;
            isWallSliding = false;
            anim.SetBool("jump", false);
            anim.SetBool("doublejump", false);
            anim.SetBool("fall", false);
            anim.SetBool("wall", false);
        }
    }
    #endregion
}
