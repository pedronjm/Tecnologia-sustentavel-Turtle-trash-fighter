using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject painelPause; // Arraste o seu painel de menu aqui
    private bool jogoPausado = false;

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (jogoPausado)
                Retomar();
            else
                Pausar();
        }
    }

    public void Retomar()
    {
        painelPause.SetActive(false);
        Time.timeScale = 1f; // Volta o tempo do jogo ao normal
        jogoPausado = false;
    }

    void Pausar()
    {
        painelPause.SetActive(true);
        Time.timeScale = 0f; // Congela o jogo (física, movimentos, etc)
        jogoPausado = true;
    }

    public void IrParaMenuPrincipal()
    {
        Time.timeScale = 1f; // Importante: reseta o tempo antes de mudar de cena
        SceneManager.LoadScene("menu");
    }
}
