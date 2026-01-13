using System;
using UnityEngine;

[System.Serializable]
public enum ItemCategory
{
    Consumable,
    Equipment
}

[CreateAssetMenu(fileName = "New Shop Item", menuName = "RPG/Shop Item")]
public class ShopItem : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite itemIcon;
    public ItemCategory category;

    [Header("Shop Settings")]
    public int price;
    [Range(0f, 100f)]
    public float spawnChance = 100f; // Percentage chance to appear in shop

    [Header("Consumable Effects")]
    public int hpRestore;
    public int bitPointsRestore;
    public string consumableEffect;

    [Header("Equipment Stats")]
    public int attackBoost;
    public int defenseBoost;
    public int hpBoost;
    public string equipmentEffect;

    [Header("Visual")]
    public GameObject itemPrefab;
    public AnimationClip useAnimation;
    public AudioClip useSound;
    public ParticleSystem useEffect;

    public bool IsConsumable()
    {
        return category == ItemCategory.Consumable;
    }

    public bool IsEquipment()
    {
        return category == ItemCategory.Equipment;
    }
}