using UnityEngine;
using UnityEngine.UI;

public class MakeButton : MonoBehaviour
{
    // Reference to the button component
    private Button button;

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

        // Add your button functionality here
        // For example, if this is a panel that should open/close:
        // gameObject.SetActive(false);

        // Or if it should show/hide something:
        // SomeOtherPanel.SetActive(true);
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