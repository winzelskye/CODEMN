using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    [Header("References")]
    public shopItem itemData;
    public Image itemImage;

    [Header("State")]
    private bool isSelected = false;
    private float animationTimer = 0f;
    private int currentFrame = 0; // 0, 1, or 2 for the 3 frames

    private void Start()
    {
        if (itemData != null && itemImage != null)
        {
            // Start with static sprite (not selected)
            itemImage.sprite = itemData.staticSprite;
        }
    }

    private void Update()
    {
        // Only animate when selected
        if (isSelected && itemData != null)
        {
            animationTimer += Time.deltaTime;

            if (animationTimer >= itemData.animationSpeed)
            {
                animationTimer = 0f;

                // Cycle through the 3 frames
                currentFrame = (currentFrame + 1) % 3;

                // Set the sprite based on current frame
                switch (currentFrame)
                {
                    case 0:
                        itemImage.sprite = itemData.selectedFrame1;
                        break;
                    case 1:
                        itemImage.sprite = itemData.selectedFrame2;
                        break;
                    case 2:
                        itemImage.sprite = itemData.selectedFrame3;
                        break;
                }
            }
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selected)
        {
            // Start animation from frame 1
            currentFrame = 0;
            animationTimer = 0f;
            if (itemData != null)
            {
                itemImage.sprite = itemData.selectedFrame1;
            }
        }
        else
        {
            // Return to static sprite when deselected
            if (itemData != null)
            {
                itemImage.sprite = itemData.staticSprite;
            }
            currentFrame = 0;
            animationTimer = 0f;
        }
    }

    public shopItem GetItemData()
    {
        return itemData;
    }
}