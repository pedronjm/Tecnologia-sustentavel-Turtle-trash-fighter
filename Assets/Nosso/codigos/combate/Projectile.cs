using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;

    [Header("Owner (ignora colisão com quem atirou)")]
    [HideInInspector] public Transform owner;

    [Header("Lifetime")]
    public float groundDestroyDelay = 0.2f;

    [Header("Collision")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Tenta pegar o componente de dano
        Damageable dmg = collision.GetComponent<Damageable>();

        if (dmg != null)
        {
            // Não dá dano no próprio jogador que atirou (owner ou filho do owner)
            if (owner != null && (collision.transform == owner || collision.transform.IsChildOf(owner)))
                return;

            dmg.TakeDamage(damage, transform.position);
            Destroy(gameObject);
            return;
        }

        // Verifica se atingiu o chão usando a LayerMask
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // Para o projétil ao tocar o chão para não atravessar enquanto espera o Delay
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            StartCoroutine(DestroyAfterGroundHit());
        }
    }

    IEnumerator DestroyAfterGroundHit()
    {
        yield return new WaitForSeconds(groundDestroyDelay);
        Destroy(gameObject);
    }
}
