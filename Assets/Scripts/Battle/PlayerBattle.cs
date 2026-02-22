using UnityEngine;
using UnityEngine.UI;

public class PlayerBattle : MonoBehaviour
{
    public float currentHealth = 0f;
    public float bitpoints = 0f;
    public float bitpointRate = 10f;
    public bool specialReady = false;
    public AttackData currentAttack;
    public AttackData specialAttack;
    private string characterName;
    public Slider healthBar;
    public Slider bitpointBar;

    [Header("Character Sprites")]
    public GameObject estherSprite;
    public GameObject michaelSprite;

    public void Setup(CharacterStats stats, string charName)
    {
        characterName = charName;
        currentHealth = stats.health;
        bitpointRate = stats.bitpointRate;
        specialAttack = SaveLoadManager.Instance.GetSpecialAttack(characterName);

        if (estherSprite != null) estherSprite.SetActive(charName == "Esther");
        if (michaelSprite != null) michaelSprite.SetActive(charName == "Michael");

        if (healthBar != null) { healthBar.minValue = 0; healthBar.maxValue = 100; healthBar.value = currentHealth; }
        if (bitpointBar != null) { bitpointBar.minValue = 0; bitpointBar.maxValue = 100; bitpointBar.value = 0; }
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"TakeDamage called! Amount: {amount}, Before: {currentHealth}");
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        if (healthBar != null) healthBar.value = currentHealth;
        Debug.Log($"After: {currentHealth}, Bar value: {healthBar?.value}");
    }

    public void AddBitpoints(float amount)
    {
        bitpoints += amount;
        bitpoints = Mathf.Clamp(bitpoints, 0, 100);
        if (bitpointBar != null) bitpointBar.value = bitpoints;
        if (bitpoints >= 100) specialReady = true;
    }

    public void UseAttack(AttackData attack)
    {
        currentAttack = attack;
    }

    public void UseSpecial()
    {
        if (!specialReady) return;
        bitpoints = 0;
        specialReady = false;
        if (bitpointBar != null) bitpointBar.value = 0;
        BattleManager.Instance.OnPlayerAttackResult(true, true);
    }
}