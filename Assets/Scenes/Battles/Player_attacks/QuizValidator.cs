using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DropZoneAnswer
{
    public Transform dropZone;
    public GameObject correctItem;
}

public class QuizValidator : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupCanvas;
    [SerializeField] private Button doneButton;
    [SerializeField] private TimerComponent timerComponent;

    [Header("Scatter Reference")]
    [SerializeField] private ScatterDragObjects scatterScript;

    [Header("Drop Zone Answers")]
    [SerializeField] private List<DropZoneAnswer> answers = new List<DropZoneAnswer>();

    [Header("Penalty Settings")]
    [SerializeField] private float timePenalty = 5f;

    private void Start()
    {
        if (doneButton != null)
            doneButton.onClick.AddListener(OnDoneClicked);

        if (timerComponent == null && popupCanvas != null)
            timerComponent = popupCanvas.GetComponentInChildren<TimerComponent>();
    }

    private void OnDoneClicked()
    {
        StartCoroutine(CheckWithDelay());
    }

    private IEnumerator CheckWithDelay()
    {
        yield return new WaitForEndOfFrame();

        if (CheckAllCorrect())
        {
            if (popupCanvas != null)
                popupCanvas.SetActive(false);

            FindFirstObjectByType<AttackListManager>()?.ShowBattleUI();
            BattleManager.Instance.OnPlayerAttackResult(true, false);
            Debug.Log("Quiz completed correctly!");
        }
        else
        {
            if (timerComponent != null)
                timerComponent.ApplyTimePenalty(timePenalty);

            if (scatterScript != null)
                scatterScript.ScatterObjects();

            Debug.Log("Incorrect! Time penalty applied.");
        }
    }

    private bool CheckAllCorrect()
    {
        foreach (DropZoneAnswer answer in answers)
        {
            if (answer.dropZone == null || answer.correctItem == null)
                continue;

            Transform parent = answer.correctItem.transform.parent;
            bool found = false;

            while (parent != null)
            {
                if (parent == answer.dropZone)
                {
                    found = true;
                    break;
                }
                parent = parent.parent;
            }

            if (!found)
            {
                Debug.Log($"FAILED: {answer.correctItem.name} parent is {answer.correctItem.transform.parent?.name}, expected under {answer.dropZone.name}");
                return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        if (doneButton != null)
            doneButton.onClick.RemoveListener(OnDoneClicked);
    }
}