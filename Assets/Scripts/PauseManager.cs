using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour  // ← Check this line!
{
    public static PauseManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject pauseMenuUI;
    public GameObject settingsPrefab; // Your existing settings prefab
    public Button returnButton;
    public Button settingsButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private GameObject settingsInstance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Setup button listeners
        if (returnButton) returnButton.onClick.AddListener(ReturnToGame);
        if (settingsButton) settingsButton.onClick.AddListener(OpenSettings);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(LoadMainMenu);

        // Hide pause menu at start
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // If settings is open, close it first
                if (settingsInstance != null && settingsInstance.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    ReturnToGame();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        Debug.Log("Pause() called!"); // Add this
        Debug.Log("pauseMenuUI is: " + pauseMenuUI); // Add this

        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnToGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);

        // Destroy settings if it exists
        if (settingsInstance != null)
        {
            Destroy(settingsInstance);
            settingsInstance = null;
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);

        // Instantiate your settings prefab
        if (settingsPrefab != null && settingsInstance == null)
        {
            settingsInstance = Instantiate(settingsPrefab, transform.parent);
        }
        else if (settingsInstance != null)
        {
            settingsInstance.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsInstance != null)
        {
            settingsInstance.SetActive(false);
        }
        pauseMenuUI.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Reset time scale before loading
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}