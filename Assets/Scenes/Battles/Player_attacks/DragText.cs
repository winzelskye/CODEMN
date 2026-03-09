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

    [Header("Boundary")]
    [Tooltip("Assign the RectTransform that defines the drag boundary. Leave empty for no boundary.")]
    public RectTransform boundaryRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }
    }

    private Canvas GetCanvas()
    {
        Canvas c = GetComponentInParent<Canvas>();
        if (c == null)
            c = FindObjectOfType<Canvas>();
        return c;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvas = GetCanvas();
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

        Vector2 targetPos = mousePos + offset;

        if (boundaryRect != null)
            targetPos = ClampToBoundary(targetPos);

        rectTransform.anchoredPosition = targetPos;
    }

    private Vector2 ClampToBoundary(Vector2 targetPos)
    {
        // Get boundary corners in canvas local space
        Vector3[] boundaryCorners = new Vector3[4];
        boundaryRect.GetWorldCorners(boundaryCorners);

        // Get dragged object corners offset from its pivot
        Vector2 sizeDelta = rectTransform.rect.size;
        Vector2 pivot = rectTransform.pivot;

        // Convert boundary world corners to canvas local space
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 minBoundary = canvasRect.InverseTransformPoint(boundaryCorners[0]);
        Vector2 maxBoundary = canvasRect.InverseTransformPoint(boundaryCorners[2]);

        // Calculate how much space the object's pivot offsets the edges
        float minX = minBoundary.x + sizeDelta.x * pivot.x;
        float maxX = maxBoundary.x - sizeDelta.x * (1f - pivot.x);
        float minY = minBoundary.y + sizeDelta.y * pivot.y;
        float maxY = maxBoundary.y - sizeDelta.y * (1f - pivot.y);

        return new Vector2(
            Mathf.Clamp(targetPos.x, minX, maxX),
            Mathf.Clamp(targetPos.y, minY, maxY)
        );
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
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

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