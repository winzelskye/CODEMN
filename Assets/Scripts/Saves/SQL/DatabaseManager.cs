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
        SeedData();
        Debug.Log("DB Connected at: " + dbPath);
    }

    void SeedData()
    {
        if (db.Table<LevelData>().Count() > 0) return;

        // Characters
        db.Insert(new CharacterStats { characterName = "Esther", health = 0, maxHealth = 100, attack = 15, defense = 10, speed = 5, bitpointRate = 20 });
        db.Insert(new CharacterStats { characterName = "Michael", health = 0, maxHealth = 100, attack = 10, defense = 15, speed = 7, bitpointRate = 18 });

        // Normal attacks
        db.Insert(new AttackData { attackName = "Border Attack", damage = 10, unlockLevel = 1, isUnlocked = 1, isSpecial = 0, forCharacter = "both" });
        db.Insert(new AttackData { attackName = "Power Strike", damage = 20, unlockLevel = 2, isUnlocked = 0, isSpecial = 0, forCharacter = "both" });

        // Healing skill - negative damage = heal
        db.Insert(new AttackData { attackName = "Heal", damage = -20, unlockLevel = 1, isUnlocked = 1, isSpecial = 0, forCharacter = "both" });

        // Special attacks
        db.Insert(new AttackData { attackName = "Cascadia", damage = 60, unlockLevel = 1, isUnlocked = 1, isSpecial = 1, forCharacter = "Esther" });
        db.Insert(new AttackData { attackName = "Stack Overflow", damage = 60, unlockLevel = 1, isUnlocked = 1, isSpecial = 1, forCharacter = "Michael" });

        // Levels
        db.Insert(new LevelData { id = 1, levelName = "Level 1", isUnlocked = 1, isCompleted = 0 });
        db.Insert(new LevelData { id = 2, levelName = "Level 2", isUnlocked = 0, isCompleted = 0 });
        db.Insert(new LevelData { id = 3, levelName = "Level 3", isUnlocked = 0, isCompleted = 0 });

        // Enemies
        db.Insert(new EnemyData { levelId = 1, enemyName = "Enemy1", attackDamage = 10, defense = 5 });
        db.Insert(new EnemyData { levelId = 2, enemyName = "Enemy2", attackDamage = 15, defense = 8 });
        db.Insert(new EnemyData { levelId = 3, enemyName = "Enemy3", attackDamage = 20, defense = 10 });
    }
}