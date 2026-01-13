using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;

public class SimpleTextController : MonoBehaviour
{
    [Header("Text Component")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Text Messages")]
    [SerializeField] private List<TextMessage> messages = new List<TextMessage>();

    [Header("Current Message")]
    [SerializeField] private int currentMessageIndex = 0;
    [SerializeField] private bool messagesActive = false;

    [Header("Display Settings")]
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float typewriterSpeed = 0.05f;

    [Header("Global Next Message Control")]
    [SerializeField] private NextMessageTrigger globalNextTrigger = NextMessageTrigger.PressEnter;

    [Header("Conditions (For Testing)")]
    [SerializeField] private List<string> activeConditions = new List<string>();

    private Coroutine typewriterCoroutine;
    private Coroutine effectCoroutine;
    private Coroutine displayDurationCoroutine;
    private GameObject textPanel;
    private Dictionary<string, bool> conditionStates = new Dictionary<string, bool>();
    private bool waitingForTrigger = false;
    private bool isTyping = false;
    private bool showingSpecialMessage = false;
    private int lastNormalMessageIndex = -1;

    void Start()
    {
        // Try to find text panel (parent of text component)
        if (textComponent != null)
        {
            textPanel = textComponent.transform.parent?.gameObject;
        }

        // Set up button listeners for special messages
        for (int i = 0; i < messages.Count; i++)
        {
            int messageIndex = i; // Capture index for closure
            TextMessage msg = messages[messageIndex];

            if (msg.isSpecialMessage && msg.triggerButton != null)
            {
                msg.triggerButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Button '{msg.triggerButton.name}' clicked! Showing special message {messageIndex}: {msg.text}");
                    ShowSpecialMessage(messageIndex);
                });
            }
        }

        // Check for any messages marked as "show on start"
        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i].showOnStart && !messages[i].isSpecialMessage)
            {
                Debug.Log($"Found message {i} marked as Show on Start. Starting message sequence.");
                messagesActive = true;
                ShowMessage(i);
                return;
            }
        }

        // If no message is marked, hide text
        Debug.Log("No messages marked as Show on Start. Hiding text.");
        HideText();
    }

    /// <summary>
    /// Show a special message (doesn't affect the main message sequence)
    /// </summary>
    private void ShowSpecialMessage(int index)
    {
        if (index < 0 || index >= messages.Count)
        {
            Debug.LogWarning($"Message index {index} out of range!");
            return;
        }

        TextMessage message = messages[index];

        // Save the current normal message index
        if (!showingSpecialMessage && messagesActive)
        {
            lastNormalMessageIndex = currentMessageIndex;
        }

        showingSpecialMessage = true;

        if (textPanel != null)
        {
            textPanel.SetActive(true);
        }

        if (textComponent == null)
        {
            Debug.LogError("Text component not assigned!");
            return;
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        if (displayDurationCoroutine != null)
        {
            StopCoroutine(displayDurationCoroutine);
        }

        isTyping = false;
        waitingForTrigger = false;

        DisplayMessageWithEffects(message);

        Debug.Log($"Showing special message {index}: {message.text}");
    }

    void Update()
    {
        if (!messagesActive && !showingSpecialMessage)
            return;

        // Get current message to check its specific trigger
        if (currentMessageIndex >= 0 && currentMessageIndex < messages.Count)
        {
            TextMessage currentMsg = messages[currentMessageIndex];
            NextMessageTrigger trigger = currentMsg.useCustomTrigger ? currentMsg.nextMessageTrigger : globalNextTrigger;

            // If still typing and user presses key, complete the typing immediately
            if (isTyping)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                    Input.GetKeyDown(KeyCode.Space) || Input.anyKeyDown)
                {
                    CompleteTyping();
                }
                return;
            }

            // Check for input based on trigger type (only if not typing and waiting for trigger)
            if (!waitingForTrigger)
                return;

            switch (trigger)
            {
                case NextMessageTrigger.PressEnter:
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        Debug.Log("Enter pressed - advancing message");
                        waitingForTrigger = false;

                        if (showingSpecialMessage)
                        {
                            ResumeNormalMessages();
                        }
                        else
                        {
                            NextMessage();
                        }
                    }
                    break;

                case NextMessageTrigger.PressSpace:
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        Debug.Log("Space pressed - advancing message");
                        waitingForTrigger = false;

                        if (showingSpecialMessage)
                        {
                            ResumeNormalMessages();
                        }
                        else
                        {
                            NextMessage();
                        }
                    }
                    break;

                case NextMessageTrigger.PressAnyKey:
                    if (Input.anyKeyDown)
                    {
                        Debug.Log("Key pressed - advancing message");
                        waitingForTrigger = false;

                        if (showingSpecialMessage)
                        {
                            ResumeNormalMessages();
                        }
                        else
                        {
                            NextMessage();
                        }
                    }
                    break;

                case NextMessageTrigger.MouseClick:
                    if (Input.GetMouseButtonDown(0))
                    {
                        Debug.Log("Mouse clicked - advancing message");
                        waitingForTrigger = false;

                        if (showingSpecialMessage)
                        {
                            ResumeNormalMessages();
                        }
                        else
                        {
                            NextMessage();
                        }
                    }
                    break;

                case NextMessageTrigger.RightClick:
                    if (Input.GetMouseButtonDown(1))
                    {
                        Debug.Log("Right clicked - advancing message");
                        waitingForTrigger = false;

                        if (showingSpecialMessage)
                        {
                            ResumeNormalMessages();
                        }
                        else
                        {
                            NextMessage();
                        }
                    }
                    break;
            }
        }
    }

    private void ResumeNormalMessages()
    {
        showingSpecialMessage = false;

        if (messagesActive && lastNormalMessageIndex >= 0)
        {
            Debug.Log($"Resuming normal messages from index {lastNormalMessageIndex}");
            ShowMessage(lastNormalMessageIndex);
        }
        else
        {
            HideText();
        }
    }

    private void CompleteTyping()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (displayDurationCoroutine != null)
        {
            StopCoroutine(displayDurationCoroutine);
            displayDurationCoroutine = null;
        }

        if (currentMessageIndex >= 0 && currentMessageIndex < messages.Count)
        {
            textComponent.text = messages[currentMessageIndex].text;
        }

        isTyping = false;
        waitingForTrigger = true;
        Debug.Log("Typing completed - waiting for next input");
    }

    // ============================================
    // PUBLIC METHODS
    // ============================================

    public void ShowMessage(int index)
    {
        if (index < 0 || index >= messages.Count)
        {
            Debug.LogWarning($"Message index {index} out of range!");
            return;
        }

        currentMessageIndex = index;
        TextMessage message = messages[index];

        Debug.Log($"=== Attempting to show message {index} ===");

        // Check trigger condition
        if (!EvaluateTriggerCondition(message))
        {
            Debug.Log($"Message {index} skipped - trigger condition not met");
            if (messagesActive)
            {
                NextMessage();
            }
            return;
        }

        // Show the text panel
        if (textPanel != null)
        {
            textPanel.SetActive(true);
        }

        if (textComponent == null)
        {
            Debug.LogError("Text component not assigned!");
            return;
        }

        // Stop any ongoing effects
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        if (displayDurationCoroutine != null)
        {
            StopCoroutine(displayDurationCoroutine);
        }

        isTyping = false;
        waitingForTrigger = false;

        // Display the message with effects
        DisplayMessageWithEffects(message);

        // Handle next message trigger
        NextMessageTrigger trigger = message.useCustomTrigger ? message.nextMessageTrigger : globalNextTrigger;

        Debug.Log($"Message {index} trigger type: {trigger}");

        if (trigger == NextMessageTrigger.Auto)
        {
            StartCoroutine(AutoAdvanceMessage(message.autoAdvanceDelay));
        }
        else if (trigger == NextMessageTrigger.Button)
        {
            // Button trigger - wait for manual call
            Debug.Log("Waiting for button click to advance");
        }
        else
        {
            // Will be handled by Update() after typing completes
            Debug.Log($"Will wait for {trigger} after typing completes");
        }

        Debug.Log($"Successfully showing message {index}: {message.text}");
    }

    private IEnumerator AutoAdvanceMessage(float delay)
    {
        Debug.Log($"Auto-advancing in {delay} seconds...");
        yield return new WaitForSeconds(delay);

        if (showingSpecialMessage)
        {
            ResumeNormalMessages();
        }
        else
        {
            NextMessage();
        }
    }

    public void NextMessage()
    {
        if (!messagesActive)
        {
            Debug.Log("Messages not active - cannot advance");
            return;
        }

        // Check if current message is a final text
        if (currentMessageIndex >= 0 && currentMessageIndex < messages.Count)
        {
            if (messages[currentMessageIndex].isFinalText)
            {
                Debug.Log("Final text reached - stopping messages");
                StopMessages();
                return;
            }
        }

        Debug.Log($"NextMessage called. Current index: {currentMessageIndex}, Total messages: {messages.Count}");

        // Skip special messages in normal sequence
        int nextIndex = currentMessageIndex + 1;
        while (nextIndex < messages.Count && messages[nextIndex].isSpecialMessage)
        {
            Debug.Log($"Skipping special message at index {nextIndex}");
            nextIndex++;
        }

        if (nextIndex < messages.Count)
        {
            ShowMessage(nextIndex);
        }
        else
        {
            Debug.Log("Reached the end of messages.");
            StopMessages();
        }
    }

    public void StartMessages()
    {
        messagesActive = true;

        // Find first non-special message
        int firstIndex = 0;
        while (firstIndex < messages.Count && messages[firstIndex].isSpecialMessage)
        {
            firstIndex++;
        }

        if (firstIndex < messages.Count)
        {
            ShowMessage(firstIndex);
            Debug.Log($"Messages started from index {firstIndex}");
        }
        else
        {
            Debug.LogWarning("No normal messages found!");
        }
    }

    public void StopMessages()
    {
        messagesActive = false;
        showingSpecialMessage = false;
        waitingForTrigger = false;
        isTyping = false;
        HideText();
        Debug.Log("Messages stopped");
    }

    public void TriggerSpecialMessage(int index)
    {
        ShowSpecialMessage(index);
    }

    // ============================================
    // TRIGGER CONDITIONS
    // ============================================

    private bool EvaluateTriggerCondition(TextMessage message)
    {
        switch (message.triggerCondition)
        {
            case TriggerCondition.Always:
                return true;

            case TriggerCondition.OnTurnStart:
                return CheckCondition("TurnStart");

            case TriggerCondition.OnTurnEnd:
                return CheckCondition("TurnEnd");

            case TriggerCondition.OnHit:
                return CheckCondition("Hit");

            case TriggerCondition.OnLowHealth:
                return CheckCondition("LowHealth");

            case TriggerCondition.OnPhaseChange:
                return CheckCondition("PhaseChange");

            default:
                return true;
        }
    }

    // ============================================
    // CONDITION SYSTEM
    // ============================================

    public void SetCondition(string conditionName, bool value = true)
    {
        conditionStates[conditionName] = value;

        if (!activeConditions.Contains(conditionName) && value)
        {
            activeConditions.Add(conditionName);
        }
        else if (activeConditions.Contains(conditionName) && !value)
        {
            activeConditions.Remove(conditionName);
        }

        Debug.Log($"Condition '{conditionName}' set to {value}");
    }

    public bool CheckCondition(string conditionName)
    {
        if (string.IsNullOrEmpty(conditionName))
            return true;

        return conditionStates.ContainsKey(conditionName) && conditionStates[conditionName];
    }

    public void ClearCondition(string conditionName)
    {
        SetCondition(conditionName, false);
    }

    public void ClearAllConditions()
    {
        conditionStates.Clear();
        activeConditions.Clear();
        Debug.Log("All conditions cleared");
    }

    public List<string> GetActiveConditions()
    {
        return new List<string>(activeConditions);
    }

    public void SetText(string text)
    {
        if (textComponent != null)
        {
            textComponent.text = text;

            if (textPanel != null)
            {
                textPanel.SetActive(true);
            }
        }
    }

    public void ClearText()
    {
        if (textComponent != null)
        {
            textComponent.text = "";
        }
    }

    public void HideText()
    {
        if (textPanel != null)
        {
            textPanel.SetActive(false);
        }
        Debug.Log("Text hidden");
    }

    public void ShowText()
    {
        if (textPanel != null)
        {
            textPanel.SetActive(true);
        }
    }

    public void AddMessage(string text, bool useTypewriter = true)
    {
        messages.Add(new TextMessage
        {
            text = text,
            useTypewriter = useTypewriter
        });
    }

    public void RemoveMessage(int index)
    {
        if (index >= 0 && index < messages.Count)
        {
            messages.RemoveAt(index);
            Debug.Log($"Removed message at index {index}");
        }
    }

    public void ClearAllMessages()
    {
        messages.Clear();
        Debug.Log("All messages cleared");
    }

    // ============================================
    // PRIVATE METHODS
    // ============================================

    private void DisplayMessageWithEffects(TextMessage message)
    {
        textComponent.transform.localPosition = Vector3.zero;
        textComponent.transform.localScale = Vector3.one;

        if (message.useCustomColor)
        {
            textComponent.color = message.textColor;
        }
        else
        {
            textComponent.color = Color.white;
        }

        if (message.useCustomFontSize)
        {
            textComponent.fontSize = message.fontSize;
        }

        if (useTypewriter && message.useTypewriter)
        {
            isTyping = true;
            typewriterCoroutine = StartCoroutine(TypewriterEffect(message));
        }
        else
        {
            textComponent.text = message.text;
            isTyping = false;

            // Handle display duration
            if (message.useDisplayDuration && message.displayDuration > 0)
            {
                displayDurationCoroutine = StartCoroutine(DisplayDurationTimer(message));
            }
            else
            {
                // Immediately ready for next input
                NextMessageTrigger trigger = message.useCustomTrigger ? message.nextMessageTrigger : globalNextTrigger;
                if (trigger != NextMessageTrigger.Auto && trigger != NextMessageTrigger.Button)
                {
                    waitingForTrigger = true;
                }
            }
        }

        if (message.textEffect != TextEffect.None)
        {
            effectCoroutine = StartCoroutine(ApplyEffect(message.textEffect, message.effectDuration, message.effectSettings));
        }

        // Invoke UnityEvent
        message.onMessageShown?.Invoke();
    }

    private IEnumerator TypewriterEffect(TextMessage message)
    {
        textComponent.text = "";

        foreach (char c in message.text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        // Handle display duration after typing
        if (message.useDisplayDuration && message.displayDuration > 0)
        {
            displayDurationCoroutine = StartCoroutine(DisplayDurationTimer(message));
        }
        else
        {
            // After typing completes, enable waiting for trigger
            if (currentMessageIndex >= 0 && currentMessageIndex < messages.Count)
            {
                TextMessage currentMsg = messages[currentMessageIndex];
                NextMessageTrigger trigger = currentMsg.useCustomTrigger ? currentMsg.nextMessageTrigger : globalNextTrigger;

                if (trigger != NextMessageTrigger.Auto && trigger != NextMessageTrigger.Button)
                {
                    waitingForTrigger = true;
                    Debug.Log($"Typing complete - now waiting for {trigger}");
                }
            }
        }
    }

    private IEnumerator DisplayDurationTimer(TextMessage message)
    {
        Debug.Log($"Display duration: waiting {message.displayDuration} seconds before accepting input");
        yield return new WaitForSeconds(message.displayDuration);

        // After display duration, enable waiting for trigger
        NextMessageTrigger trigger = message.useCustomTrigger ? message.nextMessageTrigger : globalNextTrigger;
        if (trigger != NextMessageTrigger.Auto && trigger != NextMessageTrigger.Button)
        {
            waitingForTrigger = true;
            Debug.Log($"Display duration complete - now waiting for {trigger}");
        }
    }

    // ============================================
    // TEXT EFFECTS
    // ============================================

    private IEnumerator ApplyEffect(TextEffect effect, float duration, EffectSettings settings)
    {
        switch (effect)
        {
            case TextEffect.Wave:
                yield return WaveEffect(duration, settings);
                break;
            case TextEffect.Shake:
                yield return ShakeEffect(duration, settings);
                break;
            case TextEffect.Rainbow:
                yield return RainbowEffect(duration, settings);
                break;
            case TextEffect.Pulse:
                yield return PulseEffect(duration, settings);
                break;
            case TextEffect.Bounce:
                yield return BounceEffect(duration, settings);
                break;
            case TextEffect.Fade:
                yield return FadeEffect(duration, settings);
                break;
        }
    }

    private IEnumerator WaveEffect(float duration, EffectSettings settings)
    {
        float elapsed = 0f;
        Vector3 originalPos = textComponent.transform.localPosition;

        while (elapsed < duration)
        {
            float wave = Mathf.Sin(elapsed * settings.waveSpeed) * settings.waveIntensity;
            textComponent.transform.localPosition = originalPos + new Vector3(0, wave, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textComponent.transform.localPosition = originalPos;
    }

    private IEnumerator ShakeEffect(float duration, EffectSettings settings)
    {
        float elapsed = 0f;
        Vector3 originalPos = textComponent.transform.localPosition;

        while (elapsed < duration)
        {
            float x = Random.Range(-settings.shakeIntensity, settings.shakeIntensity);
            float y = Random.Range(-settings.shakeIntensity, settings.shakeIntensity);
            textComponent.transform.localPosition = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(1f / settings.shakeSpeed);
        }

        textComponent.transform.localPosition = originalPos;
    }

    private IEnumerator RainbowEffect(float duration, EffectSettings settings)
    {
        float elapsed = 0f;
        Color originalColor = textComponent.color;

        while (elapsed < duration)
        {
            float hue = (elapsed * settings.rainbowSpeed) % 1f;
            textComponent.color = Color.HSVToRGB(hue, settings.rainbowSaturation, 1f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textComponent.color = originalColor;
    }

    private IEnumerator PulseEffect(float duration, EffectSettings settings)
    {
        float elapsed = 0f;
        Vector3 originalScale = textComponent.transform.localScale;

        while (elapsed < duration)
        {
            float scale = 1f + Mathf.Sin(elapsed * settings.pulseSpeed) * settings.pulseIntensity;
            textComponent.transform.localScale = originalScale * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        textComponent.transform.localScale = originalScale;
    }

    private IEnumerator BounceEffect(float duration, EffectSettings settings)
    {
        float elapsed = 0f;
        Vector3 originalPos = textComponent.transform.localPosition;

        while (elapsed < duration)
        {
            float bounce = Mathf.Abs(Mathf.Sin(elapsed * settings.bounceSpeed)) * settings.bounceHeight;
            textComponent.transform.localPosition = originalPos + new Vector3(0, bounce, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textComponent.transform.localPosition = originalPos;
    }

    private IEnumerator FadeEffect(float duration, EffectSettings settings)
    {
        float elapsed = 0f;
        Color originalColor = textComponent.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(settings.fadeMin, settings.fadeMax, Mathf.PingPong(elapsed * settings.fadeSpeed, 1f));
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        textComponent.color = originalColor;
    }

    // ============================================
    // INSPECTOR BUTTONS
    // ============================================

    [ContextMenu("Start Message Sequence")]
    private void TestStart()
    {
        StartMessages();
    }

    [ContextMenu("Show Next Message")]
    private void TestNext()
    {
        NextMessage();
    }

    [ContextMenu("Stop Messages")]
    private void TestStop()
    {
        StopMessages();
    }

    [ContextMenu("Trigger Special Message 0")]
    private void TestTriggerSpecial()
    {
        TriggerSpecialMessage(0);
    }

    [ContextMenu("Set Test Condition (TurnStart)")]
    private void TestSetConditionTurnStart()
    {
        SetCondition("TurnStart", true);
    }

    [ContextMenu("Set Test Condition (LowHealth)")]
    private void TestSetConditionLowHealth()
    {
        SetCondition("LowHealth", true);
    }

    [ContextMenu("Clear All Conditions")]
    private void TestClearAllConditions()
    {
        ClearAllConditions();
    }
}

// ============================================
// CLASSES AND ENUMS
// ============================================

[System.Serializable]
public class EffectSettings
{
    [Header("Wave Settings")]
    [Range(1f, 10f)] public float waveSpeed = 3f;
    [Range(1f, 20f)] public float waveIntensity = 5f;

    [Header("Shake Settings")]
    [Range(1f, 50f)] public float shakeIntensity = 3f;
    [Range(10f, 60f)] public float shakeSpeed = 30f;

    [Header("Rainbow Settings")]
    [Range(0.1f, 2f)] public float rainbowSpeed = 0.5f;
    [Range(0f, 1f)] public float rainbowSaturation = 1f;

    [Header("Pulse Settings")]
    [Range(1f, 10f)] public float pulseSpeed = 5f;
    [Range(0.05f, 0.5f)] public float pulseIntensity = 0.1f;

    [Header("Bounce Settings")]
    [Range(1f, 10f)] public float bounceSpeed = 4f;
    [Range(5f, 30f)] public float bounceHeight = 10f;

    [Header("Fade Settings")]
    [Range(0.5f, 5f)] public float fadeSpeed = 2f;
    [Range(0f, 1f)] public float fadeMin = 0.3f;
    [Range(0f, 1f)] public float fadeMax = 1f;
}

[System.Serializable]
public class TextMessage
{
    [Header("Message Content")]
    [TextArea(3, 10)]
    public string text = "Your message here...";
    public string messageName = "Message";

    [Header("Display Options")]
    public bool showOnStart = false;
    public bool useTypewriter = true;

    [Header("Display Duration")]
    [Tooltip("Wait for a set duration before accepting input to advance")]
    public bool useDisplayDuration = false;
    [Tooltip("How long to display the message before accepting input (in seconds)")]
    public float displayDuration = 2f;

    [Header("Special Message (Only shows when triggered)")]
    public bool isSpecialMessage = false;
    [Tooltip("Assign a button from your scene. When clicked, this message will show.")]
    public UnityEngine.UI.Button triggerButton;

    [Header("Final Text")]
    [Tooltip("Mark this as the last message. After dismissing it, messages will stop and text will hide.")]
    public bool isFinalText = false;

    [Header("Trigger Condition")]
    [Tooltip("When should this message appear?")]
    public TriggerCondition triggerCondition = TriggerCondition.Always;

    [Header("Next Message Trigger")]
    public bool useCustomTrigger = false;
    public NextMessageTrigger nextMessageTrigger = NextMessageTrigger.PressEnter;
    public float autoAdvanceDelay = 2f;

    [Header("Effects")]
    public TextEffect textEffect = TextEffect.None;
    public float effectDuration = 2f;
    public EffectSettings effectSettings = new EffectSettings();

    [Header("Styling")]
    public bool useCustomColor = false;
    public Color textColor = Color.white;
    public bool useCustomFontSize = false;
    public float fontSize = 36f;

    [Header("Events")]
    public UnityEvent onMessageShown;
}

[System.Serializable]
public class MessageCondition
{
    public string conditionName = "";
    public bool mustBeFalse = false;

    public override string ToString()
    {
        return mustBeFalse ? $"NOT {conditionName}" : conditionName;
    }
}

public enum TextEffect
{
    None, Wave, Shake, Rainbow, Pulse, Bounce, Fade
}

public enum NextMessageTrigger
{
    PressEnter,
    PressSpace,
    PressAnyKey,
    MouseClick,
    RightClick,
    Button,
    Auto
}

public enum TriggerCondition
{
    Always,
    OnTurnStart,
    OnTurnEnd,
    OnHit,
    OnLowHealth,
    OnPhaseChange
}