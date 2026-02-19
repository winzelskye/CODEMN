using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject pauseMenuUI;
    public Transform canvasTransform;

    [Header("Settings")]
    public GameObject settingsPrefab;
    public string mainMenuSceneName = "CODEMN(GAME)";

    private bool isPaused = false;
    private GameObject settingsInstance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (settingsInstance != null && settingsInstance.activeSelf)
                    CloseSettings();
                else
                    ReturnToGame();
            }
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ReturnToGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        if (settingsInstance != null)
        {
            Destroy(settingsInstance);
            settingsInstance = null;
        }
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (settingsPrefab != null)
        {
            pauseMenuUI.SetActive(false);
            settingsInstance = Instantiate(settingsPrefab, canvasTransform);
        }
    }

    public void CloseSettings()
    {
        if (settingsInstance != null)
        {
            Destroy(settingsInstance);
            settingsInstance = null;
        }
        pauseMenuUI.SetActive(true);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }
}