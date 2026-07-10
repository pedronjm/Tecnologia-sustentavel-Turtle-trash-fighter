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

    // ID resolvido e travado no Awake (antes de qualquer movimento). Antes,
    // GetSaveId() recalculava a posição toda vez que era chamado; para
    // inimigos que se movem (ex.: sacola.cs, que anda em FixedUpdate), o ID
    // gerado ao morrer (posição já deslocada) ficava diferente do ID gerado
    // no Start (posição de spawn). Resultado: o backend salvava o inimigo
    // como morto com um ID, mas na hora de checar "já morreu?" o jogo
    // comparava com outro ID (posição de spawn) -> nunca batia, e o inimigo
    // voltava vivo ao carregar o save, mesmo o registro existindo no banco.
    private string cachedSaveId;

    protected virtual void Awake()
    {
        cachedSaveId = ResolveSaveId();
    }

    protected virtual void Start()
    {
        if (EnemyState.instance != null)
        {
            EnemyState.instance.RegistrarInimigo(GetSaveId());

            if (EnemyState.instance.EstaMorto(GetSaveId()))
            {
                Debug.Log($"[enemy] '{GetSaveId()}' já estava marcado como morto -> destruindo ao carregar a cena.");
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
        {
            EnemyState.instance.MarcarMorto(GetSaveId());
            Debug.Log($"[enemy] Marcado como morto: id='{GetSaveId()}'");
        }

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

    /// <summary>
    /// Retorna sempre o mesmo ID durante toda a vida do objeto (calculado
    /// uma única vez no Awake). Use este método tanto para registrar quanto
    /// para marcar como morto — nunca recalcule a partir da posição atual.
    /// </summary>
    public string GetSaveId()
    {
        if (string.IsNullOrEmpty(cachedSaveId))
            cachedSaveId = ResolveSaveId();

        return cachedSaveId;
    }

    private string ResolveSaveId()
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
