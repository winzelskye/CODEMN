using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Buttons")]
    public Button proceedButton;
    public Button retryButton;
    public Button mainMenuButton;

    [Header("Scene Names")]
    public string levelSelectScene = "Level Select";
    public string mainMenuScene = "CODEMN(GAME)";

    void Awake()
    {
        if (proceedButton != null)
            proceedButton.onClick.AddListener(Proceed);
        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void ShowLose()
    {
        gameObject.SetActive(true);
        if (losePanel != null) losePanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void ShowWin()
    {
        gameObject.SetActive(true);
        if (winPanel != null) winPanel.SetActive(true);
        if (losePanel != null) losePanel.SetActive(false);
    }

    void Proceed()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(levelSelectScene);
        else
            SceneManager.LoadScene(levelSelectScene);
    }

    void Retry()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GoToMainMenu()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(mainMenuScene);
        else
            SceneManager.LoadScene(mainMenuScene);
    }
}