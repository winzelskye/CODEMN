using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Shop system matching flowchart: Item Preview → Buy Item → Check Gold → Check Inventory → Store Item.
/// Handles "Not enough Money" and "You don't have enough space" feedback.
/// </summary>
public class ShopController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject itemPreviewPanel;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button exitButton;

    [Header("Item Display")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemPriceText;

    [Header("Feedback Messages")]
    [SerializeField] private GameObject notEnoughMoneyPanel;
    [SerializeField] private GameObject inventoryFullPanel;
    [SerializeField] private GameObject purchaseSuccessPanel;
    [SerializeField] private TextMeshProUGUI feedbackMessageText;

    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> availableItems = new List<ShopItem>();

    [Header("Scene Names")]
    [SerializeField] private string levelSelectScene = "Level Select";

    private ShopItem currentPreviewItem;
    private PlayerInventory playerInventory;
    private const int MAX_INVENTORY_SIZE = 6;

    private void Start()
    {
        playerInventory = PlayerInventory.Instance;
        if (playerInventory == null)
        {
            playerInventory = FindObjectOfType<PlayerInventory>();
        }

        SetupButtons();
        HideFeedbackPanels();
    }

    private void SetupButtons()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(OnBuyItemClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    private void HideFeedbackPanels()
    {
        if (notEnoughMoneyPanel != null)
            notEnoughMoneyPanel.SetActive(false);
        if (inventoryFullPanel != null)
            inventoryFullPanel.SetActive(false);
        if (purchaseSuccessPanel != null)
            purchaseSuccessPanel.SetActive(false);
    }

    /// <summary>Show item preview (called when player clicks an item)</summary>
    public void ShowItemPreview(ShopItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot preview null item");
            return;
        }

        currentPreviewItem = item;

        // Update preview UI
        if (itemIconImage != null && item.itemIcon != null)
            itemIconImage.sprite = item.itemIcon;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = item.description;

        if (itemPriceText != null)
            itemPriceText.text = $"Price: {item.price} Gold";

        // Show preview panel
        if (itemPreviewPanel != null)
            itemPreviewPanel.SetActive(true);
    }

    /// <summary>Buy Item button clicked - follows flowchart logic</summary>
    public void OnBuyItemClicked()
    {
        if (currentPreviewItem == null)
        {
            Debug.LogWarning("No item selected for purchase");
            return;
        }

        Debug.Log($"Attempting to buy: {currentPreviewItem.itemName} for {currentPreviewItem.price} gold");

        // Step 1: Check if player has enough gold
        if (!HasEnoughGold(currentPreviewItem.price))
        {
            ShowNotEnoughMoney();
            return;
        }

        // Step 2: Check if inventory is full
        if (IsInventoryFull())
        {
            ShowInventoryFull();
            return;
        }

        // Step 3: Both checks passed - complete purchase
        CompletePurchase();
    }

    private bool HasEnoughGold(int price)
    {
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory not found!");
            return false;
        }

        return playerInventory.CanAfford(price);
    }

    private bool IsInventoryFull()
    {
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory not found!");
            return true; // Assume full if inventory not found
        }

        int currentItemCount = playerInventory.GetItemCount();
        return currentItemCount >= MAX_INVENTORY_SIZE;
    }

    private void CompletePurchase()
    {
        if (playerInventory == null || currentPreviewItem == null)
            return;

        // Deduct gold
        bool goldSpent = playerInventory.SpendGold(currentPreviewItem.price);
        if (!goldSpent)
        {
            Debug.LogError("Failed to spend gold despite check!");
            return;
        }

        // Add item to inventory
        bool itemAdded = playerInventory.AddItem(currentPreviewItem);
        if (!itemAdded)
        {
            Debug.LogError("Failed to add item to inventory!");
            // Refund gold
            playerInventory.AddGold(currentPreviewItem.price);
            return;
        }

        // Save purchase to GameData
        if (DataPersistenceManager.instance != null)
        {
            if (DataPersistenceManager.instance != null)
            {
                // Purchase history is tracked in GameData
                DataPersistenceManager.instance.SaveGame();
            }
        }

        // Show success feedback
        string purchasedItemName = currentPreviewItem.itemName;
        ShowPurchaseSuccess();

        // Clear preview
        currentPreviewItem = null;
        if (itemPreviewPanel != null)
            itemPreviewPanel.SetActive(false);

        Debug.Log($"Successfully purchased: {purchasedItemName}");
    }

    private void ShowNotEnoughMoney()
    {
        Debug.Log("Not enough money!");

        if (notEnoughMoneyPanel != null)
        {
            notEnoughMoneyPanel.SetActive(true);
            Invoke(nameof(HideFeedbackPanels), 2f); // Hide after 2 seconds
        }

        if (feedbackMessageText != null)
            feedbackMessageText.text = "Not enough Money";

        // Return to shop (panel stays visible)
    }

    private void ShowInventoryFull()
    {
        Debug.Log("Inventory is full!");

        if (inventoryFullPanel != null)
        {
            inventoryFullPanel.SetActive(true);
            Invoke(nameof(HideFeedbackPanels), 2f); // Hide after 2 seconds
        }

        if (feedbackMessageText != null)
            feedbackMessageText.text = "You don't have enough space for more";

        // Return to shop (panel stays visible)
    }

    private void ShowPurchaseSuccess()
    {
        if (purchaseSuccessPanel != null)
        {
            purchaseSuccessPanel.SetActive(true);
            Invoke(nameof(HideFeedbackPanels), 1.5f); // Hide after 1.5 seconds
        }

        if (feedbackMessageText != null)
            feedbackMessageText.text = "Item is now stored in your inventory";
    }

    /// <summary>Exit/Next level button - return to Level Selection</summary>
    public void OnExitClicked()
    {
        Debug.Log("Exiting shop...");

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToLevelSelection();
        }
        else
        {
            SceneManager.LoadScene(levelSelectScene);
        }
    }

    /// <summary>Initialize shop with available items</summary>
    public void SetAvailableItems(List<ShopItem> items)
    {
        availableItems = items;
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveListener(OnBuyItemClicked);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);
    }
}
