using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerComponent : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject popupCanvas;
    [SerializeField] private Button resetButton;

    [Header("Timer Settings")]
    [SerializeField] private float startTime = 20f;
    [SerializeField] private bool countUp = false;

    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private float warningThreshold = 10f;
    [SerializeField] private float dangerThreshold = 5f;

    [Header("Scatter Reference")]
    [SerializeField] private ScatterDragObjects scatterScript;

    private float currentTime;
    private bool isRunning = true;
    private bool preventReset = false;

    void Start()
    {
        currentTime = countUp ? 0 : startTime;
        UpdateTimerDisplay();

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetTimer);
        }
    }

    void Update()
    {
        if (isRunning)
        {
            if (countUp)
            {
                currentTime += Time.deltaTime;
            }
            else
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0)
                {
                    currentTime = 0;
                    isRunning = false;
                    OnTimerComplete();
                }
            }

            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(currentTime).ToString();

            if (!countUp)
            {
                if (currentTime <= dangerThreshold)
                    timerText.color = dangerColor;
                else if (currentTime <= warningThreshold)
                    timerText.color = warningColor;
                else
                    timerText.color = normalColor;
            }
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        // Block reset if we're applying a penalty
        if (preventReset)
        {
            Debug.Log("ResetTimer BLOCKED - penalty in progress");
            return;
        }

        Debug.Log("ResetTimer executed");
        currentTime = countUp ? 0 : startTime;
        isRunning = true;
        UpdateTimerDisplay();

        // Scatter objects when timer resets
        if (scatterScript != null)
        {
            scatterScript.ScatterObjects();
        }
    }

    public void ApplyTimePenalty(float penalty)
    {
        preventReset = true; // Block any ResetTimer calls

        Debug.Log($"Penalty applied: {currentTime} -> {currentTime - penalty}");
        currentTime = Mathf.Max(0, currentTime - penalty);
        UpdateTimerDisplay();

        // Check if penalty brought timer to 0
        if (!countUp && currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            OnTimerComplete();
        }

        // Use Invoke to unblock reset after a short delay (to ensure scatter completes)
        Invoke(nameof(UnblockReset), 0.1f);
    }

    private void UnblockReset()
    {
        preventReset = false;
        Debug.Log("Reset unblocked");
    }

    private void OnTimerComplete()
    {
        if (popupCanvas != null)
        {
            popupCanvas.SetActive(false);
        }

        BattleManager.Instance.OnPlayerAttackResult(false, false);
        Debug.Log("Timer completed!");
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    private void OnDestroy()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(ResetTimer);
        }
    }
}