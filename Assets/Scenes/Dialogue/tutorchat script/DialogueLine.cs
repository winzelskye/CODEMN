using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string characterName;
    [TextArea(2, 5)]
    public string dialogueText;
    public Color textColor = Color.white;
    public bool isPlayer = false;
    public AudioClip messageSound;

    [Header("Custom Timing (Optional)")]
    [Tooltip("Custom delay after this message. Leave at 0 to use automatic delays.")]
    public float delayAfter = 0f;

    [Header("Message Continuation")]
    [Tooltip("Check this to append to the previous message without showing the character name again")]
    public bool isContinuation = false;
}

[Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)]
    public string choiceText;
    public DialogueNode nextNode;
    public AudioClip choiceSound;

    [Header("Switch Conversation (Optional)")]
    [Tooltip("Switch to a different character's conversation")]
    public string switchToCharacter;
}

[CreateAssetMenu(fileName = "New Dialogue Node", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public DialogueLine[] npcLines;
    public DialogueChoice[] playerChoices;
    public DialogueNode nextNode;
}