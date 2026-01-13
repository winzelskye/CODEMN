using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSceneController : MonoBehaviour
{
    [Header("Battle Data")]
    [SerializeField] private TextAsset battleJsonFile;
    [SerializeField] private int battleIndexToLoad = 0; // Which battle to load

    [Header("Scene References")]
    [SerializeField] private SpriteRenderer enemySpriteRenderer;
    [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
    [SerializeField] private Animator enemyAnimator;

    [Header("UI References (Optional)")]
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI battleNameText;

    private BattleCollection battleCollection;
    private BattleData currentBattle;

    private void Start()
    {
        LoadBattlesFromJSON();
        LoadBattle(battleIndexToLoad);
    }

    // Load all battles from JSON
    public void LoadBattlesFromJSON()
    {
        if (battleJsonFile != null)
        {
            battleCollection = JsonUtility.FromJson<BattleCollection>(battleJsonFile.text);
            Debug.Log($"✅ Loaded {battleCollection.battles.Count} battles from JSON");
        }
        else
        {
            Debug.LogError("❌ No battle JSON file assigned!");
        }
    }

    // Load specific battle by index
    public void LoadBattle(int battleIndex)
    {
        if (battleCollection == null || battleCollection.battles.Count == 0)
        {
            Debug.LogError("❌ No battles loaded!");
            return;
        }

        if (battleIndex < 0 || battleIndex >= battleCollection.battles.Count)
        {
            Debug.LogError($"❌ Battle index {battleIndex} out of range!");
            return;
        }

        currentBattle = battleCollection.battles[battleIndex];
        Debug.Log($"🎮 Loading Battle: {currentBattle.battleName}");

        ApplyBattleVisuals();
    }

    // Apply all visual settings from battle data
    private void ApplyBattleVisuals()
    {
        if (currentBattle == null) return;

        // Set enemy sprite
        if (enemySpriteRenderer != null && currentBattle.enemy.enemySprite != null)
        {
            enemySpriteRenderer.sprite = currentBattle.enemy.enemySprite;
            enemySpriteRenderer.color = currentBattle.enemy.enemyColor;
            enemySpriteRenderer.transform.localScale = currentBattle.enemy.scale;
            enemySpriteRenderer.transform.position = currentBattle.enemy.battlePosition;
            Debug.Log($"✅ Enemy sprite set: {currentBattle.enemy.enemyName}");
        }

        // Set background (if you have a background sprite in BattleData)
        if (backgroundSpriteRenderer != null)
        {
            // You can add background sprite to BattleData or load by scene name
            Debug.Log($"✅ Background scene: {currentBattle.backgroundScene}");
        }

        // Setup animator
        if (enemyAnimator != null && !string.IsNullOrEmpty(currentBattle.enemy.animatorController))
        {
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(currentBattle.enemy.animatorController);
            if (controller != null)
            {
                enemyAnimator.runtimeAnimatorController = controller;
                enemyAnimator.Play("Idle");
                Debug.Log($"✅ Animator loaded: {currentBattle.enemy.animatorController}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Animator not found: {currentBattle.enemy.animatorController}");
            }
        }

        // Set UI text
        if (enemyNameText != null)
        {
            enemyNameText.text = currentBattle.enemy.enemyName;
        }

        if (battleNameText != null)
        {
            battleNameText.text = currentBattle.battleName;
        }

        Debug.Log($"✅ Battle '{currentBattle.battleName}' loaded successfully!");
    }

    // Public method to change battle (call from button)
    public void ChangeBattle(int newBattleIndex)
    {
        battleIndexToLoad = newBattleIndex;
        LoadBattle(newBattleIndex);
    }

    // Get total battle count
    public int GetBattleCount()
    {
        return battleCollection?.battles.Count ?? 0;
    }

    // Get current battle
    public BattleData GetCurrentBattle()
    {
        return currentBattle;
    }
}