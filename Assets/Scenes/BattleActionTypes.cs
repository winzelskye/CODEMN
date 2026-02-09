using UnityEngine;

/// <summary>
/// Player action chosen from the battle menu (FIGHT, SKILLS, ITEMS, etc.).
/// Used to drive which minigame runs and how damage is applied.
/// </summary>
public enum ActionType
{
    Attack,
    Skill,
    Item
}
