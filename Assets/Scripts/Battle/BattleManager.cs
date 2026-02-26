using UnityEngine;
using UnityEngine.UI;
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

    [Header("Player Action Buttons")]
    public Button fightButton;
    public Button skillsButton;
    public Button itemsButton;

    [Header("Dialogue Colors")]
    public Color systemTextColor = Color.white;

    [Header("Dialogue Prompt Image")]
    public GameObject dialoguePromptImage;

    private bool defenseUpActive = false;
    private int itemDamageReduction = 0;
    private int preBattleLineIndex = 0;
    private bool enemyTookDamageThisTurn = false;
    private Coroutine typewriterCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePromptImage != null)
            dialoguePromptImage.SetActive(false);

        StartBattle();
    }

    void Update()
    {
        if (state == BattleState.PreBattle && Input.GetKeyDown(KeyCode.Z))
        {
            preBattleLineIndex = enemyDialogue.preBattleLines.Count;
            StopAllCoroutines();
            typewriterCoroutine = null;
            if (dialoguePromptImage != null) dialoguePromptImage.SetActive(false);
            if (battleStartButton != null) battleStartButton.SetActive(true);
        }
    }

    void SetButtonsInteractable(bool interactable)
    {
        if (fightButton != null) fightButton.interactable = interactable;
        if (skillsButton != null) skillsButton.interactable = interactable;
        if (itemsButton != null) itemsButton.interactable = interactable;
    }

    void StartBattle()
    {
        state = BattleState.Start;

        int levelToLoad = enemy.selectedLevelId >= 0 ? enemy.selectedLevelId : currentLevelId;
        var enemyData = SaveLoadManager.Instance.GetEnemyForLevel(levelToLoad);
        if (enemyData == null) { Debug.LogError($"No enemy found for level {levelToLoad}!"); return; }
        enemy.Setup(enemyData);

        var playerData = SaveLoadManager.Instance.LoadPlayer();
        if (playerData == null) { Debug.LogError("No player data found!"); return; }

        var stats = SaveLoadManager.Instance.LoadCharacterStats(playerData.selectedCharacter);
        if (stats == null) { Debug.LogError($"No stats found for {playerData.selectedCharacter}!"); return; }

        // Unlock all attacks available for the player's current level
        SaveLoadManager.Instance.UnlockAttacksForLevel(playerData.currentLevel);
        Debug.Log($"Player current level: {playerData.currentLevel}");

        player.Setup(stats, playerData.selectedCharacter);

        if (battleStartButton != null)
            battleStartButton.SetActive(false);

        SetButtonsInteractable(false);
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

        string line = enemyDialogue.GetNextPreBattleLine(preBattleLineIndex);
        preBattleLineIndex++;
        ShowEnemyDialogue(line);

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
        SetButtonsInteractable(false);

        if (enemyDialogue != null)
        {
            string line = enemyDialogue.GetNextTurnLine(enemyTookDamageThisTurn);
            if (!string.IsNullOrEmpty(line))
            {
                ShowEnemyDialogue(line);
                yield return StartCoroutine(WaitForKeyPress());
            }
        }

        enemyTookDamageThisTurn = false;

        ShowDialogue("* Your turn!");
        yield return new WaitForSeconds(1f);

        SetButtonsInteractable(true);
        state = BattleState.PlayerTurn;
    }

    IEnumerator WaitForKeyPress()
    {
        while (typewriterCoroutine != null)
            yield return null;

        if (dialoguePromptImage != null)
            dialoguePromptImage.SetActive(true);

        yield return null;
        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
            yield return null;

        if (dialoguePromptImage != null)
            dialoguePromptImage.SetActive(false);
    }

    IEnumerator WaitForKeyThenDo(System.Action callback)
    {
        while (typewriterCoroutine != null)
            yield return null;

        if (dialoguePromptImage != null)
            dialoguePromptImage.SetActive(true);

        yield return null;
        while (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
            yield return null;

        if (dialoguePromptImage != null)
            dialoguePromptImage.SetActive(false);

        callback?.Invoke();
    }

    public void PlayerTurn()
    {
        state = BattleState.PlayerTurn;
        SetButtonsInteractable(true);
        ShowDialogue("* Your turn!");
    }

    public void OnPlayerAttackResult(bool success, bool isSpecial)
    {
        SetButtonsInteractable(false);
        StartCoroutine(PlayerAttackSequence(success, isSpecial));
    }

    IEnumerator PlayerAttackSequence(bool success, bool isSpecial)
    {
        if (success)
        {
            int damage = isSpecial ? player.specialAttack.damage : player.currentAttack.damage;
            string attackName = isSpecial ? player.specialAttack.attackName : player.currentAttack.attackName;

            if (attackName == "Debug" || attackName == "Debug EX")
            {
                enemy.TakeDamage(damage);
                enemyTookDamageThisTurn = true;
                player.TakeDamage(-12);
                string hitLine = enemyDialogue != null ? enemyDialogue.GetNextHitLine() : "";
                ShowDialogue(!string.IsNullOrEmpty(hitLine) ? hitLine : $"* {attackName}! Dealt {damage} damage and recovered 12 HP!");
                yield return new WaitForSeconds(2f);
            }
            else if (damage < 0)
            {
                player.TakeDamage(damage);

                if (attackName == "Protection" || attackName == "Protection EX")
                    defenseUpActive = true;

                ShowDialogue($"* You used {attackName}! Recovered {Mathf.Abs(damage)} HP and raised your defense!");
                yield return new WaitForSeconds(2f);
            }
            else
            {
                enemy.TakeDamage(damage);
                enemyTookDamageThisTurn = true;
                string hitLine = enemyDialogue != null ? enemyDialogue.GetNextHitLine() : "";
                ShowDialogue(!string.IsNullOrEmpty(hitLine) ? hitLine : $"* {attackName}! Dealt {damage} damage!");
                yield return new WaitForSeconds(2f);
            }

            // Only add bitpoints for normal attacks (not skills, not specials)
            if (player.currentAttack != null && player.currentAttack.isSkill == 0 && !isSpecial)
                player.AddBitpoints(player.bitpointRate);

            if (dialogueController != null)
            {
                dialogueController.ClearAllConditions();
                dialogueController.SetCondition("Hit", true);
            }

            if (enemy.currentHealth >= 100)
            {
                WinBattle();
                yield break;
            }
        }
        else
        {
            ShowDialogue("* Time's up! The enemy takes their turn.");
            enemyTookDamageThisTurn = false;
            yield return new WaitForSeconds(2f);
        }

        state = BattleState.EnemyTurn;
        EnemyAttack();
    }

    void EnemyAttack()
    {
        SetButtonsInteractable(false);
        StartCoroutine(EnemyAttackSequence());
    }

    IEnumerator EnemyAttackSequence()
    {
        int damage = enemy.GetRandomDamage();

        if (defenseUpActive)
        {
            int reducedDamage = Mathf.RoundToInt(damage * 0.5f);
            enemy.AttackWithDamage(player, reducedDamage);
            defenseUpActive = false;
            ShowDialogue($"* The enemy attacked for {damage} damage...");
            yield return new WaitForSeconds(2.5f);
            ShowDialogue($"* But your defense reduced it to {reducedDamage}!");
            yield return new WaitForSeconds(2.5f);
        }
        else if (itemDamageReduction > 0)
        {
            int reducedDamage = Mathf.Max(0, damage - itemDamageReduction);
            enemy.AttackWithDamage(player, reducedDamage);
            ShowDialogue($"* The enemy attacked for {damage} damage...");
            yield return new WaitForSeconds(2.5f);
            ShowDialogue($"* But your item reduced it to {reducedDamage}!");
            yield return new WaitForSeconds(2.5f);
            itemDamageReduction = 0;
        }
        else
        {
            enemy.AttackWithDamage(player, damage);
            ShowDialogue($"* The enemy attacked you for {damage} damage!");
            yield return new WaitForSeconds(2.5f);
        }

        if (glitchEffect != null)
            glitchEffect.TriggerGlitch();

        if (player.currentHealth >= 75 && enemyDialogue != null)
        {
            string lowHealthLine = enemyDialogue.GetNextLowHealthLine();
            if (!string.IsNullOrEmpty(lowHealthLine))
            {
                ShowEnemyDialogue(lowHealthLine);
                yield return StartCoroutine(WaitForKeyPress());
            }
        }

        if (player.currentHealth >= 100)
        {
            LoseBattle();
            yield break;
        }

        StartCoroutine(EnemyTurnDialogue());
    }

    public void ShowBattleDialogue(string message)
    {
        ShowDialogue(message);
    }

    public void ApplyDamageReduction(int amount)
    {
        itemDamageReduction = amount;
    }

    void ShowDialogue(string message)
    {
        if (dialogueController != null)
        {
            dialogueController.ShowText();
            dialogueController.SetTextColor(systemTextColor);
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterDialogue(message));
        }
    }

    void ShowEnemyDialogue(string message)
    {
        if (dialogueController != null)
        {
            dialogueController.ShowText();
            if (enemyDialogue != null)
                dialogueController.SetTextColor(enemyDialogue.enemyTextColor);
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterDialogue(message));
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
        typewriterCoroutine = null;
    }

    void WinBattle()
    {
        state = BattleState.Won;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, true);
        SaveLoadManager.Instance.CompleteLevel(currentLevelId);

        // Auto-increment player level on win
        var p = SaveLoadManager.Instance.LoadPlayer();
        if (p != null)
        {
            p.currentLevel += 1;
            SaveLoadManager.Instance.SavePlayer(p.playerName, p.selectedCharacter, p.currentLevel);
            Debug.Log($"Player leveled up to {p.currentLevel}!");
        }

        var enemyData = SaveLoadManager.Instance.GetEnemyForLevel(currentLevelId);
        if (enemyData != null)
        {
            SaveLoadManager.Instance.AddCurrency(enemyData.currencyReward);
            ShowDialogue($"* You won! You earned {enemyData.currencyReward} currency!");
        }

        Invoke(nameof(ShowWinScreen), 3f);
    }

    void ShowWinScreen()
    {
        if (gameOverManager != null) gameOverManager.ShowWin();
    }

    void LoseBattle()
    {
        state = BattleState.Lost;
        SaveLoadManager.Instance.SaveBattleResult(currentLevelId, false);
        ShowDialogue("* You were defeated...");
        Invoke(nameof(ShowLoseScreen), 3f);
    }

    void ShowLoseScreen()
    {
        if (gameOverManager != null) gameOverManager.ShowLose();
    }
}