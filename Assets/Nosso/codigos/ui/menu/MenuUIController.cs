using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject settingsPanel;

    [SerializeField]
    private GameObject volumePanel;

    [SerializeField]
    private GameObject keybindsPanel;

    [SerializeField]
    private GameObject contactsPanel;

    [SerializeField]
    private GameObject savesPanel;

    [Header("Authentication")]
    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject registerPanel;

    [Header("New Game")]
    [SerializeField]
    private GameObject newGameCharacterPanel;

    [SerializeField]
    private GameObject newGameOptionsPanel;

    [Header("Game")]
    [SerializeField]
    private string gameSceneName = "SampleScene";

    private void Awake()
    {
        MenuBindingStore.EnsureLoaded();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ShowLogin();
    }

    public void ShowMainMenu()
    {
        SetOnly(mainMenuPanel);
    }

    public void ShowSettings()
    {
        SetOnly(settingsPanel);
    }

    public void ShowVolume()
    {
        SetOnly(volumePanel);
    }

    public void ShowKeybinds()
    {
        SetOnly(keybindsPanel);
    }

    public void ShowContacts()
    {
        SetOnly(contactsPanel);
    }

    public void ShowSaves()
    {
        SetOnly(savesPanel);
    }

    // LOGIN

    public void ShowLogin()
    {
        SetOnly(loginPanel);
    }

    // CADASTRO

    public void ShowRegister()
    {
        SetOnly(registerPanel);
    }

    public void StartGame()
    {
        ShowSaves();
    }

    public void ShowNewGameCharacter()
    {
        SetOnly(newGameCharacterPanel);
    }

    public void ShowNewGameOptions()
    {
        SetOnly(newGameOptionsPanel);
    }

    public void LoadConfiguredGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetOnly(GameObject visiblePanel)
    {
        SetPanelActive(mainMenuPanel, visiblePanel == mainMenuPanel);

        SetPanelActive(settingsPanel, visiblePanel == settingsPanel);

        SetPanelActive(volumePanel, visiblePanel == volumePanel);

        SetPanelActive(keybindsPanel, visiblePanel == keybindsPanel);

        SetPanelActive(contactsPanel, visiblePanel == contactsPanel);

        SetPanelActive(savesPanel, visiblePanel == savesPanel);

        SetPanelActive(loginPanel, visiblePanel == loginPanel);

        SetPanelActive(registerPanel, visiblePanel == registerPanel);

        SetPanelActive(newGameCharacterPanel, visiblePanel == newGameCharacterPanel);

        SetPanelActive(newGameOptionsPanel, visiblePanel == newGameOptionsPanel);
    }

    private static void SetPanelActive(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }
}
