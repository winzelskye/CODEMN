using UnityEngine;
using System.Collections.Generic;

public class ScatterDragObjects : MonoBehaviour
{
    [Header("Scatter Settings")]
    [SerializeField] private List<GameObject> draggableObjects = new List<GameObject>();
    [SerializeField] private RectTransform scatterBounds;
    [SerializeField] private float edgePadding = 50f;

    [Header("Optional: Auto-find by Tag")]
    [SerializeField] private bool autoFindByTag = false;
    [SerializeField] private string draggableTag = "Draggable";

    void OnEnable()
    {
        ScatterObjects();
    }

    public void ScatterObjects()
    {
        if (scatterBounds == null)
        {
            Debug.LogWarning("Scatter bounds not assigned!");
            return;
        }

        if (autoFindByTag)
        {
            GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(draggableTag);
            draggableObjects.Clear();
            draggableObjects.AddRange(foundObjects);
        }

        foreach (GameObject dragObj in draggableObjects)
        {
            if (dragObj == null) continue;

            RectTransform dragRect = dragObj.GetComponent<RectTransform>();
            if (dragRect == null) continue;

            dragRect.SetParent(scatterBounds);
            Vector2 randomPos = GetRandomPositionInBounds(dragRect);
            dragRect.anchoredPosition = randomPos;
            dragRect.SetAsLastSibling();

            // Update DraggableWord if it exists
            DraggableWord dw = dragObj.GetComponent<DraggableWord>();
            if (dw != null)
                dw.ResetOriginalPosition(scatterBounds, randomPos);
        }

        DropTarget[] dropTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);
        foreach (DropTarget target in dropTargets)
            target.ClearDropZone();

        Debug.Log("Objects scattered!");
    }

    public void ScatterObjectsEvenly()
    {
        if (scatterBounds == null || draggableObjects.Count == 0) return;

        if (autoFindByTag)
        {
            GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(draggableTag);
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
            if (dragRect == null) continue;

            dragRect.SetParent(scatterBounds);

            int col = i % columns;
            int row = i / columns;

            float x = -scatterBounds.rect.width / 2 + edgePadding + cellWidth * col + cellWidth / 2;
            float y = scatterBounds.rect.height / 2 - edgePadding - cellHeight * row - cellHeight / 2;

            x += Random.Range(-20f, 20f);
            y += Random.Range(-20f, 20f);

            dragRect.anchoredPosition = new Vector2(x, y);
            dragRect.SetAsLastSibling();

            DraggableWord dw = draggableObjects[i].GetComponent<DraggableWord>();
            if (dw != null)
                dw.ResetOriginalPosition(scatterBounds, new Vector2(x, y));
        }

        DropTarget[] dropTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);
        foreach (DropTarget target in dropTargets)
            target.ClearDropZone();

        Debug.Log("Objects scattered evenly!");
    }

    private Vector2 GetRandomPositionInBounds(RectTransform itemRect)
    {
        float minX = -scatterBounds.rect.width / 2 + edgePadding + itemRect.rect.width / 2;
        float maxX = scatterBounds.rect.width / 2 - edgePadding - itemRect.rect.width / 2;
        float minY = -scatterBounds.rect.height / 2 + edgePadding + itemRect.rect.height / 2;
        float maxY = scatterBounds.rect.height / 2 - edgePadding - itemRect.rect.height / 2;

        return new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }
}