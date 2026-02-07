using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public GameObject dialogueLinePrefab;
    public Transform dialogueContainer;
    public ScrollRect scrollRect;

    [Header("Choice UI")]
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;

    [Header("Typing Indicator")]
    public GameObject typingIndicatorPrefab;
    private GameObject currentTypingIndicator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip defaultMessageSound;

    [Header("Settings")]
    public DialogueNode startingNode;
    public float timeBetweenLines = 0.3f;
    public bool useTypewriter = true;
    public float typewriterSpeed = 0.05f;
    public bool showTypingIndicator = true;
    public float typingIndicatorDelay = 0.8f;

    [Header("Delay Options")]
    public bool useTextLengthDelay = true; // Delay based on text length
    public bool useRandomDelay = false; // Random delay between messages
    public float minRandomDelay = 0.8f;
    public float maxRandomDelay = 2.0f;

    private DialogueNode currentNode;
    private bool isTyping = false;

    void Start()
    {
        // Check all required references
        if (!ValidateReferences())
        {
            Debug.LogError("DialogueManager is missing required references! Check the Inspector.");
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (startingNode != null)
        {
            StartDialogue(startingNode);
        }
    }

    bool ValidateReferences()
    {
        bool isValid = true;

        if (dialogueLinePrefab == null)
        {
            Debug.LogError("Dialogue Line Prefab is not assigned!");
            isValid = false;
        }

        if (dialogueContainer == null)
        {
            Debug.LogError("Dialogue Container is not assigned!");
            isValid = false;
        }

        if (scrollRect == null)
        {
            Debug.LogError("Scroll Rect is not assigned!");
            isValid = false;
        }

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("Choice Button Prefab is not assigned!");
            isValid = false;
        }

        if (choiceContainer == null)
        {
            Debug.LogError("Choice Container is not assigned!");
            isValid = false;
        }

        return isValid;
    }

    public void StartDialogue(DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogError("DialogueNode is null!");
            return;
        }

        currentNode = node;
        ClearChoices();
        StartCoroutine(ProcessNode(node));
    }

    IEnumerator ProcessNode(DialogueNode node)
    {
        // Display all NPC lines
        if (node.npcLines != null && node.npcLines.Length > 0)
        {
            foreach (DialogueLine line in node.npcLines)
            {
                yield return StartCoroutine(DisplayLine(line));

                // Calculate delay based on settings
                float delay = CalculateDelay(line);
                yield return new WaitForSeconds(delay);
            }
        }

        // Show player choices or auto-continue
        if (node.playerChoices != null && node.playerChoices.Length > 0)
        {
            ShowChoices(node.playerChoices);
        }
        else if (node.nextNode != null)
        {
            yield return new WaitForSeconds(1f);
            StartDialogue(node.nextNode);
        }
    }

    float CalculateDelay(DialogueLine line)
    {
        // If line has custom delay, use it
        if (line.delayAfter > 0)
        {
            return line.delayAfter;
        }

        // Use text length based delay
        if (useTextLengthDelay)
        {
            // 0.02 seconds per character, minimum 0.5 seconds
            return Mathf.Max(0.5f, line.dialogueText.Length * 0.02f);
        }

        // Use random delay
        if (useRandomDelay)
        {
            return Random.Range(minRandomDelay, maxRandomDelay);
        }

        // Use default delay
        return timeBetweenLines;
    }

    IEnumerator DisplayLine(DialogueLine line)
    {
        if (dialogueLinePrefab == null || dialogueContainer == null)
        {
            Debug.LogError("Cannot display line - prefab or container is null!");
            yield break;
        }

        // Show typing indicator for NPC messages
        if (!line.isPlayer && showTypingIndicator)
        {
            ShowTypingIndicator();
            yield return new WaitForSeconds(typingIndicatorDelay);
            HideTypingIndicator();
        }

        PlayMessageSound(line.messageSound);

        GameObject lineObj = Instantiate(dialogueLinePrefab, dialogueContainer);
        lineObj.SetActive(true);

        DialogueLineUI lineUI = lineObj.GetComponent<DialogueLineUI>();

        if (lineUI == null)
        {
            Debug.LogError("DialogueLinePrefab is missing DialogueLineUI component!");
            yield break;
        }

        if (useTypewriter && !line.isPlayer)
        {
            yield return StartCoroutine(TypewriterEffect(lineUI, line));
        }
        else
        {
            lineUI.Setup(line);
        }

        StartCoroutine(ScrollToBottom());
    }

    IEnumerator TypewriterEffect(DialogueLineUI lineUI, DialogueLine line)
    {
        isTyping = true;

        // Build the complete prefix with colored name
        string colorHex = ColorUtility.ToHtmlStringRGB(line.textColor);
        string fullPrefix = $"<color=#{colorHex}><b>{line.characterName}:</b></color> ";

        // Start with just the prefix visible
        lineUI.messageText.text = fullPrefix;
        lineUI.messageText.color = Color.white;

        // Now type out ONLY the dialogue text (not the name again)
        string textSoFar = fullPrefix;
        foreach (char c in line.dialogueText)
        {
            if (lineUI != null && lineUI.messageText != null)
            {
                textSoFar += c;
                lineUI.messageText.text = textSoFar;
            }
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    void ShowTypingIndicator()
    {
        if (!showTypingIndicator || typingIndicatorPrefab == null || dialogueContainer == null)
            return;

        // Remove any existing typing indicator
        HideTypingIndicator();

        // Create new typing indicator
        currentTypingIndicator = Instantiate(typingIndicatorPrefab, dialogueContainer);
        StartCoroutine(AnimateTypingIndicator());
        StartCoroutine(ScrollToBottom());
    }

    void HideTypingIndicator()
    {
        if (currentTypingIndicator != null)
        {
            Destroy(currentTypingIndicator);
            currentTypingIndicator = null;
        }
    }

    IEnumerator AnimateTypingIndicator()
    {
        if (currentTypingIndicator == null) yield break;

        TextMeshProUGUI text = currentTypingIndicator.GetComponentInChildren<TextMeshProUGUI>();
        if (text == null)
        {
            text = currentTypingIndicator.GetComponent<TextMeshProUGUI>();
        }

        if (text == null) yield break;

        string[] states = { ".", "..", "..." };
        int index = 0;

        while (currentTypingIndicator != null)
        {
            text.text = states[index];
            index = (index + 1) % states.Length;
            yield return new WaitForSeconds(0.5f);
        }
    }

    void ShowChoices(DialogueChoice[] choices)
    {
        if (choiceButtonPrefab == null || choiceContainer == null)
        {
            Debug.LogError("Cannot show choices - prefab or container is null!");
            return;
        }

        ClearChoices();

        foreach (DialogueChoice choice in choices)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choiceContainer);
            ChoiceButton choiceButton = choiceObj.GetComponent<ChoiceButton>();

            if (choiceButton != null)
            {
                choiceButton.Setup(choice, this);
            }
            else
            {
                Debug.LogError("ChoiceButtonPrefab is missing ChoiceButton component!");
            }
        }
    }

    public void OnPlayerChoice(DialogueChoice choice)
    {
        PlayMessageSound(choice.choiceSound);

        DialogueLine playerLine = new DialogueLine
        {
            characterName = "You",
            dialogueText = choice.choiceText,
            textColor = Color.white,
            isPlayer = true
        };

        StartCoroutine(DisplayPlayerChoice(playerLine, choice.nextNode));
    }

    IEnumerator DisplayPlayerChoice(DialogueLine playerLine, DialogueNode nextNode)
    {
        ClearChoices();

        if (dialogueLinePrefab != null && dialogueContainer != null)
        {
            GameObject lineObj = Instantiate(dialogueLinePrefab, dialogueContainer);
            DialogueLineUI lineUI = lineObj.GetComponent<DialogueLineUI>();

            if (lineUI != null)
            {
                lineUI.Setup(playerLine);
            }
        }

        yield return StartCoroutine(ScrollToBottom());

        // Wait longer before starting next node (changed from 0.5f)
        yield return new WaitForSeconds(1.5f);

        if (nextNode != null)
        {
            StartDialogue(nextNode);
        }
    }

    void PlayMessageSound(AudioClip clip)
    {
        if (audioSource != null)
        {
            AudioClip soundToPlay = clip != null ? clip : defaultMessageSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
    }

    IEnumerator ScrollToBottom()
    {
        if (scrollRect == null)
        {
            yield break;
        }

        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void ClearChoices()
    {
        if (choiceContainer == null)
        {
            return;
        }

        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public void ClearDialogue()
    {
        if (dialogueContainer != null)
        {
            foreach (Transform child in dialogueContainer)
            {
                Destroy(child.gameObject);
            }
        }
        ClearChoices();
    }
}