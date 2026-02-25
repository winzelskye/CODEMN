using UnityEngine;
using SQLite;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance;
    public SQLiteConnection db;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDB();
        }
        else Destroy(gameObject);
    }

    public void InitDB()
    {
        if (db != null) return;
        string dbPath = Application.persistentDataPath + "/gamedata.db";
        db = new SQLiteConnection(dbPath);
        db.CreateTable<Player>();
        db.CreateTable<CharacterStats>();
        db.CreateTable<AttackData>();
        db.CreateTable<EnemyData>();
        db.CreateTable<LevelData>();
        db.CreateTable<BattleResult>();
        db.CreateTable<InventoryItem>();
        db.CreateTable<PlayerCurrency>();
        SeedData();
        Debug.Log("DB Connected at: " + dbPath);
    }

    void SeedData()
    {
        if (db.Table<LevelData>().Count() > 0) return;

        // Characters
        db.Insert(new CharacterStats { characterName = "Esther", health = 0, maxHealth = 100, attack = 10, defense = 8, speed = 5, bitpointRate = 10 });
        db.Insert(new CharacterStats { characterName = "Michael", health = 0, maxHealth = 100, attack = 15, defense = 10, speed = 7, bitpointRate = 8 });

        // Normal attacks
        db.Insert(new AttackData { attackName = "Classics", damage = 8, unlockLevel = 1, isUnlocked = 1, isSpecial = 0, isSkill = 0, forCharacter = "both", bitpointCost = 0 });
        db.Insert(new AttackData { attackName = "Styalize", damage = 12, unlockLevel = 2, isUnlocked = 0, isSpecial = 0, isSkill = 0, forCharacter = "both", bitpointCost = 3 });
        db.Insert(new AttackData { attackName = "Pictionary", damage = 18, unlockLevel = 3, isUnlocked = 0, isSpecial = 0, isSkill = 0, forCharacter = "both", bitpointCost = 5 });
        db.Insert(new AttackData { attackName = "HyperLink", damage = 20, unlockLevel = 4, isUnlocked = 0, isSpecial = 0, isSkill = 0, forCharacter = "both", bitpointCost = 8 });

        // Special attacks — unlock after completing Level 2 (A.I.)
        db.Insert(new AttackData { attackName = "Debug EX", damage = 35, unlockLevel = 4, isUnlocked = 0, isSpecial = 1, isSkill = 1, forCharacter = "Michael", bitpointCost = 50 });
        db.Insert(new AttackData { attackName = "Protection EX", damage = -40, unlockLevel = 4, isUnlocked = 0, isSpecial = 1, isSkill = 1, forCharacter = "Esther", bitpointCost = 50 });

        // Skills
        db.Insert(new AttackData { attackName = "Debug", damage = 20, unlockLevel = 1, isUnlocked = 1, isSpecial = 0, isSkill = 1, forCharacter = "both", bitpointCost = 30 });
        db.Insert(new AttackData { attackName = "Protection", damage = -15, unlockLevel = 3, isUnlocked = 0, isSpecial = 0, isSkill = 1, forCharacter = "both", bitpointCost = 50 });

        // Levels
        db.Insert(new LevelData { id = 0, levelName = "Tutorial", isUnlocked = 1, isCompleted = 0 });
        db.Insert(new LevelData { id = 1, levelName = "Level 1", isUnlocked = 1, isCompleted = 0 });
        db.Insert(new LevelData { id = 2, levelName = "Level 2", isUnlocked = 0, isCompleted = 0 });
        db.Insert(new LevelData { id = 3, levelName = "Level 3", isUnlocked = 0, isCompleted = 0 });
        db.Insert(new LevelData { id = 4, levelName = "Level 4", isUnlocked = 0, isCompleted = 0 });

        db.Insert(new EnemyData { levelId = 0, enemyName = "Handler", attackDamage = 5, maxAttackDamage = 10, defense = 5, currencyReward = 0 });
        db.Insert(new EnemyData { levelId = 1, enemyName = "Dee Bug", attackDamage = 8, maxAttackDamage = 15, defense = 5, currencyReward = 15 });
        db.Insert(new EnemyData { levelId = 2, enemyName = "Lady Bug", attackDamage = 12, maxAttackDamage = 20, defense = 8, currencyReward = 20 });
        db.Insert(new EnemyData { levelId = 3, enemyName = "A.I.", attackDamage = 15, maxAttackDamage = 25, defense = 10, currencyReward = 35 });
        db.Insert(new EnemyData { levelId = 4, enemyName = "Hacker", attackDamage = 20, maxAttackDamage = 35, defense = 10, currencyReward = 50 });

        // Starting currency
        db.Insert(new PlayerCurrency { id = 1, amount = 0 });
    }
}