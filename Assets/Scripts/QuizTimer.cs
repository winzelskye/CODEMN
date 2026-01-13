using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class QuizTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float startTime = 60f;
    public bool countDown = true;
    public bool autoStart = true;

    [Header("Display Settings")]
    public TextMeshProUGUI timerText;
    public string timeFormat = "{0:00}:{1:00}"; // MM:SS
    public bool showMilliseconds = false;

    [Header("Color Settings")]
    public bool useColorWarnings = true;
    public float warningTime = 30f;
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public float criticalTime = 10f;

    [Header("Events")]
    public UnityEvent onTimerEnd;
    public UnityEvent onWarningTime;
    public UnityEvent onCriticalTime;

    [Header("Canvas Control")]
    public GameObject targetCanvas;
    public bool hideCanvasOnEnd = false;

    private float currentTime;
    private bool isRunning;
    private bool warningTriggered;
    private bool criticalTriggered;

    void Start()
    {
        currentTime = startTime;

        if (autoStart)
            StartTimer();
        else
            UpdateDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        // Update time
        if (countDown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                OnTimerEnd();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }

        UpdateDisplay();
        CheckWarnings();
    }

    void UpdateDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        int milliseconds = Mathf.FloorToInt((currentTime * 100) % 100);

        if (showMilliseconds)
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        else
            timerText.text = string.Format(timeFormat, minutes, seconds);

        UpdateColor();
    }

    void UpdateColor()
    {
        if (!useColorWarnings || timerText == null || !countDown) return;

        if (currentTime <= criticalTime)
            timerText.color = criticalColor;
        else if (currentTime <= warningTime)
            timerText.color = warningColor;
        else
            timerText.color = normalColor;
    }

    void CheckWarnings()
    {
        if (!countDown) return;

        if (!warningTriggered && currentTime <= warningTime)
        {
            warningTriggered = true;
            onWarningTime?.Invoke();
        }

        if (!criticalTriggered && currentTime <= criticalTime)
        {
            criticalTriggered = true;
            onCriticalTime?.Invoke();
        }
    }

    void OnTimerEnd()
    {
        isRunning = false;
        onTimerEnd?.Invoke();

        if (hideCanvasOnEnd && targetCanvas != null)
        {
            targetCanvas.SetActive(false);
        }
    }

    // Public methods to control timer
    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public void ResetTimer()
    {
        currentTime = startTime;
        isRunning = false;
        warningTriggered = false;
        criticalTriggered = false;
        UpdateDisplay();
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        if (currentTime < 0) currentTime = 0;
    }

    public void SetTime(float seconds)
    {
        currentTime = seconds;
        if (currentTime < 0) currentTime = 0;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}