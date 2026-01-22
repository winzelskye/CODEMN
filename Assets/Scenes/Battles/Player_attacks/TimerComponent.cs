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

    void Start()
    {
        currentTime = countUp ? 0 : startTime;
        UpdateTimerDisplay();

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetTimer);
        }
    }

    void OnEnable()
    {
        // Reset timer every time popup becomes visible
        currentTime = countUp ? 0 : startTime;
        isRunning = true;
        UpdateTimerDisplay();
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
        currentTime = countUp ? 0 : startTime;
        isRunning = true;
        UpdateTimerDisplay();

        // Scatter objects when timer resets
        if (scatterScript != null)
        {
            scatterScript.ScatterObjects();
        }
    }

    public void SetTime(float time)
    {
        currentTime = time;
        UpdateTimerDisplay();
    }

    private void OnTimerComplete()
    {
        if (popupCanvas != null)
        {
            popupCanvas.SetActive(false);
        }
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