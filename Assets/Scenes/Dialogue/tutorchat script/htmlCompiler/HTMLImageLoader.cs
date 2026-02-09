using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Optional: Add this component alongside HTMLCodingSystem to enable actual image loading
/// This will replace image placeholders with actual images in the output
/// </summary>
public class HTMLImageLoader : MonoBehaviour
{
    [Header("References")]
    public HTMLCodingSystem htmlSystem;
    public Transform imageContainer; // Parent object to hold spawned images

    [Header("Settings")]
    public GameObject imagePrefab; // Prefab with Image component
    public int maxImageWidth = 400;
    public int maxImageHeight = 300;

    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();

    void Start()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted += OnHTMLExecuted;
            htmlSystem.OnSystemCleared += ClearImages;
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
            string src = match.Groups[1].Value;
            StartCoroutine(LoadImage(src));
        }
    }

    IEnumerator LoadImage(string src)
    {
        // Check cache first
        if (imageCache.ContainsKey(src))
        {
            DisplayImage(imageCache[src], src);
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
                Debug.LogError($"Failed to load image from URL: {src}\nError: {request.error}");
                yield break;
            }
        }
        // Load from Resources folder
        else if (src.StartsWith("Resources/"))
        {
            string resourcePath = src.Replace("Resources/", "").Replace(".png", "").Replace(".jpg", "");
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                DisplayImage(sprite, src);
                imageCache[src] = sprite;
            }
            else
            {
                Debug.LogError($"Failed to load image from Resources: {resourcePath}");
            }
            yield break;
        }
        // Load from file path (StreamingAssets or absolute path)
        else
        {
            string filePath = src;
            if (!System.IO.Path.IsPathRooted(filePath))
            {
                filePath = System.IO.Path.Combine(Application.streamingAssetsPath, src);
            }

            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
            }
            else
            {
                Debug.LogError($"Image file not found: {filePath}");
                yield break;
            }
        }

        if (texture != null)
        {
            // Create sprite from texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageCache[src] = sprite;
            DisplayImage(sprite, src);
        }
    }

    void DisplayImage(Sprite sprite, string src)
    {
        if (imagePrefab == null || imageContainer == null)
        {
            Debug.LogWarning("Image prefab or container not assigned!");
            return;
        }

        GameObject imgObj = Instantiate(imagePrefab, imageContainer);
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
        if (imageContainer != null)
        {
            foreach (Transform child in imageContainer)
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
            htmlSystem.OnSystemCleared -= ClearImages;
        }
    }
}