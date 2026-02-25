using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WordHighlight
{
    [Tooltip("The exact word to highlight (case-sensitive)")]
    public string word;
    [Tooltip("Color for this word")]
    public Color highlightColor = Color.yellow;
}

[System.Serializable]
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
    [Header("Word Highlighting (Optional)")]
    [Tooltip("Highlight specific words with different colors for emphasis")]
    public WordHighlight[] wordHighlights;
    [Header("Attached GameObject (Optional)")]
    [Tooltip("Show a GameObject/Prefab after this message appears")]
    public bool showAttachedObject = false;
    [Tooltip("The GameObject/Prefab to show (drag from Project folder)")]
    public GameObject attachedObjectPrefab;
    [Tooltip("Delay in seconds before showing the attached object")]
    public float attachedObjectDelay = 1.0f;
    [Tooltip("Destroy the object when moving to the next message")]
    public bool destroyOnNextMessage = true;
}

[System.Serializable]
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
    [SerializeField]
    public DialogueLine[] npcLines;
    [SerializeField]
    public DialogueChoice[] playerChoices;
    [SerializeField]
    public DialogueNode nextNode;

    [Header("On Node End (Optional)")]
    [Tooltip("Show the matching object from DialogueManager's Node End Overrides when this node finishes")]
    public bool showObjectOnEnd = false;

#if UNITY_EDITOR
    private void OnValidate() { }
    private void OnEnable()
    {
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}