using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Settings menu controller: Audio, Controls, About submenus.
/// All submenus loop back to Settings, Settings returns to Main Menu.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    [Header("Main Settings Panel")]
    [SerializeField] private GameObject settingsMainPanel;

    [Header("Submenu Panels")]
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject aboutPanel;

    [Header("Navigation Buttons")]
    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button backToMainMenuButton;

    [Header("Audio Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private Button audioBackButton;

    [Header("Controls Settings")]
    [SerializeField] private Button controlsBackButton;
    // Add control keybinding UI here if needed

    [Header("About Panel")]
    [SerializeField] private TextMeshProUGUI aboutText;
    [SerializeField] private Button aboutBackButton;

    private DataPersistenceManager dataManager;
    private GameData gameData;

    private void Start()
    {
        dataManager = DataPersistenceManager.instance;
        if (dataManager == null)
        {
            dataManager = FindObjectOfType<DataPersistenceManager>();
        }

        LoadSettings();
        SetupButtons();
        ShowMainPanel();
    }

    private void SetupButtons()
    {
        if (audioButton != null)
            audioButton.onClick.AddListener(() => ShowSubmenu(audioPanel));

        if (controlsButton != null)
            controlsButton.onClick.AddListener(() => ShowSubmenu(controlsPanel));

        if (aboutButton != null)
            aboutButton.onClick.AddListener(() => ShowSubmenu(aboutPanel));

        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenu);

        if (audioBackButton != null)
            audioBackButton.onClick.AddListener(ShowMainPanel);

        if (controlsBackButton != null)
            controlsBackButton.onClick.AddListener(ShowMainPanel);

        if (aboutBackButton != null)
            aboutBackButton.onClick.AddListener(ShowMainPanel);

        // Audio slider listeners
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void LoadSettings()
    {
        if (dataManager != null)
        {
            dataManager.LoadGame();
            // Settings are stored in GameData
        }

        // Load from GameData or PlayerPrefs
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVol;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVol;

        UpdateVolumeText();
    }

    private void ShowMainPanel()
    {
        HideAllSubmenus();
        if (settingsMainPanel != null)
            settingsMainPanel.SetActive(true);
    }

    private void ShowSubmenu(GameObject submenuPanel)
    {
        HideAllSubmenus();
        if (settingsMainPanel != null)
            settingsMainPanel.SetActive(false);
        if (submenuPanel != null)
            submenuPanel.SetActive(true);
    }

    private void HideAllSubmenus()
    {
        if (audioPanel != null)
            audioPanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();

        // Update GameData if available
        if (dataManager != null && dataManager != null)
        {
            // GameData.musicVolume should be updated
            dataManager.SaveGame();
        }

        UpdateVolumeText();

        // Apply to audio system
        // AudioManager.Instance?.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();

        // Update GameData if available
        if (dataManager != null && dataManager != null)
        {
            // GameData.sfxVolume should be updated
            dataManager.SaveGame();
        }

        UpdateVolumeText();

        // Apply to audio system
        // AudioManager.Instance?.SetSFXVolume(value);
    }

    private void UpdateVolumeText()
    {
        if (musicVolumeText != null && musicVolumeSlider != null)
            musicVolumeText.text = $"Music: {(int)(musicVolumeSlider.value * 100)}%";

        if (sfxVolumeText != null && sfxVolumeSlider != null)
            sfxVolumeText.text = $"SFX: {(int)(sfxVolumeSlider.value * 100)}%";
    }

    private void OnBackToMainMenu()
    {
        Debug.Log("Returning to main menu from settings");

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("CODEMN(GAME)");
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (audioButton != null)
            audioButton.onClick.RemoveAllListeners();
        if (controlsButton != null)
            controlsButton.onClick.RemoveAllListeners();
        if (aboutButton != null)
            aboutButton.onClick.RemoveAllListeners();
        if (backToMainMenuButton != null)
            backToMainMenuButton.onClick.RemoveAllListeners();
        if (audioBackButton != null)
            audioBackButton.onClick.RemoveAllListeners();
        if (controlsBackButton != null)
            controlsBackButton.onClick.RemoveAllListeners();
        if (aboutBackButton != null)
            aboutBackButton.onClick.RemoveAllListeners();
    }
}
