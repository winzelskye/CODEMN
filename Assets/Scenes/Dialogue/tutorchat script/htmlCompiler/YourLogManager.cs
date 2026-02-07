using TMPro;
using UnityEngine;

public class YourLogManager : MonoBehaviour
{
    public HTMLCodingSystem htmlSystem;
    public TextMeshProUGUI logText;

    void Start()
    {
        // Subscribe to events
        htmlSystem.OnCodeExecuted += LogCodeExecution;
        htmlSystem.OnError += LogError;
        htmlSystem.OnSystemCleared += LogClear;
    }

    void LogCodeExecution(string code)
    {
        logText.text += $"\n[{System.DateTime.Now:HH:mm:ss}] Executed: {code}";
    }

    void LogError(string error)
    {
        logText.text += $"\n[{System.DateTime.Now:HH:mm:ss}] ERROR: {error}";
    }

    void LogClear()
    {
        logText.text += $"\n[{System.DateTime.Now:HH:mm:ss}] System cleared";
    }
}