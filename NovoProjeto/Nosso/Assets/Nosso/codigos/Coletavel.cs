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

    [Tooltip("ID único (gerado pelo menu Nosso > Gerar IDs dos Coletáveis). Deixe vazio para não persistir entre cenas.")]
    [SerializeField] private string id;

    protected SpriteRenderer sr;
    protected Collider2D col;
    protected bool collected;

    public string Id => id;

    protected virtual void Start()
    {
        if (ColetavelState.instance != null && ColetavelState.instance.FoiColetado(id))
        {
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
        if (ColetavelState.instance != null && !string.IsNullOrEmpty(id))
            ColetavelState.instance.RegistrarColetado(id);
        TocarSomColetar();
        Destroy(gameObject, 0.25f);
    }

    /// <summary>
    /// Chamado quando o player coleta. Cada subclasse adiciona ao contador correto no GameController.
    /// </summary>
    protected abstract void OnCollected();

    void TocarSomColetar()
    {
        // Áudio desativado por enquanto
        // if (GameControler.instance != null && GameControler.instance.audioSource != null && GameControler.instance.coletar != null)
        //     GameControler.instance.audioSource.PlayOneShot(GameControler.instance.coletar);
    }
}
