using UnityEngine;

public class DebugButton : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== DebugButton script loaded! ===");
    }

    public void OnButtonClicked()
    {
        Debug.LogWarning("⚠️⚠️⚠️ BUTTON CLICKED! ⚠️⚠️⚠️");
        Debug.LogError("🔴🔴🔴 YES IT WORKS! 🔴🔴🔴");
    }
}