using UnityEngine;

public class DragBoundary : MonoBehaviour
{
    [SerializeField] private RectTransform boundary;
    [SerializeField] private float padding = 1f;
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void LateUpdate()
    {
        if (canvas == null) return;
        if (rectTransform.parent == canvas.transform)
            ApplyBoundary();
    }

    private void ApplyBoundary()
    {
        if (boundary == null) return;

        Vector2 pos = rectTransform.anchoredPosition;
        float minX = boundary.anchoredPosition.x - boundary.rect.width / 2 + padding;
        float maxX = boundary.anchoredPosition.x + boundary.rect.width / 2 - padding;
        float minY = boundary.anchoredPosition.y - boundary.rect.height / 2 + padding;
        float maxY = boundary.anchoredPosition.y + boundary.rect.height / 2 - padding;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        rectTransform.anchoredPosition = pos;
    }
}