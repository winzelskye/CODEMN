using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Replaces the simple text output with a dynamic UI that can show images and clickable links
/// </summary>
public class HTMLRichOutputRenderer : MonoBehaviour
{
    [Header("References")]
    public ScrollRect outputScrollRect;
    public Transform contentContainer; // Content object inside ScrollRect

    [Header("Prefabs")]
    public GameObject textPrefab; // Prefab with TextMeshProUGUI
    public GameObject imagePrefab; // Prefab with Image component
    public GameObject linkButtonPrefab; // Prefab with Button and TextMeshProUGUI

    [Header("Settings")]
    public int maxImageWidth = 500;
    public int maxImageHeight = 400;
    public Color linkColor = new Color(0, 0.75f, 1f); // Cyan blue

    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();

    public void RenderHTML(string html)
    {
        // Clear existing content
        ClearContent();

        // Parse and render the HTML
        StartCoroutine(ParseAndRender(html));
    }

    IEnumerator ParseAndRender(string html)
    {
        // Split HTML into segments: text, images, and links
        List<ContentSegment> segments = ParseHTMLSegments(html);

        foreach (var segment in segments)
        {
            switch (segment.type)
            {
                case SegmentType.Text:
                    CreateTextElement(segment.content);
                    break;

                case SegmentType.Image:
                    yield return CreateImageElement(segment.content, segment.attribute);
                    break;

                case SegmentType.Link:
                    CreateLinkElement(segment.content, segment.attribute);
                    break;
            }
        }

        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>());
    }

    List<ContentSegment> ParseHTMLSegments(string html)
    {
        List<ContentSegment> segments = new List<ContentSegment>();
        int currentIndex = 0;

        // Find all images and links
        MatchCollection imageMatches = Regex.Matches(html, @"<img[^>]*src=[""']([^""']+)[""']([^>]*alt=[""']([^""']+)[""'])?[^>]*>", RegexOptions.IgnoreCase);
        MatchCollection linkMatches = Regex.Matches(html, @"<link=""([^""]+)""><color[^>]*><u>([^<]+)</u></color></link>", RegexOptions.IgnoreCase);

        // Combine and sort all matches by position
        List<Match> allMatches = new List<Match>();
        allMatches.AddRange(imageMatches);
        allMatches.AddRange(linkMatches);
        allMatches.Sort((a, b) => a.Index.CompareTo(b.Index));

        foreach (Match match in allMatches)
        {
            // Add text before this match
            if (match.Index > currentIndex)
            {
                string textBefore = html.Substring(currentIndex, match.Index - currentIndex);
                if (!string.IsNullOrWhiteSpace(textBefore))
                {
                    segments.Add(new ContentSegment
                    {
                        type = SegmentType.Text,
                        content = textBefore
                    });
                }
            }

            // Add the match
            if (match.Value.Contains("<img"))
            {
                string src = match.Groups[1].Value;
                string alt = match.Groups[3].Success ? match.Groups[3].Value : "image";
                segments.Add(new ContentSegment
                {
                    type = SegmentType.Image,
                    content = src,
                    attribute = alt
                });
            }
            else if (match.Value.Contains("<link"))
            {
                string url = match.Groups[1].Value;
                string linkText = match.Groups[2].Value;
                segments.Add(new ContentSegment
                {
                    type = SegmentType.Link,
                    content = linkText,
                    attribute = url
                });
            }

            currentIndex = match.Index + match.Length;
        }

        // Add remaining text
        if (currentIndex < html.Length)
        {
            string remainingText = html.Substring(currentIndex);
            if (!string.IsNullOrWhiteSpace(remainingText))
            {
                segments.Add(new ContentSegment
                {
                    type = SegmentType.Text,
                    content = remainingText
                });
            }
        }

        return segments;
    }

    void CreateTextElement(string text)
    {
        if (textPrefab == null) return;

        GameObject textObj = Instantiate(textPrefab, contentContainer);
        TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.enableAutoSizing = false;
        }
    }

    IEnumerator CreateImageElement(string src, string alt)
    {
        if (imagePrefab == null) yield break;

        // Check cache
        if (imageCache.ContainsKey(src))
        {
            DisplayImage(imageCache[src], alt);
            yield break;
        }

        Texture2D texture = null;

        // Load from URL
        if (src.StartsWith("http://") || src.StartsWith("https://"))
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(src);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                texture = DownloadHandlerTexture.GetContent(request);
            }
            else
            {
                Debug.LogError($"Failed to load image: {src}");
                CreateTextElement($"[Failed to load image: {alt}]");
                yield break;
            }
        }
        // Load from file
        else
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, src);
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
            }
            else
            {
                Debug.LogError($"Image not found: {filePath}");
                CreateTextElement($"[Image not found: {alt}]");
                yield break;
            }
        }

        if (texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageCache[src] = sprite;
            DisplayImage(sprite, alt);
        }
    }

    void DisplayImage(Sprite sprite, string alt)
    {
        if (imagePrefab == null) return;

        GameObject imgObj = Instantiate(imagePrefab, contentContainer);
        Image imgComponent = imgObj.GetComponent<Image>();

        if (imgComponent != null)
        {
            imgComponent.sprite = sprite;
            imgComponent.preserveAspect = true;

            // Resize
            RectTransform rt = imgComponent.GetComponent<RectTransform>();
            float aspectRatio = (float)sprite.texture.width / sprite.texture.height;

            if (sprite.texture.width > maxImageWidth)
            {
                rt.sizeDelta = new Vector2(maxImageWidth, maxImageWidth / aspectRatio);
            }
            else if (sprite.texture.height > maxImageHeight)
            {
                rt.sizeDelta = new Vector2(maxImageHeight * aspectRatio, maxImageHeight);
            }
            else
            {
                rt.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
            }
        }

        // Add tooltip with alt text
        var tooltip = imgObj.AddComponent<TooltipHandler>();
        tooltip.tooltipText = alt;
    }

    void CreateLinkElement(string linkText, string url)
    {
        if (linkButtonPrefab == null)
        {
            // Fallback to text with color
            CreateTextElement($"<color=#{ColorUtility.ToHtmlStringRGB(linkColor)}><u>{linkText}</u></color>");
            return;
        }

        GameObject linkObj = Instantiate(linkButtonPrefab, contentContainer);
        Button button = linkObj.GetComponent<Button>();
        TextMeshProUGUI textComponent = linkObj.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = linkText;
            textComponent.color = linkColor;
        }

        if (button != null)
        {
            button.onClick.AddListener(() => OpenURL(url));
        }
    }

    void OpenURL(string url)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        Debug.Log($"Opening: {url}");
        Application.OpenURL(url);
    }

    void ClearContent()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        imageCache.Clear();
    }
}

[System.Serializable]
public class ContentSegment
{
    public SegmentType type;
    public string content;
    public string attribute; // URL for links, alt for images
}

public enum SegmentType
{
    Text,
    Image,
    Link
}

// Simple tooltip component
public class TooltipHandler : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    public string tooltipText;
    private GameObject tooltipObject;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // You can create a tooltip UI here
        Debug.Log($"Tooltip: {tooltipText}");
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // Hide tooltip
    }
}