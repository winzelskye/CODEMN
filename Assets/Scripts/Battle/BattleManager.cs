using UnityEngine;

public enum BattleState { Start, PlayerTurn, EnemyTurn, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    public BattleState state;
    public int currentLevelId = 1;

    public PlayerBattle player;
    public EnemyBattle enemy;
    public SimpleTextController dialogueController; // ← changed this

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
        enemy.Setup(enemyData);

        var playerData = SaveLoadManager.Instance.LoadPlayer();
        var stats = SaveLoadManager.Instance.LoadCharacterStats(playerData.selectedCharacter);
        player.Setup(stats, playerData.selectedCharacter);

        // Set condition and start dialogue
        dialogueController.SetCondition("TurnStart", true);
        dialogueController.StartMessages();

        Invoke(nameof(PlayerTurn), 3f);
    }

    public void PlayerTurn()
    {
        state = BattleState.PlayerTurn;
        dialogueController.SetCondition("TurnStart", true);
    }

    public void OnPlayerAttackResult(bool success, bool isSpecial)
    {
        if (success)
        {
            int damage = isSpecial ? player.specialAttack.damage : player.currentAttack.damage;
            enemy.TakeDamage(damage);

            dialogueController.ClearAllConditions();
            dialogueController.SetCondition("Hit", true);
        }
        else
        {
            dialogueController.ClearAllConditions();
        }

        if (enemy.currentHealth >= 100)
            WinBattle();
        else
        {
            state = BattleState.EnemyTurn;
            Invoke(nameof(EnemyTurn), 2f);
        }
    }

    void EnemyTurn()
    {
        enemy.Attack(player);

        // Check low health
        if (player.currentHealth >= 75)
            dialogueController.SetCondition("LowHealth", true);

        if (player.currentHealth >= 100)
            LoseBattle();
        else
            Invoke(nameof(PlayerTurn), 1.5f);
    }

    void WinBattle()
    {
        state = BattleState.Won;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, true);
        SaveLoadManager.Instance.CompleteLevel(currentLevelId);
        Debug.Log("You Win!");
    }

    void LoseBattle()
    {
        state = BattleState.Lost;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, false);
        Debug.Log("You Lose!");
    }
}