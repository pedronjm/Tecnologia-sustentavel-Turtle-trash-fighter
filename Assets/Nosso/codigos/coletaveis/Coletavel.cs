using UnityEngine;

/// <summary>
/// Classe base para todos os itens coletáveis (garrafa, circuito, engrenagem, maçã, etc.).
/// Cuida da detecção do player, som e destruição; cada tipo só define qual contador atualizar.
/// </summary>
public abstract class Coletavel : MonoBehaviour
{
    [Header("Coletável")]
    [Tooltip("Pontos/quantidade adicionada ao coletar")]
    public int score = 1;

    [Tooltip("ID único (gerado pelo menu Nosso > Gerar IDs dos Coletáveis). Deixe vazio para gerar um ID automático baseado em cena+nome+posição.")]
    [SerializeField] private string id;

    protected SpriteRenderer sr;
    protected Collider2D col;
    protected bool collected;

    // Antes, se o campo "id" ficasse vazio no Inspector (o que acontece
    // sempre que ninguém rodou o gerador de IDs), RegistrarColetado() nunca
    // era chamado -- o item nunca era registrado como coletado em memória,
    // então o save mandava uma lista de coletados vazia e o item sempre
    // reaparecia ao carregar. Agora, se "id" estiver vazio, calculamos um ID
    // automático (mesma ideia usada em enemy.cs) e o travamos no Awake.
    private string cachedId;

    public string Id => cachedId;

    protected virtual void Awake()
    {
        cachedId = !string.IsNullOrEmpty(id)
            ? id
            : $"{gameObject.scene.name}:{gameObject.name}:{Mathf.RoundToInt(transform.position.x * 100f)}:{Mathf.RoundToInt(transform.position.y * 100f)}";
    }

    protected virtual void Start()
    {
        if (ColetavelState.instance != null && ColetavelState.instance.FoiColetado(Id))
        {
            Debug.Log($"[Coletavel] '{Id}' já estava coletado -> destruindo ao carregar a cena.");
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || collected)
            return;

        collected = true;

        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        OnCollected();
        if (GameControler.instance != null)
            GameControler.instance.RegistrarColetavelColetado();

        if (ColetavelState.instance != null)
        {
            ColetavelState.instance.RegistrarColetado(Id);
            Debug.Log($"[Coletavel] Registrado como coletado: id='{Id}'");
        }
        else
        {
            Debug.LogWarning($"[Coletavel] ColetavelState.instance é nulo -> '{Id}' NÃO foi registrado como coletado.");
        }

        TocarSomColetar();
        Destroy(gameObject, 0.25f);
    }

    /// <summary>
    /// Chamado quando o player coleta. Cada subclasse adiciona ao contador correto no GameController.
    /// </summary>
    protected abstract void OnCollected();

    void TocarSomColetar()
    {
        if (GameControler.instance != null)
            GameControler.instance.PlayCollectSound();
    }
}
