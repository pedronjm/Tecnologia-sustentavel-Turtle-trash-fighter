using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWarrior : Player
{
    [Header("Warrior Melee")]
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public LayerMask enemyLayer;
    public int damage = 3;
    public float cooldown = 0.3f;
    public float meleeHitDelaySeconds = 0.12f;

    [Header("Warrior Ranged")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 8f;
    public float rangedCooldown = 0.7f;
    public string rangedAttackTrigger = "Long range";

    private float meleeTimer;
    private float rangedTimer;
    private Coroutine meleeHitCoroutine;

    protected override void Start()
    {
        base.Start();

        
    }

    protected override void HandleCombatInput()
    {
        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.MeleeAttack) && meleeTimer <= 0f)
        {
            meleeTimer = cooldown;
            StartMeleeAttack();
        }

        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.RangedAttack) && rangedTimer <= 0f)
        {
            rangedTimer = rangedCooldown;
            RangedAttack();
        }
    }

    private void StartMeleeAttack()
    {
        anim.SetTrigger("Close range");

        if (meleeHitCoroutine != null)
        {
            StopCoroutine(meleeHitCoroutine);
            meleeHitCoroutine = null;
        }

        meleeHitCoroutine = StartCoroutine(MeleeHitAfterDelay());
    }

    private IEnumerator MeleeHitAfterDelay()
    {
        if (meleeHitDelaySeconds > 0f)
            yield return new WaitForSeconds(meleeHitDelaySeconds);

        DoMeleeHit();
        meleeHitCoroutine = null;
    }

    private void DoMeleeHit()
    {
        if (attackPoint == null)
            return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<Damageable>()?.TakeDamage(damage, transform.position);
        }
    }

    private void RangedAttack()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
            return;

        anim.SetTrigger(rangedAttackTrigger);

        float dir = transform.localScale.x > 0 ? 1f : -1f;
        GameObject proj = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        Projectile projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
            projScript.owner = transform;

        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(projectileSpeed * dir, 0f);
        }

        Vector3 scale = proj.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        proj.transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
