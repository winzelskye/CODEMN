using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
    public string name;
    public int bitCost;
    public int damage;
    public string description;
}

[System.Serializable]
public class SpecialSkill
{
    public string name;
    public int bitCost;
    public string effect;
    public string description;
}

[System.Serializable]
public class AnimationPhase
{
    public string phaseName;
    public string description;
    public AnimationClip animationClip;
    public RuntimeAnimatorController animatorController;
    public Sprite phaseSprite;
    public Texture2D spriteSheet;
    public float frameRate = 12f;
    public bool loop = true;
}

[CreateAssetMenu(fileName = "New Character", menuName = "RPG/Character Stats")]
public class CharacterStats : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    public Sprite characterSprite;

    [Header("Stats")]
    public int currentHP;
    public int maxHP = 100;
    public int level = 1;
    public string attackType;
    public int bitPoints = 0;

    [Header("Abilities")]
    public List<Attack> attacksAvailable = new List<Attack>();
    public SpecialSkill specialSkill;

    [Header("Animations")]
    public List<AnimationPhase> animationPhases = new List<AnimationPhase>();

    // Events for stat changes
    public event Action OnStatsChanged;
    public event Action OnBitPointsChanged;
    public event Action OnHPChanged;

    public void Initialize()
    {
        currentHP = maxHP;
        bitPoints = 0;
    }

    public void ModifyHP(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        OnHPChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public void SetHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, maxHP);
        OnHPChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public void AddBitPoints(int amount)
    {
        bitPoints += amount;
        bitPoints = Mathf.Max(0, bitPoints);
        OnBitPointsChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public void SetBitPoints(int value)
    {
        bitPoints = Mathf.Max(0, value);
        OnBitPointsChanged?.Invoke();
        OnStatsChanged?.Invoke();
    }

    public bool CanUseAttack(Attack attack)
    {
        return bitPoints >= attack.bitCost;
    }

    public bool UseAttack(Attack attack)
    {
        if (CanUseAttack(attack))
        {
            bitPoints -= attack.bitCost;
            OnBitPointsChanged?.Invoke();
            OnStatsChanged?.Invoke();
            Debug.Log(characterName + " used " + attack.name + " for " + attack.damage + " damage!");
            return true;
        }
        Debug.Log("Not enough Bit Points to use " + attack.name);
        return false;
    }

    public bool CanUseSpecialSkill()
    {
        return bitPoints >= specialSkill.bitCost;
    }

    public bool UseSpecialSkill()
    {
        if (CanUseSpecialSkill())
        {
            bitPoints -= specialSkill.bitCost;
            OnBitPointsChanged?.Invoke();
            OnStatsChanged?.Invoke();
            Debug.Log(characterName + " used " + specialSkill.name + "!");
            return true;
        }
        Debug.Log("Not enough Bit Points to use " + specialSkill.name);
        return false;
    }

    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
        OnStatsChanged?.Invoke();
    }

    public AnimationPhase GetAnimationPhase(string phaseName)
    {
        foreach (AnimationPhase phase in animationPhases)
        {
            if (phase.phaseName == phaseName)
            {
                return phase;
            }
        }
        return null;
    }
}