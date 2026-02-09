using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button script for New Game functionality.
/// Calls MainMenuController.OnNewGameClicked()
/// </summary>
public class NewGameButton : MonoBehaviour
{
    private Button button;
    private MainMenuController mainMenuController;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            mainMenuController = FindObjectOfType<MainMenuController>();
            if (mainMenuController != null)
            {
                button.onClick.AddListener(mainMenuController.OnNewGameClicked);
            }
            else
            {
                Debug.LogWarning("NewGameButton: MainMenuController not found in scene!");
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null && mainMenuController != null)
        {
            button.onClick.RemoveListener(mainMenuController.OnNewGameClicked);
        }
    }
}
