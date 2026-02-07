using UnityEngine;
using TMPro;

public class DialogueLineUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public GameObject playerIndicator;

    public void Setup(DialogueLine line)
    {
        if (messageText == null)
        {
            Debug.LogError("MessageText is not assigned in DialogueLineUI!");
            return;
        }

        string prefix = line.isPlayer ? "You: " : $"<b>{line.characterName}:</b> ";
        messageText.text = prefix + line.dialogueText;
        messageText.color = line.textColor;

        if (playerIndicator != null)
        {
            playerIndicator.SetActive(line.isPlayer);
        }
    }

    public void SetText(string text, Color color)
    {
        if (messageText == null)
        {
            Debug.LogError("MessageText is not assigned in DialogueLineUI!");
            return;
        }

        messageText.text = text;
        messageText.color = color;
    }
}