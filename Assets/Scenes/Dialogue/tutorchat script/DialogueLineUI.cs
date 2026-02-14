using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueLineUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public GameObject playerIndicator;

    private ContinuationLayoutHelper layoutHelper;

    void Awake()
    {
        layoutHelper = GetComponent<ContinuationLayoutHelper>();
    }

    public void Setup(DialogueLine line)
    {
        if (messageText == null)
        {
            Debug.LogError("MessageText is not assigned in DialogueLineUI!");
            return;
        }

        // Apply layout indentation for continuation messages
        if (layoutHelper != null)
        {
            layoutHelper.SetContinuation(line.isContinuation);
        }

        string textToDisplay = line.dialogueText;

        // Apply word highlighting if any
        if (line.wordHighlights != null && line.wordHighlights.Length > 0)
        {
            foreach (WordHighlight highlight in line.wordHighlights)
            {
                if (!string.IsNullOrEmpty(highlight.word))
                {
                    string colorHex = ColorUtility.ToHtmlStringRGB(highlight.highlightColor);
                    // Replace all instances of the word with colored version
                    textToDisplay = textToDisplay.Replace(highlight.word,
                        $"<color=#{colorHex}>{highlight.word}</color>");
                }
            }
        }

        // For continuation messages, don't show the name prefix
        if (line.isContinuation)
        {
            // Just show the text without any prefix
            messageText.text = textToDisplay;
            messageText.color = line.textColor; // Use the same color as the character
        }
        else if (line.isPlayer)
        {
            messageText.text = "You: " + textToDisplay;
            messageText.color = Color.white;
        }
        else
        {
            // NPC message - use bold name with color, then colored message text
            string colorHex = ColorUtility.ToHtmlStringRGB(line.textColor);
            messageText.text = $"<color=#{colorHex}><b>{line.characterName}:</b> {textToDisplay}</color>";
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