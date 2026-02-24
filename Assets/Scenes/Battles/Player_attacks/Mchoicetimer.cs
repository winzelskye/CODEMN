using UnityEngine;
using TMPro;

/// <summary>
/// MChoiceTimer - Dedicated countdown timer for the MChoice panel.
/// Attach to the MChoicePanel alongside MChoiceUI.
/// </summary>
public class MChoiceTimer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 15f;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 8f;
    [SerializeField] private float dangerThreshold = 4f;

    // ── Runtime ──────────────────────────────────────────────
    private float currentTime;
    private bool isRunning = false;
    private System.Action onTimeUp;

    // ─────────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Fully resets and starts the timer fresh every call.
    /// </summary>
    public void StartTimer(System.Action timeUpCallback)
    {
        onTimeUp = timeUpCallback;
        currentTime = startTime;
        isRunning = true;
        UpdateDisplay();
    }

    public void StopTimer()
    {
        isRunning = false;
        onTimeUp = null; // clear stale callback
    }

    public float GetCurrentTime() => currentTime;
    public bool IsRunning() => isRunning;

    // ─────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────

    void OnEnable()
    {
        // Always fully reset when panel is activated
        onTimeUp = null;
        isRunning = false;
        currentTime = startTime;
        UpdateDisplay();
    }

    void OnDisable()
    {
        // Clean stop whenever panel hides
        isRunning = false;
        onTimeUp = null;
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            UpdateDisplay();
            onTimeUp?.Invoke();
            onTimeUp = null; // prevent double firing
            return;
        }

        UpdateDisplay();
    }

    // ─────────────────────────────────────────────────────────
    // INTERNAL
    // ─────────────────────────────────────────────────────────

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (currentTime <= dangerThreshold)
            timerText.color = dangerColor;
        else if (currentTime <= warningThreshold)
            timerText.color = warningColor;
        else
            timerText.color = normalColor;
    }
}