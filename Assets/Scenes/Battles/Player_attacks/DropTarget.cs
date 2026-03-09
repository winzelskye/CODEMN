using UnityEngine;
using UnityEngine.EventSystems;
public class DropTarget : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        DragObject draggedItem = eventData.pointerDrag.GetComponent<DragObject>();
        if (draggedItem == null) return;

        RectTransform draggedRect = draggedItem.GetComponent<RectTransform>();
        RectTransform dropRect = GetComponent<RectTransform>();
        draggedRect.SetParent(dropRect);
        draggedRect.anchoredPosition = Vector2.zero;
        draggedRect.SetAsLastSibling();
    }

    public void ClearDropZone() { }

    public void RemoveItem(DragObject item) { }
}