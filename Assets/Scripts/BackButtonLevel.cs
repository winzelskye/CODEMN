using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    public Button backButton;
    public string mainMenuSceneName = "CODEMN(GAME)";

    void Start()
    {
        if (backButton == null)
            backButton = GetComponent<Button>();

        if (backButton != null)
            backButton.onClick.AddListener(GoToMainMenu);
        else
            Debug.LogError("Back button is not assigned!");
    }

    public void GoToMainMenu()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(mainMenuSceneName);
        else
            SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(GoToMainMenu);
    }
}