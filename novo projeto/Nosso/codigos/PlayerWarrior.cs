using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWarrior : Player
{
    [Header("Warrior Combat")]
    public Transform attackPoint;
    public float attackRange = 0.6f;
    public LayerMask enemyLayer;
    public int damage = 2;
    public float cooldown = 0.3f;
    private float timer;

    protected override void HandleCombatInput()
    {
        timer -= Time.deltaTime;
        if (Mouse.current.leftButton.wasPressedThisFrame && timer <= 0)
        {
            Attack();
            timer = cooldown;
        }
    }

    void Attack()
    {
        anim.SetTrigger("attack");
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

    void OnDrawGizmosSelected()
    {
        if (attackPoint)
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
