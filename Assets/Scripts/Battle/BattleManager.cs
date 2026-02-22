using UnityEngine;
using System.Collections;

public enum BattleState { Start, PreBattle, EnemyDialogue, PlayerTurn, EnemyTurn, Won, Lost }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public BattleState state;
    public int currentLevelId = 1;
    public PlayerBattle player;
    public EnemyBattle enemy;
    public EnemyDialogue enemyDialogue;
    public SimpleTextController dialogueController;

    [Header("Game Over")]
    public GameOverManager gameOverManager;

    [Header("Glitch Effect")]
    public GlitchEffect glitchEffect;

    [Header("Battle Start Button")]
    public GameObject battleStartButton;

    private bool defenseUpActive = false;
    private bool damageUpActive = false;
    private int preBattleLineIndex = 0;

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

        if (battleStartButton != null)
            battleStartButton.SetActive(false);

        Invoke(nameof(StartPreBattleDialogue), 1f);
    }

    void StartPreBattleDialogue()
    {
        state = BattleState.PreBattle;
        preBattleLineIndex = 0;

        if (enemyDialogue != null && enemyDialogue.preBattleLines.Count > 0)
            ShowNextPreBattleLine();
        else
            StartCoroutine(EnemyTurnDialogue());
    }

    void ShowNextPreBattleLine()
    {
        if (enemyDialogue == null || preBattleLineIndex >= enemyDialogue.preBattleLines.Count)
        {
            if (battleStartButton != null)
                battleStartButton.SetActive(true);
            return;
        }

        string line = enemyDialogue.preBattleLines[preBattleLineIndex].line;
        preBattleLineIndex++;
        ShowDialogue(line);

        StartCoroutine(WaitForKeyThenDo(ShowNextPreBattleLine));
    }

    public void OnBattleStartButtonPressed()
    {
        if (battleStartButton != null)
            battleStartButton.SetActive(false);

        StartCoroutine(EnemyTurnDialogue());
    }

    IEnumerator EnemyTurnDialogue()
    {
        state = BattleState.EnemyDialogue;

        if (enemyDialogue != null)
        {
            string line = enemyDialogue.GetNextTurnLine();
            if (!string.IsNullOrEmpty(line))
            {
                ShowDialogue(line);
                yield return StartCoroutine(WaitForKeyPress());
            }
        }

        PlayerTurn();
    }

    IEnumerator WaitForKeyPress()
    {
        yield return null;
        while (!Input.anyKeyDown)
            yield return null;
    }

    IEnumerator WaitForKeyThenDo(System.Action callback)
    {
        yield return null;
        while (!Input.anyKeyDown)
            yield return null;
        callback?.Invoke();
    }

    public void PlayerTurn()
    {
        state = BattleState.PlayerTurn;
        ShowDialogue("Your turn!");
    }

    public void OnPlayerAttackResult(bool success, bool isSpecial)
    {
        if (success)
        {
            int damage = isSpecial ? player.specialAttack.damage : player.currentAttack.damage;
            string attackName = isSpecial ? player.specialAttack.attackName : player.currentAttack.attackName;

            if (damageUpActive && damage > 0)
            {
                damage = Mathf.RoundToInt(damage * 1.5f);
                damageUpActive = false;
            }

            if (damage < 0)
            {
                player.TakeDamage(damage);
                ShowDialogue($"You used {attackName} and recovered {Mathf.Abs(damage)} HP!");
            }
            else if (attackName == "Border Attack")
            {
                defenseUpActive = true;
                ShowDialogue("You raised your defense! Incoming damage will be halved!");
            }
            else if (attackName == "Extra Damage")
            {
                damageUpActive = true;
                ShowDialogue("You powered up! Next attack will deal 1.5x damage!");
            }
            else
            {
                enemy.TakeDamage(damage);
                string hitLine = enemyDialogue != null ? enemyDialogue.GetNextHitLine() : "";
                ShowDialogue(!string.IsNullOrEmpty(hitLine) ? hitLine : $"You dealt {damage} damage!");
            }

            if (player.currentAttack != null && player.currentAttack.isSkill == 0)
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
            ShowDialogue("Time's up! The enemy takes their turn.");
        }

        state = BattleState.EnemyTurn;
        Invoke(nameof(EnemyAttack), 2f);
    }

    void EnemyAttack()
    {
        int damage = enemy.GetRandomDamage();

        if (defenseUpActive)
        {
            int reducedDamage = Mathf.RoundToInt(damage * 0.5f);
            enemy.AttackWithDamage(player, reducedDamage);
            defenseUpActive = false;
            ShowDialogue($"The enemy attacked for {damage} but your defense reduced it to {reducedDamage}!");
        }
        else
        {
            enemy.AttackWithDamage(player, damage);
            ShowDialogue($"The enemy attacked you for {damage} damage!");
        }

        if (glitchEffect != null)
            glitchEffect.TriggerGlitch();

        if (player.currentHealth >= 75 && enemyDialogue != null)
        {
            string lowHealthLine = enemyDialogue.GetNextLowHealthLine();
            if (!string.IsNullOrEmpty(lowHealthLine))
                Invoke(nameof(ShowLowHealthLine), 1.5f);
        }

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

        Invoke(nameof(StartEnemyTurnDialogue), 2f);
    }

    void StartEnemyTurnDialogue()
    {
        StartCoroutine(EnemyTurnDialogue());
    }

    void ShowLowHealthLine()
    {
        if (enemyDialogue != null)
        {
            string line = enemyDialogue.GetNextLowHealthLine();
            if (!string.IsNullOrEmpty(line))
                ShowDialogue(line);
        }
    }

    void ShowDialogue(string message)
    {
        if (dialogueController != null)
        {
            dialogueController.ShowText();
            StopAllCoroutines();
            StartCoroutine(TypewriterDialogue(message));
        }
    }

    IEnumerator TypewriterDialogue(string message)
    {
        dialogueController.SetText("");
        foreach (char c in message)
        {
            dialogueController.SetText(dialogueController.GetCurrentText() + c);
            yield return new WaitForSeconds(0.05f);
        }
    }

    void WinBattle()
    {
        state = BattleState.Won;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, true);
        SaveLoadManager.Instance.CompleteLevel(currentLevelId);

        var enemyData = SaveLoadManager.Instance.GetEnemyForLevel(currentLevelId);
        if (enemyData != null)
        {
            SaveLoadManager.Instance.AddCurrency(enemyData.currencyReward);
            ShowDialogue($"You won! You earned {enemyData.currencyReward} currency!");
        }

        if (gameOverManager != null) gameOverManager.ShowWin();
    }

    void LoseBattle()
    {
        state = BattleState.Lost;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, false);
        ShowDialogue("You were defeated...");
        if (gameOverManager != null) gameOverManager.ShowLose();
    }
}