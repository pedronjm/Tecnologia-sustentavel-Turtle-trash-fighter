using UnityEngine;

public class enemy : MonoBehaviour
{
    protected Rigidbody2D rig;
    protected Animator anim;

    [Header("Save")]
    [Tooltip("ID unico para persistencia de inimigos mortos. Se vazio, usa fallback por nome/posicao.")]
    [SerializeField] private string saveId;

    [Header("Configurações de Movimento")]
    public float speed = 2f;

    protected bool isDead = false;

    protected virtual void Start()
    {
        if (EnemyState.instance != null)
        {
            EnemyState.instance.RegistrarInimigo(GetSaveId());

            if (EnemyState.instance.EstaMorto(GetSaveId()))
            {
                Destroy(gameObject);
                return;
            }
        }

        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (GameControler.instance != null)
            GameControler.instance.PlayEnemyDeathSound();

        if (EnemyState.instance != null)
            EnemyState.instance.MarcarMorto(GetSaveId());

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

    public string GetSaveId()
    {
        if (!string.IsNullOrEmpty(saveId))
            return saveId;

        Vector3 p = transform.position;
        return $"{gameObject.scene.name}:{gameObject.name}:{Mathf.RoundToInt(p.x * 100f)}:{Mathf.RoundToInt(p.y * 100f)}";
    }

    public virtual void ApplyKnockback(Vector2 attackPos)
    {
        if (rig == null)
            return;

        float side = transform.position.x < attackPos.x ? -1f : 1f;
        rig.AddForce(new Vector2(side * 5f, 2f), ForceMode2D.Impulse);
    }
}
