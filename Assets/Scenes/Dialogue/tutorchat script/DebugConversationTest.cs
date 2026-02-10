/*using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEngine.Rendering.DebugUI.Table;

public class DebugConversationTest : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public DialogueNode handlerNode;
    public DialogueNode hackerNode;

    void Update()
    {
        // Press H for Handler
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("===== PRESSING H - SWITCH TO HANDLER =====");
            TestSwitchToHandler();
        }

        // Press J for Hacker
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("===== PRESSING J - SWITCH TO HACKER =====");
            TestSwitchToHacker();
        }
    }

    void TestSwitchToHandler()
    {
        Debug.Log($"Handler Node: {(handlerNode != null ? handlerNode.name : "NULL")}");
        Debug.Log($"Handler Character Name: {(handlerNode != null ? handlerNode.characterName : "NULL")}");

        if (handlerNode != null && dialogueManager != null)
        {
            handlerNode.characterName = "Handler";
            dialogueManager.TriggerConversation("Handler", handlerNode);
        }
    }

    void TestSwitchToHacker()
    {
        Debug.Log($"Hacker Node: {(hackerNode != null ? hackerNode.name : "NULL")}");
        Debug.Log($"Hacker Character Name: {(hackerNode != null ? hackerNode.characterName : "NULL")}");

        if (hackerNode != null && dialogueManager != null)
        {
            hackerNode.characterName = "Hacker";
            dialogueManager.TriggerConversation("Hacker", hackerNode);
        }
    }
}*/