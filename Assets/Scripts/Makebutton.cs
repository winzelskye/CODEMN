using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MakeButton : MonoBehaviour
{
    // Reference to the button component
    private Button button;
    
    [Header("Confirmation Dialog")]
    [Tooltip("Drag the ConfirmationDialog panel here")]
    public GameObject confirmationDialog;

    void Start()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();

        // Check if button exists
        if (button != null)
        {
            // Add a listener to handle button clicks
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("No Button component found on " + gameObject.name);
        }
    }

    // This method is called when the button is clicked
    void OnButtonClick()
    {
        Debug.Log(gameObject.name + " button was clicked!");

        // Show the confirmation dialog
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            Debug.Log("Confirmation dialog opened!");
        }
        else
        {
            Debug.LogError("Confirmation Dialog is not assigned in the Inspector!");
        }
    }

    // Call this method from the "Proceed" button in your confirmation dialog
    public void OnProceedClicked()
    {
        Debug.Log("Proceeding with New Game - Resetting progress...");
        
        // Reset player progress (delete all saved data)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // Load the first game scene
        // IMPORTANT: Change "GameScene" to the name of your actual first game scene
        SceneManager.LoadScene("GameScene");
        
        // Alternative: If you want to reload the current scene for testing:
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Call this method from the "Dont" button in your confirmation dialog
    public void OnDontClicked()
    {
        Debug.Log("New Game cancelled - Closing dialog...");
        
        // Hide the confirmation dialog
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
    }

    // Optional: Remove listener when object is destroyed to prevent memory leaks
    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}