using UnityEngine;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public BattleState state;
    public int currentLevelId = 1;
    public PlayerBattle player;
    public EnemyBattle enemy;
    public SimpleTextController dialogueController;

    [Header("Game Over")]
    public GameOverManager gameOverManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartBattle();
    }

    void StartBattle()
    {
        state = BattleState.Start;

        var enemyData = SaveLoadManager.Instance.GetEnemyForLevel(currentLevelId);
        if (enemyData == null) { Debug.LogError($"No enemy found for level {currentLevelId}!"); return; }
        enemy.Setup(enemyData);

        var playerData = SaveLoadManager.Instance.LoadPlayer();
        if (playerData == null) { Debug.LogError("No player data found!"); return; }

        var stats = SaveLoadManager.Instance.LoadCharacterStats(playerData.selectedCharacter);
        if (stats == null) { Debug.LogError($"No stats found for {playerData.selectedCharacter}!"); return; }

        player.Setup(stats, playerData.selectedCharacter);

        if (dialogueController != null)
        {
            dialogueController.SetCondition("TurnStart", true);
            dialogueController.StartMessages();
        }

        Invoke(nameof(PlayerTurn), 3f);
    }

    public void PlayerTurn()
    {
        state = BattleState.PlayerTurn;
        if (dialogueController != null)
            dialogueController.SetCondition("TurnStart", true);
    }

    public void OnPlayerAttackResult(bool success, bool isSpecial)
    {
        if (success)
        {
            int damage = isSpecial ? player.specialAttack.damage : player.currentAttack.damage;

            if (damage < 0)
            {
                // Negative damage = healing
                player.TakeDamage(damage);
                Debug.Log($"Player healed! HP: {player.currentHealth}");
            }
            else
            {
                // Normal attack
                enemy.TakeDamage(damage);
                Debug.Log($"Player hit! Damage: {damage}, Bitpoints: {player.bitpoints}");
            }

            player.AddBitpoints(player.bitpointRate);

            if (dialogueController != null)
            {
                dialogueController.ClearAllConditions();
                dialogueController.SetCondition("Hit", true);
            }

            if (enemy.currentHealth >= 100)
            {
                WinBattle();
                return;
            }
        }
        else
        {
            Debug.Log("Player missed! Enemy turn.");
            if (dialogueController != null)
                dialogueController.ClearAllConditions();
        }

        state = BattleState.EnemyTurn;
        Invoke(nameof(EnemyTurn), 2f);
    }

    void EnemyTurn()
    {
        enemy.Attack(player);
        Debug.Log($"Enemy attacked! Player HP: {player.currentHealth}");

        if (dialogueController != null)
        {
            if (player.currentHealth >= 75)
                dialogueController.SetCondition("LowHealth", true);
        }

        if (player.currentHealth >= 100)
        {
            LoseBattle();
            return;
        }

        Invoke(nameof(PlayerTurn), 1.5f);
    }

    void WinBattle()
    {
        state = BattleState.Won;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, true);
        SaveLoadManager.Instance.CompleteLevel(currentLevelId);
        Debug.Log("=== YOU WIN! ===");
        if (gameOverManager != null) gameOverManager.ShowWin();
    }

    void LoseBattle()
    {
        state = BattleState.Lost;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, false);
        Debug.Log("=== YOU LOSE! ===");
        if (gameOverManager != null) gameOverManager.ShowLose();
    }
}