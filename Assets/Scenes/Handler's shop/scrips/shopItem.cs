using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Item")]
public class shopItem : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public int cost;

    [Header("Item Stats")]
    public int hpHealing;
    public int bitPointsAdded;
    public int damageReduction;

    [Header("Item Visuals")]
    public Sprite staticSprite; // Shown when NOT selected

    [Header("Selection Animation (3 frames)")]
    public Sprite selectedFrame1;
    public Sprite selectedFrame2;
    public Sprite selectedFrame3;

    [Header("Animation Settings")]
    public float animationSpeed = 0.2f; // Time between frames
}