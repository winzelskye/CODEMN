using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Simple image display system - pre-load images in Inspector, reference by name in HTML
/// </summary>
public class HTMLSimpleImageSystem : MonoBehaviour
{
    [Header("References")]
    public HTMLCodingSystem htmlSystem;
    public Transform imageDisplayArea; // Where images will appear

    [Header("Pre-loaded Images")]
    public List<NamedSprite> images = new List<NamedSprite>();

    [Header("Settings")]
    public GameObject imagePrefab; // Prefab with Image component
    public int maxImageWidth = 400;
    public int maxImageHeight = 300;

    private Dictionary<string, Sprite> imageDictionary = new Dictionary<string, Sprite>();

    void Start()
    {
        // Build dictionary from list
        foreach (var namedSprite in images)
        {
            if (!string.IsNullOrEmpty(namedSprite.name) && namedSprite.sprite != null)
            {
                imageDictionary[namedSprite.name.ToLower()] = namedSprite.sprite;
            }
        }

        // Subscribe to HTML execution
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted += OnHTMLExecuted;
        }
    }

    void OnHTMLExecuted(string html)
    {
        // Clear previous images
        ClearImages();

        // Find all image tags
        MatchCollection matches = Regex.Matches(html, @"<img[^>]*src=[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            string imageName = match.Groups[1].Value.ToLower();
            DisplayImage(imageName);
        }
    }

    void DisplayImage(string imageName)
    {
        if (!imageDictionary.ContainsKey(imageName))
        {
            Debug.LogWarning($"Image '{imageName}' not found in pre-loaded images!");
            return;
        }

        if (imagePrefab == null || imageDisplayArea == null)
        {
            Debug.LogWarning("Image prefab or display area not assigned!");
            return;
        }

        Sprite sprite = imageDictionary[imageName];
        GameObject imgObj = Instantiate(imagePrefab, imageDisplayArea);
        Image imgComponent = imgObj.GetComponent<Image>();

        if (imgComponent != null)
        {
            imgComponent.sprite = sprite;
            imgComponent.preserveAspect = true;

            // Resize to fit max dimensions
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
    }

    void ClearImages()
    {
        if (imageDisplayArea != null)
        {
            foreach (Transform child in imageDisplayArea)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void OnDestroy()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted -= OnHTMLExecuted;
        }
    }
}

[System.Serializable]
public class NamedSprite
{
    public string name;    // The name you'll use in HTML: <img src="table">
    public Sprite sprite;  // The actual sprite to display
}