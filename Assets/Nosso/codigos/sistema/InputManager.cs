using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        // Configuração Padrão
        keys.Add("Esquerda", KeyCode.A);
        keys.Add("Direita", KeyCode.D);
        keys.Add("Pulo", KeyCode.Space);
        keys.Add("Dash", KeyCode.LeftShift);
        keys.Add("Checkpoint", KeyCode.E);
        keys.Add("AtaqueMelee", KeyCode.Mouse0);
        keys.Add("AtaqueRanged", KeyCode.Mouse1);
    }

    public bool GetKeyDown(string acao) => Input.GetKeyDown(keys[acao]);
    public bool GetKey(string acao) => Input.GetKey(keys[acao]);
}