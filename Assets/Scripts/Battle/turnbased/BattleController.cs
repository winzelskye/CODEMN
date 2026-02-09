using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    public enum GameState
    {
        TURN_PLAYER,
        TURN_PLAYER_MINIGAME,
        TURN_ENEMY,
        WIN,
        LOSS
    }

    [Header("HUD")]
    public UnitHud PlayerHud;
    public UnitHud EnemyHud;
    public BattleHud BattleHud;

    [Header("Spawns & Prefabs")]
    public Transform playerSummon, enemySummon;
    public GameObject playerPrefab;
    public GameObject[] enemyPrefab;

    [Header("Minigame")]
    public MinigameController minigameController;
    [Tooltip("Optional: parent of FIGHT/SKILLS/ITEMS buttons to disable during minigame.")]
    public GameObject actionButtonsContainer;

    [Header("Minigame config per action")]
    [SerializeField] private MinigameConfig attackMinigameConfig = new MinigameConfig { duration = 3f, keyToPress = KeyCode.Space, promptText = "Press SPACE!" };
    [SerializeField] private MinigameConfig skillMinigameConfig = new MinigameConfig { duration = 3f, keyToPress = KeyCode.Space, promptText = "Press SPACE!" };
    [SerializeField] private MinigameConfig itemMinigameConfig = new MinigameConfig { duration = 3f, keyToPress = KeyCode.Space, promptText = "Press SPACE!" };

    [Header("Optional: hook SFX/VFX")]
    public System.Action<int> OnEnemyTookDamage;
    public System.Action<int> OnPlayerTookDamage;

    private GameObject player, enemy;
    private UnitController playerController, enemyController;
    private GameState state;
    private ActionType selectedAction;

    private void Start()
    {
        player = Instantiate(playerPrefab, playerSummon.position, Quaternion.identity);
        playerController = player.GetComponent<UnitController>();
        SummonMonster();
        state = GameState.TURN_PLAYER;
        StartCoroutine(PlayerHud.StartHud(PlayerHud, playerController));
        StartCoroutine(EnemyHud.StartHud(EnemyHud, enemyController));
        playerController.SetBattleHud(BattleHud);
        enemyController.SetBattleHud(BattleHud);
        BattleHud.ChooseText();

        if (minigameController != null)
            minigameController.OnMinigameCompleted += HandleMinigameCompleted;
    }

    private void OnDestroy()
    {
        if (minigameController != null)
            minigameController.OnMinigameCompleted -= HandleMinigameCompleted;
    }

    /// <summary>Called when player chooses FIGHT. Starts minigame; result applied in HandleMinigameCompleted.</summary>
    public void OnActionSelectedAttack()
    {
        OnActionSelected(ActionType.Attack);
    }

    /// <summary>Called when player chooses SKILLS. Starts minigame; result applied in HandleMinigameCompleted.</summary>
    public void OnActionSelectedSkill()
    {
        OnActionSelected(ActionType.Skill);
    }

    /// <summary>Called when player chooses ITEMS. Starts minigame; result applied in HandleMinigameCompleted.</summary>
    public void OnActionSelectedItem()
    {
        OnActionSelected(ActionType.Item);
    }

    private void OnActionSelected(ActionType action)
    {
        if (state != GameState.TURN_PLAYER) return;

        selectedAction = action;

        if (action == ActionType.Skill && !playerController.EnoughManaForSpell("heal"))
        {
            BattleHud.ManaText(playerController.unitScriptableObject.healMana);
            return;
        }

        state = GameState.TURN_PLAYER_MINIGAME;
        SetActionButtonsEnabled(false);

        MinigameConfig config = GetConfigForAction(action);
        if (minigameController != null)
            minigameController.StartMinigame(config);
        else
            HandleMinigameCompleted(true);
    }

    private MinigameConfig GetConfigForAction(ActionType action)
    {
        switch (action)
        {
            case ActionType.Attack: return attackMinigameConfig;
            case ActionType.Skill: return skillMinigameConfig;
            case ActionType.Item: return itemMinigameConfig;
            default: return attackMinigameConfig;
        }
    }

    private void HandleMinigameCompleted(bool success)
    {
        state = GameState.TURN_ENEMY;
        SetActionButtonsEnabled(true);

        if (minigameController != null)
            minigameController.StopMinigame();

        if (success)
        {
            switch (selectedAction)
            {
                case ActionType.Attack:
                    BattleHud.UsedText(playerController.unitScriptableObject.name, "Attack");
                    playerController.AttackTurn(enemyController, () => OnPlayerActionResolved());
                    break;
                case ActionType.Skill:
                    BattleHud.UsedText(playerController.unitScriptableObject.name, "Heal");
                    playerController.HealTurn(() => OnPlayerActionResolved());
                    break;
                case ActionType.Item:
                    BattleHud.UsedText(playerController.unitScriptableObject.name, "Item");
                    ApplyItemEffectSuccess();
                    break;
                default:
                    OnPlayerActionResolved();
                    break;
            }
        }
        else
        {
            ApplyCounterattackDamage();
        }
    }

    private void ApplyItemEffectSuccess()
    {
        // Placeholder: no item effect by default; just go to resolve (enemy turn).
        OnPlayerActionResolved();
    }

    private void ApplyCounterattackDamage()
    {
        int damage = Mathf.Max(0, enemyController.currentDamage - playerController.currentDefense);
        BattleHud.DamageText(playerController.unitScriptableObject.name, damage);
        playerController.currentHealth -= damage;
        playerController.currentHealth = Mathf.Max(0, playerController.currentHealth);
        PlayerHud.UpdateHud(playerController);
        OnPlayerTookDamage?.Invoke(damage);
        CheckBattleEndAndTransition();
    }

    private void OnPlayerActionResolved()
    {
        EnemyHud.UpdateHud(enemyController);
        CheckBattleEndAndTransition();
    }

    private void CheckBattleEndAndTransition()
    {
        if (enemyController.currentHealth <= 0)
        {
            state = GameState.WIN;
            EndBattle();
            return;
        }
        if (playerController.currentHealth <= 0)
        {
            state = GameState.LOSS;
            EndBattle();
            return;
        }
        state = GameState.TURN_ENEMY;
        StartCoroutine(TurnEnemy());
    }

    private void TurnPlayer()
    {
        if (enemyController.currentHealth <= 0)
        {
            state = GameState.WIN;
            EndBattle();
            return;
        }
        state = GameState.TURN_ENEMY;
        StartCoroutine(TurnEnemy());
    }

    private IEnumerator TurnEnemy()
    {
        Debug.Log("ENEMY TURN");
        yield return new WaitForSeconds(0.1f);
        enemyController.MakeTurn(playerController, () =>
        {
            state = GameState.TURN_PLAYER;
            BattleHud.ChooseText();
            SetActionButtonsEnabled(true);
            if (playerController.currentHealth <= 0)
            {
                state = GameState.LOSS;
                EndBattle();
            }
        });
    }

    private void SetActionButtonsEnabled(bool enabled)
    {
        if (actionButtonsContainer != null)
            actionButtonsContainer.SetActive(enabled);
    }

    private void EndBattle()
    {
        BattleFlowController battleFlow = FindObjectOfType<BattleFlowController>();
        
        if (state == GameState.WIN)
        {
            enemyController.Delete();
            BattleHud.EndText(true);
            Debug.Log("YOU WON!");
            
            // Notify BattleFlowController of victory
            if (battleFlow != null)
            {
                battleFlow.OnBattleWon(50, ""); // Default rewards, can be configured
            }
        }
        else if (state == GameState.LOSS)
        {
            playerController.Delete();
            BattleHud.EndText(false);
            Debug.Log("YOU LOST!");
            
            // Notify BattleFlowController of defeat
            if (battleFlow != null)
            {
                battleFlow.OnBattleLost();
            }
        }
        SetActionButtonsEnabled(false);
    }

    private void SummonMonster()
    {
        enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], enemySummon.position, Quaternion.identity);
        enemyController = enemy.GetComponent<UnitController>();
    }

    /// <summary>Legacy: direct attack without minigame. Prefer OnActionSelectedAttack + minigame.</summary>
    public void ButtonAttack()
    {
        OnActionSelectedAttack();
    }

    /// <summary>Legacy: direct heal without minigame. Prefer OnActionSelectedSkill + minigame.</summary>
    public void ButtonHeal()
    {
        OnActionSelectedSkill();
    }

    public void ButtonRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Call from ITEMS button. Starts item minigame; on success applies item effect, on fail player takes damage.</summary>
    public void ButtonItems()
    {
        OnActionSelectedItem();
    }

    public void ButtonExit()
    {
        Application.Quit();
    }
}
