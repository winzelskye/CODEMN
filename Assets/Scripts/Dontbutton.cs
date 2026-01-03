using UnityEngine;
using UnityEngine.UI;

public class DontButton : MonoBehaviour
{
    // Reference to the ConfirmationDialog GameObject
    public GameObject confirmationDialog;

    void Start()
    {
        // Get the Button component and add a listener
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnDontButtonClicked);
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

    void OnDontButtonClicked()
    {
        Debug.Log("Don't button clicked - Closing confirmation dialog");
        
        // Close/hide the confirmation dialog
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ConfirmationDialog reference is missing!");
        }

        // Add any additional logic here
        // For example, if you don't want to reset progress:
        // - Resume the game
        // - Return to previous menu
    }
}