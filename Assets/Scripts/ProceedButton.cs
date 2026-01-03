using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProceedButton : MonoBehaviour
{
    // Reference to the ConfirmationDialog GameObject
    public GameObject confirmationDialog;

    // Name of the scene to load (e.g., "MainMenu", "NewGame", etc.)
    public string sceneToLoad = "MainMenu";

    // Alternative: use scene build index
    public int sceneIndex = 0;
    public bool useSceneIndex = false;

    void Start()
    {
        // Get the Button component and add a listener
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnProceedButtonClicked);
        }

        // If confirmationDialog is not assigned, try to find it automatically
        if (confirmationDialog == null)
        {
            // Navigate up the hierarchy to find the ConfirmationDialog
            Transform current = transform;
            while (current != null)
            {
                if (current.name == "ConfirmationDialog")
                {
                    confirmationDialog = current.gameObject;
                    break;
                }
                current = current.parent;
            }
        }
    }

    void OnProceedButtonClicked()
    {
        Debug.Log("Proceed button clicked - Resetting progress");
        
        // Close the confirmation dialog first
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }

        // Reset player progress (customize based on your game)
        ResetProgress();

        // Load the scene
        LoadScene();
    }

    void ResetProgress()
    {
        // Clear PlayerPrefs (saved data)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("Progress has been reset!");

        // Add any additional reset logic here:
        // - Reset game variables
        // - Clear inventory
        // - Reset player stats
        // - etc.
    }

    void LoadScene()
    {
        if (useSceneIndex)
        {
            // Load scene by index
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            // Load scene by name
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}