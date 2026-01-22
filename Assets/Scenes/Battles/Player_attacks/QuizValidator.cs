using UnityEngine;
using UnityEngine.UI;
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
        {
            doneButton.onClick.AddListener(OnDoneClicked);
        }
    }

    private void OnDoneClicked()
    {
        if (CheckAllCorrect())
        {
            // All correct - hide the popup and scatter objects
            if (popupCanvas != null)
            {
                popupCanvas.SetActive(false);
            }

            // Scatter all objects back for next round
            if (scatterScript != null)
            {
                scatterScript.ScatterObjects();
            }

            Debug.Log("Quiz completed correctly!");
        }
        else
        {
            // Something wrong - apply time penalty but DON'T scatter or close popup
            if (timerComponent != null)
            {
                float currentTime = timerComponent.GetCurrentTime();
                float newTime = Mathf.Max(0, currentTime - timePenalty);
                timerComponent.SetTime(newTime);
                timerComponent.StartTimer();
            }
            Debug.Log("Incorrect! Time penalty applied.");
        }
    }

    private bool CheckAllCorrect()
    {
        foreach (DropZoneAnswer answer in answers)
        {
            if (answer.dropZone == null || answer.correctItem == null)
                continue;

            // Check if the correct item is a child of this drop zone
            Transform itemParent = answer.correctItem.transform.parent;
            if (itemParent != answer.dropZone)
            {
                Debug.Log($"{answer.correctItem.name} is not in {answer.dropZone.name}");
                return false;
            }
        }

        // Check if any drop zones are empty
        foreach (DropZoneAnswer answer in answers)
        {
            if (answer.dropZone != null && answer.dropZone.childCount == 0)
            {
                Debug.Log($"{answer.dropZone.name} is empty");
                return false;
            }
        }

        return true;
    }

    private void OnDestroy()
    {
        if (doneButton != null)
        {
            doneButton.onClick.RemoveListener(OnDoneClicked);
        }
    }
}