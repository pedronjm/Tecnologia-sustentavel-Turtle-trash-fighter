using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMage : Player
{
    [Header("Mage Melee")]
    public Transform attackPoint;
    public float attackRange = 0.4f;
    public LayerMask enemyLayer;
    public int meleeDamage = 1;
    public float meleeCooldown = 0.7f;

    [Header("Mage Ranged")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 16f;
    public float castCooldown = 0.4f;

    private float meleeTimer;
    private float castTimer;

    protected override void HandleCombatInput()
    {
        meleeTimer -= Time.deltaTime;
        castTimer -= Time.deltaTime;

        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.MeleeAttack) && meleeTimer <= 0f)
        {
            meleeTimer = meleeCooldown;
            MeleeAttack();
        }

        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.RangedAttack) && castTimer <= 0f)
        {
            castTimer = castCooldown;
            CastProjectile();
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

    private void CastProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
            return;

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.owner = transform;

        float dir = transform.localScale.x > 0 ? 1f : -1f;
        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();

        if (projRb != null)
        {
            projRb.linearVelocity = new Vector2(projectileSpeed * dir, 0f);
        }

        Vector3 scale = proj.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        proj.transform.localScale = scale;

        anim.SetTrigger("cast");
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}

