using UnityEngine;
using UnityEngine.SceneManagement; // Necessário se for mudar de cena

public class MenuHandler : MonoBehaviour
{
    // Opção A: Se o menu for apenas um Painel (UI Panel) que abre e fecha
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;
    public GameObject VolumePanel;

    public void VoltarParaMenu()
    {
        // Desativa a tela de configurações e ativa a principal
        SettingsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void VoltarParaSettings()
    {
        // Desativa a tela de volume e ativa a de configurações
        VolumePanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void AbrirVolume()
    {
        // Desativa a tela de volume e ativa a de configurações
        VolumePanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }

    public void IniciarJogo()
    {
        // Exemplo de como iniciar o jogo, pode ser adaptado para abrir um painel de seleção de personagem ou opções
        SceneManager.LoadScene("SampleScene"); // Substitua pelo nome da cena do jogo
    }
}