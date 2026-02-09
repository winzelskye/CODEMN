using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Button script for Battle Return to Menu functionality.
/// Calls BattleFlowController.OnReturnToMenuClicked()
/// </summary>
public class BattleReturnMenuButton : MonoBehaviour
{
    private Button button;
    private BattleFlowController battleFlowController;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            battleFlowController = FindObjectOfType<BattleFlowController>();
            if (battleFlowController != null)
            {
                button.onClick.AddListener(battleFlowController.OnReturnToMenuClicked);
            }
            else
            {
                Debug.LogWarning("BattleReturnMenuButton: BattleFlowController not found in scene!");
            }
        }
    }

    private void OnDestroy()
    {
        if (button != null && battleFlowController != null)
        {
            button.onClick.RemoveListener(battleFlowController.OnReturnToMenuClicked);
        }
    }
}
