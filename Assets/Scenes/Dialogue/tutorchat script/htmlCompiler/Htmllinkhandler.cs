using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Optional: Add this to your OutputText object to make links clickable
/// Detects when user clicks on a link in the TextMeshPro text
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HTMLLinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TextMeshProUGUI textMesh;
    private Camera mainCamera;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        mainCamera = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (textMesh == null) return;

        // Get the index of the character that was clicked
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMesh, eventData.position, mainCamera);

        if (linkIndex != -1)
        {
            // Get the link info
            TMP_LinkInfo linkInfo = textMesh.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();

            // Open the URL
            OpenURL(url);
        }
    }

    void OpenURL(string url)
    {
        Debug.Log($"Opening URL: {url}");

        // Add http:// if not present
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        Application.OpenURL(url);
    }
}