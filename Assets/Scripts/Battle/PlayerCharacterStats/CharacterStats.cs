using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

[System.Serializable]
public class CharacterStatsData
{
    public string characterName;
    public int currentHP;
    public int maxHP;
    public int level;
    public List<Attack> attacksAvailable;
    public SpecialSkill specialSkill;
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

    [Header("Abilities")]
    public List<Attack> attacksAvailable = new List<Attack>();
    public SpecialSkill specialSkill;

    [Header("Animations")]
    public List<AnimationPhase> animationPhases = new List<AnimationPhase>();

    // Events for stat changes
    public event Action OnStatsChanged;
    public event Action OnHPChanged;

    public void Initialize()
    {
        currentHP = maxHP;

        // Initialize animation phases if empty
        if (animationPhases.Count == 0)
        {
            animationPhases.Add(new AnimationPhase
            {
                phaseName = "Idle",
                description = "Idle animation",
                frameRate = 12f,
                loop = true
            });

            animationPhases.Add(new AnimationPhase
            {
                phaseName = "Hurt",
                description = "Hurt animation",
                frameRate = 12f,
                loop = false
            });
        }
    }

    public void ModifyHP(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        if (OnHPChanged != null) OnHPChanged.Invoke();
        if (OnStatsChanged != null) OnStatsChanged.Invoke();
    }

    public void SetHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, maxHP);
        if (OnHPChanged != null) OnHPChanged.Invoke();
        if (OnStatsChanged != null) OnStatsChanged.Invoke();
    }

    public bool CanUseAttack(Attack attack)
    {
        return true; // No bit cost requirement anymore
    }

    public bool UseAttack(Attack attack)
    {
        Debug.Log(characterName + " used " + attack.name + " for " + attack.damage + " damage!");
        if (OnStatsChanged != null) OnStatsChanged.Invoke();
        return true;
    }

    public bool CanUseSpecialSkill()
    {
        return true; // No bit cost requirement anymore
    }

    public bool UseSpecialSkill()
    {
        Debug.Log(characterName + " used " + specialSkill.name + "!");
        if (OnStatsChanged != null) OnStatsChanged.Invoke();
        return true;
    }

    public void SetLevel(int newLevel)
    {
        level = Mathf.Max(1, newLevel);
        if (OnStatsChanged != null) OnStatsChanged.Invoke();
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

    // JSON Save/Load Methods
    public void SaveToJson(string filePath)
    {
        CharacterStatsData data = new CharacterStatsData
        {
            characterName = this.characterName,
            currentHP = this.currentHP,
            maxHP = this.maxHP,
            level = this.level,
            attacksAvailable = this.attacksAvailable,
            specialSkill = this.specialSkill
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Character stats saved to: " + filePath);
    }

    public void LoadFromJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CharacterStatsData data = JsonUtility.FromJson<CharacterStatsData>(json);

            this.characterName = data.characterName;
            this.currentHP = data.currentHP;
            this.maxHP = data.maxHP;
            this.level = data.level;
            this.attacksAvailable = data.attacksAvailable;
            this.specialSkill = data.specialSkill;

            if (OnStatsChanged != null) OnStatsChanged.Invoke();
            Debug.Log("Character stats loaded from: " + filePath);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    public string ExportToJson()
    {
        CharacterStatsData data = new CharacterStatsData
        {
            characterName = this.characterName,
            currentHP = this.currentHP,
            maxHP = this.maxHP,
            level = this.level,
            attacksAvailable = this.attacksAvailable,
            specialSkill = this.specialSkill
        };

        return JsonUtility.ToJson(data, true);
    }

    public void ImportFromJson(string json)
    {
        CharacterStatsData data = JsonUtility.FromJson<CharacterStatsData>(json);

        this.characterName = data.characterName;
        this.currentHP = data.currentHP;
        this.maxHP = data.maxHP;
        this.level = data.level;
        this.attacksAvailable = data.attacksAvailable;
        this.specialSkill = data.specialSkill;

        if (OnStatsChanged != null) OnStatsChanged.Invoke();
    }
}