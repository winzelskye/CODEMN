using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private Vector2 originalPosition; // Store original position
    private Transform originalParent; // Store original parent
    private int originalSiblingIndex; // Store original sibling index

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the original position and parent before dragging
        originalPosition = rectTransform.anchoredPosition;
        originalParent = rectTransform.parent;
        originalSiblingIndex = rectTransform.GetSiblingIndex();

        canvasGroup.alpha = 0.8f; // Make the item slightly transparent while dragging
        canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through the item

        // Move to canvas root so it appears above everything
        rectTransform.SetParent(canvas.transform);
        rectTransform.SetAsLastSibling(); // Bring to front

        // Calculate offset between mouse position and object position
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
        // Convert screen position to canvas position and apply offset
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
        canvasGroup.alpha = 1f; // Restore the item's opacity
        canvasGroup.blocksRaycasts = true; // Re-enable raycasts

        // If not dropped in a valid drop zone, return to original parent and position
        if (rectTransform.parent == canvas.transform)
        {
            rectTransform.SetParent(originalParent);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // Method to return to original position
    public void ReturnToOriginalPosition()
    {
        if (originalParent != null)
        {
            rectTransform.SetParent(originalParent);
            rectTransform.SetSiblingIndex(originalSiblingIndex);
        }
        rectTransform.anchoredPosition = originalPosition;
    }
}