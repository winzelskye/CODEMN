using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    private DragObject currentItem = null; // Track the current item in this drop zone

    public void OnDrop(PointerEventData eventData)
    {
        DragObject draggedItem = eventData.pointerDrag.GetComponent<DragObject>();
        if (draggedItem != null)
        {
            RectTransform draggedRectTransform = draggedItem.GetComponent<RectTransform>();
            RectTransform dropTargetRectTransform = GetComponent<RectTransform>();

            // Check if there's already an item in this drop zone
            if (currentItem != null && currentItem != draggedItem)
            {
                // Eject the dragged item back to its original position
                draggedItem.ReturnToOriginalPosition();
                return; // Don't allow the drop
            }

            // Set the dragged item as a child of the drop target
            draggedRectTransform.SetParent(dropTargetRectTransform);

            // Set the dragged item's position to the drop target's position
            draggedRectTransform.anchoredPosition = Vector2.zero; // Use zero since it's now a child

            // Store this as the current item
            currentItem = draggedItem;
        }
    }

    // Optional: Call this method to clear the drop zone
    public void ClearDropZone()
    {
        currentItem = null;
    }

    // Optional: Call this to remove a specific item
    public void RemoveItem(DragObject item)
    {
        if (currentItem == item)
        {
            currentItem = null;
        }
    }
}