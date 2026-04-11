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
    [Tooltip("Exemplo: http://localhost:5000")]
    public string baseUrl = "http://localhost:5000";

    [Header("Auto Save")]
    public bool autoSaveOnSceneChange;

    void OnEnable()
    {
        if (autoSaveOnSceneChange)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        var req = new AuthRequest { username = username, password = password };
        var json = JsonUtility.ToJson(req);

        using var www = BuildJsonRequest("POST", "/auth/register", json, false);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro ao registrar: {www.responseCode} - {www.error} - {www.downloadHandler.text}");
            yield break;
        }

        var data = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
        EnsureSession();
        RemoteAuthSession.instance.SetSession(data.username, data.accessToken);
        Debug.Log("Registro concluido e sessao autenticada.");
    }

    IEnumerator LoginRoutine(string username, string password)
    {
        var req = new AuthRequest { username = username, password = password };
        var json = JsonUtility.ToJson(req);

        using var www = BuildJsonRequest("POST", "/auth/login", json, false);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro no login: {www.responseCode} - {www.error} - {www.downloadHandler.text}");
            yield break;
        }

        var data = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
        EnsureSession();
        RemoteAuthSession.instance.SetSession(data.username, data.accessToken);
        Debug.Log("Login concluido.");
    }

    IEnumerator SaveRoutine()
    {
        if (!ValidateAuth())
            yield break;

        var payload = BuildSavePayload();
        var json = JsonUtility.ToJson(payload);

        using var www = BuildJsonRequest("PUT", "/save", json, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Erro ao salvar: {www.responseCode} - {www.error} - {www.downloadHandler.text}");
            yield break;
        }

        Debug.Log("Save remoto concluido.");
    }

    IEnumerator LoadRoutine()
    {
        if (!ValidateAuth())
            yield break;

        using var www = BuildJsonRequest("GET", "/save", null, true);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            if (www.responseCode == 404)
            {
                Debug.Log("Nenhum save remoto para este usuario.");
                yield break;
            }

            Debug.LogError($"Erro ao carregar: {www.responseCode} - {www.error} - {www.downloadHandler.text}");
            yield break;
        }

        var payload = JsonUtility.FromJson<SavePayload>(www.downloadHandler.text);
        ApplySavePayload(payload);
        Debug.Log("Load remoto concluido.");
    }

    SavePayload BuildSavePayload()
    {
        var payload = new SavePayload();
        payload.sceneName = SceneManager.GetActiveScene().name;

        if (ColetavelState.instance != null)
            payload.collectedIds = ColetavelState.instance.GetCollectedIds().ToList();

        if (EnemyState.instance != null)
            payload.deadEnemyIds = EnemyState.instance.GetDeadEnemyIds();

        if (CheckpointState.instance != null)
        {
            payload.checkpointId = CheckpointState.instance.CurrentCheckpointId;
            payload.checkpointPosition = new Vector3Data(CheckpointState.instance.LastCheckpointPosition);
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
            collectibleRatio = (float)GameControler.instance.Coletados / GameControler.instance.TotalColetaveis;
        }

        if (EnemyState.instance != null && EnemyState.instance.TotalEnemies > 0)
        {
            enemyRatio = (float)EnemyState.instance.DeadEnemies / EnemyState.instance.TotalEnemies;
        }

        if (CheckpointState.instance != null && CheckpointState.instance.HasCheckpoint())
        {
            checkpointRatio = 1f;
        }

        float completion = (collectibleRatio * 0.5f + enemyRatio * 0.3f + checkpointRatio * 0.2f) * 100f;
        return Mathf.Clamp(completion, 0f, 100f);
    }

    void ApplySavePayload(SavePayload payload)
    {
        EnsureStateObjects();

        if (ColetavelState.instance != null)
            ColetavelState.instance.CarregarIds(payload.collectedIds);

        if (EnemyState.instance != null)
            EnemyState.instance.CarregarInimigosMortos(payload.deadEnemyIds);

        if (CheckpointState.instance != null)
            CheckpointState.instance.Restaurar(payload.checkpointId, payload.checkpointPosition.ToVector3());

        ApplyCollectedInCurrentScene();
        EnemyState.instance?.AplicarEstadoNaCena();
    }

    void ApplyCollectedInCurrentScene()
    {
        if (ColetavelState.instance == null)
            return;

        var coletaveis = FindObjectsByType<Coletavel>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
            req.SetRequestHeader("Authorization", $"Bearer {RemoteAuthSession.instance.AccessToken}");

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
        public string username;
        public string password;
    }

    [Serializable]
    class AuthResponse
    {
        public string accessToken;
        public string username;
    }

    [Serializable]
    class SavePayload
    {
        public string sceneName;
        public List<string> collectedIds = new List<string>();
        public List<string> deadEnemyIds = new List<string>();
        public string checkpointId;
        public Vector3Data checkpointPosition = new Vector3Data(Vector3.zero);
        public float completionPercent;
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
}
