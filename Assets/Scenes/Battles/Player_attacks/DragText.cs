using UnityEngine;
using UnityEngine.EventSystems;

public class DragObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;

    [Header("Boundary Settings")]
    [SerializeField] private RectTransform boundaryRect; // Assign your popup canvas here

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.8f; // Make the item slightly transparent while dragging
        canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through the item
        transform.SetAsLastSibling(); // Bring to front

        // Calculate offset between mouse position and object center
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 mousePos);
        offset = rectTransform.anchoredPosition - mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert screen position to canvas position and apply offset
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 mousePos);

        Vector2 newPosition = mousePos + offset;

        // Clamp position within boundary if boundary is set
        if (boundaryRect != null)
        {
            newPosition = ClampToBoundary(newPosition);
        }

        rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // Restore the item's opacity
        canvasGroup.blocksRaycasts = true; // Re-enable raycasts
    }

    private Vector2 ClampToBoundary(Vector2 position)
    {
        // Get the boundaries of the boundary rect
        Vector2 minPosition = boundaryRect.rect.min - rectTransform.rect.min;
        Vector2 maxPosition = boundaryRect.rect.max - rectTransform.rect.max;

        // Clamp the position
        position.x = Mathf.Clamp(position.x, minPosition.x, maxPosition.x);
        position.y = Mathf.Clamp(position.y, minPosition.y, maxPosition.y);

        return position;
    }
}