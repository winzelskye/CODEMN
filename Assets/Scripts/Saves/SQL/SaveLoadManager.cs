using UnityEngine;
using SQLite;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private SQLiteConnection db => DatabaseManager.Instance.db;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // PLAYER
    public void SavePlayer(string name, string character, int level)
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.db == null)
        {
            Debug.LogError("Database not ready!");
            return;
        }
        db.InsertOrReplace(new Player { id = 1, playerName = name, selectedCharacter = character, currentLevel = level });
    }

    public Player LoadPlayer()
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.db == null) return null;
        return db.Find<Player>(1);
    }

    // CHARACTER STATS
    public CharacterStats LoadCharacterStats(string characterName)
    {
        return db.Table<CharacterStats>().Where(c => c.characterName == characterName).FirstOrDefault();
    }

    // ATTACKS
    public List<AttackData> GetNormalAttacks(string characterName)
    {
        return db.Table<AttackData>()
                 .Where(a => a.isUnlocked == 1 && a.isSpecial == 0 && a.isSkill == 0 &&
                            (a.forCharacter == "both" || a.forCharacter == characterName))
                 .ToList();
    }

    public AttackData GetSpecialAttack(string characterName)
    {
        return db.Table<AttackData>()
                 .Where(a => a.isSpecial == 1 && a.forCharacter == characterName)
                 .FirstOrDefault();
    }

    // SKILLS
    public List<AttackData> GetSkills(string characterName)
    {
        return db.Table<AttackData>()
                 .Where(a => a.isSkill == 1 && a.isUnlocked == 1 &&
                            (a.forCharacter == "both" || a.forCharacter == characterName))
                 .ToList();
    }

    public void UnlockAttacksForLevel(int level)
    {
        var attacks = db.Table<AttackData>().Where(a => a.unlockLevel <= level).ToList();
        foreach (var a in attacks) { a.isUnlocked = 1; db.Update(a); }
    }

    // LEVELS
    public List<LevelData> GetAllLevels()
    {
        if (DatabaseManager.Instance == null || DatabaseManager.Instance.db == null) return new List<LevelData>();
        return db.Table<LevelData>().ToList();
    }

    public void CompleteLevel(int levelId)
    {
        var current = db.Find<LevelData>(levelId);
        if (current != null) { current.isCompleted = 1; db.Update(current); }
        var next = db.Find<LevelData>(levelId + 1);
        if (next != null) { next.isUnlocked = 1; db.Update(next); }
        UnlockAttacksForLevel(levelId + 1);
    }

    // ENEMY
    public EnemyData GetEnemyForLevel(int levelId)
    {
        return db.Table<EnemyData>().Where(e => e.levelId == levelId).FirstOrDefault();
    }

    // BATTLE RESULT
    public void SaveBattleResult(int levelId, bool cleared)
    {
        var existing = db.Table<BattleResult>().Where(b => b.levelId == levelId).FirstOrDefault();
        if (existing != null)
        {
            if (cleared) existing.isCleared = 1;
            existing.attempts += 1;
            db.Update(existing);
        }
        else
        {
            db.Insert(new BattleResult { levelId = levelId, isCleared = cleared ? 1 : 0, attempts = 1 });
        }
    }

    // CURRENCY
    public int GetCurrency()
    {
        var currency = db.Find<PlayerCurrency>(1);
        return currency != null ? currency.amount : 0;
    }

    public void SetCurrency(int amount)
    {
        db.InsertOrReplace(new PlayerCurrency { id = 1, amount = amount });
    }

    public void AddCurrency(int amount)
    {
        int current = GetCurrency();
        SetCurrency(current + amount);
    }

    public bool SpendCurrency(int amount)
    {
        int current = GetCurrency();
        if (current < amount) return false;
        SetCurrency(current - amount);
        return true;
    }

    // INVENTORY
    public List<InventoryItem> GetInventory()
    {
        return db.Table<InventoryItem>().ToList();
    }

    public void AddToInventory(shopItem item)
    {
        var existing = db.Table<InventoryItem>().Where(i => i.itemName == item.itemName).FirstOrDefault();
        if (existing != null)
        {
            existing.quantity += 1;
            db.Update(existing);
        }
        else
        {
            db.Insert(new InventoryItem
            {
                itemName = item.itemName,
                quantity = 1,
                hpHealing = item.hpHealing,
                bitPointsAdded = item.bitPointsAdded,
                damageReduction = item.damageReduction
            });
        }
        Debug.Log($"Added {item.itemName} to inventory!");
    }

    public bool UseItem(string itemName)
    {
        var item = db.Table<InventoryItem>().Where(i => i.itemName == itemName).FirstOrDefault();
        if (item == null || item.quantity <= 0) return false;

        item.quantity -= 1;
        if (item.quantity <= 0)
            db.Delete(item);
        else
            db.Update(item);

        return true;
    }
}