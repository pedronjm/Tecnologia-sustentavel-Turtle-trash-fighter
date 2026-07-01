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
    private string Tutorial = "SampleScene";
    private string Nivel1 = "Nao tutorial";

    private bool logado = false;

    private void Awake()
    {
        MenuBindingStore.EnsureLoaded();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Se não tem painel de login configurado, é o menu de pause — vai direto pro menu principal
        if (loginPanel == null)
        {
            ShowMainMenu();
            return;
        }

        MenuBindingStore.EnsureLoaded();

        bool logado =
            RemoteAuthSession.instance != null && RemoteAuthSession.instance.IsAuthenticated;

        if (logado)
            ShowMainMenu();
        else
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

    public void LoadConfiguredGameScene(bool tutorial)
    {
        if (tutorial)
            SceneManager.LoadScene(Tutorial);
        else
            SceneManager.LoadScene(Nivel1);
    }

    public void Logout()
    {
        if (RemoteAuthSession.instance != null && RemoteAuthSession.instance.IsAuthenticated)
        {
            StartCoroutine(LogoutAfterSettingsSaved());
            return;
        }

        PerformLogoutCleanup();
    }

    private System.Collections.IEnumerator LogoutAfterSettingsSaved()
    {
        bool settingsSaved = false;

        void OnSaved()
        {
            settingsSaved = true;
        }

        RemoteSaveService.OnSettingsSaved += OnSaved;

        try
        {
            RemoteSaveService.getInstance()?.SaveSettings();

            float timeout = 5f;
            while (!settingsSaved && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
        }
        finally
        {
            RemoteSaveService.OnSettingsSaved -= OnSaved;
        }

        PerformLogoutCleanup();
    }

    private void PerformLogoutCleanup()
    {
        if (RemoteAuthSession.instance != null)
        {
            RemoteAuthSession.instance.Logout();
        }

        if (CurrentSaveSession.instance != null)
        {
            CurrentSaveSession.instance.ClearSlot();
        }

        MenuBindingStore.ReloadForCurrentUser();
        AudioSettingsManager.getInstance()?.ReloadForCurrentUser();

        ShowLogin();
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
