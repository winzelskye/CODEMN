using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Attach to the same GameObject as your output TextMeshProUGUI.
/// Detects clicks on TMP <link> tags and opens the URL in the browser.
///
/// REQUIREMENTS:
///   - Output TextMeshProUGUI must have "Raycast Target" ON
///   - Canvas must have a GraphicRaycaster component
///   - Scene must have an EventSystem
///   - Output TextMeshProUGUI must have "Rich Text" ON (so <link> tags work)
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HTMLLinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMesh;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();

        // Rich text MUST be on for <link> tags to work in the output
        if (!textMesh.richText)
        {
            textMesh.richText = true;
            Debug.Log("[LinkHandler] Enabled Rich Text on output panel (required for clickable links).");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (textMesh == null) return;

        // Screen Space Overlay → pass null as the camera (correct for Overlay mode)
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            textMesh, eventData.position, null);

        if (linkIndex == -1) return; // Click wasn't on a link

        TMP_LinkInfo linkInfo = textMesh.textInfo.linkInfo[linkIndex];
        string url = linkInfo.GetLinkID();

        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[LinkHandler] Clicked a link but URL was empty.");
            return;
        }

        OpenURL(url);
    }

    private void OpenURL(string url)
    {
        // Ensure there's a scheme — bare domains like "google.com" need https://
        if (!url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        Debug.Log($"[LinkHandler] Opening: {url}");
        Application.OpenURL(url);
    }
}