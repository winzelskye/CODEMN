using UnityEngine;
using System.Collections.Generic;

public class BattleDialogueController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueDisplay dialogueDisplay;

    [Header("Battle Data")]
    [SerializeField] private BattleData currentBattle; // Your battle data from JSON

    private int currentTurn = 0;
    private List<DialogueEntry> triggeredDialogues = new List<DialogueEntry>();

    void Start()
    {
        // Load your battle data here if needed
        // currentBattle = LoadBattleFromJSON();
    }

    // Call this when a new turn starts
    public void OnTurnStart(int turnNumber)
    {
        currentTurn = turnNumber;
        CheckDialogues("OnTurnStart");
    }

    // Call this when turn ends
    public void OnTurnEnd(int turnNumber)
    {
        currentTurn = turnNumber;
        CheckDialogues("OnTurnEnd");
    }

    // Call this when player gets hit
    public void OnPlayerHit()
    {
        CheckDialogues("OnHit");
    }

    // Call this when enemy health is low
    public void OnEnemyHealthChange(int currentHealth, int maxHealth)
    {
        float healthPercent = (float)currentHealth / maxHealth * 100f;

        foreach (var dialogue in currentBattle.dialogues)
        {
            if (dialogue.triggerCondition == "OnLowHealth" &&
                healthPercent <= dialogue.healthThreshold &&
                !triggeredDialogues.Contains(dialogue))
            {
                ShowDialogue(dialogue);
                triggeredDialogues.Add(dialogue);
            }
        }
    }

    private void CheckDialogues(string trigger)
    {
        if (currentBattle == null || currentBattle.dialogues == null) return;

        foreach (var dialogue in currentBattle.dialogues)
        {
            // Check if this dialogue should trigger
            bool shouldTrigger = false;

            if (dialogue.triggerCondition == trigger && dialogue.turnNumber == currentTurn)
            {
                shouldTrigger = true;
            }
            else if (dialogue.triggerCondition == "Always" && dialogue.turnNumber == currentTurn)
            {
                shouldTrigger = true;
            }

            // Show the dialogue if it should trigger
            if (shouldTrigger && !triggeredDialogues.Contains(dialogue))
            {
                ShowDialogue(dialogue);
                triggeredDialogues.Add(dialogue); // Mark as shown
            }
        }
    }

    private void ShowDialogue(DialogueEntry dialogue)
    {
        if (dialogueDisplay != null)
        {
            dialogueDisplay.ShowDialogue(dialogue);
        }
        else
        {
            Debug.LogError("DialogueDisplay is not assigned!");
        }
    }

    // FOR TESTING - Press D to test dialogue
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            TestDialogue();
        }
    }

    private void TestDialogue()
    {
        if (currentBattle != null && currentBattle.dialogues.Count > 0)
        {
            ShowDialogue(currentBattle.dialogues[0]);
            Debug.Log("Testing first dialogue!");
        }
        else
        {
            Debug.LogWarning("No dialogues to test!");
        }
    }
}