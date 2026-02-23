using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ItemSlot
{
    public Button button;
    public TextMeshProUGUI itemNameText;
}

public class ItemListManager : MonoBehaviour
{
    [Header("Item Slots (max 6)")]
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    [Header("Player Action Buttons")]
    public GameObject fightButtons;
    public GameObject skillsButtons;
    public GameObject itemsButtons;

    private List<InventoryItem> inventory;

    void OnEnable()
    {
        LoadItems();
    }

    void LoadItems()
    {
        inventory = SaveLoadManager.Instance.GetInventory();

        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < inventory.Count)
            {
                InventoryItem item = inventory[i];

                if (itemSlots[i].button != null)
                    itemSlots[i].button.gameObject.SetActive(true);

                if (itemSlots[i].itemNameText != null)
                    itemSlots[i].itemNameText.text = item.itemName;

                int index = i;
                if (itemSlots[i].button != null)
                {
                    itemSlots[i].button.onClick.RemoveAllListeners();
                    itemSlots[i].button.onClick.AddListener(() => UseItem(index));
                }
            }
            else
            {
                if (itemSlots[i].button != null)
                    itemSlots[i].button.gameObject.SetActive(false);
            }
        }
    }

    void UseItem(int index)
    {
        if (index >= inventory.Count) return;

        InventoryItem item = inventory[index];

        if (item.hpHealing > 0)
        {
            Debug.Log($"Healing for {item.hpHealing}, current HP: {BattleManager.Instance.player.currentHealth}");
            BattleManager.Instance.player.TakeDamage(-item.hpHealing);
            Debug.Log($"HP after heal: {BattleManager.Instance.player.currentHealth}");
            BattleManager.Instance.ShowBattleDialogue($"Used {item.itemName}! Recovered {item.hpHealing} HP!");
        }

        if (item.bitPointsAdded > 0)
        {
            BattleManager.Instance.player.AddBitpoints(item.bitPointsAdded);
            BattleManager.Instance.ShowBattleDialogue($"Used {item.itemName}! Added {item.bitPointsAdded} BP!");
        }

        if (item.damageReduction > 0)
        {
            BattleManager.Instance.ApplyDamageReduction(item.damageReduction);
            BattleManager.Instance.ShowBattleDialogue($"Used {item.itemName}! Damage reduced by {item.damageReduction}!");
        }

        SaveLoadManager.Instance.DeleteInventoryItem(item.id);

        gameObject.SetActive(false);
        ShowBattleUI();
        BattleManager.Instance.OnPlayerAttackResult(true, false);
    }

    public void ShowBattleUI()
    {
        if (fightButtons != null) fightButtons.SetActive(true);
        if (skillsButtons != null) skillsButtons.SetActive(true);
        if (itemsButtons != null) itemsButtons.SetActive(true);
    }
}