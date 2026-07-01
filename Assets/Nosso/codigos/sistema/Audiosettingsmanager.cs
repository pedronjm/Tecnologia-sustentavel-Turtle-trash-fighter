using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton responsável por aplicar volume no AudioMixer (Assets/Audio/MainMixer)
/// e converter entre o valor linear (0-1, usado nos sliders e no backend) e
/// decibéis (escala que o AudioMixer realmente usa internamente).
///
/// Mesmo padrão de singleton usado em RemoteSaveService: instância única,
/// sobrevive entre cenas, acessada via AudioSettingsManager.getInstance().
/// </summary>
public class AudioSettingsManager : MonoBehaviour
{
    public enum VolumeChannel
    {
        Master,
        Music,
        Sfx,
    }

    [Header("Mixer")]
    [Tooltip("Arraste aqui o asset Assets/Audio/MainMixer.")]
    [SerializeField]
    private AudioMixer mixer;

    // Nomes EXATOS dos parâmetros expostos no MainMixer (Exposed Parameters).
    private const string ParamMaster = "MasterVolume";
    private const string ParamMusic = "MusicVolume";
    private const string ParamSfx = "SFXVolume";

    // dB aplicado quando o volume linear é 0 (silêncio). -80dB é o padrão
    // da Unity para "totalmente mudo" sem usar -infinito.
    private const float MutedDb = -80f;

    public const string PrefsPrefix = "TurtleTrashFighter.Volume.";

    private static AudioSettingsManager instance;
    private string loadedScope = string.Empty;

    // Disparado sempre que qualquer volume muda (local ou aplicado do servidor),
    // para a UI (sliders) poder se atualizar sem precisar fazer polling.
    public static event Action VolumesChanged;

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLocal();
        ApplyAllToMixer();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static AudioSettingsManager getInstance()
    {
        return instance;
    }

    public void ReloadForCurrentUser()
    {
        LoadLocal();
        ApplyAllToMixer();
        VolumesChanged?.Invoke();
    }

    /// <summary>
    /// Volume linear (0-1) do canal pedido. Usado para preencher o slider
    /// na UI e para montar o payload enviado ao backend.
    /// </summary>
    public float GetVolume(VolumeChannel channel)
    {
        EnsureLoadedForCurrentUser();

        return channel switch
        {
            VolumeChannel.Master => masterVolume,
            VolumeChannel.Music => musicVolume,
            VolumeChannel.Sfx => sfxVolume,
            _ => 1f,
        };
    }

    /// <summary>
    /// Define o volume (0-1), aplica no AudioMixer imediatamente e salva
    /// localmente no PlayerPrefs. Não faz chamada de rede aqui — quem chama
    /// este método decide quando sincronizar com o servidor (debounce, etc).
    /// </summary>
    public void SetVolume(VolumeChannel channel, float linearVolume)
    {
        EnsureLoadedForCurrentUser();

        linearVolume = Mathf.Clamp01(linearVolume);

        switch (channel)
        {
            case VolumeChannel.Master:
                masterVolume = linearVolume;
                break;
            case VolumeChannel.Music:
                musicVolume = linearVolume;
                break;
            case VolumeChannel.Sfx:
                sfxVolume = linearVolume;
                break;
        }

        ApplyToMixer(channel, linearVolume);
        SaveLocal();
        VolumesChanged?.Invoke();
    }

    /// <summary>
    /// Aplica os 3 volumes recebidos do servidor (GET /settings) de uma vez,
    /// salva localmente e notifica a UI. Valores fora de 0-1 são limitados.
    /// </summary>
    public void ApplyFromRemote(float remoteMaster, float remoteMusic, float remoteSfx)
    {
        EnsureLoadedForCurrentUser();

        masterVolume = Mathf.Clamp01(remoteMaster);
        musicVolume = Mathf.Clamp01(remoteMusic);
        sfxVolume = Mathf.Clamp01(remoteSfx);

        ApplyAllToMixer();
        SaveLocal();
        VolumesChanged?.Invoke();
    }

    public static float GetPersistedVolume(VolumeChannel channel)
    {
        return channel switch
        {
            VolumeChannel.Master => PlayerPrefs.GetFloat(GetPrefsKey("Master"), 1f),
            VolumeChannel.Music => PlayerPrefs.GetFloat(GetPrefsKey("Music"), 1f),
            VolumeChannel.Sfx => PlayerPrefs.GetFloat(GetPrefsKey("Sfx"), 1f),
            _ => 1f,
        };
    }

    private void ApplyAllToMixer()
    {
        ApplyToMixer(VolumeChannel.Master, masterVolume);
        ApplyToMixer(VolumeChannel.Music, musicVolume);
        ApplyToMixer(VolumeChannel.Sfx, sfxVolume);
    }

    private void ApplyToMixer(VolumeChannel channel, float linearVolume)
    {
        if (mixer == null)
        {
            Debug.LogWarning("AudioSettingsManager: campo 'mixer' nao esta atribuido no Inspector.");
            return;
        }

        string paramName = channel switch
        {
            VolumeChannel.Master => ParamMaster,
            VolumeChannel.Music => ParamMusic,
            VolumeChannel.Sfx => ParamSfx,
            _ => ParamMaster,
        };

        float dB = LinearToDecibel(linearVolume);
        mixer.SetFloat(paramName, dB);
    }

    private static float LinearToDecibel(float linearVolume)
    {
        if (linearVolume <= 0.0001f)
            return MutedDb;

        return Mathf.Log10(linearVolume) * 20f;
    }

    private void LoadLocal()
    {
        loadedScope = GetScopeKey();
        masterVolume = PlayerPrefs.GetFloat(GetPrefsKey("Master"), 1f);
        musicVolume = PlayerPrefs.GetFloat(GetPrefsKey("Music"), 1f);
        sfxVolume = PlayerPrefs.GetFloat(GetPrefsKey("Sfx"), 1f);
    }

    private void SaveLocal()
    {
        loadedScope = GetScopeKey();
        PlayerPrefs.SetFloat(GetPrefsKey("Master"), masterVolume);
        PlayerPrefs.SetFloat(GetPrefsKey("Music"), musicVolume);
        PlayerPrefs.SetFloat(GetPrefsKey("Sfx"), sfxVolume);
        PlayerPrefs.Save();
    }

    private void EnsureLoadedForCurrentUser()
    {
        string currentScope = GetScopeKey();
        if (loadedScope == currentScope)
            return;

        LoadLocal();
        ApplyAllToMixer();
    }

    private static string GetPrefsKey(string suffix)
    {
        return PrefsPrefix + GetScopeKey() + "." + suffix;
    }

    private static string GetScopeKey()
    {
        string username = RemoteAuthSession.instance != null ? RemoteAuthSession.instance.Username : string.Empty;
        if (string.IsNullOrWhiteSpace(username))
            return "guest";

        return username.Trim().ToLowerInvariant();
    }
}