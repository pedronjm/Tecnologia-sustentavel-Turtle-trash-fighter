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

    private int activeRowIndex = -1;
    private bool hasWiredButtons;

    private void Awake()
    {
        MenuBindingStore.EnsureLoaded();
    }

    private void Start()
    {
        WireButtonsOnce();
        RefreshAllLabels();
    }

    private void OnEnable()
    {
        RefreshAllLabels();
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
    }

    private void WireButtonsOnce()
    {
        if (hasWiredButtons)
            return;

        for (int index = 0; index < rows.Length; index++)
        {
            int capturedIndex = index;
            BindingRow row = rows[index];

            if (row != null && row.rebindButton != null)
                row.rebindButton.onClick.AddListener(() => BeginRebind(capturedIndex));
        }

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);

        hasWiredButtons = true;
    }

    public void BeginRebind(int rowIndex)
    {
        if (rows == null || rowIndex < 0 || rowIndex >= rows.Length)
            return;

        activeRowIndex = rowIndex;
        string actionLabel = GetActionLabel(rows[rowIndex].action);
        SetStatusText($"Pressione uma tecla ou clique um botão do mouse para {actionLabel}");
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

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None)
                    continue;

                KeyControl keyControl = Keyboard.current[key];
                if (keyControl != null && keyControl.wasPressedThisFrame)
                {
                    MenuBindingStore.SetKeyboardBinding(row.action, key);
                    FinishRebind($"{GetActionLabel(row.action)} alterado para {key}");
                    return;
                }
            }
        }

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Left);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Esquerdo");
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Right);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Direito");
            return;
        }

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Middle);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Meio");
            return;
        }

        if (Mouse.current.forwardButton.wasPressedThisFrame)
        {
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Forward);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Forward");
            return;
        }

        if (Mouse.current.backButton.wasPressedThisFrame)
        {
            MenuBindingStore.SetMouseBinding(row.action, MouseButton.Back);
            FinishRebind($"{GetActionLabel(row.action)} alterado para Mouse Back");
        }
    }

    private void FinishRebind(string message)
    {
        activeRowIndex = -1;
        SetStatusText(message);
        RefreshAllLabels();
    }

    private void CancelRebind()
    {
        activeRowIndex = -1;
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

    private static string GetActionLabel(MenuActionId action)
    {
        return action switch
        {
            MenuActionId.MoveLeft => "Mover esquerda",
            MenuActionId.MoveRight => "Mover direita",
            MenuActionId.Jump => "Pular",
            MenuActionId.Dash => "Dash",
            MenuActionId.MeleeAttack => "Ataque corpo a corpo",
            MenuActionId.RangedAttack => "Ataque à distância",
            _ => action.ToString(),
        };
    }
}
