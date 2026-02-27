using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private GameObject backgroundBlocker; // add a full screen transparent panel

    private void Start()
    {
        closeButton.onClick.AddListener(CloseSettings);
        volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }

    private void OnEnable()
    {
        // Block everything behind when panel opens
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(true);
    }

    private void OnDisable()
    {
        // Unblock when panel closes
        if (backgroundBlocker != null)
            backgroundBlocker.SetActive(false);
    }

    public void CloseSettings()
    {
        AudioSettings.Instance?.ResumeMusicAfterPanel();
        Time.timeScale = 1f;
        SaveSettings();
        gameObject.SetActive(false);
    }

    private void SetMusicVolume(float value)
    {
        AudioSettings.Instance?.SetMusicVolume(value);
    }

    private void SetSFXVolume(float value)
    {
        AudioSettings.Instance?.SetSFXVolume(value);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", volumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.Save();
    }
}