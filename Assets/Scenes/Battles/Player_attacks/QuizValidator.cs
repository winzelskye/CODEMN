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

        // Try to auto-find TimerComponent if not assigned
        if (timerComponent == null && popupCanvas != null)
        {
            timerComponent = popupCanvas.GetComponentInChildren<TimerComponent>();
        }
    }

    private void OnDoneClicked()
    {
        if (CheckAllCorrect())
        {
            // All correct - hide the popup
            if (popupCanvas != null)
            {
                popupCanvas.SetActive(false);
            }

            Debug.Log("Quiz completed correctly!");
        }
        else
        {
            // Wrong answer - apply time penalty FIRST
            if (timerComponent != null)
            {
                timerComponent.ApplyTimePenalty(timePenalty);
            }

            // THEN scatter all objects
            if (scatterScript != null)
            {
                scatterScript.ScatterObjects();
            }

            Debug.Log("Incorrect! Time penalty applied and objects scattered.");
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
                return false;
            }
        }

        // Check if any drop zones are empty
        foreach (DropZoneAnswer answer in answers)
        {
            if (answer.dropZone != null && answer.dropZone.childCount == 0)
            {
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