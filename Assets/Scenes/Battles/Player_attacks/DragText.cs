using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private Vector2 originalPosition;
    private Transform originalParent;
    private int originalSiblingIndex;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent = rectTransform.parent;
        originalSiblingIndex = rectTransform.GetSiblingIndex();

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        rectTransform.SetParent(canvas.transform);
        rectTransform.SetAsLastSibling();

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out mousePos);
        offset = rectTransform.anchoredPosition - mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out mousePos);
        rectTransform.anchoredPosition = mousePos + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (rectTransform.parent == canvas.transform)
        {
            rectTransform.SetParent(originalParent);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }

        transform.SetAsLastSibling();
    }

    public void ReturnToOriginalPosition()
    {
        if (originalParent != null)
        {
            rectTransform.SetParent(originalParent);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
        }
        rectTransform.anchoredPosition = originalPosition;
    }

    public void ResetOriginalPosition(Transform newParent, Vector2 newPosition)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        originalParent = newParent;
        originalPosition = newPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
    }
}