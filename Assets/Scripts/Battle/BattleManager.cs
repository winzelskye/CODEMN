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
            // Deal damage to enemy
            int damage = isSpecial ? player.specialAttack.damage : player.currentAttack.damage;
            enemy.TakeDamage(damage);

            // Add bitpoints on success
            player.AddBitpoints(player.bitpointRate);

            Debug.Log($"Player hit! Damage: {damage}, Bitpoints: {player.bitpoints}");

            if (dialogueController != null)
            {
                dialogueController.ClearAllConditions();
                dialogueController.SetCondition("Hit", true);
            }

            // Check win condition
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

        // Enemy attacks after player turn
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
    }

    void LoseBattle()
    {
        state = BattleState.Lost;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, false);
        Debug.Log("=== YOU LOSE! ===");
    }
}