using UnityEngine;
using UnityEngine.EventSystems;

public class ItemClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private ItemDisplay itemDisplay;
    private ShopManager shopManager;

    private void Start()
    {
        itemDisplay = GetComponent<ItemDisplay>();
        shopManager = FindObjectOfType<ShopManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (shopManager != null && itemDisplay != null)
        {
            shopManager.SelectItem(itemDisplay);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Optional: Add hover effect here
        // For example, slightly scale up the item
        transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset scale when mouse leaves
        transform.localScale = Vector3.one;
    }
}