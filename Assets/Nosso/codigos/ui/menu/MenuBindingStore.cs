using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public enum MenuActionId
{
    MoveLeft = 0,
    MoveRight = 1,
    Jump = 2,
    Dash = 3,
    MeleeAttack = 4,
    RangedAttack = 5,
    Interact = 6,
}

public enum MouseButton
{
    Forward = 0,
    Back = 1,
    Left = 2,
    Right = 3,
    Middle = 4,
}

public static class MenuBindingStore
{
    [Serializable]
    private struct BindingData
    {
        public bool isMouse;
        public Key keyboardKey;
        public MouseButton mouseButton;
    }

    private const string PrefsPrefix = "TurtleTrashFighter.Binding.";

    private static readonly Dictionary<MenuActionId, BindingData> bindings = new()
    {
        [MenuActionId.MoveLeft] = new BindingData { isMouse = false, keyboardKey = Key.A },
        [MenuActionId.MoveRight] = new BindingData { isMouse = false, keyboardKey = Key.D },
        [MenuActionId.Jump] = new BindingData { isMouse = false, keyboardKey = Key.Space },
        [MenuActionId.Dash] = new BindingData { isMouse = false, keyboardKey = Key.LeftShift },
        [MenuActionId.Interact] = new BindingData { isMouse = false, keyboardKey = Key.E },
        [MenuActionId.MeleeAttack] = new BindingData
        {
            isMouse = true,
            mouseButton = MouseButton.Left,
        },
        [MenuActionId.RangedAttack] = new BindingData
        {
            isMouse = true,
            mouseButton = MouseButton.Right,
        },
    };

    private static bool isLoaded;

    public static event Action BindingsChanged;

    public static void EnsureLoaded()
    {
        if (isLoaded)
            return;

        Load();
    }

    public static void Load()
    {
        foreach (MenuActionId action in Enum.GetValues(typeof(MenuActionId)))
        {
            string rawValue = PlayerPrefs.GetString(PrefsPrefix + action, string.Empty);
            if (TryParseBinding(rawValue, out BindingData parsedBinding))
            {
                bindings[action] = parsedBinding;
            }
        }

        isLoaded = true;
    }

    public static void ResetToDefaults()
    {
        bindings[MenuActionId.MoveLeft] = new BindingData { isMouse = false, keyboardKey = Key.A };
        bindings[MenuActionId.MoveRight] = new BindingData { isMouse = false, keyboardKey = Key.D };
        bindings[MenuActionId.Jump] = new BindingData { isMouse = false, keyboardKey = Key.Space };
        bindings[MenuActionId.Dash] = new BindingData
        {
            isMouse = false,
            keyboardKey = Key.LeftShift,
        };
        bindings[MenuActionId.Interact] = new BindingData { isMouse = false, keyboardKey = Key.E };
        bindings[MenuActionId.MeleeAttack] = new BindingData
        {
            isMouse = true,
            mouseButton = MouseButton.Left,
        };
        bindings[MenuActionId.RangedAttack] = new BindingData
        {
            isMouse = true,
            mouseButton = MouseButton.Right,
        };

        isLoaded = true;
        SaveAll();
        BindingsChanged?.Invoke();
    }

    public static void SaveAll()
    {
        foreach (MenuActionId action in Enum.GetValues(typeof(MenuActionId)))
        {
            SaveBinding(action);
        }

        PlayerPrefs.Save();
    }

    public static void SaveBinding(MenuActionId action)
    {
        EnsureLoaded();
        PlayerPrefs.SetString(PrefsPrefix + action, SerializeBinding(bindings[action]));
    }

    public static string GetDisplayName(MenuActionId action)
    {
        EnsureLoaded();
        BindingData binding = bindings[action];

        if (binding.isMouse)
            return GetMouseLabel(binding.mouseButton);

        return GetKeyLabel(binding.keyboardKey);
    }

    public static bool IsPressed(MenuActionId action)
    {
        EnsureLoaded();
        BindingData binding = bindings[action];

        if (binding.isMouse)
        {
            Mouse mouse = Mouse.current;
            return mouse != null && GetMouseButton(mouse, binding.mouseButton).isPressed;
        }

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[binding.keyboardKey].isPressed;
    }

    public static bool WasPressedThisFrame(MenuActionId action)
    {
        EnsureLoaded();
        BindingData binding = bindings[action];

        if (binding.isMouse)
        {
            Mouse mouse = Mouse.current;
            return mouse != null && GetMouseButton(mouse, binding.mouseButton).wasPressedThisFrame;
        }

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[binding.keyboardKey].wasPressedThisFrame;
    }

    public static void SetKeyboardBinding(MenuActionId action, Key key)
    {
        EnsureLoaded();

        BindingData currentBinding = bindings[action];
        if (!currentBinding.isMouse && currentBinding.keyboardKey == key)
            return;

        MenuActionId? conflictingAction = FindKeyboardAction(key);
        if (conflictingAction.HasValue && conflictingAction.Value != action)
            bindings[conflictingAction.Value] = currentBinding;

        bindings[action] = new BindingData { isMouse = false, keyboardKey = key };
        isLoaded = true;
        SaveAll();
        BindingsChanged?.Invoke();
    }

    public static void SetMouseBinding(MenuActionId action, MouseButton mouseButton)
    {
        EnsureLoaded();

        BindingData currentBinding = bindings[action];
        if (currentBinding.isMouse && currentBinding.mouseButton == mouseButton)
            return;

        MenuActionId? conflictingAction = FindMouseAction(mouseButton);
        if (conflictingAction.HasValue && conflictingAction.Value != action)
            bindings[conflictingAction.Value] = currentBinding;

        bindings[action] = new BindingData { isMouse = true, mouseButton = mouseButton };
        isLoaded = true;
        SaveAll();
        BindingsChanged?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Integração com o backend remoto (RemoteSaveService / GET-PUT /settings).
    // Reaproveita o mesmo formato de string já usado no PlayerPrefs
    // ("key:A" / "mouse:Left"), então nenhuma tradução extra é necessária:
    // o que sai daqui é exatamente o que volta do servidor.
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Monta os 7 valores atuais de binding no formato esperado pelo
    /// payload remoto (mesmas chaves usadas no DTO do backend).
    /// </summary>
    public static RemoteBindingsPayload ExportForRemote()
    {
        EnsureLoaded();

        return new RemoteBindingsPayload
        {
            keyEsquerda = SerializeBinding(bindings[MenuActionId.MoveLeft]),
            keyDireita = SerializeBinding(bindings[MenuActionId.MoveRight]),
            keyDash = SerializeBinding(bindings[MenuActionId.Dash]),
            keyInteragir = SerializeBinding(bindings[MenuActionId.Interact]),
            keyPular = SerializeBinding(bindings[MenuActionId.Jump]),
            keyMelee = SerializeBinding(bindings[MenuActionId.MeleeAttack]),
            keyRanger = SerializeBinding(bindings[MenuActionId.RangedAttack]),
        };
    }

    /// <summary>
    /// Aplica os 7 valores recebidos do servidor (GET /settings) e
    /// persiste localmente no PlayerPrefs, mantendo os dois em sincronia.
    /// Valores vazios ou inválidos são ignorados (mantém o binding atual),
    /// para não derrubar o controle do jogador por um campo nulo isolado.
    /// </summary>
    public static void ApplyFromRemote(RemoteBindingsPayload remote)
    {
        if (remote == null)
            return;

        EnsureLoaded();

        TryApplyOne(MenuActionId.MoveLeft, remote.keyEsquerda);
        TryApplyOne(MenuActionId.MoveRight, remote.keyDireita);
        TryApplyOne(MenuActionId.Dash, remote.keyDash);
        TryApplyOne(MenuActionId.Interact, remote.keyInteragir);
        TryApplyOne(MenuActionId.Jump, remote.keyPular);
        TryApplyOne(MenuActionId.MeleeAttack, remote.keyMelee);
        TryApplyOne(MenuActionId.RangedAttack, remote.keyRanger);

        isLoaded = true;
        SaveAll();
        BindingsChanged?.Invoke();
    }

    private static void TryApplyOne(MenuActionId action, string rawValue)
    {
        if (TryParseBinding(rawValue, out BindingData parsedBinding))
        {
            bindings[action] = parsedBinding;
        }
    }

    private static MenuActionId? FindKeyboardAction(Key key)
    {
        foreach (KeyValuePair<MenuActionId, BindingData> pair in bindings)
        {
            if (!pair.Value.isMouse && pair.Value.keyboardKey == key)
                return pair.Key;
        }

        return null;
    }

    private static MenuActionId? FindMouseAction(MouseButton mouseButton)
    {
        foreach (KeyValuePair<MenuActionId, BindingData> pair in bindings)
        {
            if (pair.Value.isMouse && pair.Value.mouseButton == mouseButton)
                return pair.Key;
        }

        return null;
    }

    private static string SerializeBinding(BindingData binding)
    {
        return binding.isMouse ? $"mouse:{binding.mouseButton}" : $"key:{binding.keyboardKey}";
    }

    private static bool TryParseBinding(string rawValue, out BindingData binding)
    {
        binding = default;

        if (string.IsNullOrWhiteSpace(rawValue))
            return false;

        if (rawValue.StartsWith("mouse:", StringComparison.OrdinalIgnoreCase))
        {
            string buttonName = rawValue.Substring("mouse:".Length);
            if (Enum.TryParse(buttonName, true, out MouseButton mouseButton))
            {
                binding = new BindingData { isMouse = true, mouseButton = mouseButton };
                return true;
            }

            return false;
        }

        if (rawValue.StartsWith("key:", StringComparison.OrdinalIgnoreCase))
        {
            string keyName = rawValue.Substring("key:".Length);
            if (Enum.TryParse(keyName, true, out Key key))
            {
                binding = new BindingData { isMouse = false, keyboardKey = key };
                return true;
            }
        }

        return false;
    }

    private static ButtonControl GetMouseButton(Mouse mouse, MouseButton mouseButton)
    {
        return mouseButton switch
        {
            MouseButton.Left => mouse.leftButton,
            MouseButton.Right => mouse.rightButton,
            MouseButton.Middle => mouse.middleButton,
            MouseButton.Forward => mouse.forwardButton,
            MouseButton.Back => mouse.backButton,
            _ => mouse.leftButton,
        };
    }

    private static string GetKeyLabel(Key key)
    {
        return key == Key.None ? "Desativado" : key.ToString();
    }

    private static string GetMouseLabel(MouseButton mouseButton)
    {
        return mouseButton switch
        {
            MouseButton.Left => "Mouse Esquerdo",
            MouseButton.Right => "Mouse Direito",
            MouseButton.Middle => "Mouse Meio",
            MouseButton.Forward => "Mouse Forward",
            MouseButton.Back => "Mouse Back",
            _ => mouseButton.ToString(),
        };
    }
}

/// <summary>
/// Espelha o GameSettingsUpsertRequest / GameSettingsResponse do backend.
/// Usado tanto para enviar (PUT /settings) quanto para receber (GET /settings).
/// Os 3 campos de volume são mantidos aqui só para casar com o JSON do
/// servidor; por enquanto são sempre enviados como 0 (áudio ainda não
/// está implementado no jogo).
/// </summary>
[Serializable]
public class RemoteBindingsPayload
{
    public string keyEsquerda;
    public string keyDireita;
    public string keyDash;
    public string keyInteragir;
    public string keyPular;
    public string keyMelee;
    public string keyRanger;

    public float volumeGeral;
    public float volumeMusica;
    public float volumeSfx;
}
