using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableWord : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [TextArea(1, 5)]
    public string wordText;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform startParent;
    private Vector2 startPosition;
    private WordSlot currentSlot;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetText(string text)
    {
        wordText = text;

        TMPro.TextMeshProUGUI tmpText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = text;
        }
        else
        {
            Text regularText = GetComponentInChildren<Text>();
            if (regularText != null)
            {
                regularText.text = text;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;

        // Clear from slot if in one
        if (currentSlot != null)
        {
            currentSlot.currentWord = null;
            currentSlot = null;
        }

        // Make transparent and non-blocking
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        // Move to top level
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // If not in a slot, return to start
        if (currentSlot == null)
        {
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
        }
    }

    public void PlaceInSlot(WordSlot slot)
    {
        currentSlot = slot;
        transform.SetParent(slot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    public void ReturnToStart()
    {
        currentSlot = null;
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
    }
}