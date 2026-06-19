using TMPro;
using UnityEngine;

public class MenuNewGameFlowController : MonoBehaviour
{
    [SerializeField]
    private MenuUIController menuUIController;

    [SerializeField]
    private TMP_Text summaryLabel;

    private int selectedSlot = -1;

    [SerializeField]
    private TMP_Text statusLabel;

    private bool playTutorial = true;
    private GameDifficulty selectedDifficulty = GameDifficulty.Normal;

    [Header("Tutorial Buttons")]
    [SerializeField]
    private GameObject tutorialYesButton; // Botão "Sim"

    [SerializeField]
    private GameObject tutorialNoButton; // Botão "Não"

    [Header("Difficult Buttons")]
    [SerializeField]
    private GameObject EasyButton; // Botão "faxcil"

    [SerializeField]
    private GameObject NormalButton; // Botão "Normal"

    [SerializeField]
    private GameObject HardButton; // Botão "Difícil"

    private void Awake()
    {
        if (menuUIController == null)
            menuUIController = FindFirstObjectByType<MenuUIController>();

        SyncWithLastSelection();
    }

    public void BeginFlow()
    {
        SyncWithLastSelection();
        SetStatus(string.Empty);
        RefreshSummary();
        menuUIController?.ShowNewGameOptions();
    }

    public void SetSelectedSlot(int slot)
    {
        selectedSlot = slot;

        if (CurrentSaveSession.instance != null)
        {
            CurrentSaveSession.instance.SetSlot(slot);
        }

        Debug.Log("Novo jogo será salvo no Slot: " + slot);
    }

    public void SetPlayTutorial(bool value)
    {
        playTutorial = value;
        RefreshSummary();
    }

    // Esse método vai ser chamado no botão "Sim"
    public void SelectTutorialYes()
    {
        playTutorial = true;
        tutorialYesButton.SetActive(false); // Esconde o "Sim"
        tutorialNoButton.SetActive(true); // Mostra o "Não"
        RefreshSummary();
    }

    // Esse método vai ser chamado no botão "Não"
    public void SelectTutorialNo()
    {
        playTutorial = false;
        tutorialNoButton.SetActive(false); // Esconde o "Não"
        tutorialYesButton.SetActive(true); // Mostra o "Sim"
        RefreshSummary();
    }

    public void SetDifficultyEasy()
    {
        selectedDifficulty = GameDifficulty.Easy;
        EasyButton.SetActive(false);
        NormalButton.SetActive(true);
        HardButton.SetActive(false);
        RefreshSummary();
    }

    public void SetDifficultyNormal()
    {
        selectedDifficulty = GameDifficulty.Normal;
        EasyButton.SetActive(false);
        NormalButton.SetActive(false);
        HardButton.SetActive(true);
        RefreshSummary();
    }

    public void SetDifficultyHard()
    {
        selectedDifficulty = GameDifficulty.Hard;
        EasyButton.SetActive(true);
        NormalButton.SetActive(false);
        HardButton.SetActive(false);
        RefreshSummary();
    }

    public void ConfirmAndStartGame()
    {
        if (selectedSlot < 0)
        {
            Debug.Log("Nenhum slot selecionado.");
            return;
        }

        PlayableCharacterId characterId = PlayableCharacterId.Warrior;

        NewGameSessionSettings.Apply(characterId, playTutorial, selectedDifficulty);

        // cria o save local
        CurrentSaveSession.instance.SetSlot(selectedSlot);

        var service = RemoteSaveService.getInstance();

        if (service != null)
        {
            service.SaveGame(selectedSlot + 1);
        }
        else
        {
            Debug.LogWarning(
                "RemoteSaveService não encontrado. O progresso do novo jogo pode não ser salvo remotamente."
            );
        }
        SetStatus("Iniciando partida...");

        menuUIController?.LoadConfiguredGameScene();
    }

    public void CancelFlow()
    {
        SetStatus(string.Empty);
        menuUIController?.ShowSaves();
    }

    private void SyncWithLastSelection()
    {
        NewGameSessionSettings.Load();
        playTutorial = NewGameSessionSettings.PlayTutorial;
        selectedDifficulty = NewGameSessionSettings.Difficulty;

        // Ajusta o visual inicial dos botões ao carregar
        if (playTutorial)
        {
            tutorialYesButton.SetActive(false); // Está no "Sim", então escondemos "Sim"
            tutorialNoButton.SetActive(true); // Mostramos o "Não"
        }
        else
        {
            tutorialYesButton.SetActive(true); // Está no "Não", então mostramos "Sim"
            tutorialNoButton.SetActive(false); // Escondemos o "Não"
        }
    }

    private void RefreshSummary()
    {
        if (summaryLabel == null)
            return;

        string tutorialText = playTutorial ? "Sim" : "Nao";
        string difficultyText = selectedDifficulty switch
        {
            GameDifficulty.Easy => "Facil",
            GameDifficulty.Normal => "Normal",
            GameDifficulty.Hard => "Dificil",
            _ => "Normal",
        };

        summaryLabel.text =
            $"Personagem: Guerreiro\n"
            + $"Tutorial: {tutorialText}\n"
            + $"Dificuldade: {difficultyText}";
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }
}
