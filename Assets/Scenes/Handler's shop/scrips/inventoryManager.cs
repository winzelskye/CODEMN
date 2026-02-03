using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;
    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InventoryManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("InventoryManager");
                    instance = obj.AddComponent<InventoryManager>();
                }
            }
            return instance;
        }
    }

    [System.Serializable]
    public class InventoryItem
    {
        public shopItem item;
        public int quantity;

        public InventoryItem(shopItem item, int quantity = 1)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    public List<InventoryItem> inventory = new List<InventoryItem>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(shopItem item)
    {
        // Check if item already exists in inventory
        InventoryItem existingItem = inventory.Find(x => x.item == item);

        if (existingItem != null)
        {
            existingItem.quantity++;
        }
        else
        {
            inventory.Add(new InventoryItem(item, 1));
        }

        Debug.Log($"Added {item.itemName} to inventory. Total: {(existingItem?.quantity ?? 1)}");
    }

    public void RemoveItem(shopItem item)
    {
        InventoryItem existingItem = inventory.Find(x => x.item == item);

        if (existingItem != null)
        {
            existingItem.quantity--;
            if (existingItem.quantity <= 0)
            {
                inventory.Remove(existingItem);
            }
        }
    }

    public void UseItem(shopItem item)
    {
        // Apply item effects (you can customize this based on your game)
        Debug.Log($"Using {item.itemName}:");

        if (item.hpHealing > 0)
        {
            Debug.Log($"  Healing {item.hpHealing} HP");
            // Add your healing logic here
        }

        if (item.bitPointsAdded > 0)
        {
            Debug.Log($"  Adding {item.bitPointsAdded} bit points");
            // Add your bit points logic here
        }

        if (item.damageReduction > 0)
        {
            Debug.Log($"  Reducing damage by {item.damageReduction}");
            // Add your damage reduction logic here
        }

        RemoveItem(item);
    }

    public int GetItemCount(shopItem item)
    {
        InventoryItem existingItem = inventory.Find(x => x.item == item);
        return existingItem?.quantity ?? 0;
    }

    public List<InventoryItem> GetAllItems()
    {
        return inventory;
    }
}