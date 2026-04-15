using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuVolumeController : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text volumeLabel;
    [SerializeField] private string prefsKey = "TurtleTrashFighter.MasterVolume";

    private void OnEnable()
    {
        LoadVolume();
        RefreshLabel();
    }

    public void OnSliderChanged(float value)
    {
        float clampedValue = Mathf.Clamp01(value);
        AudioListener.volume = clampedValue;
        PlayerPrefs.SetFloat(prefsKey, clampedValue);
        PlayerPrefs.Save();
        RefreshLabel();
    }

    public void LoadVolume()
    {
        float volume = PlayerPrefs.GetFloat(prefsKey, 1f);
        AudioListener.volume = volume;

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(volume);
    }

    private void RefreshLabel()
    {
        if (volumeLabel != null)
            volumeLabel.text = $"Volume: {Mathf.RoundToInt(AudioListener.volume * 100f)}%";
    }
}
