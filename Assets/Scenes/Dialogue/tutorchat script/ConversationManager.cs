using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterConversation
{
    public string characterName;
    public DialogueNode startingDialogueNode;
    public Color characterColor = Color.white;
    [HideInInspector]
    public bool hasStarted = false;
    [HideInInspector]
    public List<GameObject> messageObjects = new List<GameObject>();
}

public class ConversationManager : MonoBehaviour
{
    public DialogueManager dialogueManager;

    [Header("Add/Remove Characters Here")]
    public List<CharacterConversation> characters = new List<CharacterConversation>();

    private string currentCharacter = "";

    void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
    }

    public void SwitchToCharacter(string characterName)
    {
        CharacterConversation character = characters.Find(c => c.characterName == characterName);

        if (character == null)
        {
            Debug.LogError($"Character '{characterName}' not found in ConversationManager!");
            return;
        }

        if (character.startingDialogueNode == null)
        {
            Debug.LogError($"No dialogue node assigned for '{characterName}'!");
            return;
        }

        Debug.Log($"Switching to: {characterName}, Current: {currentCharacter}");

        // Hide current character's messages
        if (!string.IsNullOrEmpty(currentCharacter) && currentCharacter != characterName)
        {
            HideCharacterMessages(currentCharacter);
        }

        character.startingDialogueNode.characterName = characterName;
        currentCharacter = characterName;

        if (!character.hasStarted)
        {
            // First time - start the conversation
            Debug.Log($"First time talking to {characterName}");
            dialogueManager.TriggerConversation(characterName, character.startingDialogueNode, this);
            character.hasStarted = true;
        }
        else
        {
            // Show existing messages
            Debug.Log($"Showing existing messages for {characterName}");
            ShowCharacterMessages(characterName);
            dialogueManager.SetCurrentCharacter(characterName);
        }
    }

    void HideCharacterMessages(string characterName)
    {
        CharacterConversation character = characters.Find(c => c.characterName == characterName);
        if (character != null)
        {
            Debug.Log($"Hiding {character.messageObjects.Count} messages for {characterName}");
            foreach (GameObject msg in character.messageObjects)
            {
                if (msg != null)
                {
                    msg.SetActive(false);
                }
            }
        }
    }

    void ShowCharacterMessages(string characterName)
    {
        CharacterConversation character = characters.Find(c => c.characterName == characterName);
        if (character != null)
        {
            Debug.Log($"Showing {character.messageObjects.Count} messages for {characterName}");
            foreach (GameObject msg in character.messageObjects)
            {
                if (msg != null)
                {
                    msg.SetActive(true);
                }
            }
        }
    }

    public void RegisterMessage(string characterName, GameObject messageObj)
    {
        CharacterConversation character = characters.Find(c => c.characterName == characterName);
        if (character != null)
        {
            character.messageObjects.Add(messageObj);
            Debug.Log($"Registered message for {characterName}. Total: {character.messageObjects.Count}");
        }
        else
        {
            Debug.LogWarning($"Tried to register message for unknown character: {characterName}");
        }
    }

    public void AddCharacter(string name, DialogueNode node)
    {
        characters.Add(new CharacterConversation
        {
            characterName = name,
            startingDialogueNode = node,
            hasStarted = false
        });
    }

    public void SendNewMessage(string characterName, DialogueNode newNode)
    {
        CharacterConversation character = characters.Find(c => c.characterName == characterName);

        if (character != null && newNode != null)
        {
            newNode.characterName = characterName;
            dialogueManager.TriggerConversation(characterName, newNode, this, true);
        }
    }

    public void ResetAll()
    {
        foreach (var character in characters)
        {
            character.hasStarted = false;
            character.messageObjects.Clear();
        }
        currentCharacter = "";
        dialogueManager.ClearDialogue();
    }
}