using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    private DragObject currentItem = null;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DragObject draggedItem = eventData.pointerDrag.GetComponent<DragObject>();
        if (draggedItem == null) return;

        if (currentItem != null && currentItem != draggedItem)
        {
            draggedItem.ReturnToOriginalPosition();
            return;
        }

        RectTransform draggedRect = draggedItem.GetComponent<RectTransform>();
        RectTransform dropRect = GetComponent<RectTransform>();

        draggedRect.SetParent(dropRect);
        draggedRect.anchoredPosition = Vector2.zero;
        draggedRect.SetAsLastSibling();

        currentItem = draggedItem;
    }

    public void ClearDropZone()
    {
        currentItem = null;
    }

    public void RemoveItem(DragObject item)
    {
        if (currentItem == item)
            currentItem = null;
    }
}