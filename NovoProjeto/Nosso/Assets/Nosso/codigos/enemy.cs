using UnityEngine;

public class enemy : MonoBehaviour
{
    protected Rigidbody2D rig;
    protected Animator anim;

    [Header("Configurações de Movimento")]
    public float speed = 2f;

    protected bool isDead = false;

    protected virtual void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        speed = 0f;

        if (rig != null)
        {
            rig.linearVelocity = Vector2.zero;
            rig.bodyType = RigidbodyType2D.Static;
        }

        if (anim != null)
            anim.SetTrigger("dano");

        Destroy(gameObject, 0.75f);
    }

    public virtual void ApplyKnockback(Vector2 attackPos)
    {
        if (rig == null)
            return;

        float side = transform.position.x < attackPos.x ? -1f : 1f;
        rig.AddForce(new Vector2(side * 5f, 2f), ForceMode2D.Impulse);
    }
}
