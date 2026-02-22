using SQLite;

[System.Serializable]
public class Player
{
    [PrimaryKey]
    public int id { get; set; }
    public string playerName { get; set; }
    public string selectedCharacter { get; set; }
    public int currentLevel { get; set; }
}

[System.Serializable]
public class CharacterStats
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string characterName { get; set; }
    public float health { get; set; }
    public float maxHealth { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }
    public float speed { get; set; }
    public float bitpointRate { get; set; }
}

[System.Serializable]
public class AttackData
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string attackName { get; set; }
    public int damage { get; set; }
    public int unlockLevel { get; set; }
    public int isUnlocked { get; set; }
    public int isSpecial { get; set; }
    public int isSkill { get; set; }
    public string forCharacter { get; set; }
    public int bitpointCost { get; set; }
}

[System.Serializable]
public class EnemyData
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public int levelId { get; set; }
    public string enemyName { get; set; }
    public int attackDamage { get; set; }
    public int maxAttackDamage { get; set; }
    public int defense { get; set; }
    public int currencyReward { get; set; }
}

[System.Serializable]
public class LevelData
{
    [PrimaryKey]
    public int id { get; set; }
    public string levelName { get; set; }
    public int isUnlocked { get; set; }
    public int isCompleted { get; set; }
}

[System.Serializable]
public class BattleResult
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public int levelId { get; set; }
    public int isCleared { get; set; }
    public int attempts { get; set; }
}

[System.Serializable]
public class InventoryItem
{
    [PrimaryKey, AutoIncrement]
    public int id { get; set; }
    public string itemName { get; set; }
    public int quantity { get; set; }
    public int hpHealing { get; set; }
    public int bitPointsAdded { get; set; }
    public int damageReduction { get; set; }
}

[System.Serializable]
public class PlayerCurrency
{
    [PrimaryKey]
    public int id { get; set; }
    public int amount { get; set; }
}