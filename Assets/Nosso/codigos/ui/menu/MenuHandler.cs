using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Necessário para interagir com UI
using TMPro; // Use se estiver usando TextMeshPro // Necessário se for mudar de cena

public class MenuHandler : MonoBehaviour
{
    // Opção A: Se o menu for apenas um Painel (UI Panel) que abre e fecha
    public GameObject MainMenuPanel;
    public GameObject SettingsPanel;
    public GameObject VolumePanel;
    public GameObject ContatoPanel;

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


    [Header("Configurações de Cópia")]
    public string infoContato = "seuemail@exemplo.com";

    // Abre a tela de contato e esconde a de settings
    public void AbrirContato()
    {
        if (ContatoPanel != null && SettingsPanel != null)
        {
            ContatoPanel.SetActive(true);
            SettingsPanel.SetActive(false);
        }
    }

    // Volta para a tela de settings
    public void FecharContato()
    {
        if (ContatoPanel != null && SettingsPanel != null)
        {
            ContatoPanel.SetActive(false);
            SettingsPanel.SetActive(true);
        }
    }

    // Função para copiar o texto
    public void CopiarParaAreaDeTransferencia()
    {
        GUIUtility.systemCopyBuffer = infoContato;
        Debug.Log("Copiado: " + infoContato);
    }
}