using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class MenuKeybindsController : MonoBehaviour
{
    [Serializable]
    public class BindingRow
    {
        public MenuActionId action;
        public Button rebindButton;
        public TMP_Text bindingLabel;
    }

    [SerializeField]
    private BindingRow[] rows;

    [SerializeField]
    private TMP_Text statusLabel;

    [SerializeField]
    private Button resetButton;

    [Header("Popup de Rebind")]
    [SerializeField]
    private GameObject rebindPopupPanel;

    [SerializeField]
    private TMP_Text rebindPopupText;

    [TextArea]
    [SerializeField]
    private string rebindPopupMessage =
        "Pressione uma tecla ou botão do mouse para {0}. ESC cancela.";

    private int activeRowIndex = -1;
    private bool hasWiredButtons;

    private void Awake()
    {
        Debug.Log("MenuKeybindsController.Awake()");
        if (statusLabel != null)
            statusLabel.text = "MenuKeybindsController Awake";
        MenuBindingStore.EnsureLoaded();
        HidePopup();
        WireButtonsOnce();
    }

    private void Start()
    {
        Debug.Log("MenuKeybindsController.Start()");
        if (statusLabel != null)
            statusLabel.text = "MenuKeybindsController Start";
        RefreshAllLabels();
    }

    private void OnEnable()
    {
        Debug.Log("MenuKeybindsController.OnEnable()");
        if (statusLabel != null)
            statusLabel.text = "MenuKeybindsController OnEnable";
        WireButtonsOnce();
        RefreshAllLabels();

        // Toda vez que a tela abre (menu principal ou pause, é o mesmo
        // prefab/script), busca os binds salvos no servidor. Se o jogador
        // não estiver logado, RemoteSaveService.ValidateAuth() apenas loga
        // o erro e a coroutine sai sem quebrar nada — fica valendo o que
        // já está no PlayerPrefs local.
        MenuBindingStore.BindingsChanged += OnRemoteBindingsApplied;
        RemoteSaveService.OnSettingsLoaded += OnRemoteSettingsLoaded;
        RemoteSaveService.getInstance()?.LoadSettings();
    }

    private void OnDisable()
    {
        MenuBindingStore.BindingsChanged -= OnRemoteBindingsApplied;
        RemoteSaveService.OnSettingsLoaded -= OnRemoteSettingsLoaded;
    }

    private void Update()
    {
        if (activeRowIndex < 0)
            return;

        ListenForBinding();
    }

    public void ResetToDefaults()
    {
        MenuBindingStore.ResetToDefaults();
        activeRowIndex = -1;
        SetStatusText("Atalhos restaurados.");
        RefreshAllLabels();
        SaveBindingsRemote();
    }

    private void WireButtonsOnce()
    {
        Debug.Log("WireButtonsOnce() called");
        if (hasWiredButtons)
            return;

        for (int index = 0; index < rows.Length; index++)
        {
            int capturedIndex = index;
            BindingRow row = rows[index];

            if (row != null && row.rebindButton != null)
                row.rebindButton.onClick.AddListener(() =>
                {
                    Debug.Log(
                        $"Rebind button clicked for row {capturedIndex} ({rows[capturedIndex].action})"
                    );
                    BeginRebind(capturedIndex);
                });
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetToDefaults);
            Debug.Log("Wired resetButton.onClick -> ResetToDefaults");
            if (statusLabel != null)
                statusLabel.text = "Wired reset button";
        }

        hasWiredButtons = true;
        Debug.Log("WireButtonsOnce() finished wiring buttons");
        if (statusLabel != null)
            statusLabel.text = "Wired rebind buttons";
    }

    public void BeginRebind(int rowIndex)
    {
        if (rows == null || rowIndex < 0 || rowIndex >= rows.Length)
            return;

        activeRowIndex = rowIndex;
        string actionLabel = GetActionLabel(rows[rowIndex].action);
        Debug.Log($"BeginRebind called for {rowIndex} -> {rows[rowIndex].action}");
        ShowPopup(string.Format(rebindPopupMessage, actionLabel));
    }

    private void ListenForBinding()
    {
        BindingRow row = rows[activeRowIndex];
        if (row == null)
            return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelRebind();
                return;
            }

            foreach (KeyControl keyControl in Keyboard.current.allKeys)
            {
                if (keyControl != null && keyControl.wasPressedThisFrame)
                {
                    Debug.Log(
                        $"Key pressed captured: {keyControl.keyCode} for action {row.action}"
                    );
                    MenuBindingStore.SetKeyboardBinding(row.action, keyControl.keyCode);
                    FinishRebind(
                        $"{GetActionLabel(row.action)} alterado para {keyControl.keyCode}"
                    );
                    return;
                }
            }
        }

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log($"Mouse Left pressed for action {row.action}");
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Left);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Esquerdo");
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log($"Mouse Right pressed for action {row.action}");
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Right);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Direito");
            return;
        }

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            Debug.Log($"Mouse Middle pressed for action {row.action}");
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Middle);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Meio");
            return;
        }

        if (Mouse.current.forwardButton.wasPressedThisFrame)
        {
            Debug.Log($"Mouse Forward pressed for action {row.action}");
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Forward);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Forward");
            return;
        }

        if (Mouse.current.backButton.wasPressedThisFrame)
        {
            Debug.Log($"Mouse Back pressed for action {row.action}");
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Back);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Back");
        }
    }

    private void FinishRebind(string message)
    {
        activeRowIndex = -1;
        HidePopup();
        SetStatusText(message);
        RefreshAllLabels();
        SaveBindingsRemote();
    }

    private void CancelRebind()
    {
        activeRowIndex = -1;
        HidePopup();
        SetStatusText("Troca de atalho cancelada.");
    }

    private void RefreshAllLabels()
    {
        if (rows == null)
            return;

        foreach (BindingRow row in rows)
        {
            if (row == null || row.bindingLabel == null)
                continue;

            string bindingText = MenuBindingStore.GetDisplayName(row.action);
            row.bindingLabel.text = $"{GetActionLabel(row.action)}: {bindingText}";
        }
    }

    private void SetStatusText(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }

    private void ShowPopup(string message)
    {
        if (rebindPopupPanel != null)
            rebindPopupPanel.SetActive(true);

        if (rebindPopupText != null)
            rebindPopupText.text = message;
    }

    private void HidePopup()
    {
        if (rebindPopupPanel != null)
            rebindPopupPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Sincronização com o servidor (/settings). Mantém o jogo 100%
    // funcional offline: se não houver login ou a requisição falhar, o
    // RemoteSaveService só loga o erro e os binds locais (PlayerPrefs)
    // continuam valendo normalmente.
    // ─────────────────────────────────────────────────────────────────────

    private void SaveBindingsRemote()
    {
        RemoteSaveService.getInstance()?.SaveSettings();
    }

    // Chamado quando o GET /settings termina e os valores já foram
    // aplicados no MenuBindingStore (via ApplyFromRemote).
    private void OnRemoteSettingsLoaded()
    {
        RefreshAllLabels();
    }

    // Chamado por qualquer mudança nos bindings (local ou remota),
    // garante que a UI nunca fique desatualizada independente da origem.
    private void OnRemoteBindingsApplied()
    {
        RefreshAllLabels();
    }

    private static string GetActionLabel(MenuActionId action)
    {
        return action switch
        {
            MenuActionId.MoveLeft => "Mover esquerda",
            MenuActionId.MoveRight => "Mover direita",
            MenuActionId.Jump => "Pular",
            MenuActionId.Dash => "Dash",
            MenuActionId.Interact => "Interagir",
            MenuActionId.MeleeAttack => "Ataque corpo a corpo",
            MenuActionId.RangedAttack => "Ataque à distância",
            _ => action.ToString(),
        };
    }
}
