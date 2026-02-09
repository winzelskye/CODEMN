using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu flow: New Game, Continue, Settings, Quit.
/// Handles "Have they played before?" check and reset progress flow.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Reset Progress UI")]
    [SerializeField] private GameObject resetProgressPanel;
    [SerializeField] private Button resetYesButton;
    [SerializeField] private Button resetNoButton;

    [Header("Scene Names")]
    [SerializeField] private string characterSelectScene = "CharacterSelect";
    [SerializeField] private string levelSelectScene = "Level Select";

    private DataPersistenceManager dataManager;

    private void Start()
    {
        dataManager = DataPersistenceManager.instance;
        if (dataManager == null)
        {
            dataManager = FindObjectOfType<DataPersistenceManager>();
        }

        SetupButtons();
        CheckContinueButtonState();
    }

    private void SetupButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (resetProgressPanel != null)
            resetProgressPanel.SetActive(false);

        if (resetYesButton != null)
            resetYesButton.onClick.AddListener(OnResetYesClicked);

        if (resetNoButton != null)
            resetNoButton.onClick.AddListener(OnResetNoClicked);
    }

    private void CheckContinueButtonState()
    {
        if (continueButton == null) return;

        bool hasSaveData = HasPlayedBefore();
        continueButton.interactable = hasSaveData;
    }

    private bool HasPlayedBefore()
    {
        if (dataManager == null) return false;

        // Check if there's any save data
        bool hasLevelProgress = PlayerPrefs.HasKey("Level_1_Completed") || 
                                 PlayerPrefs.GetInt("HasPlayedBefore", 0) == 1;

        // Also check DataPersistenceManager
        if (dataManager != null)
        {
            // Try to load game data
            dataManager.LoadGame();
        }

        return hasLevelProgress;
    }

    public void OnNewGameClicked()
    {
        Debug.Log("New Game clicked");

        if (!HasPlayedBefore())
        {
            // First-time player - go directly to character selection
            GoToCharacterSelection();
        }
        else
        {
            // Returning player - show reset progress confirmation
            ShowResetProgressPanel();
        }
    }

    private void ShowResetProgressPanel()
    {
        if (resetProgressPanel != null)
        {
            resetProgressPanel.SetActive(true);
        }
        else
        {
            // If no panel assigned, ask via debug and proceed
            Debug.LogWarning("Reset progress panel not assigned. Proceeding to character selection.");
            GoToCharacterSelection();
        }
    }

    public void OnResetYesClicked()
    {
        Debug.Log("Resetting progress...");
        ResetLevels();
        ResetProgress();
        if (resetProgressPanel != null)
            resetProgressPanel.SetActive(false);
        GoToCharacterSelection();
    }

    public void OnResetNoClicked()
    {
        Debug.Log("Not resetting progress. Going to character selection.");
        if (resetProgressPanel != null)
            resetProgressPanel.SetActive(false);
        GoToCharacterSelection();
    }

    /// <summary>Flowchart "Reset Levels" step: clear level/sublevel completion only.</summary>
    private void ResetLevels()
    {
        if (dataManager != null)
            dataManager.ResetLevelsOnly();
        for (int i = 1; i <= 10; i++)
            PlayerPrefs.DeleteKey("Level_" + i + "_Completed");
        PlayerPrefs.Save();
    }

    private void ResetProgress()
    {
        // Clear PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Reset DataPersistenceManager
        if (dataManager != null)
        {
            dataManager.NewGame();
            dataManager.SaveGame();
        }

        Debug.Log("Progress reset complete.");
    }

    private void GoToCharacterSelection()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToCharacterSelection();
        }
        else
        {
            SceneManager.LoadScene(characterSelectScene);
        }
    }

    public void OnContinueClicked()
    {
        Debug.Log("Continue clicked - Loading saved game");

        if (dataManager != null)
        {
            dataManager.LoadGame();
        }

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToLevelSelection();
        }
        else
        {
            SceneManager.LoadScene(levelSelectScene);
        }
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
        // Settings menu should be handled by SettingsMenuController
        // This might open a settings panel or load a settings scene
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SetState(GameFlowManager.GameState.Settings);
        }
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnNewGameClicked);
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
        if (resetYesButton != null)
            resetYesButton.onClick.RemoveListener(OnResetYesClicked);
        if (resetNoButton != null)
            resetNoButton.onClick.RemoveListener(OnResetNoClicked);
    }
}
