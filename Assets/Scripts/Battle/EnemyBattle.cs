using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyBattle : MonoBehaviour
{
    public float currentHealth = 0f;
    private int minAttackDamage;
    private int maxAttackDamage;
    public Slider healthBar;
    public TextMeshProUGUI hpText;

    [Header("Enemy Selection")]
    [Tooltip("-1 = use BattleManager currentLevelId, 0 = Handler, 1 = Dee Bug, 2 = Lady Bug, 3 = A.I., 4 = Hacker")]
    public int selectedLevelId = -1;

    void Start()
    {
        if (selectedLevelId >= 0)
        {
            var data = SaveLoadManager.Instance.GetEnemyForLevel(selectedLevelId);
            if (data != null) Setup(data);
            else Debug.LogWarning($"EnemyBattle: No enemy found for level {selectedLevelId}");
        }
    }

    public void Setup(EnemyData data)
    {
        minAttackDamage = data.attackDamage;
        maxAttackDamage = data.maxAttackDamage;
        currentHealth = 0f;
        if (healthBar != null) { healthBar.minValue = 0; healthBar.maxValue = 100; healthBar.value = 0; }
        UpdateHPText();
    }

    public void TakeDamage(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        if (healthBar != null) healthBar.value = currentHealth;
        UpdateHPText();
    }

    void UpdateHPText()
    {
        if (hpText != null)
            hpText.text = $"{(int)currentHealth}/100";
    }

    public int GetRandomDamage()
    {
        return Random.Range(minAttackDamage, maxAttackDamage + 1);
    }

    public void Attack(PlayerBattle player)
    {
        int damage = GetRandomDamage();
        player.TakeDamage(damage);
    }

    public void AttackWithDamage(PlayerBattle player, int damage)
    {
        player.TakeDamage(damage);
    }

    public void AttackWithReduction(PlayerBattle player, float damageMultiplier)
    {
        int damage = GetRandomDamage();
        int reducedDamage = Mathf.RoundToInt(damage * damageMultiplier);
        player.TakeDamage(reducedDamage);
    }
}