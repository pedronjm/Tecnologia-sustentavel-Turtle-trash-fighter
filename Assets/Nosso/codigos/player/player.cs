using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float Speed = 8f;
    public float JumpForce = 12f;
    public bool isJumping;
    public bool doubleJump;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool isGrounded;
    public float groundCheckRadius = 0.15f;

    [Header("Dash")]
    public float auxDashCooldown = 1f;
    protected float dashCooldown;
    public float dashforce = 20f;
    protected bool isDashing;

    [Header("Wall Settings")]
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    public float wallCheckRadius = 0.12f;
    public float wallJumpForce = 12f;
    public float wallJumpControlLockTime = 0.15f;
    public bool debugWallSlide;
    protected bool isWallSliding;
    bool lastLoggedWallSlide;
    float wallJumpLockTimer;
    float wallDirection;

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

        if (groundCheck == null)
            groundCheck = transform;

        player = this;
    }

    protected virtual void Update()
    {
        if (isDashing)
            return; // Trava outros inputs durante o Dash

        UpdateGroundState();
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

    void UpdateGroundState()
    {
        Vector2 checkPosition =
            groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;

         isGrounded =
            Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer) != null;

        if (!isGrounded)
            return;

        isJumping = false;
        doubleJump = false;
        isWallSliding = false;
        anim.SetBool("jump", false);
        anim.SetBool("doublejump", false);
        anim.SetBool("fall", false);
        anim.SetBool("wall", false);
    }

    #region Movimentação Base
    void Move()
    {
        if (wallJumpLockTimer > 0f)
        {
            wallJumpLockTimer -= Time.deltaTime;
            anim.SetBool("walk", false);
            return;
        }

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
        bool hasHorizontalVelocity = Mathf.Abs(rig.linearVelocity.x) != 0.0f;

        if (horizontal != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(horizontal) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }

        anim.SetBool("walk", hasHorizontalVelocity);
    }

    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isWallSliding)
            {
                float jumpAwayDirection = -Mathf.Sign(wallDirection);
                if (jumpAwayDirection == 0f)
                    jumpAwayDirection = -(transform.localScale.x > 0 ? 1f : -1f);

                Vector2 wallJumpVelocity =
                    new Vector2(jumpAwayDirection, 1f).normalized * wallJumpForce;
                rig.linearVelocity = wallJumpVelocity;
                isWallSliding = false;
                isJumping = true;
                doubleJump = true;
                wallJumpLockTimer = wallJumpControlLockTime;

                anim.SetBool("wall", false);
                anim.SetBool("fall", false);
                anim.SetBool("jump", false);
                anim.SetBool("doublejump", true);
                StartCoroutine(StopDoubleJumpAnim());
                return;
            }

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
        wallDirection = direction;
        Vector2 wallCheckPos = new Vector2(
            transform.position.x + (0.6f * direction),
            transform.position.y
        );
        Collider2D wallCheck = Physics2D.OverlapCircle(wallCheckPos, wallCheckRadius, wallLayer);

        float horizontal = 0f;
        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;
        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;
        bool pressingTowardWall = (horizontal * direction) > 0f;
        bool isTouchingWall = wallCheck != null;
        bool canSlide = isTouchingWall && pressingTowardWall && !isGrounded;
        

        if (canSlide)
        {
            isWallSliding = true;
            rig.linearVelocity = new Vector2(0f, -wallSlideSpeed);

            anim.SetBool("wall", true);

            Debug.Log(
                $"[WallSlide] estado={isWallSliding} tocandoParede={isTouchingWall} caindo={rig.linearVelocity.y < 0f} inputParede={pressingTowardWall} velY={rig.linearVelocity.y}",
                this
            );
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("wall", false);
        }

        if (debugWallSlide && lastLoggedWallSlide != isWallSliding)
        {
            Debug.Log(
                $"[WallSlide] estado={isWallSliding} tocandoParede={isTouchingWall} caindo={rig.linearVelocity.y < 0f} inputParede={pressingTowardWall} velY={rig.linearVelocity.y}",
                this
            );
            lastLoggedWallSlide = isWallSliding;
        }
    }

    void CheckFall()
    {
        if (rig.linearVelocity.y < -0.1f && !isWallSliding)
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

    #endregion
}
