using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Items Pool")]
    public List<GameObject> itemPrefabs = new List<GameObject>();

    [Header("Display Settings")]
    public List<Transform> itemSpawnPoints = new List<Transform>();

    [Header("UI References")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemStatsText;
    public TextMeshProUGUI inventoryFullText;
    public Button buyButton;

    private const int MAX_INVENTORY = 6;
    private ItemDisplay currentItemDisplay;
    private shopItem currentItem;
    private List<GameObject> currentSpawnedItems = new List<GameObject>();

    private void Start()
    {
        LoadRandomItem();
        UpdateCurrencyDisplay();

        if (buyButton != null)
            buyButton.onClick.AddListener(BuyCurrentItem);

        if (inventoryFullText != null)
            inventoryFullText.gameObject.SetActive(false);
    }

    public void LoadRandomItem()
    {
        if (itemPrefabs.Count == 0 || itemSpawnPoints.Count == 0) return;

        ClearAllItems();

        foreach (Transform spawnPoint in itemSpawnPoints)
        {
            int randomPrefabIndex = Random.Range(0, itemPrefabs.Count);
            GameObject itemObj = Instantiate(itemPrefabs[randomPrefabIndex], spawnPoint);

            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.localPosition = Vector3.zero;
                itemRect.localRotation = Quaternion.identity;
            }

            currentSpawnedItems.Add(itemObj);

            ItemDisplay itemDisplay = itemObj.GetComponent<ItemDisplay>();
            if (itemDisplay != null)
            {
                if (currentItemDisplay == null)
                {
                    currentItemDisplay = itemDisplay;
                    currentItem = itemDisplay.itemData;
                }
            }
        }

        UpdateItemInfoDisplay();
    }

    private void ClearAllItems()
    {
        foreach (GameObject item in currentSpawnedItems)
            if (item != null) Destroy(item);

        currentSpawnedItems.Clear();
        currentItemDisplay = null;
        currentItem = null;
    }

    private int GetTotalInventoryCount()
    {
        return SaveLoadManager.Instance.GetInventory().Count;
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

        bool inventoryFull = GetTotalInventoryCount() >= MAX_INVENTORY;
        bool canAfford = SaveLoadManager.Instance.GetCurrency() >= currentItem.cost;

        if (buyButton != null)
            buyButton.interactable = canAfford && !inventoryFull;

        if (inventoryFullText != null)
            inventoryFullText.gameObject.SetActive(inventoryFull);
    }

    public void BuyCurrentItem()
    {
        if (currentItem == null) return;

        if (GetTotalInventoryCount() >= MAX_INVENTORY)
        {
            Debug.Log("Inventory full!");
            return;
        }

        if (SaveLoadManager.Instance.SpendCurrency(currentItem.cost))
        {
            SaveLoadManager.Instance.AddToInventory(currentItem);
            Debug.Log($"Purchased {currentItem.itemName}!");
            UpdateCurrencyDisplay();

            if (currentItemDisplay != null)
            {
                GameObject itemToRemove = currentItemDisplay.gameObject;
                currentSpawnedItems.Remove(itemToRemove);
                Destroy(itemToRemove);

                currentItemDisplay = null;
                currentItem = null;

                if (itemNameText != null) itemNameText.text = "";
                if (itemPriceText != null) itemPriceText.text = "";
                if (itemStatsText != null) itemStatsText.text = "";
                if (buyButton != null) buyButton.interactable = false;
                if (inventoryFullText != null) inventoryFullText.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Not enough currency!");
        }
    }

    private void UpdateCurrencyDisplay()
    {
        if (currencyText != null)
            currencyText.text = $"Currency: {SaveLoadManager.Instance.GetCurrency()}";
    }

    public void SelectItem(ItemDisplay itemDisplay)
    {
        if (currentItemDisplay != null)
            currentItemDisplay.SetSelected(false);

        currentItemDisplay = itemDisplay;
        currentItem = itemDisplay.itemData;
        currentItemDisplay.SetSelected(true);

        UpdateItemInfoDisplay();
    }

    public void DeselectAllItems()
    {
        foreach (GameObject itemObj in currentSpawnedItems)
        {
            ItemDisplay itemDisplay = itemObj.GetComponent<ItemDisplay>();
            if (itemDisplay != null)
                itemDisplay.SetSelected(false);
        }
    }

    public void AddCurrency(int amount)
    {
        SaveLoadManager.Instance.AddCurrency(amount);
        UpdateCurrencyDisplay();
    }
}