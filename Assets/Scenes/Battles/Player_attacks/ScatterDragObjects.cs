using UnityEngine;
using System.Collections.Generic;

public class ScatterDragObjects : MonoBehaviour
{
    [Header("Scatter Settings")]
    [SerializeField] private List<DragObject> draggableObjects = new List<DragObject>();
    [SerializeField] private RectTransform scatterBounds;
    [SerializeField] private float edgePadding = 50f;

    [Header("Optional: Auto-find DragObjects")]
    [SerializeField] private bool autoFindDragObjects = false;

    public void ScatterObjects()
    {
        if (scatterBounds == null)
        {
            Debug.LogWarning("Scatter bounds not assigned!");
            return;
        }

        if (autoFindDragObjects)
        {
            DragObject[] foundObjects = FindObjectsByType<DragObject>(FindObjectsSortMode.None);
            draggableObjects.Clear();
            draggableObjects.AddRange(foundObjects);
        }

        foreach (DragObject dragObj in draggableObjects)
        {
            if (dragObj == null) continue;

            RectTransform dragRect = dragObj.GetComponent<RectTransform>();
            dragRect.SetParent(scatterBounds);
            Vector2 randomPos = GetRandomPositionInBounds(dragRect);
            dragRect.anchoredPosition = randomPos;
            dragRect.SetAsLastSibling();

            // Reset stored original position to scatter bounds
            dragObj.ResetOriginalPosition(scatterBounds, randomPos);
        }

        DropTarget[] dropTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);
        foreach (DropTarget target in dropTargets)
            target.ClearDropZone();

        Debug.Log("Objects scattered!");
    }

    private Vector2 GetRandomPositionInBounds(RectTransform itemRect)
    {
        float minX = -scatterBounds.rect.width / 2 + edgePadding + itemRect.rect.width / 2;
        float maxX = scatterBounds.rect.width / 2 - edgePadding - itemRect.rect.width / 2;
        float minY = -scatterBounds.rect.height / 2 + edgePadding + itemRect.rect.height / 2;
        float maxY = scatterBounds.rect.height / 2 - edgePadding - itemRect.rect.height / 2;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector2(randomX, randomY);
    }

    public void ScatterObjectsEvenly()
    {
        if (scatterBounds == null || draggableObjects.Count == 0) return;

        if (autoFindDragObjects)
        {
            DragObject[] foundObjects = FindObjectsByType<DragObject>(FindObjectsSortMode.None);
            draggableObjects.Clear();
            draggableObjects.AddRange(foundObjects);
        }

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

            x += Random.Range(-20f, 20f);
            y += Random.Range(-20f, 20f);

            dragRect.anchoredPosition = new Vector2(x, y);
            dragRect.SetAsLastSibling();

            draggableObjects[i].ResetOriginalPosition(scatterBounds, new Vector2(x, y));
        }

        DropTarget[] dropTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);
        foreach (DropTarget target in dropTargets)
            target.ClearDropZone();

        Debug.Log("Objects scattered evenly!");
    }
}