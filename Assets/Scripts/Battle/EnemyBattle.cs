using UnityEngine;
using UnityEngine.UI;

public class EnemyBattle : MonoBehaviour
{
    public float currentHealth = 0f;
    private int minAttackDamage;
    private int maxAttackDamage;
    public Slider healthBar;

    public void Setup(EnemyData data)
    {
        minAttackDamage = data.attackDamage;
        maxAttackDamage = data.maxAttackDamage;
        currentHealth = 0f;
        if (healthBar != null) { healthBar.minValue = 0; healthBar.maxValue = 100; healthBar.value = 0; }
    }

    public void TakeDamage(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        if (healthBar != null) healthBar.value = currentHealth;
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
        Debug.Log($"Enemy attacked with reduction! Damage: {reducedDamage} (original: {damage})");
    }
}