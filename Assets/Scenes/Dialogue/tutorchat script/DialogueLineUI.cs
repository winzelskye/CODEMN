using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

        string prefix = "";

        // For continuation messages, don't show the name prefix
        if (line.isContinuation)
        {
            // Just show the text with proper indentation to align with previous message
            messageText.text = "                     " + line.dialogueText;
            messageText.color = line.textColor; // Use the same color as the character
        }
        else if (line.isPlayer)
        {
            prefix = "You: ";
            messageText.text = prefix + line.dialogueText;
            messageText.color = Color.white;
        }
        else
        {
            // NPC message - use bold name with color, then colored message text
            string colorHex = ColorUtility.ToHtmlStringRGB(line.textColor);
            messageText.text = $"<color=#{colorHex}><b>{line.characterName}:</b> {line.dialogueText}</color>";
            messageText.color = Color.white; // Base color (overridden by rich text tags)
        }

        if (playerIndicator != null)
        {
            playerIndicator.SetActive(line.isPlayer);
        }

        // Force immediate layout update
        StartCoroutine(ForceLayoutUpdate());
    }

    System.Collections.IEnumerator ForceLayoutUpdate()
    {
        yield return null;

        if (messageText != null)
        {
            messageText.ForceMeshUpdate();
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(messageText.GetComponent<RectTransform>());

        if (transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
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
        StartCoroutine(ForceLayoutUpdate());
    }
}