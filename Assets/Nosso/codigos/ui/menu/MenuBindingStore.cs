using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum MenuActionId
{
    MoveLeft,
    MoveRight,
    Jump,
    Dash,
    MeleeAttack,
    RangedAttack,
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
        [MenuActionId.MeleeAttack] = new BindingData { isMouse = true, mouseButton = MouseButton.Left },
        [MenuActionId.RangedAttack] = new BindingData { isMouse = true, mouseButton = MouseButton.Right },
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
        bindings[MenuActionId.Dash] = new BindingData { isMouse = false, keyboardKey = Key.LeftShift };
        bindings[MenuActionId.MeleeAttack] = new BindingData { isMouse = true, mouseButton = MouseButton.Left };
        bindings[MenuActionId.RangedAttack] = new BindingData { isMouse = true, mouseButton = MouseButton.Right };

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
        bindings[action] = new BindingData { isMouse = false, keyboardKey = key };
        isLoaded = true;
        SaveBinding(action);
        PlayerPrefs.Save();
        BindingsChanged?.Invoke();
    }

    public static void SetMouseBinding(MenuActionId action, MouseButton mouseButton)
    {
        bindings[action] = new BindingData { isMouse = true, mouseButton = mouseButton };
        isLoaded = true;
        SaveBinding(action);
        PlayerPrefs.Save();
        BindingsChanged?.Invoke();
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
