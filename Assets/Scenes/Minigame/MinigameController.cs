using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Runs a quick-time minigame: show prompt and countdown; success if player presses the key in time.
/// Notifies BattleController via OnMinigameCompleted(success).
/// </summary>
public class MinigameController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text timerText;

    [Header("Default Config")]
    [SerializeField] private MinigameConfig defaultConfig = new MinigameConfig();

    private float timeRemaining;
    private bool running;
    private MinigameConfig currentConfig;

    /// <summary>Invoked when minigame ends: true = success (key pressed in time), false = timeout or failed.</summary>
    public event Action<bool> OnMinigameCompleted;

    private void Update()
    {
        if (!running) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (Input.GetKeyDown(currentConfig.keyToPress))
        {
            FinishMinigame(success: true);
            return;
        }

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            FinishMinigame(success: false);
        }
    }

    /// <summary>Start the minigame with optional config. Shows panel and begins countdown.</summary>
    public void StartMinigame(MinigameConfig config = null)
    {
        currentConfig = config ?? defaultConfig;
        timeRemaining = currentConfig.duration;
        running = true;

        if (minigamePanel == null)
            EnsureRuntimePanel();
        if (minigamePanel != null)
            minigamePanel.SetActive(true);
        if (promptText != null)
            promptText.text = currentConfig.promptText;
        UpdateTimerDisplay();
    }

    private void EnsureRuntimePanel()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        minigamePanel = new GameObject("MinigamePanel");
        minigamePanel.transform.SetParent(canvas.transform, false);
        var rect = minigamePanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0.35f);
        rect.anchorMax = new Vector2(0.75f, 0.65f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = minigamePanel.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        promptText = CreateTmpText(minigamePanel.transform, "Press the key!", new Vector2(0.5f, 0.6f));
        timerText = CreateTmpText(minigamePanel.transform, "3", new Vector2(0.5f, 0.4f));
    }

    private static TMPro.TMP_Text CreateTmpText(Transform parent, string content, Vector2 anchorY)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, anchorY.y - 0.1f);
        rect.anchorMax = new Vector2(1f, anchorY.y + 0.1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = 36;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        return tmp;
    }

    /// <summary>Stop the minigame and hide the panel without firing the event (e.g. battle ended).</summary>
    public void StopMinigame()
    {
        running = false;
        if (minigamePanel != null)
            minigamePanel.SetActive(false);
    }

    private void FinishMinigame(bool success)
    {
        if (!running) return;
        running = false;
        if (minigamePanel != null)
            minigamePanel.SetActive(false);
        OnMinigameCompleted?.Invoke(success);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
            timerText.text = Mathf.Ceil(Mathf.Max(0, timeRemaining)).ToString();
    }
}
