using UnityEngine;

public class ConversationSwitcher : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Handler")]
    public DialogueNode handlerNode;
    private bool handlerStarted = false;

    [Header("Hacker")]
    public DialogueNode hackerNode;
    private bool hackerStarted = false;

    [Header("Guard")]
    public DialogueNode guardNode;
    private bool guardStarted = false;

    void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
    }

    public void SwitchToHandler()
    {
        if (handlerNode != null)
        {
            handlerNode.characterName = "Handler";

            if (!handlerStarted)
            {
                dialogueManager.TriggerConversation("Handler", handlerNode);
                handlerStarted = true;
            }
            else
            {
                dialogueManager.SwitchToCharacter("Handler");
            }
        }
        else
        {
            Debug.LogError("Handler node is NULL!");
        }
    }

    public void SwitchToHacker()
    {
        if (hackerNode != null)
        {
            hackerNode.characterName = "Hacker";

            if (!hackerStarted)
            {
                dialogueManager.TriggerConversation("Hacker", hackerNode);
                hackerStarted = true;
            }
            else
            {
                dialogueManager.SwitchToCharacter("Hacker");
            }
        }
        else
        {
            Debug.LogError("Hacker node is NULL!");
        }
    }

    public void SwitchToGuard()
    {
        if (guardNode != null)
        {
            guardNode.characterName = "Guard";

            if (!guardStarted)
            {
                dialogueManager.TriggerConversation("Guard", guardNode);
                guardStarted = true;
            }
            else
            {
                dialogueManager.SwitchToCharacter("Guard");
            }
        }
        else
        {
            Debug.LogError("Guard node is NULL!");
        }
    }

    public void ResetAllConversations()
    {
        handlerStarted = false;
        hackerStarted = false;
        guardStarted = false;
        dialogueManager.ClearDialogue();
    }
}