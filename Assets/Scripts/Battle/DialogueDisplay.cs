using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Image dialogueBoxImage; // The dialogue box background

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("Custom Dialogue Box Sprites")]
    [SerializeField] private Sprite defaultDialogueBox;
    [SerializeField] private DialogueBoxPreset[] customDialogueBoxes;

    private Coroutine typingCoroutine;
    private Sprite originalDialogueBox;
    private Color originalTextColor;
    private Color originalNameColor;

    private void Start()
    {
        if (dialogueBoxImage != null)
        {
            originalDialogueBox = dialogueBoxImage.sprite;
            if (originalDialogueBox == null)
                originalDialogueBox = defaultDialogueBox;
        }

        if (dialogueText != null)
            originalTextColor = dialogueText.color;

        if (characterNameText != null)
            originalNameColor = characterNameText.color;
    }

    public void ShowDialogue(DialogueEntry dialogue)
    {
        dialoguePanel.SetActive(true);

        // Set character name
        if (characterNameText != null)
            characterNameText.text = dialogue.characterName;

        // Set character portrait
        if (characterPortrait != null && dialogue.characterPortrait != null)
        {
            characterPortrait.sprite = dialogue.characterPortrait;
            characterPortrait.gameObject.SetActive(true);
        }
        else if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }

        // Apply custom dialogue box - first check if dialogue has custom box, then check presets
        if (dialogue.customDialogueBox != null)
        {
            SetDialogueBox(dialogue.customDialogueBox);
        }
        else
        {
            ApplyCustomDialogueBox(dialogue.characterName);
        }

        // Display text
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (useTypewriterEffect)
        {
            typingCoroutine = StartCoroutine(TypeText(dialogue.dialogueText, dialogue.displayDuration));
        }
        else
        {
            dialogueText.text = dialogue.dialogueText;
            StartCoroutine(HideAfterDelay(dialogue.displayDuration));
        }
    }

    public void ShowDialogue(string text, string characterName, float duration = 3f, Sprite portrait = null, Sprite customBox = null)
    {
        dialoguePanel.SetActive(true);

        if (characterNameText != null)
            characterNameText.text = characterName;

        if (characterPortrait != null && portrait != null)
        {
            characterPortrait.sprite = portrait;
            characterPortrait.gameObject.SetActive(true);
        }
        else if (characterPortrait != null)
        {
            characterPortrait.gameObject.SetActive(false);
        }

        // Apply custom dialogue box directly or by character name
        if (customBox != null)
        {
            SetDialogueBox(customBox);
        }
        else
        {
            ApplyCustomDialogueBox(characterName);
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (useTypewriterEffect)
        {
            typingCoroutine = StartCoroutine(TypeText(text, duration));
        }
        else
        {
            dialogueText.text = text;
            StartCoroutine(HideAfterDelay(duration));
        }
    }

    private void ApplyCustomDialogueBox(string characterName)
    {
        if (dialogueBoxImage == null || customDialogueBoxes == null) return;

        // Look for custom box for this character
        foreach (var preset in customDialogueBoxes)
        {
            if (preset.characterName.Equals(characterName, System.StringComparison.OrdinalIgnoreCase))
            {
                SetDialogueBox(preset.dialogueBoxSprite);

                // Apply text color if specified
                if (preset.useCustomTextColor && dialogueText != null)
                {
                    dialogueText.color = preset.textColor;
                }

                // Apply name color if specified
                if (preset.useCustomNameColor && characterNameText != null)
                {
                    characterNameText.color = preset.nameColor;
                }

                return;
            }
        }

        // No custom box found, use default
        ResetDialogueBox();
    }

    public void SetDialogueBox(Sprite boxSprite)
    {
        if (dialogueBoxImage != null && boxSprite != null)
        {
            dialogueBoxImage.sprite = boxSprite;
        }
    }

    public void ResetDialogueBox()
    {
        if (dialogueBoxImage != null && originalDialogueBox != null)
        {
            dialogueBoxImage.sprite = originalDialogueBox;
        }

        // Reset text colors to default
        if (dialogueText != null)
            dialogueText.color = originalTextColor;

        if (characterNameText != null)
            characterNameText.color = originalNameColor;
    }

    private IEnumerator TypeText(string text, float duration)
    {
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(duration);
        HideDialogue();
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideDialogue();
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        ResetDialogueBox();
    }

    public void SetTextSpeed(float speed)
    {
        textSpeed = speed;
    }

    public void SetTypewriterEffect(bool enabled)
    {
        useTypewriterEffect = enabled;
    }

    public void SkipTypewriter()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            // Show full text immediately
            // Note: You'd need to store the full text to display it here
        }
    }
}

// ===================================================
// CUSTOM DIALOGUE BOX PRESET
// ===================================================
[System.Serializable]
public class DialogueBoxPreset
{
    public string characterName;
    public Sprite dialogueBoxSprite;

    [Header("Optional Text Customization")]
    public bool useCustomTextColor;
    public Color textColor = Color.white;

    public bool useCustomNameColor;
    public Color nameColor = Color.white;
}