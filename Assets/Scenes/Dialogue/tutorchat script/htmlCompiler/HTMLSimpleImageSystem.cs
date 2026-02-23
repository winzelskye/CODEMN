using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Loads images from the Resources folder automatically.
/// 
/// SETUP:
///   1. In your Unity Assets, create a folder named exactly: Resources
///   2. Place your image files inside it (png, jpg, etc.)
///   3. Select each image in the Inspector → set Texture Type to "Sprite (2D and UI)"
///   4. Reference in HTML by filename WITHOUT extension:
///        <img src="cat">          ← loads Resources/cat.png
///        <img src="cat" style="float:left">   ← loads and places left
///        <img src="cat" style="float:right">  ← loads and places right
///
///   Subfolders work too:
///        <img src="animals/cat">  ← loads Resources/animals/cat.png
/// </summary>
public class HTMLSimpleImageSystem : MonoBehaviour
{
    [Header("References")]
    public HTMLCodingSystem htmlSystem;

    [Header("Image Display Areas")]
    [Tooltip("Container for float:left images")]
    public Transform imageDisplayAreaLeft;
    [Tooltip("Container for float:right images")]
    public Transform imageDisplayAreaRight;
    [Tooltip("Default container (no float / center)")]
    public Transform imageDisplayAreaCenter;

    [Header("Settings")]
    public GameObject imagePrefab;
    public int maxImageWidth = 400;
    public int maxImageHeight = 300;

    // Runtime cache so we don't call Resources.Load repeatedly for the same image
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Start()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted += OnHTMLExecuted;
            htmlSystem.OnSystemCleared += ClearImages;
        }
        else
        {
            Debug.LogWarning("[ImageSystem] htmlSystem is not assigned in the Inspector!");
        }

        if (imageDisplayAreaCenter == null)
            Debug.LogWarning("[ImageSystem] imageDisplayAreaCenter is not assigned! Images won't display.");

        if (imagePrefab == null)
            Debug.LogWarning("[ImageSystem] imagePrefab is not assigned! Images won't display.");
    }

    // ── Called when HTML is executed ────────────────────────────────────────

    void OnHTMLExecuted(string html)
    {
        ClearImages();

        MatchCollection matches = Regex.Matches(html, @"<img([^>]*)>", RegexOptions.IgnoreCase);
        Debug.Log($"[ImageSystem] Found {matches.Count} <img> tag(s)");

        foreach (Match match in matches)
        {
            string attrs = match.Groups[1].Value;

            Match srcMatch = Regex.Match(attrs, @"src=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            if (!srcMatch.Success)
            {
                Debug.LogWarning("[ImageSystem] <img> tag is missing a src attribute, skipping.");
                continue;
            }

            string imageName = srcMatch.Groups[1].Value.Trim();
            Alignment alignment = GetAlignment(attrs);

            Debug.Log($"[ImageSystem] Loading '{imageName}' ({alignment})");
            DisplayImage(imageName, alignment);
        }
    }

    // ── Sprite loading from Resources ───────────────────────────────────────

    private Sprite LoadSprite(string name)
    {
        // Strip extension if present — Resources.Load doesn't use extensions
        // This means <img src="cat.png"> and <img src="cat"> both work
        string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(name);

        string key = nameNoExt.ToLower();
        if (spriteCache.TryGetValue(key, out Sprite cached))
            return cached;

        // Try loading directly as Sprite first
        Sprite sprite = Resources.Load<Sprite>(nameNoExt);
        if (sprite != null)
        {
            spriteCache[key] = sprite;
            Debug.Log($"[ImageSystem] Loaded Sprite from Resources/{nameNoExt}");
            return sprite;
        }

        // Try loading as Texture2D and converting (handles cases where
        // the texture type isn't set to Sprite in the Inspector)
        Texture2D tex = Resources.Load<Texture2D>(nameNoExt);
        if (tex != null)
        {
            sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            spriteCache[key] = sprite;
            Debug.Log($"[ImageSystem] Loaded Texture2D and converted from Resources/{nameNoExt}");
            return sprite;
        }

        // Not found — give a clear error with instructions
        Debug.LogError(
            $"[ImageSystem] Could not find '{nameNoExt}' in the Resources folder!\n" +
            $"  Make sure:\n" +
            $"  1. The file exists at:  Assets/Resources/{nameNoExt}.png  (or .jpg)\n" +
            $"  2. Its Texture Type is set to 'Sprite (2D and UI)' in the Inspector\n" +
            $"  3. The name in your HTML matches (with or without extension):\n" +
            $"     <img src=\"{nameNoExt}.png\">  or  <img src=\"{nameNoExt}\">"
        );
        return null;
    }

    // ── Display ─────────────────────────────────────────────────────────────

    private void DisplayImage(string imageName, Alignment alignment)
    {
        Sprite sprite = LoadSprite(imageName);
        if (sprite == null) return;

        Transform container = GetContainer(alignment);
        if (container == null)
        {
            Debug.LogWarning("[ImageSystem] No display container found. " +
                "Please assign imageDisplayAreaCenter in the Inspector.");
            return;
        }

        if (imagePrefab == null)
        {
            Debug.LogWarning("[ImageSystem] imagePrefab not assigned.");
            return;
        }

        GameObject imgObj = Instantiate(imagePrefab, container);
        Image imgComp = imgObj.GetComponent<Image>();

        if (imgComp == null)
        {
            Debug.LogWarning("[ImageSystem] imagePrefab has no Image component.");
            Destroy(imgObj);
            return;
        }

        imgComp.sprite = sprite;
        imgComp.preserveAspect = true;

        // Size to fit within max dimensions while preserving aspect ratio
        RectTransform rt = imgComp.GetComponent<RectTransform>();
        float aspect = (float)sprite.texture.width / sprite.texture.height;

        if (sprite.texture.width > maxImageWidth)
            rt.sizeDelta = new Vector2(maxImageWidth, maxImageWidth / aspect);
        else if (sprite.texture.height > maxImageHeight)
            rt.sizeDelta = new Vector2(maxImageHeight * aspect, maxImageHeight);
        else
            rt.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);

        Debug.Log($"[ImageSystem] Displayed '{imageName}' at size {rt.sizeDelta}");
    }

    // ── Alignment helpers ───────────────────────────────────────────────────

    private enum Alignment { Left, Right, Center }

    private Alignment GetAlignment(string attrs)
    {
        // style="float:left" or style="float:right"
        Match styleMatch = Regex.Match(attrs, @"style=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (styleMatch.Success)
        {
            string style = styleMatch.Groups[1].Value;
            if (Regex.IsMatch(style, @"float\s*:\s*left", RegexOptions.IgnoreCase)) return Alignment.Left;
            if (Regex.IsMatch(style, @"float\s*:\s*right", RegexOptions.IgnoreCase)) return Alignment.Right;
        }

        // Legacy: align="left" / align="right"
        Match alignMatch = Regex.Match(attrs, @"align=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (alignMatch.Success)
        {
            switch (alignMatch.Groups[1].Value.ToLower())
            {
                case "left": return Alignment.Left;
                case "right": return Alignment.Right;
            }
        }

        return Alignment.Center;
    }

    private Transform GetContainer(Alignment alignment)
    {
        switch (alignment)
        {
            case Alignment.Left: return imageDisplayAreaLeft ?? imageDisplayAreaCenter;
            case Alignment.Right: return imageDisplayAreaRight ?? imageDisplayAreaCenter;
            default: return imageDisplayAreaCenter;
        }
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>Used by HTMLCodingSystem for background-image on the body tag.</summary>
    public Sprite GetSprite(string name) => LoadSprite(name);

    public void ClearImages()
    {
        ClearContainer(imageDisplayAreaLeft);
        ClearContainer(imageDisplayAreaRight);
        ClearContainer(imageDisplayAreaCenter);
        // Don't clear the cache — sprites are already loaded, no need to reload
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
            Destroy(child.gameObject);
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