using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    [Header("Button Reference")]
    [Tooltip("Assign the back button UI element here")]
    public Button backButton;

    [Header("Scene Settings")]
    [Tooltip("Name of your main menu scene")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        // If button is not assigned, try to get it from this GameObject
        if (backButton == null)
        {
            backButton = GetComponent<Button>();
        }

        // Add listener to the button
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogError("Back button is not assigned!");
        }
    }

    public void GoToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        // Remove listener when object is destroyed to prevent memory leaks
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(GoToMainMenu);
        }
    }
}