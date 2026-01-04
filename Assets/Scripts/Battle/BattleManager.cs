using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BattleManager : MonoBehaviour
{
    [Header("Battle Data")]
    [SerializeField] private TextAsset battleJsonFile;
    [SerializeField] private DialogueDisplay dialogueDisplay;

    [Header("Enemy Display")]
    [SerializeField] private SpriteRenderer enemySpriteRenderer;
    [SerializeField] private Animator enemyAnimator;

    [Header("UI References (Optional)")]
    [SerializeField] private GameObject minigameContainer;

    private BattleCollection battleCollection;
    private BattleData currentBattle;
    private int currentTurn = 0;
    private int currentEnemyHealth;
    private int currentPlayerHealth;
    private BattlePhase currentPhase;

    // Visual effects
    private Vector3 originalEnemyPosition;
    private Color originalEnemyColor;

    // Minigame tracking
    private MinigameAttack currentMinigame;
    private float currentMinigameTimeRemaining;
    private bool isMinigameActive = false;
    private Dictionary<string, int> minigameUseCount = new Dictionary<string, int>();

    private void Start()
    {
        LoadBattles();
    }

    // ===================================================
    // BATTLE LOADING
    // ===================================================
    public void LoadBattles()
    {
        if (battleJsonFile != null)
        {
            battleCollection = JsonUtility.FromJson<BattleCollection>(battleJsonFile.text);
            Debug.Log($"Loaded {battleCollection.battles.Count} battles");
        }
        else
        {
            Debug.LogError("No battle JSON file assigned!");
        }
    }

    public BattleData GetBattle(int index)
    {
        if (battleCollection != null && index >= 0 && index < battleCollection.battles.Count)
        {
            return battleCollection.battles[index];
        }
        return null;
    }

    public BattleData GetBattleById(string id)
    {
        return battleCollection?.battles.Find(b => b.id == id);
    }

    // ===================================================
    // BATTLE FLOW
    // ===================================================
    public void StartBattle(int index)
    {
        currentBattle = GetBattle(index);

        if (currentBattle == null)
        {
            Debug.LogError($"Battle at index {index} not found!");
            return;
        }

        // Initialize battle state
        currentTurn = 0;
        currentEnemyHealth = currentBattle.enemy.maxHealth;
        currentPlayerHealth = currentBattle.basePlayerHealth;
        currentPhase = null;
        minigameUseCount.Clear();

        // Setup enemy visuals
        SetupEnemyVisuals();

        Debug.Log($"Starting battle: {currentBattle.battleName}");
        Debug.Log($"Enemy: {currentBattle.enemy.enemyName} (HP: {currentEnemyHealth})");
        Debug.Log($"Player HP: {currentPlayerHealth}");

        CheckDialogueTriggers("OnTurnStart");
    }

    private void SetupEnemyVisuals()
    {
        if (enemySpriteRenderer != null)
        {
            enemySpriteRenderer.sprite = currentBattle.enemy.enemySprite;
            enemySpriteRenderer.color = currentBattle.enemy.enemyColor;
            enemySpriteRenderer.transform.localScale = currentBattle.enemy.scale;
            enemySpriteRenderer.transform.position = currentBattle.enemy.battlePosition;

            originalEnemyPosition = enemySpriteRenderer.transform.position;
            originalEnemyColor = enemySpriteRenderer.color;
        }

        // Setup animator if provided
        if (enemyAnimator != null && !string.IsNullOrEmpty(currentBattle.enemy.animatorController))
        {
            // Load animator controller from Resources
            RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(currentBattle.enemy.animatorController);
            if (controller != null)
            {
                enemyAnimator.runtimeAnimatorController = controller;
                enemyAnimator.Play("Idle"); // Default to idle animation
            }
            else
            {
                Debug.LogWarning($"Animator controller not found: {currentBattle.enemy.animatorController}");
            }
        }
    }

    public void NextTurn()
    {
        currentTurn++;
        Debug.Log($"Turn {currentTurn}");

        CheckPhaseChange();
        CheckDialogueTriggers("OnTurnStart");
        StartRandomMinigame();
    }

    public void EndBattle(bool playerWon)
    {
        if (playerWon)
        {
            Debug.Log($"Victory! Gained {currentBattle.experienceReward} XP and {currentBattle.goldReward} Gold");

            // Play victory animation
            if (enemyAnimator != null)
            {
                enemyAnimator.Play("Death");
            }
        }
        else
        {
            Debug.Log("Defeated...");
        }
    }

    // ===================================================
    // MINIGAME SYSTEM
    // ===================================================
    public void StartRandomMinigame()
    {
        if (currentBattle.attacks.Count == 0)
        {
            Debug.LogWarning("No minigames configured for this battle!");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, currentBattle.attacks.Count);
        StartMinigame(currentBattle.attacks[randomIndex]);
    }

    public void StartMinigame(MinigameAttack minigame)
    {
        currentMinigame = minigame;
        isMinigameActive = true;

        // Play attack animation
        if (enemyAnimator != null)
        {
            enemyAnimator.Play("Attack");
        }

        if (!minigameUseCount.ContainsKey(minigame.attackName))
        {
            minigameUseCount[minigame.attackName] = 0;
        }
        minigameUseCount[minigame.attackName]++;

        float baseTime = minigame.timeLimit;
        float phaseReduction = currentPhase != null ? currentPhase.timeReduction : 0f;
        float useReduction = minigame.timeReduction * (minigameUseCount[minigame.attackName] - 1);
        float globalModifier = currentBattle.globalTimeModifier;

        currentMinigameTimeRemaining = (baseTime - phaseReduction - useReduction) * globalModifier;
        currentMinigameTimeRemaining = Mathf.Max(currentMinigameTimeRemaining, 5f);

        Debug.Log($"Starting minigame: {minigame.attackName} ({minigame.minigameType})");
        Debug.Log($"Time limit: {currentMinigameTimeRemaining}s | Damage on fail: {minigame.damageOnFail}");

        ActivateRandomDistractions();

        switch (minigame.minigameType)
        {
            case "Quiz":
                ShowQuizMinigame(minigame);
                break;
            case "DragDrop":
                ShowDragDropMinigame(minigame);
                break;
            case "CodeBlocks":
                ShowCodeBlocksMinigame(minigame);
                break;
            case "Custom":
                ShowCustomMinigame(minigame);
                break;
        }
    }

    private void Update()
    {
        if (isMinigameActive)
        {
            currentMinigameTimeRemaining -= Time.deltaTime;

            if (currentMinigameTimeRemaining <= 0)
            {
                MinigameFailed();
            }
        }
    }

    public void MinigameCompleted(bool success)
    {
        isMinigameActive = false;

        if (success)
        {
            Debug.Log("Minigame completed successfully!");
            CheckDialogueTriggers("OnHit");
            DamageEnemy(10);
        }
        else
        {
            MinigameFailed();
        }
    }

    private void MinigameFailed()
    {
        isMinigameActive = false;

        if (currentMinigame != null)
        {
            Debug.Log($"Minigame failed! Taking {currentMinigame.damageOnFail} damage");
            DamagePlayer(currentMinigame.damageOnFail);
        }
    }

    // ===================================================
    // MINIGAME UI DISPLAY (Placeholder)
    // ===================================================
    private void ShowQuizMinigame(MinigameAttack minigame)
    {
        Debug.Log("=== QUIZ MINIGAME ===");
        for (int i = 0; i < minigame.questions.Count; i++)
        {
            Debug.Log($"Q{i + 1}: {minigame.questions[i]}");
            Debug.Log($"Correct: {minigame.correctAnswers[i]}");
        }
    }

    private void ShowDragDropMinigame(MinigameAttack minigame)
    {
        Debug.Log($"=== DRAG & DROP MINIGAME ===");
        Debug.Log($"Theme: {minigame.dragDropTheme}");
        Debug.Log($"Items: {minigame.dragDropItemCount}");
    }

    private void ShowCodeBlocksMinigame(MinigameAttack minigame)
    {
        Debug.Log("=== CODE BLOCKS MINIGAME ===");
        Debug.Log("Arrange these code blocks in order:");
        foreach (var block in minigame.codeBlocks)
        {
            Debug.Log($"  - {block}");
        }
        Debug.Log($"Correct order: {minigame.correctOrder}");
    }

    private void ShowCustomMinigame(MinigameAttack minigame)
    {
        Debug.Log($"=== CUSTOM MINIGAME: {minigame.attackName} ===");
    }

    // ===================================================
    // DISTRACTION SYSTEM
    // ===================================================
    private void ActivateRandomDistractions()
    {
        foreach (var distraction in currentBattle.distractions)
        {
            if (!distraction.isEnabled) continue;

            float roll = UnityEngine.Random.Range(0f, 100f);
            if (roll <= distraction.activationChance)
            {
                StartDistraction(distraction);
            }
        }
    }

    private void StartDistraction(DistractionMechanic distraction)
    {
        Debug.Log($"Distraction activated: {distraction.mechanicName} ({distraction.mechanicType})");
        // TODO: Implement distraction effects in your UI
    }

    // ===================================================
    // HEALTH & DAMAGE WITH VISUAL EFFECTS
    // ===================================================
    public void DamagePlayer(int damage)
    {
        currentPlayerHealth -= damage;
        currentPlayerHealth = Mathf.Max(currentPlayerHealth, 0);

        Debug.Log($"Player took {damage} damage! HP: {currentPlayerHealth}/{currentBattle.basePlayerHealth}");

        if (currentPlayerHealth <= 0)
        {
            EndBattle(false);
        }
    }

    public void DamageEnemy(int damage)
    {
        currentEnemyHealth -= damage;
        currentEnemyHealth = Mathf.Max(currentEnemyHealth, 0);

        Debug.Log($"Enemy took {damage} damage! HP: {currentEnemyHealth}/{currentBattle.enemy.maxHealth}");

        // Play damage animation
        if (enemyAnimator != null)
        {
            enemyAnimator.Play("Hit");
        }

        // Apply damage visual effects
        ApplyDamageEffects();

        OnEnemyHealthChanged(currentEnemyHealth);
        CheckDialogueTriggers("OnHit");

        if (currentEnemyHealth <= 0)
        {
            EndBattle(true);
        }
    }

    private void ApplyDamageEffects()
    {
        if (enemySpriteRenderer == null) return;

        // Apply damage tint
        if (currentBattle.enemy.enableDamageTint)
        {
            StartCoroutine(DamageTintEffect());
        }

        // Apply damage shake
        if (currentBattle.enemy.enableDamageShake)
        {
            StartCoroutine(DamageShakeEffect());
        }
    }

    private IEnumerator DamageTintEffect()
    {
        Color originalColor = enemySpriteRenderer.color;
        enemySpriteRenderer.color = currentBattle.enemy.damageTintColor;

        yield return new WaitForSeconds(currentBattle.enemy.damageTintDuration);

        enemySpriteRenderer.color = originalColor;
    }

    private IEnumerator DamageShakeEffect()
    {
        float elapsed = 0f;
        float duration = currentBattle.enemy.damageShakeDuration;
        float intensity = currentBattle.enemy.damageShakeIntensity;

        while (elapsed < duration)
        {
            float x = originalEnemyPosition.x + UnityEngine.Random.Range(-intensity, intensity);
            float y = originalEnemyPosition.y + UnityEngine.Random.Range(-intensity, intensity);

            enemySpriteRenderer.transform.position = new Vector3(x, y, originalEnemyPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        enemySpriteRenderer.transform.position = originalEnemyPosition;
    }

    private void OnEnemyHealthChanged(int newHealth)
    {
        CheckPhaseChange();
        CheckDialogueTriggers("OnLowHealth");
    }

    // ===================================================
    // PHASE SYSTEM WITH SPRITE CHANGES
    // ===================================================
    private void CheckPhaseChange()
    {
        if (currentBattle.phases.Count == 0) return;

        int healthPercent = (currentEnemyHealth * 100) / currentBattle.enemy.maxHealth;

        foreach (var phase in currentBattle.phases)
        {
            if (healthPercent <= phase.healthThreshold && currentPhase != phase)
            {
                currentPhase = phase;
                Debug.Log($"Phase changed to: {phase.phaseName}");

                // Change sprite if specified
                if (phase.changeSprite && phase.phaseSprite != null && enemySpriteRenderer != null)
                {
                    enemySpriteRenderer.sprite = phase.phaseSprite;
                    enemySpriteRenderer.color = phase.phaseTintColor;
                    originalEnemyColor = phase.phaseTintColor;
                }

                // Change animator state if specified
                if (phase.changeAnimatorState && !string.IsNullOrEmpty(phase.animatorStateName) && enemyAnimator != null)
                {
                    enemyAnimator.Play(phase.animatorStateName);
                }

                if (!string.IsNullOrEmpty(phase.dialogueLine))
                {
                    var phaseDialogue = new DialogueEntry
                    {
                        dialogueText = phase.dialogueLine,
                        characterName = currentBattle.enemy.enemyName,
                        displayDuration = 3f
                    };
                    DisplayDialogue(phaseDialogue);
                }

                if (phase.changeMusic && phase.phaseMusic != null)
                {
                    // TODO: Change battle music
                }

                CheckDialogueTriggers("OnPhaseChange");
                break;
            }
        }
    }

    // ===================================================
    // DIALOGUE SYSTEM
    // ===================================================
    private void CheckDialogueTriggers(string triggerType)
    {
        if (currentBattle == null) return;

        foreach (var dialogue in currentBattle.dialogues)
        {
            bool shouldTrigger = false;

            if (dialogue.triggerCondition == triggerType)
            {
                if (triggerType == "OnTurnStart" && dialogue.turnNumber == currentTurn)
                {
                    shouldTrigger = true;
                }
                else if (triggerType == "OnLowHealth")
                {
                    int healthPercent = (currentEnemyHealth * 100) / currentBattle.enemy.maxHealth;
                    if (healthPercent <= dialogue.healthThreshold)
                    {
                        shouldTrigger = true;
                    }
                }
                else if (triggerType == "OnHit" || triggerType == "OnPhaseChange")
                {
                    shouldTrigger = true;
                }
            }
            else if (dialogue.triggerCondition == "Always")
            {
                shouldTrigger = true;
            }

            if (shouldTrigger)
            {
                DisplayDialogue(dialogue);
                break;
            }
        }
    }

    private void DisplayDialogue(DialogueEntry dialogue)
    {
        if (dialogueDisplay != null)
        {
            dialogueDisplay.ShowDialogue(dialogue);
        }
        else
        {
            Debug.Log($"[{dialogue.characterName}]: {dialogue.dialogueText}");
        }
    }

    // ===================================================
    // GETTERS
    // ===================================================
    public BattleData GetCurrentBattle() => currentBattle;
    public int GetCurrentTurn() => currentTurn;
    public int GetPlayerHealth() => currentPlayerHealth;
    public int GetEnemyHealth() => currentEnemyHealth;
    public float GetMinigameTimeRemaining() => currentMinigameTimeRemaining;
    public bool IsMinigameActive() => isMinigameActive;
    public MinigameAttack GetCurrentMinigame() => currentMinigame;
}