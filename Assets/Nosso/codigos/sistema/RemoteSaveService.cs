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

  

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("RemoteSaveService: Unsubscribed from sceneLoaded event.");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (RemoteAuthSession.instance != null && RemoteAuthSession.instance.IsAuthenticated)
            SaveGame();
    }

    public void Register(string username, string password)
    {
        StartCoroutine(RegisterRoutine(username, password));
    }

    public void Login(string username, string password)
    {
        StartCoroutine(LoginRoutine(username, password));
    }

    public void SaveGame()
    {
        StartCoroutine(SaveRoutine());
    }

    public void LoadGame()
    {
        StartCoroutine(LoadRoutine());
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
    }

    IEnumerator SaveRoutine()
    {
        if (!ValidateAuth())
            yield break;

        var payload = BuildSavePayload();
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

        Debug.Log("Save remoto concluido.");
    }

    IEnumerator LoadRoutine()
    {
        if (!ValidateAuth())
            yield break;

        using var www = BuildJsonRequest("GET", "/saves/1", null, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            if (www.responseCode == 404)
            {
                Debug.Log("Nenhum save remoto para este usuario.");
                yield break;
            }

            Debug.LogError(
                $"Erro ao carregar: {www.responseCode} - {www.error} - {www.downloadHandler.text}"
            );
            yield break;
        }

        var response = JsonUtility.FromJson<SaveResponse>(www.downloadHandler.text);
        var payload = new SavePayload
        {
            sceneName = response.sceneName,
            selectedCharacter = response.selectedCharacter,
            playTutorial = response.playTutorial,
            difficulty = response.difficulty,
            collectedIds = ParseStringList(response.collectedIdsJson),
            deadEnemyIds = ParseStringList(response.deadEnemyIdsJson),
            checkpointId = response.checkpointId,

            completionPercent = response.completionPercent,
        };
        ApplySavePayload(payload);
        Debug.Log("Load remoto concluido.");
    }

    SavePayload BuildSavePayload()
    {
        var payload = new SavePayload();

        payload.slotIndex = 1;

        payload.slotName = "Save 1";

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

        return payload;
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
        go.AddComponent<RemoteAuthSession>();
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
        public float checkpointX;
        public float checkpointY;
        public float checkpointZ;
        public string collectedIdsJson;
        public string deadEnemyIdsJson;
        public float completionPercent;
        public string lastSavedAtUtc;
    }

    [Serializable]
    class SavePayload
    {
        public int slotIndex = 1;

        public string slotName;

        public string selectedCharacter;

        public bool playTutorial;

        public string difficulty;

        public string sceneName;

        public string checkpointId;

        public List<string> collectedIds = new List<string>();

        public List<string> deadEnemyIds = new List<string>();

        public float completionPercent;

        public int qttAppleCollected;

        public int qttGlassCollected;

        public int qttPlasticCollected;

        public int qttEletronicsCollected;

        public int qttPaperCollected;

        public int qttMetalCollected;

        public int Score;
    }

    [Serializable]
    class Vector3Data
    {
        public float x;
        public float y;
        public float z;

        public Vector3Data(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
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
