using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button script for Shop Buy Item functionality.
/// Calls ShopController.OnBuyItemClicked()
/// </summary>
public class ShopBuyButton : MonoBehaviour
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
                button.onClick.AddListener(shopController.OnBuyItemClicked);
            }
            else
            {
                Debug.LogWarning("ShopBuyButton: ShopController not found in scene!");
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null && shopController != null)
        {
            button.onClick.RemoveListener(shopController.OnBuyItemClicked);
        }
    }
}
