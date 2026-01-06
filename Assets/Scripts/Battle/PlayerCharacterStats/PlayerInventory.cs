using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ShopItem item;
    public bool isEquipped;
}

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public List<InventorySlot> items = new List<InventorySlot>();
    public List<InventorySlot> equipment = new List<InventorySlot>();

    [Header("Currency")]
    public int gold = 0;

    [Header("Equipment Slots")]
    public ShopItem equippedItem;

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;
    public event Action OnGoldChanged;

    private static PlayerInventory instance;
    public static PlayerInventory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerInventory>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItem(ShopItem item)
    {
        if (item == null) return false;

        InventorySlot newSlot = new InventorySlot { item = item, isEquipped = false };

        if (item.category == ItemCategory.Consumable)
        {
            items.Add(newSlot);
            if (OnInventoryChanged != null) OnInventoryChanged.Invoke();
            Debug.Log("Added " + item.itemName + " to items");
            return true;
        }
        else if (item.category == ItemCategory.Equipment)
        {
            equipment.Add(newSlot);
            if (OnEquipmentChanged != null) OnEquipmentChanged.Invoke();
            Debug.Log("Added " + item.itemName + " to equipment");
            return true;
        }

        return false;
    }

    public bool RemoveItem(ShopItem item, bool fromEquipment)
    {
        List<InventorySlot> targetList = fromEquipment ? equipment : items;

        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].item == item)
            {
                targetList.RemoveAt(i);

                if (fromEquipment)
                {
                    if (OnEquipmentChanged != null) OnEquipmentChanged.Invoke();
                }
                else
                {
                    if (OnInventoryChanged != null) OnInventoryChanged.Invoke();
                }

                return true;
            }
        }

        return false;
    }

    public bool UseItem(ShopItem item, CharacterStats character)
    {
        if (item == null || item.category != ItemCategory.Consumable) return false;

        if (item.hpRestore > 0)
        {
            character.ModifyHP(item.hpRestore);
            Debug.Log("Restored " + item.hpRestore + " HP");
        }

        if (item.bitPointsRestore > 0)
        {
            character.AddBitPoints(item.bitPointsRestore);
            Debug.Log("Restored " + item.bitPointsRestore + " Bit Points");
        }

        RemoveItem(item, false);

        return true;
    }

    public bool EquipItem(ShopItem item)
    {
        if (item == null || item.category != ItemCategory.Equipment) return false;

        if (equippedItem != null)
        {
            UnequipItem();
        }

        for (int i = 0; i < equipment.Count; i++)
        {
            if (equipment[i].item == item)
            {
                equipment[i].isEquipped = true;
                equippedItem = item;
                if (OnEquipmentChanged != null) OnEquipmentChanged.Invoke();
                Debug.Log("Equipped " + item.itemName);
                return true;
            }
        }

        return false;
    }

    public void UnequipItem()
    {
        if (equippedItem == null) return;

        for (int i = 0; i < equipment.Count; i++)
        {
            if (equipment[i].item == equippedItem)
            {
                equipment[i].isEquipped = false;
                break;
            }
        }

        Debug.Log("Unequipped " + equippedItem.itemName);
        equippedItem = null;
        if (OnEquipmentChanged != null) OnEquipmentChanged.Invoke();
    }

    public bool HasItem(ShopItem item)
    {
        foreach (var slot in items)
        {
            if (slot.item == item) return true;
        }

        foreach (var slot in equipment)
        {
            if (slot.item == item) return true;
        }

        return false;
    }

    public bool AddGold(int amount)
    {
        gold += amount;
        gold = Mathf.Max(0, gold);
        if (OnGoldChanged != null) OnGoldChanged.Invoke();
        return true;
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            if (OnGoldChanged != null) OnGoldChanged.Invoke();
            return true;
        }
        return false;
    }

    public bool CanAfford(int price)
    {
        return gold >= price;
    }

    public int GetItemCount()
    {
        return items.Count;
    }

    public int GetEquipmentCount()
    {
        return equipment.Count;
    }

    public void ClearInventory()
    {
        items.Clear();
        equipment.Clear();
        equippedItem = null;
        if (OnInventoryChanged != null) OnInventoryChanged.Invoke();
        if (OnEquipmentChanged != null) OnEquipmentChanged.Invoke();
    }
}