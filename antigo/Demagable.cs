using UnityEngine;

public class Damageable : MonoBehaviour
{
    public int health = 3;
    public Rigidbody2D rb;
    public float knockbackForce = 5f;

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        health -= damage;

        Vector2 dir = (transform.position - (Vector3)attackerPosition).normalized;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
