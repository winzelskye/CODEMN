using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProceedButton : MonoBehaviour
{
    // Reference to the ConfirmationDialog GameObject
    public GameObject confirmationDialog;

    // Name of the scene to load (e.g., "CharacterSelect", "Level Select", etc.)
    // Default to CharacterSelect for New Game flow
    public string sceneToLoad = "CharacterSelect";

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
        // Flowchart "Reset Levels" step
        if (DataPersistenceManager.instance != null)
            DataPersistenceManager.instance.ResetLevelsOnly();
        for (int i = 1; i <= 10; i++)
            PlayerPrefs.DeleteKey("Level_" + i + "_Completed");
        PlayerPrefs.Save();

        // Full progress reset
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.NewGame();
            DataPersistenceManager.instance.SaveGame();
        }
        
        Debug.Log("Progress has been reset!");

        // Add any additional reset logic here:
        // - Reset game variables
        // - Clear inventory
        // - Reset player stats
        // - etc.
    }

    void LoadScene()
    {
        // Use GameFlowManager if available for proper flow integration
        if (GameFlowManager.Instance != null)
        {
            // Determine which scene to load based on context
            if (sceneToLoad == "CharacterSelect" || sceneToLoad.Contains("Character"))
            {
                GameFlowManager.Instance.GoToCharacterSelection();
                return;
            }
            else if (sceneToLoad == "Level Select" || sceneToLoad.Contains("Level"))
            {
                GameFlowManager.Instance.GoToLevelSelection();
                return;
            }
        }

        // Fallback to direct scene loading
        if (useSceneIndex)
        {
            // Load scene by index
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            // Load scene by name
            Debug.Log($"Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}