using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button script for Battle Retry functionality.
/// Calls BattleFlowController.OnRetryClicked() or BattleController.ButtonRestart()
/// </summary>
public class BattleRetryButton : MonoBehaviour
{
    private Button button;
    private BattleFlowController battleFlowController;
    private BattleController battleController;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            battleFlowController = FindObjectOfType<BattleFlowController>();
            battleController = FindObjectOfType<BattleController>();

            if (battleFlowController != null)
            {
                button.onClick.AddListener(battleFlowController.OnRetryClicked);
            }
            else if (battleController != null)
            {
                button.onClick.AddListener(battleController.ButtonRestart);
            }
            else
            {
                Debug.LogWarning("BattleRetryButton: Neither BattleFlowController nor BattleController found!");
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            if (battleFlowController != null)
                button.onClick.RemoveListener(battleFlowController.OnRetryClicked);
            if (battleController != null)
                button.onClick.RemoveListener(battleController.ButtonRestart);
        }
    }
}
