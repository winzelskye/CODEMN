using UnityEngine;
using UnityEngine.EventSystems;

public class DropTarget : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        DragObject draggedItem = eventData.pointerDrag.GetComponent<DragObject>();
        if (draggedItem != null)
        {
            // Snap the dragged item to the drop target's position
            RectTransform draggedRectTransform = draggedItem.GetComponent<RectTransform>();
            RectTransform dropTargetRectTransform = GetComponent<RectTransform>();

            // IMPORTANT: Make the dragged item a child of this drop zone
            draggedRectTransform.SetParent(this.transform);

            // Set the dragged item's position to the drop target's position
            draggedRectTransform.anchoredPosition = Vector2.zero; // Center it in the drop zone

            // Optionally: you can perform other actions here, such as disabling the original dragged item
            // draggedItem.gameObject.SetActive(false);
        }
    }
}