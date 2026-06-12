using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Lixeira que aceita um tipo específico de lixo coletado pelo jogador.
///
/// Três panels independentes:
///   [1] PROMPT  — aparece ao entrar na área (sempre)
///   [2] SUCESSO — jogador pressionou Interact e TEM o lixo certo
///   [3] ERRO    — jogador pressionou Interact mas NÃO tem o lixo
///
/// Segue a mesma lógica do CheckpointTrigger:
///   • MenuBindingStore para input e label do botão
///   • OnTriggerEnter/Exit2D + Update para interação
///   • HandleBindingsChanged para atualizar label ao trocar controle
/// </summary>
public class LixeiraTrigger : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // Configuração
    // ─────────────────────────────────────────────────────────────────────────

    public enum TipoLixo { Garrafa, Engrenagem, Maca, Circuito }

    [Header("Configuração da Lixeira")]
    [Tooltip("Tipo de lixo que esta lixeira aceita.")]
    [SerializeField] private TipoLixo tipoAceito = TipoLixo.Garrafa;

    [Tooltip("Pontos somados por unidade descartada.")]
    [SerializeField] private int pontosPorUnidade = 10;

    // ─────────────────────────────────────────────────────────────────────────
    // Panel 1 — PROMPT (sempre ao entrar na área)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Panel 1 — Prompt de Interação")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [TextArea]
    [SerializeField] private string promptTemplate = "Lixeira de {0}\nPressione {1} para descartar";

    // ─────────────────────────────────────────────────────────────────────────
    // Panel 2 — SUCESSO (pressionou e tinha o lixo)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Panel 2 — Sucesso ao Descartar")]
    [SerializeField] private GameObject sucessoPanel;
    [SerializeField] private TMP_Text sucessoText;
    [TextArea]
    [SerializeField] private string sucessoTemplate = "Descartado! +{0} pontos\nTotal nesta lixeira: {1}";
    [Tooltip("Segundos que o panel de sucesso fica visível antes de voltar ao prompt.")]
    [SerializeField] private float sucessoDuration = 2.5f;

    // ─────────────────────────────────────────────────────────────────────────
    // Panel 3 — ERRO (pressionou mas NÃO tinha o lixo)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Panel 3 — Erro (sem lixo ao interagir)")]
    [SerializeField] private GameObject erroPanel;
    [SerializeField] private TMP_Text erroText;
    [TextArea]
    [SerializeField] private string erroTemplate = "Você não tem {0}\npara descartar!";
    [Tooltip("Segundos que o panel de erro fica visível antes de voltar ao prompt.")]
    [SerializeField] private float erroDuration = 2f;

    // ─────────────────────────────────────────────────────────────────────────
    // Eventos
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Eventos")]
    [Tooltip("Disparado ao descartar com sucesso. Recebe a quantidade descartada.")]
    [SerializeField] private UnityEvent<int> onDescartado;

    [Header("Meta (opcional)")]
    [Tooltip("Total a descartar para atingir a meta. 0 = sem meta.")]
    [SerializeField] private int metaTotal = 0;
    [Tooltip("Disparado ao atingir a meta (ex: abrir porta, completar fase).")]
    [SerializeField] private UnityEvent onMetaAtingida;

    // ─────────────────────────────────────────────────────────────────────────
    // Estado interno
    // ─────────────────────────────────────────────────────────────────────────

    private bool playerInRange;
    private int totalDescartadoNesteLixeira;
    private bool metaJaAtingida;

    private Coroutine feedbackCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    // Ciclo de vida
    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        MenuBindingStore.EnsureLoaded();
        EsconderTodosPanels();
    }

    private void OnEnable()  => MenuBindingStore.BindingsChanged += HandleBindingsChanged;
    private void OnDisable() => MenuBindingStore.BindingsChanged -= HandleBindingsChanged;

    // ─────────────────────────────────────────────────────────────────────────
    // Trigger 2D
    // ─────────────────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = true;

        // Ao entrar, sempre mostra o prompt (independente de ter lixo ou não)
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = false;
        EsconderTodosPanels();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Update — aguarda input
    // ─────────────────────────────────────────────────────────────────────────

    void Update()
    {
        if (!playerInRange) return;

        if (MenuBindingStore.WasPressedThisFrame(MenuActionId.Interact))
        {
            int quantidade = GetQuantidadeNoInventario();

            if (quantidade > 0)
                Descartar(quantidade);   // ✅ tem lixo → sucesso
            else
                MostrarErro();           // ❌ sem lixo → erro
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lógica de descarte
    // ─────────────────────────────────────────────────────────────────────────

    private void Descartar(int quantidade)
    {
        ZerarContador();

        totalDescartadoNesteLixeira += quantidade;
        int pontosGanhos = quantidade * pontosPorUnidade;

        Debug.Log($"LixeiraTrigger [{tipoAceito}]: {quantidade} unidade(s) → +{pontosGanhos} pts. Total: {totalDescartadoNesteLixeira}");

        onDescartado?.Invoke(quantidade);

        if (metaTotal > 0 && !metaJaAtingida && totalDescartadoNesteLixeira >= metaTotal)
        {
            metaJaAtingida = true;
            Debug.Log($"LixeiraTrigger [{tipoAceito}]: META ATINGIDA! ({totalDescartadoNesteLixeira}/{metaTotal})");
            onMetaAtingida?.Invoke();
        }

        MostrarSucesso(pontosGanhos, totalDescartadoNesteLixeira);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Leitura / zeragem do GameControler
    // ─────────────────────────────────────────────────────────────────────────

    private int GetQuantidadeNoInventario()
    {
        if (GameControler.instance == null) return 0;

        return tipoAceito switch
        {
            TipoLixo.Garrafa    => Mathf.RoundToInt(GameControler.instance.qttgarrafa),
            TipoLixo.Engrenagem => Mathf.RoundToInt(GameControler.instance.qttengrenagem),
            TipoLixo.Maca       => Mathf.RoundToInt(GameControler.instance.qttmaca),
            TipoLixo.Circuito   => Mathf.RoundToInt(GameControler.instance.qttcircuito),
            _                   => 0
        };
    }

    private void ZerarContador()
    {
        if (GameControler.instance == null) return;

        switch (tipoAceito)
        {
            case TipoLixo.Garrafa:    GameControler.instance.qttgarrafa    = 0; break;
            case TipoLixo.Engrenagem: GameControler.instance.qttengrenagem = 0; break;
            case TipoLixo.Maca:       GameControler.instance.qttmaca       = 0; break;
            case TipoLixo.Circuito:   GameControler.instance.qttcircuito   = 0; break;
        }
    }

    private string NomeTipo() => tipoAceito switch
    {
        TipoLixo.Garrafa    => "Garrafa",
        TipoLixo.Engrenagem => "Engrenagem",
        TipoLixo.Maca       => "Maçã",
        TipoLixo.Circuito   => "Circuito",
        _                   => tipoAceito.ToString()
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Panel 1 — PROMPT
    // ─────────────────────────────────────────────────────────────────────────

    private void ShowPrompt()
    {
        EsconderTodosPanels();

        if (promptPanel != null) promptPanel.SetActive(true);

        if (promptText != null)
        {
            string label = MenuBindingStore.GetDisplayName(MenuActionId.Interact);
            promptText.text = string.Format(promptTemplate, NomeTipo(), label);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Panel 2 — SUCESSO
    // ─────────────────────────────────────────────────────────────────────────

    private void MostrarSucesso(int pontosGanhos, int totalDescartado)
    {
        EsconderTodosPanels();

        if (sucessoPanel != null) sucessoPanel.SetActive(true);

        if (sucessoText != null)
            sucessoText.text = string.Format(sucessoTemplate, pontosGanhos, totalDescartado);

        // Após o tempo, volta ao prompt automaticamente
        AgendarVoltaAoPrompt(sucessoDuration);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Panel 3 — ERRO
    // ─────────────────────────────────────────────────────────────────────────

    private void MostrarErro()
    {
        EsconderTodosPanels();

        if (erroPanel != null) erroPanel.SetActive(true);

        if (erroText != null)
            erroText.text = string.Format(erroTemplate, NomeTipo());

        // Após o tempo, volta ao prompt automaticamente
        AgendarVoltaAoPrompt(erroDuration);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Volta ao prompt após feedback (sucesso ou erro)
    // ─────────────────────────────────────────────────────────────────────────

    private void AgendarVoltaAoPrompt(float delay)
    {
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);

        if (delay > 0f)
            feedbackCoroutine = StartCoroutine(VoltarAoPromptAposDelay(delay));
    }

    private System.Collections.IEnumerator VoltarAoPromptAposDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackCoroutine = null;

        // Só volta ao prompt se o jogador ainda estiver na área
        if (playerInRange)
            ShowPrompt();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Esconde todos os panels de uma vez
    // ─────────────────────────────────────────────────────────────────────────

    private void EsconderTodosPanels()
    {
        if (feedbackCoroutine != null) { StopCoroutine(feedbackCoroutine); feedbackCoroutine = null; }

        if (promptPanel  != null) promptPanel.SetActive(false);
        if (sucessoPanel != null) sucessoPanel.SetActive(false);
        if (erroPanel    != null) erroPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsPlayer(Collider2D other)
    {
        if (other == null) return false;
        if (other.CompareTag("Player")) return true;
        return other.GetComponentInParent<Player>() != null;
    }

    private void HandleBindingsChanged()
    {
        // Atualiza o label do botão no prompt se o jogador estiver na área
        if (playerInRange && promptPanel != null && promptPanel.activeSelf)
            ShowPrompt();
    }
}