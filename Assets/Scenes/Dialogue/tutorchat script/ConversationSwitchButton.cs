using UnityEngine;
using UnityEngine.UI;

public class ConversationSwitchButton : MonoBehaviour
{
    [Header("Character Settings")]
    public string characterName; // e.g., "Handler", "Guard"
    public DialogueNode characterDialogueNode; // The dialogue node to start

    [Header("References")]
    public DialogueManager dialogueManager;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError("Character Name is not set!");
            return;
        }

        if (characterDialogueNode == null)
        {
            Debug.LogError("Character Dialogue Node is not assigned!");
            return;
        }

        // Make sure the node has the character name set
        characterDialogueNode.characterName = characterName;

        // Start the conversation
        dialogueManager.TriggerConversation(characterName, characterDialogueNode);
    }
}