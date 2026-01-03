using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeButton;
    
    [Header("Settings Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    
    private Button settingsButton;
    
    private void Awake()
    {
        // Get the button component from this GameObject
        settingsButton = GetComponent<Button>();
    }
    
    private void Start()
    {
        // Validate required references
        if (!ValidateReferences())
        {
            Debug.LogError("SettingsButton: Missing required references! Check Inspector.");
            enabled = false;
            return;
        }
        
        // Load saved settings
        LoadSettings();
        
        // Setup button listeners
        settingsButton.onClick.AddListener(OpenSettings);
        closeButton.onClick.AddListener(CloseSettings);
        
        // Setup settings listeners
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }
        
        // Make sure settings panel starts hidden
        settingsPanel.SetActive(true);
    }
    
    private bool ValidateReferences()
    {
        bool isValid = true;
        
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsButton: Settings Panel is not assigned in Inspector!");
            isValid = false;
        }
        
        if (settingsButton == null)
        {
            Debug.LogError("SettingsButton: No Button component found on this GameObject!");
            isValid = false;
        }
        
        if (closeButton == null)
        {
            Debug.LogError("SettingsButton: Close Button is not assigned in Inspector!");
            isValid = false;
        }
        
        return isValid;
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f; // Resume game
            SaveSettings();
        }
    }
    
    private void SetMusicVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
    
    private void SetSFXVolume(float value)
    {
        // Implement based on your audio system
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
    
    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    private void SaveSettings()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MusicVolume", volumeSlider.value);
        }
        
        if (sfxSlider != null)
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        }
        
        if (fullscreenToggle != null)
        {
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        }
        
        PlayerPrefs.Save();
    }
    
    private void LoadSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
        if (volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
        }
        
        // Apply the loaded settings
        AudioListener.volume = musicVolume;
        Screen.fullScreen = isFullscreen;
    }
    
    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(SetSFXVolume);
        }
        
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
        }
    }
}