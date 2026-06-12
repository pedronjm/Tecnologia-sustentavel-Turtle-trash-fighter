using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Mercado interativo — pausa o jogo e abre um panel com lista de itens.
///
/// Navegação:
///   • Mouse   — hover para selecionar, clique para comprar
///   • A / D   — navegar entre itens
///   • Interact (MenuBindingStore) — confirmar compra
///   • Escape / botão Sair — fechar
///
/// Integração:
///   • Pontos lidos e debitados via GameControler.instance.pontos
///   • Itens únicos já comprados persistidos via MercadoState.instance
///   • Efeitos aplicados direto no Player encontrado na cena
/// </summary>
public class MercadoTrigger : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // Referências de dados
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Itens à Venda")]
    [Tooltip("Liste os ScriptableObjects ItemMercado que este mercado vende.")]
    [SerializeField]
    private List<ItemMercado> itens = new();

    // ─────────────────────────────────────────────────────────────────────────
    // UI — Prompt (igual à lixeira)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Panel Prompt (área externa)")]
    [SerializeField]
    private GameObject promptPanel;

    [SerializeField]
    private TMP_Text promptText;

    [TextArea]
    [SerializeField]
    private string promptTemplate = "Mercado\nPressione {0} para entrar";

    // ─────────────────────────────────────────────────────────────────────────
    // UI — Panel principal da loja (pausa o jogo)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Panel da Loja")]
    [Tooltip("Panel grande que aparece ao interagir. Deve ter os filhos abaixo.")]
    [SerializeField]
    private GameObject lojaPanel;

    [Tooltip("Texto que exibe os pontos atuais do jogador.")]
    [SerializeField]
    private TMP_Text pontosText;

    [Tooltip("Container onde os cards de item serão instanciados.")]
    [SerializeField]
    private Transform itensContainer;

    [Tooltip("Prefab de um card de item. Deve ter os componentes CardItemUI.")]
    [SerializeField]
    private GameObject cardItemPrefab;

    [Tooltip("Botão de sair da loja.")]
    [SerializeField]
    private Button botaoSair;

    // ─────────────────────────────────────────────────────────────────────────
    // UI — Feedback de compra
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Feedback de Compra")]
    [SerializeField]
    private GameObject feedbackPanel;

    [SerializeField]
    private TMP_Text feedbackText;

    [SerializeField]
    private float feedbackDuration = 2f;

    // ─────────────────────────────────────────────────────────────────────────
    // Eventos
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Eventos")]
    [SerializeField]
    private UnityEvent<ItemMercado> onItemComprado;

    // ─────────────────────────────────────────────────────────────────────────
    // Estado interno
    // ─────────────────────────────────────────────────────────────────────────

    private bool playerInRange;
    private bool lojaAberta;
    private int indiceSelecionado;
    private List<CardItemUI> cards = new();
    private Coroutine feedbackCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    // Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        MenuBindingStore.EnsureLoaded();
        if (promptPanel != null)
            promptPanel.SetActive(false);
        if (lojaPanel != null)
            lojaPanel.SetActive(false);
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        if (botaoSair != null)
            botaoSair.onClick.AddListener(FecharLoja);
    }

    private void OnEnable() => MenuBindingStore.BindingsChanged += AtualizarPrompt;

    private void OnDisable() => MenuBindingStore.BindingsChanged -= AtualizarPrompt;

    // ─────────────────────────────────────────────────────────────────────────
    // Trigger 2D
    // ─────────────────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;
        playerInRange = true;
        MostrarPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
            return;
        playerInRange = false;
        if (promptPanel != null)
            promptPanel.SetActive(false);
        if (lojaAberta)
            FecharLoja();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Update — input de abertura e navegação
    // ─────────────────────────────────────────────────────────────────────────

    void Update()
    {
        // Abre a loja
        if (
            playerInRange
            && !lojaAberta
            && MenuBindingStore.WasPressedThisFrame(MenuActionId.Interact)
        )
        {
            AbrirLoja();
            return;
        }

        if (!lojaAberta)
            return;

        // Fecha com Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FecharLoja();
            return;
        }

        // Navegação A / D
        if (Input.GetKeyDown(KeyCode.A))
            NavegaItem(-1);
        else if (Input.GetKeyDown(KeyCode.D))
            NavegaItem(+1);

        // Confirma compra com Interact
        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.Interact))
            TentarComprar(indiceSelecionado);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Abrir / Fechar loja
    // ─────────────────────────────────────────────────────────────────────────

    private void AbrirLoja()
    {
        lojaAberta = true;
        Time.timeScale = 0f; // pausa o jogo

        if (promptPanel != null)
            promptPanel.SetActive(false);
        if (lojaPanel != null)
            lojaPanel.SetActive(true);

        ConstruirCards();
        AtualizarPontos();
        SelecionarCard(0);
    }

    private void FecharLoja()
    {
        lojaAberta = false;
        Time.timeScale = 1f; // retoma o jogo

        if (lojaPanel != null)
            lojaPanel.SetActive(false);
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);

        if (playerInRange)
            MostrarPrompt();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Construção dos cards
    // ─────────────────────────────────────────────────────────────────────────

    private void ConstruirCards()
    {
        // Limpa cards anteriores
        foreach (var c in cards)
            if (c != null)
                Destroy(c.gameObject);
        cards.Clear();

        if (itensContainer == null || cardItemPrefab == null)
            return;

        for (int i = 0; i < itens.Count; i++)
        {
            ItemMercado item = itens[i];
            if (item == null)
                continue;

            GameObject obj = Instantiate(cardItemPrefab, itensContainer);
            CardItemUI card = obj.GetComponent<CardItemUI>();

            if (card == null)
            {
                Debug.LogWarning("MercadoTrigger: cardItemPrefab não tem o componente CardItemUI!");
                continue;
            }

            bool jaComprado =
                !item.infinito
                && MercadoState.instance != null
                && MercadoState.instance.JaComprou(item.itemId);

            int indiceCapturado = i;
            card.Configurar(
                item,
                jaComprado,
                onHover: () => SelecionarCard(indiceCapturado),
                onClicar: () => TentarComprar(indiceCapturado)
            );

            cards.Add(card);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Seleção e navegação
    // ─────────────────────────────────────────────────────────────────────────

    private void NavegaItem(int direcao)
    {
        if (cards.Count == 0)
            return;

        int novo = (indiceSelecionado + direcao + cards.Count) % cards.Count;
        SelecionarCard(novo);
    }

    private void SelecionarCard(int indice)
    {
        if (indice < 0 || indice >= cards.Count)
            return;

        // Desmarca o anterior
        if (indiceSelecionado >= 0 && indiceSelecionado < cards.Count)
            cards[indiceSelecionado]?.SetSelecionado(false);

        indiceSelecionado = indice;
        cards[indiceSelecionado]?.SetSelecionado(true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Compra
    // ─────────────────────────────────────────────────────────────────────────

    private void TentarComprar(int indice)
    {
        if (indice < 0 || indice >= itens.Count)
            return;

        ItemMercado item = itens[indice];
        if (item == null)
            return;

        // Já comprou item único?
        if (
            !item.infinito
            && MercadoState.instance != null
            && MercadoState.instance.JaComprou(item.itemId)
        )
        {
            MostrarFeedback($"{item.nomeExibido} já foi comprado!", sucesso: false);
            return;
        }

        // Pontos suficientes?
        int pontos = GetPontos();
        if (pontos < item.custo)
        {
            MostrarFeedback(
                $"Pontos insuficientes! (faltam {item.custo - pontos})",
                sucesso: false
            );
            return;
        }

        // Debita pontos e aplica efeito
        DebitarPontos(item.custo);
        AplicarEfeito(item);

        // Persiste compra única
        if (!item.infinito)
            MercadoState.instance?.RegistrarCompra(item.itemId);

        onItemComprado?.Invoke(item);

        AtualizarPontos();
        ConstruirCards(); // atualiza estado visual (item some se único)
        SelecionarCard(Mathf.Clamp(indiceSelecionado, 0, cards.Count - 1));

        MostrarFeedback($"{item.nomeExibido} comprado! -{item.custo} pontos", sucesso: true);
        Debug.Log($"MercadoTrigger: comprou '{item.nomeExibido}' por {item.custo} pts.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Efeitos dos itens
    // ─────────────────────────────────────────────────────────────────────────

    private void AplicarEfeito(ItemMercado item)
    {
        Player player = FindFirstObjectByType<Player>();

        switch (item.tipo)
        {
            case TipoItem.RecuperarVida:
                // Chama o método de cura do seu Damageable / Player
                var damageable = player?.GetComponent<Damageable>();
                if (damageable != null)
                    damageable.Heal(item.valorVida);
                else
                    Debug.LogWarning("MercadoTrigger: Damageable não encontrado para cura.");
                break;

            case TipoItem.MaisVidaMaxima:
                // Adapte ao campo correto do seu Player / Damageable
                var dmg = player?.GetComponent<Damageable>();
                if (dmg != null)
                    dmg.MaxHealth += (int)item.valorVida;
                break;

            case TipoItem.MaisDano:
                if (player != null)
                    player.bonusDano += item.valorDano;
                break;

            case TipoItem.MaisVelocidade:
                if (player != null)
                    player.bonusVelocidade += item.valorVelocidade;
                break;

            case TipoItem.ColetavelEspecial:
                // Registra o coletável como obtido no estado de save
                if (ColetavelState.instance != null)
                    ColetavelState.instance.SetCollected(item.itemId);
                Debug.Log($"MercadoTrigger: coletável especial '{item.itemId}' entregue.");
                break;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Pontos
    // ─────────────────────────────────────────────────────────────────────────

    private int GetPontos()
    {
        if (GameControler.instance != null)
            return GameControler.instance.pontos;
        return 0;
    }

    private void DebitarPontos(int valor)
    {
        if (GameControler.instance != null)
            GameControler.instance.pontos -= valor;
    }

    private void AtualizarPontos()
    {
        if (pontosText != null)
            pontosText.text = $"Seus pontos: {GetPontos()}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI — Prompt
    // ─────────────────────────────────────────────────────────────────────────

    private void MostrarPrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(true);

        if (promptText != null)
        {
            string label = MenuBindingStore.GetDisplayName(MenuActionId.Interact);
            promptText.text = string.Format(promptTemplate, label);
        }
    }

    private void AtualizarPrompt()
    {
        if (playerInRange && !lojaAberta)
            MostrarPrompt();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UI — Feedback de compra
    // ─────────────────────────────────────────────────────────────────────────

    private void MostrarFeedback(string mensagem, bool sucesso)
    {
        if (feedbackPanel == null)
            return;

        feedbackPanel.SetActive(true);
        if (feedbackText != null)
            feedbackText.text = mensagem;

        // Cor opcional: verde = sucesso, vermelho = erro
        if (feedbackText != null)
            feedbackText.color = sucesso ? Color.green : Color.red;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(EsconderFeedbackAposDelay());
    }

    private System.Collections.IEnumerator EsconderFeedbackAposDelay()
    {
        // WaitForSecondsRealtime pois o jogo está pausado (timeScale = 0)
        yield return new WaitForSecondsRealtime(feedbackDuration);
        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
        feedbackCoroutine = null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
            return false;
        if (other.CompareTag("Player"))
            return true;
        return other.GetComponentInParent<Player>() != null;
    }
}
