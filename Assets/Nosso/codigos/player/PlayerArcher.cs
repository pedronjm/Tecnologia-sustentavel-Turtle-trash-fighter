using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerArcher : Player
{
    [Header("Archer Melee")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public int meleeDamage = 2;
    public float meleeCooldown = 0.5f;

    [Header("Archer Ranged")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowSpeed = 12f;
    public float shootCooldown = 0.5f;

    private float meleeTimer;
    private float shootTimer;

    protected override void HandleCombatInput()
    {
        meleeTimer -= Time.deltaTime;
        shootTimer -= Time.deltaTime;

        var mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame && meleeTimer <= 0f)
        {
            meleeTimer = meleeCooldown;
            MeleeAttack();
        }

        if (mouse.rightButton.wasPressedThisFrame && shootTimer <= 0f)
        {
            shootTimer = shootCooldown;
            ShootArrow();
        }
    }

    private void MeleeAttack()
    {
        anim.SetTrigger("attack");

        if (attackPoint == null)
            return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<Damageable>()?.TakeDamage(meleeDamage, transform.position);
        }
    }

    private void ShootArrow()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null)
            return;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

        Projectile projScript = arrow.GetComponent<Projectile>();
        if (projScript != null)
            projScript.owner = transform;

        float dir = transform.localScale.x > 0 ? 1f : -1f;
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        if (arrowRb != null)
        {
            arrowRb.linearVelocity = new Vector2(arrowSpeed * dir, 0f);
        }

        Vector3 scale = arrow.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        arrow.transform.localScale = scale;

        anim.SetTrigger("shoot");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

