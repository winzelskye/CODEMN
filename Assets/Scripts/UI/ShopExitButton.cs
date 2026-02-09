using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button script for Shop Exit functionality.
/// Calls ShopController.OnExitClicked()
/// </summary>
public class ShopExitButton : MonoBehaviour
{
    private Button button;
    private ShopController shopController;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            shopController = FindObjectOfType<ShopController>();
            if (shopController != null)
            {
                button.onClick.AddListener(shopController.OnExitClicked);
            }
            else
            {
                Debug.LogWarning("ShopExitButton: ShopController not found in scene!");
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null && shopController != null)
        {
            button.onClick.RemoveListener(shopController.OnExitClicked);
        }
    }
}
