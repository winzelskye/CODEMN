using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Level Progression
    public SerializableDictionary<string, bool> levelsCompleted;
    public string currentLevel;
    
    // Sublevel Tracking
    public SerializableDictionary<string, bool> sublevelsCompleted; // Key: "Level_1_Sublevel_1", Value: completion status
    public SerializableDictionary<string, int> currentSublevelIndex; // Key: "Level_1", Value: current sublevel index (0-based)
    public SerializableDictionary<string, int> totalSublevelsPerLevel; // Key: "Level_1", Value: total sublevels in this level
    
    // Shop & Rewards
    public int goldEarned; // Total gold earned from battles
    public List<string> shopPurchaseHistory; // Optional: track purchased items
    public string lastVisitedScene; // Track last scene for Continue functionality

    // Player Character Selection & Stats
    public string selectedCharacterName; // "Esther" or "Michael"
    public int health;
    public int damage;
    public int defense;
    public string specialSkill;
    public Vector3 playerPosition;

    // Player Inventory (max 6 items)
    public List<string> inventoryItems; // List of item IDs (max 6)

    // Settings
    public float musicVolume;
    public float sfxVolume;
    public bool fullscreen;

    // Constructor with default values for new game
    public GameData()
    {
        // Level Progression defaults
        this.levelsCompleted = new SerializableDictionary<string, bool>();
        this.currentLevel = "Level_1";
        
        // Sublevel defaults
        this.sublevelsCompleted = new SerializableDictionary<string, bool>();
        this.currentSublevelIndex = new SerializableDictionary<string, int>();
        this.totalSublevelsPerLevel = new SerializableDictionary<string, int>();
        
        // Shop defaults
        this.goldEarned = 0;
        this.shopPurchaseHistory = new List<string>();
        this.lastVisitedScene = "MainMenu";

        // Default Character (Esther)
        this.selectedCharacterName = "Esther";
        this.health = 100;
        this.damage = 10;
        this.defense = 40;
        this.specialSkill = "Padding - Reduces the empty space in your health. Heals 15HP.";
        this.playerPosition = Vector3.zero;

        // Inventory defaults (empty, max 6)
        this.inventoryItems = new List<string>();

        // Settings defaults
        this.musicVolume = 0.5f;
        this.sfxVolume = 0.5f;
        this.fullscreen = true;
    }
}