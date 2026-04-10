using System.Collections;
using UnityEngine;

public class sacola : enemy
{
    public LayerMask collisionLayer;
    public Transform groundCheck; // Use apenas um ponto na frente para detectar abismos
    public Transform wallCheck; // Use um ponto na frente para detectar paredes
    public Transform visualRoot;
    public bool invertVisualFacing = false;
    public float wallCheckDistance = 0.2f;
    public float groundCheckDistance = 0.9f;
    public float groundLookAheadDistance = 0.2f;
    public float turnCooldown = 0.05f;
    public float gizmoPointRadius = 0.05f;

    private float lastTurnTime = -999f;
    private int facingDirection = 1;
    private Vector3 groundCheckBaseLocalPosition;
    private Vector3 wallCheckBaseLocalPosition;
    private Vector3 visualBaseLocalScale;

    /*  [Header("Áudio")]
     public AudioSource audioSource;
     public AudioClip hitClip; // Som dele batendo no player
     public AudioClip dieClip; // Som dele morrendo */

    protected override void Start()
    {
        base.Start();

        if (visualRoot == null)
        {
            SpriteRenderer renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer != null)
                visualRoot = renderer.transform;
        }

        if (visualRoot != null)
            visualBaseLocalScale = visualRoot.localScale;

        if (groundCheck != null)
            groundCheckBaseLocalPosition = groundCheck.localPosition;

        if (wallCheck != null)
            wallCheckBaseLocalPosition = wallCheck.localPosition;

        facingDirection = speed >= 0f ? 1 : -1;
        speed = Mathf.Abs(speed);
        ApplyFacingDirection();
    }

    private void FixedUpdate()
    {
        if (isDead || rig == null)
            return;

        if (ShouldTurn() && Time.time >= lastTurnTime + turnCooldown)
            Turn();

        Move();
    }

    private void Move()
    {
        Vector2 v = rig.linearVelocity;
        v.x = speed * facingDirection;
        rig.linearVelocity = v;
    }

    private bool ShouldTurn()
    {
        if (groundCheck == null || wallCheck == null)
            return false;

        bool wallHit = HasValidRayHit(
            wallCheck.position,
            Vector2.right * facingDirection,
            wallCheckDistance
        );

        bool groundAhead = HasValidRayHit(
            (Vector2)groundCheck.position
                + Vector2.right * facingDirection * groundLookAheadDistance,
            Vector2.down,
            groundCheckDistance
        );

        return wallHit || !groundAhead;
    }

    private void Turn()
    {
        facingDirection *= -1;
        ApplyFacingDirection();
        lastTurnTime = Time.time;
    }

    private void ApplyFacingDirection()
    {
        ApplyVisualFacing(facingDirection);

        if (groundCheck != null)
        {
            groundCheck.localPosition = new Vector3(
                Mathf.Abs(groundCheckBaseLocalPosition.x) * facingDirection,
                groundCheckBaseLocalPosition.y,
                groundCheckBaseLocalPosition.z
            );
        }

        if (wallCheck != null)
        {
            wallCheck.localPosition = new Vector3(
                Mathf.Abs(wallCheckBaseLocalPosition.x) * facingDirection,
                wallCheckBaseLocalPosition.y,
                wallCheckBaseLocalPosition.z
            );
        }
    }

    private void ApplyVisualFacing(int dir)
    {
        if (visualRoot == null)
            return;

        float absX = Mathf.Abs(visualBaseLocalScale.x);
        float sign = dir < 0f ? -1f : 1f;

        if (invertVisualFacing)
            sign *= -1f;

        visualRoot.localScale = new Vector3(
            absX * sign,
            visualBaseLocalScale.y,
            visualBaseLocalScale.z
        );
    }

    private bool HasValidRayHit(Vector2 origin, Vector2 direction, float distance)
    {
        int configuredMask = collisionLayer.value;

        // Primeiro tenta com a mask configurada no Inspector.
        if (
            configuredMask != 0
            && HasValidRayHitWithMask(origin, direction, distance, configuredMask)
        )
            return true;

        // Fallback: evita comportamento quebrado quando a mask foi configurada errada.
        return HasValidRayHitWithMask(origin, direction, distance, Physics2D.DefaultRaycastLayers);
    }

    private bool HasValidRayHitWithMask(Vector2 origin, Vector2 direction, float distance, int mask)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction.normalized, distance, mask);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i].collider;
            if (hit == null || hit.isTrigger)
                continue;

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            int drawDir = Application.isPlaying ? facingDirection : 1;
            Vector3 groundStart =
                groundCheck.position + Vector3.right * drawDir * groundLookAheadDistance;
            Vector3 groundEnd = groundStart + Vector3.down * groundCheckDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundStart, gizmoPointRadius);
            Gizmos.DrawLine(groundStart, groundEnd);
            Gizmos.DrawWireSphere(groundEnd, gizmoPointRadius * 0.7f);
        }

        if (wallCheck != null)
        {
            int drawDir = Application.isPlaying ? facingDirection : 1;
            Vector3 wallEnd = wallCheck.position + Vector3.right * drawDir * wallCheckDistance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, gizmoPointRadius);
            Gizmos.DrawLine(wallCheck.position, wallEnd);
            Gizmos.DrawWireSphere(wallEnd, gizmoPointRadius * 0.7f);
        }
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
