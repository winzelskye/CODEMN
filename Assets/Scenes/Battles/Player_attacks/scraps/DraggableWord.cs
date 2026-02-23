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
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        ResetCanvasGroup();
    }

    void OnDisable()
    {
        if (isDragging)
        {
            isDragging = false;
            ResetCanvasGroup();
        }
    }

    private void ResetCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    // Call this after scatter to update stored position
    public void ResetOriginalPosition(Transform newParent, Vector2 newPosition)
    {
        startParent = newParent;
        startPosition = newPosition;
    }

    public void SetText(string text)
    {
        wordText = text;
        TMPro.TextMeshProUGUI tmpText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.text = text;
        else
        {
            Text regularText = GetComponentInChildren<Text>();
            if (regularText != null)
                regularText.text = text;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Always refresh canvas reference
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;
        }

        isDragging = true;
        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;

        if (currentSlot != null)
        {
            currentSlot.currentWord = null;
            currentSlot = null;
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        ResetCanvasGroup();

        if (currentSlot == null)
        {
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
        }

        transform.SetAsLastSibling();
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