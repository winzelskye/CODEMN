using UnityEngine;
using UnityEngine.UI;

public class SimpleConversationButton : MonoBehaviour
{
    public ConversationManager conversationManager;
    public string characterName;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        if (conversationManager != null)
        {
            conversationManager.SwitchToCharacter(characterName);
        }
        else
        {
            Debug.LogError("ConversationManager not assigned!");
        }
    }
}