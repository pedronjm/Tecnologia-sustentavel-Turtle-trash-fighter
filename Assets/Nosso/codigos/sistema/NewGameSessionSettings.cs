using UnityEngine;

public enum PlayableCharacterId
{
    Warrior,
    Archer,
    Mage,
}

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard,
}

public static class NewGameSessionSettings
{
    private const string CharacterKey = "TurtleTrashFighter.NewGame.Character";
    private const string TutorialKey = "TurtleTrashFighter.NewGame.PlayTutorial";
    private const string DifficultyKey = "TurtleTrashFighter.NewGame.Difficulty";

    public static PlayableCharacterId SelectedCharacter { get; private set; } = PlayableCharacterId.Warrior;
    public static bool PlayTutorial { get; private set; } = true;
    public static GameDifficulty Difficulty { get; private set; } = GameDifficulty.Normal;

    static NewGameSessionSettings()
    {
        Load();
    }

    public static void Apply(PlayableCharacterId character, bool playTutorial, GameDifficulty difficulty)
    {
        SelectedCharacter = character;
        PlayTutorial = playTutorial;
        Difficulty = difficulty;
        Save();
    }

    public static void Load()
    {
        SelectedCharacter = (PlayableCharacterId)PlayerPrefs.GetInt(CharacterKey, (int)PlayableCharacterId.Warrior);
        PlayTutorial = PlayerPrefs.GetInt(TutorialKey, 1) == 1;
        Difficulty = (GameDifficulty)PlayerPrefs.GetInt(DifficultyKey, (int)GameDifficulty.Normal);
    }

    public static float GetDifficultyMultiplier()
    {
        return Difficulty switch
        {
            GameDifficulty.Easy => 0.8f,
            GameDifficulty.Normal => 1f,
            GameDifficulty.Hard => 1.2f,
            _ => 1f,
        };
    }

    private static void Save()
    {
        PlayerPrefs.SetInt(CharacterKey, (int)SelectedCharacter);
        PlayerPrefs.SetInt(TutorialKey, PlayTutorial ? 1 : 0);
        PlayerPrefs.SetInt(DifficultyKey, (int)Difficulty);
        PlayerPrefs.Save();
    }
}
