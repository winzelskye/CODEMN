using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WordSlot : MonoBehaviour, IDropHandler
{
    [TextArea(1, 5)]
    public string correctWord;
    public DraggableWord currentWord;

    [Header("Visual Feedback")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color incorrectColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    private Image slotImage;

    void Start()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.color = normalColor;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableWord draggableWord = eventData.pointerDrag.GetComponent<DraggableWord>();
            if (draggableWord != null)
            {
                // If slot already has a word, swap them
                if (currentWord != null)
                {
                    // Store the old word's data
                    DraggableWord oldWord = currentWord;

                    // Clear this slot
                    currentWord = null;

                    // Place new word in this slot
                    draggableWord.PlaceInSlot(this);
                    currentWord = draggableWord;

                    // Return old word to start position
                    oldWord.ReturnToStart();
                }
                else
                {
                    // Slot is empty, just place the word
                    draggableWord.PlaceInSlot(this);
                    currentWord = draggableWord;
                }
            }
        }
    }

    public bool IsCorrect()
    {
        if (currentWord == null) return false;
        return currentWord.wordText.Trim() == correctWord.Trim();
    }

    public void ClearSlot()
    {
        if (currentWord != null)
        {
            currentWord.ReturnToStart();
            currentWord = null;
        }

        if (slotImage != null)
        {
            slotImage.color = normalColor;
        }
    }

    public void ShowFeedback(bool isCorrect)
    {
        if (slotImage != null)
        {
            slotImage.color = isCorrect ? correctColor : incorrectColor;
        }
    }

    public void ResetColor()
    {
        if (slotImage != null)
        {
            slotImage.color = normalColor;
        }
    }
}