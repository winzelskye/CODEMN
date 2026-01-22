using UnityEngine;
using System.Collections.Generic;

public class ScatterDragObjects : MonoBehaviour
{
    [Header("Scatter Settings")]
    [SerializeField] private List<DragObject> draggableObjects = new List<DragObject>();
    [SerializeField] private RectTransform scatterBounds; // The boundary area (e.g., your main canvas or a panel)
    [SerializeField] private float edgePadding = 50f; // Padding from edges

    [Header("Optional: Auto-find DragObjects")]
    [SerializeField] private bool autoFindDragObjects = true;

    private void Start()
    {
        // Auto-find all DragObject components if enabled
        if (autoFindDragObjects)
        {
            DragObject[] foundObjects = FindObjectsByType<DragObject>(FindObjectsSortMode.None);
            draggableObjects.Clear();
            draggableObjects.AddRange(foundObjects);
        }
    }

    public void ScatterObjects()
    {
        if (scatterBounds == null)
        {
            Debug.LogWarning("Scatter bounds not assigned!");
            return;
        }

        foreach (DragObject dragObj in draggableObjects)
        {
            if (dragObj == null) continue;

            RectTransform dragRect = dragObj.GetComponent<RectTransform>();

            // Reset parent to canvas or scatter area
            dragRect.SetParent(scatterBounds);

            // Get random position within bounds
            Vector2 randomPos = GetRandomPositionInBounds(dragRect);
            dragRect.anchoredPosition = randomPos;
        }

        // Clear all drop zones
        DropTarget[] dropTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);
        foreach (DropTarget target in dropTargets)
        {
            target.ClearDropZone();
        }

        Debug.Log("Objects scattered!");
    }

    private Vector2 GetRandomPositionInBounds(RectTransform itemRect)
    {
        // Get the bounds of the scatter area
        float minX = -scatterBounds.rect.width / 2 + edgePadding + itemRect.rect.width / 2;
        float maxX = scatterBounds.rect.width / 2 - edgePadding - itemRect.rect.width / 2;
        float minY = -scatterBounds.rect.height / 2 + edgePadding + itemRect.rect.height / 2;
        float maxY = scatterBounds.rect.height / 2 - edgePadding - itemRect.rect.height / 2;

        // Generate random position
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector2(randomX, randomY);
    }

    // Optional: Scatter to specific positions (evenly distributed)
    public void ScatterObjectsEvenly()
    {
        if (scatterBounds == null || draggableObjects.Count == 0)
            return;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(draggableObjects.Count));
        int rows = Mathf.CeilToInt((float)draggableObjects.Count / columns);

        float cellWidth = (scatterBounds.rect.width - edgePadding * 2) / columns;
        float cellHeight = (scatterBounds.rect.height - edgePadding * 2) / rows;

        for (int i = 0; i < draggableObjects.Count; i++)
        {
            if (draggableObjects[i] == null) continue;

            RectTransform dragRect = draggableObjects[i].GetComponent<RectTransform>();
            dragRect.SetParent(scatterBounds);

            int col = i % columns;
            int row = i / columns;

            float x = -scatterBounds.rect.width / 2 + edgePadding + cellWidth * col + cellWidth / 2;
            float y = scatterBounds.rect.height / 2 - edgePadding - cellHeight * row - cellHeight / 2;

            // Add small random offset for natural look
            x += Random.Range(-20f, 20f);
            y += Random.Range(-20f, 20f);

            dragRect.anchoredPosition = new Vector2(x, y);
        }

        // Clear all drop zones
        DropTarget[] dropTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);
        foreach (DropTarget target in dropTargets)
        {
            target.ClearDropZone();
        }

        Debug.Log("Objects scattered evenly!");
    }
}