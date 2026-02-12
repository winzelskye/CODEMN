using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add this component to your Dialogue Line Prefab to handle continuation message indentation
/// </summary>
public class ContinuationLayoutHelper : MonoBehaviour
{
    [Header("Indentation Settings")]
    [Tooltip("Left padding for continuation messages (in pixels)")]
    public float continuationIndent = 100f;

    private RectTransform rectTransform;
    private HorizontalLayoutGroup layoutGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetContinuation(bool isContinuation)
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        // Add or adjust layout group for indentation
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null && isContinuation)
        {
            layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        if (layoutGroup != null)
        {
            if (isContinuation)
            {
                // Set left padding for continuation
                layoutGroup.padding.left = (int)continuationIndent;
                layoutGroup.padding.right = 10;
                layoutGroup.padding.top = 2;
                layoutGroup.padding.bottom = 2;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
            }
            else
            {
                // No indentation for regular messages
                layoutGroup.padding.left = 10;
                layoutGroup.padding.right = 10;
                layoutGroup.padding.top = 5;
                layoutGroup.padding.bottom = 5;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
            }
        }
    }
}