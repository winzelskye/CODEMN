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

    private float currentTime;
    private bool isRunning = true;
    private bool preventReset = false;

    void Start()
    {
        currentTime = countUp ? 0 : startTime;
        isRunning = true;
        UpdateTimerDisplay();
    }

    void OnEnable()
    {
        currentTime = countUp ? 0 : startTime;
        isRunning = true;
        preventReset = false;
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

    public void StartTimer() { isRunning = true; }
    public void StopTimer() { isRunning = false; }

    public void ResetTimer()
    {
        if (preventReset)
        {
            Debug.Log("ResetTimer BLOCKED");
            return;
        }
        isRunning = true;
        UpdateTimerDisplay();
    }

    public void ApplyTimePenalty(float penalty)
    {
        preventReset = true;

        Debug.Log($"Penalty applied: {currentTime} -> {currentTime - penalty}");
        currentTime = Mathf.Max(0, currentTime - penalty);
        UpdateTimerDisplay();

        if (!countUp && currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            OnTimerComplete();
        }

        Invoke(nameof(UnblockReset), 0.5f);
    }

    private void UnblockReset()
    {
        preventReset = false;
        Debug.Log("Reset unblocked");
    }

    private void OnTimerComplete()
    {
        if (popupCanvas != null)
            popupCanvas.SetActive(false);

        FindFirstObjectByType<AttackListManager>()?.ShowBattleUI();
        BattleManager.Instance.OnPlayerAttackResult(false, false);
        Debug.Log("Timer completed!");
    }

    public float GetCurrentTime() { return currentTime; }
    public bool IsRunning() { return isRunning; }

    private void OnDestroy()
    {
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetTimer);
    }
}