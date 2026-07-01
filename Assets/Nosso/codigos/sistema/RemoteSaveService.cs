using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class RemoteSaveService : MonoBehaviour
{
    [Header("Servidor")]
    [Tooltip("Exemplo: http://localhost:8080")]
    public string baseUrl = "http://localhost:8080";

    private static RemoteSaveService instance;
    private SaveResponse pendingLoadSave;
    private string pendingLoadSceneName;
    private int pendingSaveSlotIndex = -1;

    public static event Action OnLoginSuccess;
    public static event Action OnSettingsSaved;

    // Disparado quando GET /settings termina (sucesso ou 1a vez sem dado salvo).
    // MenuKeybindsController pode escutar isso para atualizar a UI assim que
    // os binds remotos chegarem, sem precisar fazer polling.
    public static event Action OnSettingsLoaded;

    // ─────────────────────────────────────────────────────────────────────
    // Cache em memória dos slots, alimentado por CarregarTodosSlots().
    // Substitui o antigo SaveSlotManager (PlayerPrefs / local).
    // ─────────────────────────────────────────────────────────────────────
    public static List<SaveSlotInfo> SlotsCache { get; private set; } = new List<SaveSlotInfo>();
    public const int TotalSlots = 3; // mesmo limite do backend (MIN_SLOT_INDEX..MAX_SLOT_INDEX)

    [Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex; // 0-based (slotIndex do backend - 1)
        public string slotName;
        public string selectedCharacter;
        public string difficulty;
        public float completionPercent;
        public string lastSavedTime;
        public bool hasData;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // ← só destrói, sem logar erro
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static RemoteSaveService getInstance()
    {
        return instance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("RemoteSaveService: Unsubscribed from sceneLoaded event.");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadSave == null)
        {
            if (pendingSaveSlotIndex >= 0)
            {
                StartCoroutine(SaveAfterSceneLoadRoutine(pendingSaveSlotIndex));
                pendingSaveSlotIndex = -1;
            }

            return;
        }

        if (!string.IsNullOrEmpty(pendingLoadSceneName) && scene.name != pendingLoadSceneName)
            return;

        StartCoroutine(ApplyPendingSaveAfterSceneLoad());
    }

    IEnumerator ApplyPendingSaveAfterSceneLoad()
    {
        yield return null;

        if (pendingLoadSave == null)
            yield break;

        var saveToApply = pendingLoadSave;
        ClearPendingLoad();

        AplicarSave(saveToApply);
        Debug.Log("Save aplicado após carregamento da cena.");
    }

    private void ClearPendingLoad()
    {
        pendingLoadSave = null;
        pendingLoadSceneName = string.Empty;
    }

    IEnumerator SaveAfterSceneLoadRoutine(int slotIndex)
    {
        yield return null;

        SaveGame(slotIndex);
    }

    public void Register(string username, string password)
    {
        StartCoroutine(RegisterRoutine(username, password));
    }

    public void Login(string username, string password)
    {
        StartCoroutine(LoginRoutine(username, password));
    }

    public void SaveGame(int slotIndex = 1)
    {
        StartCoroutine(SaveRoutine(slotIndex));
    }

    public void SaveGameAfterSceneLoad(int slotIndex)
    {
        pendingSaveSlotIndex = slotIndex;
    }

    public void LoadGame(int slotIndex = 1)
    {
        StartCoroutine(LoadRoutine(slotIndex));
    }

    // ─────────────────────────────────────────────────────────────────────
    // Configurações (keybinds + volume). Mesmo padrão dos métodos de save:
    // método público inicia a coroutine, que faz a chamada e aplica o
    // resultado de volta no MenuBindingStore.
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Envia os bindings atuais (lidos do MenuBindingStore) para o servidor.
    /// Volume é sempre enviado como 0 por enquanto (áudio ainda não implementado).
    /// </summary>
    public void SaveSettings()
    {
        StartCoroutine(SaveSettingsRoutine());
    }

    /// <summary>
    /// Busca as configurações salvas no servidor e aplica no MenuBindingStore.
    /// Se o jogador nunca salvou nada, o backend retorna os defaults e eles
    /// são aplicados normalmente (não é tratado como erro).
    /// </summary>
    public void LoadSettings()
    {
        StartCoroutine(LoadSettingsRoutine());
    }

    IEnumerator SaveSettingsRoutine()
    {
        if (!ValidateAuth())
            yield break;

        RemoteBindingsPayload payload = MenuBindingStore.ExportForRemote();

        AudioSettingsManager audioManager = AudioSettingsManager.getInstance();

        if (audioManager != null)
        {
            payload.volumeGeral = audioManager.GetVolume(AudioSettingsManager.VolumeChannel.Master);
            payload.volumeMusica = audioManager.GetVolume(AudioSettingsManager.VolumeChannel.Music);
            payload.volumeSfx = audioManager.GetVolume(AudioSettingsManager.VolumeChannel.Sfx);
        }
        else
        {
            payload.volumeGeral = AudioSettingsManager.GetPersistedVolume(
                AudioSettingsManager.VolumeChannel.Master
            );
            payload.volumeMusica = AudioSettingsManager.GetPersistedVolume(
                AudioSettingsManager.VolumeChannel.Music
            );
            payload.volumeSfx = AudioSettingsManager.GetPersistedVolume(
                AudioSettingsManager.VolumeChannel.Sfx
            );
        }

        string json = JsonUtility.ToJson(payload);

        Debug.Log(
            $"[Settings Save] Usuario={RemoteAuthSession.instance?.Username} Master={payload.volumeGeral:0.###} Music={payload.volumeMusica:0.###} Sfx={payload.volumeSfx:0.###}"
        );

        Debug.Log("ENVIO SETTINGS:");
        Debug.Log(json);

        using var www = BuildJsonRequest("PUT", "/settings", json, true);

        yield return www.SendWebRequest();

        Debug.Log("STATUS SETTINGS: " + www.responseCode);
        Debug.Log(www.downloadHandler.text);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro salvar config: {www.responseCode} {www.error}");
            yield break;
        }

        Debug.Log("Config salva");
        OnSettingsSaved?.Invoke();
    }

    IEnumerator LoadSettingsRoutine()
    {
        if (!ValidateAuth())
            yield break;

        using var www = BuildJsonRequest("GET", "/settings", null, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro ao carregar configuracoes: {www.responseCode} - {www.error}");
            yield break;
        }

        var response = JsonUtility.FromJson<RemoteBindingsPayload>(www.downloadHandler.text);

        Debug.Log(
            $"[Settings Load] Usuario={RemoteAuthSession.instance?.Username} Master={response.volumeGeral:0.###} Music={response.volumeMusica:0.###} Sfx={response.volumeSfx:0.###}"
        );
        Debug.Log("[Settings Load] JSON recebido da API: " + www.downloadHandler.text);

        MenuBindingStore.ApplyFromRemote(response);

        AudioSettingsManager audioManager = AudioSettingsManager.getInstance();
        if (audioManager != null)
        {
            audioManager.ApplyFromRemote(
                response.volumeGeral,
                response.volumeMusica,
                response.volumeSfx
            );
        }

        Debug.Log("Configuracoes carregadas do servidor.");

        OnSettingsLoaded?.Invoke();
    }

    IEnumerator RegisterRoutine(string username, string password)
    {
        var req = new AuthRequest
        {
            login = username,
            password = password,
            nome = username,
        };
        var json = JsonUtility.ToJson(req);

        using var www = BuildJsonRequest("POST", "/auth/register", json, false);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"Erro ao registrar: {www.responseCode} - {www.error} - {www.downloadHandler.text}"
            );
            yield break;
        }

        var data = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
        EnsureSession();
        RemoteAuthSession.instance.SetSession(data.login, data.accessToken);
        Debug.Log("Registro concluido e sessao autenticada.");

        MenuBindingStore.ReloadForCurrentUser();
        AudioSettingsManager.getInstance()?.ReloadForCurrentUser();
    }

    IEnumerator LoginRoutine(string username, string password)
    {
        var req = new AuthRequest { login = username, password = password };

        var json = JsonUtility.ToJson(req);

        Debug.Log("Enviando login: " + json);

        using var www = BuildJsonRequest("POST", "/auth/login", json, false);

        yield return www.SendWebRequest();

        Debug.Log("Status API: " + www.responseCode);
        Debug.Log("Resposta: " + www.downloadHandler.text);

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro no login: {www.responseCode} - {www.error}");
            yield break;
        }

        var data = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);

        EnsureSession();
        RemoteAuthSession.instance.SetSession(data.login, data.accessToken);
        Debug.Log("Login concluído");

        MenuBindingStore.ReloadForCurrentUser();
        AudioSettingsManager.getInstance()?.ReloadForCurrentUser();
        OnLoginSuccess?.Invoke();
    }

    IEnumerator SaveRoutine(int slotIndex)
    {
        if (!ValidateAuth())
            yield break;

        var payload = BuildSavePayload(slotIndex);
        var json = JsonUtility.ToJson(payload);

        using var www = BuildJsonRequest("PUT", "/saves", json, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"Erro ao salvar: {www.responseCode} - {www.error} - {www.downloadHandler.text}"
            );
            yield break;
        }

        Debug.Log($"Save remoto concluido (slot {slotIndex}).");
        OnSettingsSaved?.Invoke();

        StartCoroutine(
            CarregarTodosSlots(() =>
            {
                Debug.Log("Cache de saves atualizado");
            })
        );
    }

    IEnumerator LoadRoutine(int slotIndex)
    {
        if (!ValidateAuth())
            yield break;

        using var www = BuildJsonRequest("GET", $"/saves/{slotIndex}", null, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            if (www.responseCode == 404)
            {
                Debug.Log("Nenhum save encontrado.");
                yield break;
            }

            Debug.LogError($"Erro ao carregar: {www.responseCode} - {www.error}");

            yield break;
        }

        var response = JsonUtility.FromJson<SaveResponse>(www.downloadHandler.text);

        Debug.Log("Checkpoint recebido API: " + response.checkpointId);

        pendingLoadSave = response;
        pendingLoadSceneName = string.IsNullOrWhiteSpace(response.sceneName)
            ? "SampleScene"
            : response.sceneName;

        Debug.Log("Carregando cena salva: " + pendingLoadSceneName);
        SceneManager.LoadScene(pendingLoadSceneName);
    }

    public IEnumerator DeleteSlotRoutine(int slotIndex, Action onCompleto = null)
    {
        if (!ValidateAuth())
        {
            onCompleto?.Invoke();
            yield break;
        }

        using var www = BuildJsonRequest("DELETE", $"/saves/{slotIndex}", null, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"Erro ao deletar save: {www.responseCode} - {www.error} - {www.downloadHandler.text}"
            );
        }
        else
        {
            Debug.Log($"Save remoto deletado (slot {slotIndex}).");
        }

        onCompleto?.Invoke();
    }

    public IEnumerator CarregarTodosSlots(Action onCompleto)
    {
        if (!ValidateAuth())
        {
            onCompleto?.Invoke();
            yield break;
        }

        using var www = BuildJsonRequest("GET", "/saves", null, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro ao buscar saves: {www.responseCode} - {www.error}");
            onCompleto?.Invoke();
            yield break;
        }

        // JsonUtility não suporta array direto, usa wrapper
        var wrappedJson = "{\"items\":" + www.downloadHandler.text + "}";
        var response = JsonUtility.FromJson<SaveResponseWrapper>(wrappedJson);

        // Recria o cache do zero a cada busca (slots fixos: 0..TotalSlots-1)
        var novoCache = new List<SaveSlotInfo>();
        for (int i = 0; i < TotalSlots; i++)
            novoCache.Add(new SaveSlotInfo { slotIndex = i, hasData = false });

        if (response?.items != null)
        {
            foreach (var save in response.items)
            {
                int idx = save.slotIndex - 1; // backend é 1-based, cache é 0-based
                if (idx < 0 || idx >= novoCache.Count)
                    continue;

                novoCache[idx] = new SaveSlotInfo
                {
                    slotIndex = idx,
                    slotName = save.slotName,
                    selectedCharacter = save.selectedCharacter,
                    difficulty = save.difficulty,
                    completionPercent = save.completionPercent,
                    lastSavedTime = save.lastSavedAtUtc,
                    hasData = true,
                };
            }
        }

        SlotsCache = novoCache;
        onCompleto?.Invoke();
    }

    [Serializable]
    class SaveResponseWrapper
    {
        public SaveResponse[] items;
    }

    SavePayload BuildSavePayload(int slotIndex)
    {
        var payload = new SavePayload();

        payload.slotIndex = slotIndex;

        payload.slotName = "Save " + slotIndex;

        payload.sceneName = SceneManager.GetActiveScene().name;

        payload.selectedCharacter = NewGameSessionSettings.SelectedCharacter.ToString();

        payload.playTutorial = NewGameSessionSettings.PlayTutorial;

        payload.difficulty = NewGameSessionSettings.Difficulty.ToString();

        if (CheckpointState.instance != null)
        {
            payload.checkpointId = CheckpointState.instance.CurrentCheckpointId;
        }

        if (ColetavelState.instance != null)
        {
            payload.collectedIds = ColetavelState.instance.GetCollectedIds().ToList();
        }

        if (EnemyState.instance != null)
        {
            payload.deadEnemyIds = EnemyState.instance.GetDeadEnemyIds();
        }

        payload.completionPercent = CalculateCompletionPercentage();
        if (GameControler.instance != null)
        {
            payload.currentHealth = GameControler.instance.health;
            payload.maxHealth = GameControler.instance.maxHealth;
        }
        else
        {
            payload.currentHealth = 100;
            payload.maxHealth = 100;
        }

        return payload;
    }

    private void AplicarSave(SaveResponse save)
    {
        if (save == null)
        {
            Debug.LogError("Save vazio recebido");
            return;
        }

        EnsureStateObjects();

        Debug.Log("Restaurando checkpoint: " + save.checkpointId);

        if (CheckpointState.instance != null)
        {
            CheckpointState.instance.Restaurar(save.checkpointId);
        }

        if (
            Enum.TryParse(save.selectedCharacter, true, out PlayableCharacterId character)
            && Enum.TryParse(save.difficulty, true, out GameDifficulty difficulty)
        )
        {
            NewGameSessionSettings.Apply(character, save.playTutorial, difficulty);
        }

        if (ColetavelState.instance != null)
        {
            ColetavelState.instance.CarregarIds(ParseStringList(save.collectedIdsJson));
        }

        if (EnemyState.instance != null)
        {
            EnemyState.instance.CarregarInimigosMortos(ParseStringList(save.deadEnemyIdsJson));
        }

        if (GameControler.instance != null && CheckpointState.instance != null)
        {
            GameControler.instance.lastCheckpoint =
                CheckpointState.instance.GetCheckpointPosition();

            GameControler.instance.hasCheckpoint = true;

            GameControler.instance.health = save.currentHealth;

            GameControler.instance.maxHealth = save.maxHealth;
        }

        ApplyCollectedInCurrentScene();

        EnemyState.instance?.AplicarEstadoNaCena();
    }

    float CalculateCompletionPercentage()
    {
        float collectibleRatio = 0f;
        float enemyRatio = 0f;
        float checkpointRatio = 0f;

        if (GameControler.instance != null && GameControler.instance.TotalColetaveis > 0)
        {
            collectibleRatio =
                (float)GameControler.instance.Coletados / GameControler.instance.TotalColetaveis;
        }

        if (EnemyState.instance != null && EnemyState.instance.TotalEnemies > 0)
        {
            enemyRatio = (float)EnemyState.instance.DeadEnemies / EnemyState.instance.TotalEnemies;
        }

        if (CheckpointState.instance != null && CheckpointState.instance.HasCheckpoint())
        {
            checkpointRatio = 1f;
        }

        float completion =
            (collectibleRatio * 0.5f + enemyRatio * 0.3f + checkpointRatio * 0.2f) * 100f;
        return Mathf.Clamp(completion, 0f, 100f);
    }

    void ApplySavePayload(SavePayload payload)
    {
        EnsureStateObjects();

        if (
            !string.IsNullOrWhiteSpace(payload.selectedCharacter)
            && Enum.TryParse(payload.selectedCharacter, true, out PlayableCharacterId characterId)
            && Enum.TryParse(payload.difficulty, true, out GameDifficulty difficulty)
        )
        {
            NewGameSessionSettings.Apply(characterId, payload.playTutorial, difficulty);
        }

        if (ColetavelState.instance != null)
            ColetavelState.instance.CarregarIds(payload.collectedIds);

        if (EnemyState.instance != null)
            EnemyState.instance.CarregarInimigosMortos(payload.deadEnemyIds);

        if (CheckpointState.instance != null)
            CheckpointState.instance.Restaurar(payload.checkpointId);

        ApplyCollectedInCurrentScene();
        EnemyState.instance?.AplicarEstadoNaCena();
    }

    void ApplyCollectedInCurrentScene()
    {
        if (ColetavelState.instance == null)
            return;

        var coletaveis = FindObjectsByType<Coletavel>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        foreach (var c in coletaveis)
        {
            if (ColetavelState.instance.FoiColetado(c.Id))
                Destroy(c.gameObject);
        }
    }

    UnityWebRequest BuildJsonRequest(string method, string route, string body, bool authorized)
    {
        var url = baseUrl.TrimEnd('/') + route;
        var req = new UnityWebRequest(url, method);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(body))
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));

        if (authorized && RemoteAuthSession.instance != null)
            req.SetRequestHeader(
                "Authorization",
                $"Bearer {RemoteAuthSession.instance.AccessToken}"
            );

        return req;
    }

    bool ValidateAuth()
    {
        if (RemoteAuthSession.instance == null || !RemoteAuthSession.instance.IsAuthenticated)
        {
            Debug.LogError("Usuario nao autenticado. Faca login antes de salvar/carregar.");
            return false;
        }

        return true;
    }

    void EnsureSession()
    {
        if (RemoteAuthSession.instance != null)
            return;

        var go = new GameObject("RemoteAuthSession");
        DontDestroyOnLoad(go);
        go.AddComponent<RemoteAuthSession>();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void EnsureStateObjects()
    {
        if (ColetavelState.instance == null)
        {
            var go = new GameObject("ColetavelState");
            go.AddComponent<ColetavelState>();
        }

        if (EnemyState.instance == null)
        {
            var go = new GameObject("EnemyState");
            go.AddComponent<EnemyState>();
        }

        if (CheckpointState.instance == null)
        {
            var go = new GameObject("CheckpointState");
            go.AddComponent<CheckpointState>();
        }
    }

    [Serializable]
    class AuthRequest
    {
        public string login;
        public string password;
        public string nome;
    }

    [Serializable]
    class AuthResponse
    {
        public string accessToken;
        public string login;
        public string nome;
    }

    [Serializable]
    class SaveResponse
    {
        public int slotIndex;

        public string slotName;

        public string selectedCharacter;

        public bool playTutorial;

        public string difficulty;

        public string sceneName;

        public string checkpointId;

        public string collectedIdsJson;

        public string deadEnemyIdsJson;

        public float completionPercent;

        public int maxHealth;

        public int currentHealth;

        public int deathCount;

        public int qttAppleCollected;

        public int qttGlassCollected;

        public int qttPlasticCollected;

        public int qttElectronicsCollected;

        public int qttPaperCollected;

        public int qttMetalCollected;

        public int score;

        public string lastSavedAtUtc;
    }

    [Serializable]
    class SavePayload
    {
        public int slotIndex;
        public string slotName;
        public string selectedCharacter;
        public bool playTutorial;
        public string difficulty;
        public string sceneName;
        public string checkpointId;

        public List<string> collectedIds = new List<string>();
        public List<string> deadEnemyIds = new List<string>();

        public float completionPercent;

        public float currentHealth;
        public float maxHealth;

        public int deathCount;

        public int qttAppleCollected;
        public int qttGlassCollected;
        public int qttPlasticCollected;
        public int qttElectronicsCollected;
        public int qttPaperCollected;
        public int qttMetalCollected;
        public int score;
    }

    [Serializable]
    class StringListWrapper
    {
        public List<string> values = new List<string>();
    }

    static List<string> ParseStringList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        var wrapperJson = "{\"values\":" + json + "}";
        var wrapper = JsonUtility.FromJson<StringListWrapper>(wrapperJson);
        return wrapper?.values ?? new List<string>();
    }
}
