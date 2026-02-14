using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public float timeBetweenLines = 1.5f;
    public bool useTypewriter = true;
    public float typewriterSpeed = 0.05f;
    public bool showTypingIndicator = true;
    public float typingIndicatorDelay = 0.8f;

    [Header("Delay Options")]
    public bool useTextLengthDelay = true;
    public bool useRandomDelay = false;
    public float minRandomDelay = 0.8f;
    public float maxRandomDelay = 2.0f;
    public float delayBeforeNewNode = 1.5f;
    public float continuationDelay = 0.1f;

    private DialogueNode currentNode;
    private string currentCharacter = "";
    private bool isTyping = false;
    private bool isNewNode = false;
    private ConversationManager conversationManager;
    private GameObject lastDialogueLineObj;
    private GameObject currentAttachedObject;

    private HashSet<DialogueNode> completedNodes = new HashSet<DialogueNode>();

    void Start()
    {
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
            string tempName = startingNode.characterName;
            int lineCount = startingNode.npcLines != null ? startingNode.npcLines.Length : 0;

            Debug.Log($"Starting Node: {startingNode.name}");
            Debug.Log($"Character: {tempName}");
            Debug.Log($"Lines: {lineCount}");

            if (lineCount == 0)
            {
                Debug.LogWarning("Starting node has 0 lines! Did you forget to set up the dialogue?");
            }

            StartDialogue(startingNode);
        }
        else
        {
            Debug.LogError("Starting Node is not assigned in the Inspector!");
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

    public void SetCurrentCharacter(string characterName)
    {
        currentCharacter = characterName;
        ClearChoices();
        StartCoroutine(ScrollToBottom());
    }

    public void StartDialogue(DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogError("DialogueNode is null!");
            return;
        }

        if (node.npcLines == null || node.npcLines.Length == 0)
        {
            Debug.LogWarning($"Node '{node.name}' has no lines! Make sure it's set up properly.");
        }
        else
        {
            string tempText = node.npcLines[0].dialogueText;
            Debug.Log($"Loading node: {node.name} - First line: {(string.IsNullOrEmpty(tempText) ? "EMPTY" : tempText.Substring(0, Mathf.Min(20, tempText.Length)))}...");
        }

        if (!string.IsNullOrEmpty(node.characterName))
        {
            currentCharacter = node.characterName;
        }

        currentNode = node;
        isNewNode = true;
        ClearChoices();
        StartCoroutine(ProcessNode(node));
    }

    IEnumerator ProcessNode(DialogueNode node)
    {
        if (isNewNode && currentNode != startingNode)
        {
            yield return new WaitForSeconds(delayBeforeNewNode);
        }

        if (node.npcLines == null)
        {
            Debug.LogError($"Node '{node.name}' npcLines is NULL!");
            yield break;
        }

        if (node.npcLines != null && node.npcLines.Length > 0)
        {
            for (int i = 0; i < node.npcLines.Length; i++)
            {
                DialogueLine line = node.npcLines[i];

                yield return StartCoroutine(DisplayLine(line));

                bool isLastMessage = (i == node.npcLines.Length - 1);
                bool hasChoices = (node.playerChoices != null && node.playerChoices.Length > 0);

                if (!isLastMessage || !hasChoices)
                {
                    bool nextIsContinuation = (i + 1 < node.npcLines.Length) && node.npcLines[i + 1].isContinuation;

                    if (nextIsContinuation)
                    {
                        yield return new WaitForSeconds(continuationDelay);
                    }
                    else
                    {
                        float delay = CalculateDelay(line);
                        yield return new WaitForSeconds(delay);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        isNewNode = false;

        if (node.playerChoices != null && node.playerChoices.Length > 0)
        {
            ShowChoices(node.playerChoices);
        }
        else if (node.nextNode != null)
        {
            DialogueNode next = node.nextNode;

            if (next.npcLines == null || next.npcLines.Length == 0)
            {
                Debug.LogWarning($"Next node '{next.name}' has no lines loaded!");
            }
            else
            {
                Debug.Log($"Transitioning to next node: {next.name}");
            }

            yield return new WaitForSeconds(1f);
            StartDialogue(next);
        }
    }

    float CalculateDelay(DialogueLine line)
    {
        if (line.delayAfter > 0)
        {
            return line.delayAfter;
        }

        if (useTextLengthDelay)
        {
            float calculatedDelay = Mathf.Clamp(line.dialogueText.Length * 0.03f, 1.0f, 3.0f);
            return calculatedDelay;
        }

        if (useRandomDelay)
        {
            return Random.Range(minRandomDelay, maxRandomDelay);
        }

        return timeBetweenLines;
    }

    IEnumerator DisplayLine(DialogueLine line)
    {
        if (dialogueLinePrefab == null || dialogueContainer == null)
        {
            Debug.LogError("Cannot display line - prefab or container is null!");
            yield break;
        }

        // Destroy previous attached object if needed
        if (currentAttachedObject != null)
        {
            Destroy(currentAttachedObject);
            currentAttachedObject = null;
        }

        if (!line.isPlayer && !line.isContinuation && showTypingIndicator && typingIndicatorPrefab != null)
        {
            ShowTypingIndicator();
            yield return new WaitForSeconds(typingIndicatorDelay);
            HideTypingIndicator();
        }

        PlayMessageSound(line.messageSound);

        GameObject lineObj = Instantiate(dialogueLinePrefab, dialogueContainer);
        lineObj.SetActive(true);
        lastDialogueLineObj = lineObj;

        if (conversationManager != null)
        {
            conversationManager.RegisterMessage(currentCharacter, lineObj);
        }

        DialogueLineUI lineUI = lineObj.GetComponent<DialogueLineUI>();

        if (lineUI == null)
        {
            Debug.LogError("DialogueLinePrefab is missing DialogueLineUI component!");
            yield break;
        }

        if (useTypewriter && !line.isPlayer && !line.isContinuation)
        {
            yield return StartCoroutine(TypewriterEffect(lineUI, line));
        }
        else
        {
            lineUI.Setup(line);
        }

        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform containerRect = dialogueContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }

        yield return StartCoroutine(ScrollToBottom());
        yield return new WaitForSeconds(0.05f);

        // Show attached GameObject if enabled
        if (line.showAttachedObject && line.attachedObjectPrefab != null)
        {
            StartCoroutine(ShowAttachedObject(line));
        }
    }

    IEnumerator TypewriterEffect(DialogueLineUI lineUI, DialogueLine line)
    {
        isTyping = true;

        string colorHex = ColorUtility.ToHtmlStringRGB(line.textColor);
        string fullPrefix = $"<color=#{colorHex}><b>{line.characterName}:</b></color> ";

        lineUI.messageText.text = fullPrefix;
        lineUI.messageText.color = Color.white;

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

        HideTypingIndicator();

        currentTypingIndicator = Instantiate(typingIndicatorPrefab, dialogueContainer);
        currentTypingIndicator.SetActive(true);

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

        StartCoroutine(DisplayPlayerChoice(playerLine, choice));
    }

    IEnumerator DisplayPlayerChoice(DialogueLine playerLine, DialogueChoice choice)
    {
        ClearChoices();

        if (dialogueLinePrefab != null && dialogueContainer != null)
        {
            GameObject lineObj = Instantiate(dialogueLinePrefab, dialogueContainer);

            if (conversationManager != null)
            {
                conversationManager.RegisterMessage(currentCharacter, lineObj);
            }

            DialogueLineUI lineUI = lineObj.GetComponent<DialogueLineUI>();

            if (lineUI != null)
            {
                lineUI.Setup(playerLine);
            }
        }

        yield return StartCoroutine(ScrollToBottom());
        yield return new WaitForSeconds(0.8f);

        if (!string.IsNullOrEmpty(choice.switchToCharacter))
        {
            if (conversationManager != null)
            {
                conversationManager.SwitchToCharacter(choice.switchToCharacter);
            }
        }
        else if (choice.nextNode != null)
        {
            DialogueNode next = choice.nextNode;

            if (next.npcLines == null || next.npcLines.Length == 0)
            {
                Debug.LogWarning($"Choice leading to node '{next.name}' which has no lines!");
            }
            else
            {
                Debug.Log($"Choice leading to node: {next.name}");
            }

            StartDialogue(next);
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
        yield return null;

        Canvas.ForceUpdateCanvases();

        RectTransform containerRect = dialogueContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }

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
        completedNodes.Clear();
        ClearChoices();
        lastDialogueLineObj = null;

        if (currentAttachedObject != null)
        {
            Destroy(currentAttachedObject);
            currentAttachedObject = null;
        }
    }

    public void TriggerConversation(string characterName, DialogueNode node, ConversationManager cm, bool forceRerun = false)
    {
        if (node == null)
        {
            Debug.LogError("TriggerConversation: Node is null!");
            return;
        }

        conversationManager = cm;
        node.characterName = characterName;
        currentCharacter = characterName;

        if (!completedNodes.Contains(node) || forceRerun)
        {
            completedNodes.Add(node);
            StartDialogue(node);
        }
    }

    IEnumerator ShowAttachedObject(DialogueLine line)
    {
        yield return new WaitForSeconds(line.attachedObjectDelay);

        if (line.attachedObjectPrefab != null)
        {
            // Find Canvas to spawn under
            Canvas canvas = FindObjectOfType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;

            // Instantiate the prefab
            GameObject spawnedObject = Instantiate(line.attachedObjectPrefab, parent);
            spawnedObject.SetActive(true);

            currentAttachedObject = spawnedObject;

            Debug.Log($"Showing attached object: {spawnedObject.name}");

            if (!line.destroyOnNextMessage)
            {
                currentAttachedObject = null;
            }
        }
    }
}