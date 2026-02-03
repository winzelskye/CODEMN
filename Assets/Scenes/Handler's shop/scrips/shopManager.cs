using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Items Pool")]
    public List<GameObject> itemPrefabs = new List<GameObject>(); // List of different item prefabs

    [Header("Display Settings")]
    public List<Transform> itemSpawnPoints = new List<Transform>(); // All spawn points - items spawn at each one

    [Header("UI References")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemStatsText;
    public Button buyButton;

    [Header("Player Data")]
    public int playerCurrency = 100; // Starting currency

    private ItemDisplay currentItemDisplay;
    private shopItem currentItem;
    private List<shopItem> purchasedItems = new List<shopItem>();
    private List<GameObject> currentSpawnedItems = new List<GameObject>(); // Track all spawned items

    private void Start()
    {
        LoadRandomItem();
        UpdateCurrencyDisplay();

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(BuyCurrentItem);
        }
    }

    public void LoadRandomItem()
    {
        if (itemPrefabs.Count == 0)
        {
            Debug.LogWarning("No item prefabs available in the shop!");
            return;
        }

        if (itemSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points assigned in the shop!");
            return;
        }

        // Clear all previous items
        ClearAllItems();

        // Spawn a random item at EACH spawn point
        foreach (Transform spawnPoint in itemSpawnPoints)
        {
            // Pick random prefab for this spawn point
            int randomPrefabIndex = Random.Range(0, itemPrefabs.Count);
            GameObject selectedPrefab = itemPrefabs[randomPrefabIndex];

            // Instantiate the random prefab at this spawn point
            GameObject itemObj = Instantiate(selectedPrefab, spawnPoint);

            // IMPORTANT: Set local position to zero so it appears exactly at spawn point
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localPosition = Vector3.zero;
                itemRect.localRotation = Quaternion.identity;
            }

            currentSpawnedItems.Add(itemObj);

            // Get the ItemDisplay component
            ItemDisplay itemDisplay = itemObj.GetComponent<ItemDisplay>();

            if (itemDisplay != null)
            {
                // If this is the first item, set it as the current item for UI display
                if (currentItemDisplay == null)
                {
                    currentItemDisplay = itemDisplay;
                    currentItem = itemDisplay.itemData;
                }
            }
            else
            {
                Debug.LogError("Item prefab doesn't have ItemDisplay component!");
            }
        }

        UpdateItemInfoDisplay();
    }

    private void ClearAllItems()
    {
        // Destroy all currently spawned items
        foreach (GameObject item in currentSpawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }

        currentSpawnedItems.Clear();
        currentItemDisplay = null;
        currentItem = null;
    }

    private void UpdateItemInfoDisplay()
    {
        if (currentItem == null) return;

        if (itemNameText != null)
            itemNameText.text = currentItem.itemName;

        if (itemPriceText != null)
            itemPriceText.text = $"Cost: {currentItem.cost}";

        if (itemStatsText != null)
        {
            string stats = "";
            if (currentItem.hpHealing > 0)
                stats += $"HP Healing: +{currentItem.hpHealing}\n";
            if (currentItem.bitPointsAdded > 0)
                stats += $"Bit Points: +{currentItem.bitPointsAdded}\n";
            if (currentItem.damageReduction > 0)
                stats += $"Damage Reduction: +{currentItem.damageReduction}\n";

            itemStatsText.text = stats;
        }

        // Update buy button state
        if (buyButton != null)
        {
            buyButton.interactable = playerCurrency >= currentItem.cost;
        }
    }

    public void BuyCurrentItem()
    {
        if (currentItem == null) return;

        if (playerCurrency >= currentItem.cost)
        {
            playerCurrency -= currentItem.cost;
            purchasedItems.Add(currentItem);

            Debug.Log($"Purchased {currentItem.itemName}!");

            UpdateCurrencyDisplay();

            // Remove only the purchased item
            if (currentItemDisplay != null)
            {
                GameObject itemToRemove = currentItemDisplay.gameObject;
                currentSpawnedItems.Remove(itemToRemove);
                Destroy(itemToRemove);

                // Clear selection
                currentItemDisplay = null;
                currentItem = null;

                // Clear UI info
                if (itemNameText != null)
                    itemNameText.text = "";
                if (itemPriceText != null)
                    itemPriceText.text = "";
                if (itemStatsText != null)
                    itemStatsText.text = "";

                // Disable buy button since nothing is selected
                if (buyButton != null)
                    buyButton.interactable = false;
            }

            // Save to inventory system
            SaveToInventory(currentItem);
        }
        else
        {
            Debug.Log("Not enough currency!");
        }
    }

    private void UpdateCurrencyDisplay()
    {
        if (currencyText != null)
        {
            currencyText.text = $"Currency: {playerCurrency}";
        }
    }

    private void SaveToInventory(shopItem item)
    {
        // This is where you'd integrate with your inventory system
        // For now, we're just storing it in the purchasedItems list

        // Example using PlayerPrefs for simple persistence:
        string inventoryKey = "Inventory_" + item.itemName;
        int currentCount = PlayerPrefs.GetInt(inventoryKey, 0);
        PlayerPrefs.SetInt(inventoryKey, currentCount + 1);
        PlayerPrefs.Save();

        Debug.Log($"Saved {item.itemName} to inventory. Total: {currentCount + 1}");
    }

    public List<shopItem> GetPurchasedItems()
    {
        return purchasedItems;
    }

    public void AddCurrency(int amount)
    {
        playerCurrency += amount;
        UpdateCurrencyDisplay();
    }

    public void SelectItem(ItemDisplay itemDisplay)
    {
        // Deselect previous item
        if (currentItemDisplay != null)
        {
            currentItemDisplay.SetSelected(false);
        }

        // Select new item
        currentItemDisplay = itemDisplay;
        currentItem = itemDisplay.itemData;

        // Show selection animation
        currentItemDisplay.SetSelected(true);

        // Update UI to show this item's info
        UpdateItemInfoDisplay();
    }

    public void DeselectAllItems()
    {
        foreach (GameObject itemObj in currentSpawnedItems)
        {
            ItemDisplay itemDisplay = itemObj.GetComponent<ItemDisplay>();
            if (itemDisplay != null)
            {
                itemDisplay.SetSelected(false);
            }
        }
    }
}