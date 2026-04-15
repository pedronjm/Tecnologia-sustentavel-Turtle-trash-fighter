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

    [Tooltip("Tempo minimo apos saltar para permitir reset por contato com o chao.")]
    public float jumpResetDelay = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool isGrounded;
    public float groundCheckRadius = 0.15f;

    [Tooltip("Mantem o estado de chao por alguns milissegundos para evitar flicker.")]
    public float groundedGraceTime = 0.08f;

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
    public int maxJumps = 2;
    public bool debugWallSlide;
    protected bool isWallSliding;
    bool lastLoggedWallSlide;
    bool jumpResetByWallContact;
    float wallJumpLockTimer;
    float wallDirection;
    int jumpsUsed;
    float jumpResetLockTimer;
    float groundedGraceTimer;

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

        // Regras do projeto: apenas pulo duplo (2 saltos no ar).
        maxJumps = 2;

        if (groundCheck == null)
            groundCheck = transform;

        player = this;
    }

    protected virtual void Update()
    {
        if (jumpResetLockTimer > 0f)
            jumpResetLockTimer -= Time.deltaTime;

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

    protected float GetMovementAxis()
    {
        float horizontal = 0f;

        if (MenuBindingStore.IsPressed(MenuActionId.MoveLeft))
            horizontal -= 1f;

        if (MenuBindingStore.IsPressed(MenuActionId.MoveRight))
            horizontal += 1f;

        return horizontal;
    }

    protected bool WasJumpPressedThisFrame()
    {
        return MenuBindingStore.WasPressedThisFrame(MenuActionId.Jump);
    }

    protected bool WasDashPressedThisFrame()
    {
        return MenuBindingStore.WasPressedThisFrame(MenuActionId.Dash);
    }

    // Método obrigatório para as filhas
    protected abstract void HandleCombatInput();

    void UpdateGroundState()
    {
        Vector2 checkPosition =
            groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position;

        bool touchingGround =
            Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayer) != null;

        if (touchingGround)
            groundedGraceTimer = groundedGraceTime;
        else
            groundedGraceTimer -= Time.deltaTime;

        isGrounded = groundedGraceTimer > 0f;

        if (!isGrounded || jumpResetLockTimer > 0f)
            return;

        isJumping = false;
        doubleJump = false;
        jumpsUsed = 0;
        jumpResetByWallContact = false;
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

        float horizontal = GetMovementAxis();

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
        if (WasJumpPressedThisFrame())
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
                jumpsUsed = 1;
                doubleJump = true;
                jumpResetLockTimer = jumpResetDelay;
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
                jumpsUsed = 1;
                isJumping = true;
                doubleJump = true;
                jumpResetLockTimer = jumpResetDelay;
                anim.SetBool("jump", true);
            }
            else if (doubleJump)
            {
                rig.linearVelocity = new Vector2(rig.linearVelocity.x, JumpForce);
                jumpsUsed = 2;
                doubleJump = false;
                jumpResetLockTimer = jumpResetDelay;
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

        if (WasDashPressedThisFrame() && !isDashing && dashCooldown <= 0)
        {
            StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldown = auxDashCooldown;
        float dashDirection = transform.localScale.x > 0 ? 1 : -1;

        if (sprite != null)
            sprite.flipX = !sprite.flipX;

        rig.linearVelocity = new Vector2(dashDirection * dashforce, 0f);
        anim.SetBool("dash", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("dash", false);

        if (sprite != null)
            sprite.flipX = !sprite.flipX;

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

        float horizontal = GetMovementAxis();
        bool pressingTowardWall = (horizontal * direction) > 0f;
        bool isTouchingWall = wallCheck != null;
        bool canSlide = isTouchingWall && pressingTowardWall && !isGrounded;

        if (isTouchingWall)
        {
            if (!jumpResetByWallContact)
            {
                jumpsUsed = 0;
                isJumping = false;
                doubleJump = false;
                jumpResetByWallContact = true;
            }
        }
        else
        {
            jumpResetByWallContact = false;
        }

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
        if (!isGrounded && rig.linearVelocity.y < -0.15f && !isWallSliding)
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
