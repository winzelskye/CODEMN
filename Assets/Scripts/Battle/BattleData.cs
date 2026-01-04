using System;
using System.Collections.Generic;
using UnityEngine;

// ===================================================
// DIALOGUE SYSTEM
// ===================================================
[Serializable]
public class DialogueEntry
{
    public int turnNumber;
    public string dialogueText;
    public Sprite characterPortrait;
    public string characterName;
    public float displayDuration = 3f;
    public string triggerCondition;
    public int healthThreshold;
    public Sprite customDialogueBox;
}

// ===================================================
// MINIGAME ATTACKS
// ===================================================
[Serializable]
public class MinigameAttack
{
    public string attackName;
    public string minigameType;
    public int damageOnFail;
    public float timeLimit;
    public string description;

    public List<string> questions = new List<string>();
    public List<string> correctAnswers = new List<string>();
    public List<string> wrongAnswers = new List<string>();

    public int dragDropItemCount;
    public string dragDropTheme;

    public List<string> codeBlocks = new List<string>();
    public string correctOrder;

    public Sprite attackIcon;
    public Color themeColor = Color.white;

    public bool shuffleOptions;
    public float timeReduction;
}

// ===================================================
// DISTRACTION MECHANICS
// ===================================================
[Serializable]
public class DistractionMechanic
{
    public string mechanicName;
    public string mechanicType;
    public bool isEnabled;
    public float activationChance;
    public float duration;
    public string description;

    public Sprite distractionImage;
    public Color flashColor;
    public float intensity;

    public float delayBeforeActivation;
    public bool triggerOnce;
}

// ===================================================
// ENEMY STATS (UPDATED)
// ===================================================
[Serializable]
public class EnemyStats
{
    public string enemyName;
    public Sprite enemySprite;
    public string animatorController;

    // Core stats
    public int maxHealth;
    public int attack;
    public int defense;
    public int speed;

    // Visual settings
    public Color enemyColor = Color.white;
    public Vector3 scale = Vector3.one;
    public Vector3 battlePosition;

    // Damage effect settings
    [Header("Damage Effects")]
    public bool enableDamageTint = true;
    public Color damageTintColor = Color.red;
    public float damageTintDuration = 0.2f;

    public bool enableDamageShake = true;
    public float damageShakeIntensity = 0.5f;
    public float damageShakeDuration = 0.3f;
}

// ===================================================
// BATTLE PHASES (UPDATED)
// ===================================================
[Serializable]
public class BattlePhase
{
    public string phaseName;
    public int healthThreshold;
    public float phaseSpeed = 1f;
    public List<string> enabledAttacks = new List<string>();
    public string dialogueLine;
    public bool changeMusic;
    public AudioClip phaseMusic;
    public float timeReduction;

    // Phase sprite change
    [Header("Phase Visual Changes")]
    public bool changeSprite;
    public Sprite phaseSprite;
    public Color phaseTintColor = Color.white;

    // Phase animation
    public bool changeAnimatorState;
    public string animatorStateName;
}

// ===================================================
// MAIN BATTLE DATA
// ===================================================
[Serializable]
public class BattleData
{
    public string id;
    public string battleName;
    public string description;

    public EnemyStats enemy = new EnemyStats();
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
    public List<MinigameAttack> attacks = new List<MinigameAttack>();
    public List<BattlePhase> phases = new List<BattlePhase>();
    public List<DistractionMechanic> distractions = new List<DistractionMechanic>();

    public string backgroundScene;
    public AudioClip battleMusic;
    public bool isBossBattle;
    public int experienceReward;
    public int goldReward;
    public List<string> itemDrops = new List<string>();

    public float globalTimeModifier = 1f;
    public int basePlayerHealth = 100;

    public BattleData()
    {
        id = Guid.NewGuid().ToString();
        battleName = "New Battle";
        description = "Battle description";
        enemy.enemyName = "New Enemy";
        enemy.maxHealth = 100;
        enemy.attack = 10;
        enemy.defense = 5;
        enemy.speed = 5;
        basePlayerHealth = 100;
    }
}

[Serializable]
public class BattleCollection
{
    public List<BattleData> battles = new List<BattleData>();
}