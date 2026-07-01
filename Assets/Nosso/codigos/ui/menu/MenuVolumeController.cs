using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla os 3 sliders de volume (Geral, Música, SFX) da tela de
/// configurações. Mesma tela/script serve tanto para o menu principal
/// quanto para o menu de pause.
///
/// Fluxo:
/// - OnEnable: pede load remoto (RemoteSaveService.LoadSettings) e também
///   já preenche os sliders com o valor local (PlayerPrefs) na hora,
///   para a UI não aparecer "zerada" enquanto a resposta do servidor não chega.
/// - Slider.onValueChanged: aplica no AudioSettingsManager (efeito imediato)
///   e agenda um save remoto com pequeno debounce, para não disparar uma
///   requisição HTTP a cada frame de arraste do slider.
/// </summary>
public class MenuVolumeController : MonoBehaviour
{
    [Header("Sliders (0 a 1)")]
    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private Slider musicVolumeSlider;

    [SerializeField]
    private Slider sfxVolumeSlider;

    [Header("Labels (opcional)")]
    [SerializeField]
    private TMP_Text masterVolumeLabel;

    [SerializeField]
    private TMP_Text musicVolumeLabel;

    [SerializeField]
    private TMP_Text sfxVolumeLabel;

    [Header("Sincronização remota")]
    [Tooltip("Tempo (segundos) sem mudanças no slider antes de salvar no servidor.")]
    [SerializeField]
    private float saveDebounceSeconds = 0.5f;

    private bool hasWiredSliders;
    private bool isApplyingRemoteValues;
    private float saveCountdown = -1f;

    private void Awake()
    {
        WireSlidersOnce();
    }

    private void OnEnable()
    {
        WireSlidersOnce();
        RefreshSlidersFromManager();

        AudioSettingsManager.VolumesChanged += OnVolumesChangedExternally;
        RemoteSaveService.OnSettingsLoaded += OnRemoteSettingsLoaded;
        RemoteSaveService.getInstance()?.LoadSettings();
    }

    private void OnDisable()
    {
        AudioSettingsManager.VolumesChanged -= OnVolumesChangedExternally;
        RemoteSaveService.OnSettingsLoaded -= OnRemoteSettingsLoaded;

        // Se havia um save pendente (debounce) e o menu foi fechado antes
        // dele disparar, salva agora para não perder o último ajuste.
        if (saveCountdown >= 0f)
        {
            saveCountdown = -1f;
            SaveVolumesRemote();
        }
    }

    private void Update()
    {
        if (saveCountdown < 0f)
            return;

        saveCountdown -= Time.unscaledDeltaTime;
        if (saveCountdown <= 0f)
        {
            saveCountdown = -1f;
            SaveVolumesRemote();
        }
    }

    private void WireSlidersOnce()
    {
        if (hasWiredSliders)
            return;

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxSliderChanged);

        hasWiredSliders = true;
    }

    private void OnMasterSliderChanged(float value)
    {
        OnSliderChanged(AudioSettingsManager.VolumeChannel.Master, value, masterVolumeLabel);
    }

    private void OnMusicSliderChanged(float value)
    {
        OnSliderChanged(AudioSettingsManager.VolumeChannel.Music, value, musicVolumeLabel);
    }

    private void OnSfxSliderChanged(float value)
    {
        OnSliderChanged(AudioSettingsManager.VolumeChannel.Sfx, value, sfxVolumeLabel);
    }

    private void OnSliderChanged(
        AudioSettingsManager.VolumeChannel channel,
        float value,
        TMP_Text label
    )
    {
        if (isApplyingRemoteValues)
            return;

        AudioSettingsManager manager = AudioSettingsManager.getInstance();

        if (manager == null)
            return;

        manager.SetVolume(channel, value);

        UpdateLabel(label, value);

        if (RemoteAuthSession.instance != null && RemoteAuthSession.instance.IsAuthenticated)
        {
            saveCountdown = saveDebounceSeconds;
        }
    }

    private void RefreshSlidersFromManager()
    {
        AudioSettingsManager manager = AudioSettingsManager.getInstance();
        if (manager == null)
            return;

        isApplyingRemoteValues = true;

        SetSliderSilently(
            masterVolumeSlider,
            manager.GetVolume(AudioSettingsManager.VolumeChannel.Master)
        );
        SetSliderSilently(
            musicVolumeSlider,
            manager.GetVolume(AudioSettingsManager.VolumeChannel.Music)
        );
        SetSliderSilently(
            sfxVolumeSlider,
            manager.GetVolume(AudioSettingsManager.VolumeChannel.Sfx)
        );

        UpdateLabel(
            masterVolumeLabel,
            manager.GetVolume(AudioSettingsManager.VolumeChannel.Master)
        );
        UpdateLabel(musicVolumeLabel, manager.GetVolume(AudioSettingsManager.VolumeChannel.Music));
        UpdateLabel(sfxVolumeLabel, manager.GetVolume(AudioSettingsManager.VolumeChannel.Sfx));

        isApplyingRemoteValues = false;
    }

    private static void SetSliderSilently(Slider slider, float value)
    {
        if (slider != null)
            slider.value = value;
    }

    private static void UpdateLabel(TMP_Text label, float linearVolume)
    {
        if (label != null)
            label.text = Mathf.RoundToInt(linearVolume * 100f) + "%";
    }

    private void SaveVolumesRemote()
    {
        RemoteSaveService.getInstance()?.SaveSettings();
    }

    // Chamado quando GET /settings termina e AudioSettingsManager já
    // aplicou os valores recebidos (via ApplyFromRemote).
    private void OnRemoteSettingsLoaded()
    {
        RefreshSlidersFromManager();
    }

    // Chamado por qualquer mudança de volume, local ou remota, garantindo
    // que a UI nunca fique fora de sincronia com o estado real.
    private void OnVolumesChangedExternally()
    {
        RefreshSlidersFromManager();
    }
}
