using UnityEngine;
using UnityEngine.UI;

public class EnemyBattle : MonoBehaviour
{
    public float currentHealth = 0f;
    private int attackDamage;

    public Slider healthBar;

    public void Setup(EnemyData data)
    {
        attackDamage = data.attackDamage;
        currentHealth = 0f;
        if (healthBar != null) { healthBar.minValue = 0; healthBar.maxValue = 100; healthBar.value = 0; }
    }

    public void TakeDamage(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        if (healthBar != null) healthBar.value = currentHealth;
    }

    public void Attack(PlayerBattle player)
    {
        player.TakeDamage(attackDamage);
    }
}