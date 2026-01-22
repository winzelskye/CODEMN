using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoundaryConstraint : MonoBehaviour, IDragHandler
{
    [SerializeField] private RectTransform boundary;
    [SerializeField] private float padding = 50f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        ApplyBoundary();
    }

    void LateUpdate()
    {
        // Continuously apply boundary
        if (Input.GetMouseButton(0))
        {
            ApplyBoundary();
        }
    }

    private void ApplyBoundary()
    {
        if (boundary == null) return;

        // Get current position relative to boundary
        Vector2 localPos = rectTransform.anchoredPosition;

        // Calculate bounds
        float minX = -boundary.rect.width / 2 + padding;
        float maxX = boundary.rect.width / 2 - padding;
        float minY = -boundary.rect.height / 2 + padding;
        float maxY = boundary.rect.height / 2 - padding;

        // Clamp position
        localPos.x = Mathf.Clamp(localPos.x, minX, maxX);
        localPos.y = Mathf.Clamp(localPos.y, minY, maxY);

        rectTransform.anchoredPosition = localPos;
    }
}