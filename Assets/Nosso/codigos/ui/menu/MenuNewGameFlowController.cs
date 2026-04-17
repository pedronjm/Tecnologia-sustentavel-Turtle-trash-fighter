using System;
using TMPro;
using UnityEngine;

public class MenuNewGameFlowController : MonoBehaviour
{
    [Serializable]
    private struct CharacterOption
    {
        public PlayableCharacterId id;
        public string displayName;
    }

    [SerializeField] private MenuUIController menuUIController;
    [SerializeField] private TMP_Text selectedCharacterLabel;
    [SerializeField] private TMP_Text summaryLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private CharacterOption[] characterOptions =
    {
        new CharacterOption { id = PlayableCharacterId.Warrior, displayName = "Guerreiro" },
        new CharacterOption { id = PlayableCharacterId.Archer, displayName = "Arqueiro" },
        new CharacterOption { id = PlayableCharacterId.Mage, displayName = "Mago" },
    };

    private int selectedCharacterIndex;
    private bool playTutorial;
    private GameDifficulty selectedDifficulty;

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
        RefreshCharacterLabel();
        RefreshSummary();
        menuUIController?.ShowNewGameCharacter();
    }

    public void SelectCharacterByIndex(int index)
    {
        if (characterOptions == null || index < 0 || index >= characterOptions.Length)
            return;

        selectedCharacterIndex = index;
        RefreshCharacterLabel();
    }

    public void ContinueToOptions()
    {
        if (characterOptions == null || characterOptions.Length == 0)
        {
            SetStatus("Nenhum personagem configurado.");
            return;
        }

        RefreshSummary();
        SetStatus(string.Empty);
        menuUIController?.ShowNewGameOptions();
    }

    public void BackToCharacterSelection()
    {
        menuUIController?.ShowNewGameCharacter();
    }

    public void SetPlayTutorial(bool value)
    {
        playTutorial = value;
        RefreshSummary();
    }

    public void SetDifficultyEasy()
    {
        selectedDifficulty = GameDifficulty.Easy;
        RefreshSummary();
    }

    public void SetDifficultyNormal()
    {
        selectedDifficulty = GameDifficulty.Normal;
        RefreshSummary();
    }

    public void SetDifficultyHard()
    {
        selectedDifficulty = GameDifficulty.Hard;
        RefreshSummary();
    }

    public void ConfirmAndStartGame()
    {
        PlayableCharacterId characterId = GetSelectedCharacterId();
        NewGameSessionSettings.Apply(characterId, playTutorial, selectedDifficulty);

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
        selectedCharacterIndex = FindCharacterIndex(NewGameSessionSettings.SelectedCharacter);

        if (selectedCharacterIndex < 0)
            selectedCharacterIndex = 0;
    }

    private int FindCharacterIndex(PlayableCharacterId id)
    {
        if (characterOptions == null)
            return -1;

        for (int i = 0; i < characterOptions.Length; i++)
        {
            if (characterOptions[i].id == id)
                return i;
        }

        return -1;
    }

    private PlayableCharacterId GetSelectedCharacterId()
    {
        if (characterOptions == null || characterOptions.Length == 0)
            return PlayableCharacterId.Warrior;

        int clamped = Mathf.Clamp(selectedCharacterIndex, 0, characterOptions.Length - 1);
        return characterOptions[clamped].id;
    }

    private void RefreshCharacterLabel()
    {
        if (selectedCharacterLabel == null)
            return;

        string name = GetSelectedCharacterName();
        selectedCharacterLabel.text = $"Personagem selecionado: {name}";
    }

    private string GetSelectedCharacterName()
    {
        if (characterOptions == null || characterOptions.Length == 0)
            return "Nao definido";

        int clamped = Mathf.Clamp(selectedCharacterIndex, 0, characterOptions.Length - 1);
        string displayName = characterOptions[clamped].displayName;
        return string.IsNullOrWhiteSpace(displayName) ? characterOptions[clamped].id.ToString() : displayName;
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
            $"Personagem: {GetSelectedCharacterName()}\nTutorial: {tutorialText}\nDificuldade: {difficultyText}";
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }
}
