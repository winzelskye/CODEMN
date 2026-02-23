using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Handles ALL image display — both static images and animated GIFs.
///
/// STATIC IMAGES (.png, .jpg, etc.):
///   Put them in Assets/Resources/
///   Set Texture Type to "Sprite (2D and UI)" in Inspector
///   Use in HTML: <img src="cat.png"> or <img src="cat">
///
/// ANIMATED GIFS (.gif):
///   Put them in Assets/StreamingAssets/ (keep .gif extension, no renaming needed)
///   Use in HTML: <img src="confusedjohn.gif">
///   Requires UniGif: https://github.com/WestHillApps/UniGif
///
/// SIZE CONTROL (per image in HTML):
///   <img src="cat.png" width="300" height="200">
///   <img src="cat.png" width="300">          (height auto from aspect ratio)
///   <img src="cat.png" height="200">         (width auto from aspect ratio)
///   <img src="cat.png">                      (clamped to Max Image Width/Height in Inspector)
///
/// ALIGNMENT:
///   <img src="cat.png" style="float:left">
///   <img src="cat.png" style="float:right">
///   <img src="cat.png">                      (default center)
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

    // Cache for static sprites
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    // Track all active GIF coroutines so we can stop them on clear
    private List<Coroutine> activeGifCoroutines = new List<Coroutine>();

    void Start()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted += OnHTMLExecuted;
            htmlSystem.OnSystemCleared += ClearImages;
        }
        else
            Debug.LogWarning("[ImageSystem] htmlSystem not assigned!");

        if (imageDisplayAreaCenter == null)
            Debug.LogWarning("[ImageSystem] imageDisplayAreaCenter not assigned!");

        if (imagePrefab == null)
            Debug.LogWarning("[ImageSystem] imagePrefab not assigned!");
    }

    // ── Called when HTML is run ─────────────────────────────────────────────

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
                Debug.LogWarning("[ImageSystem] <img> tag missing src, skipping.");
                continue;
            }

            string filename = srcMatch.Groups[1].Value.Trim();
            Alignment alignment = GetAlignment(attrs);
            string ext = Path.GetExtension(filename).ToLower();
            int w = GetIntAttr(attrs, "width");
            int h = GetIntAttr(attrs, "height");

            Transform container = GetContainer(alignment);
            if (container == null)
            {
                Debug.LogWarning("[ImageSystem] No container assigned, skipping image.");
                continue;
            }

            if (ext == ".gif")
            {
                string nameNoExt = Path.GetFileNameWithoutExtension(filename);
                Coroutine c = StartCoroutine(LoadAndDisplayGif(nameNoExt, container, w, h));
                activeGifCoroutines.Add(c);
            }
            else
            {
                StartCoroutine(LoadAndDisplayStatic(filename, container, w, h));
            }
        }
    }

    // ── Attribute helpers ───────────────────────────────────────────────────

    private int GetIntAttr(string attrs, string attrName)
    {
        Match m = Regex.Match(attrs,
            $@"{attrName}=[""']?(\d+)[""']?", RegexOptions.IgnoreCase);
        if (m.Success && int.TryParse(m.Groups[1].Value, out int val))
            return val;
        return -1; // not specified
    }

    // ── Size calculation ────────────────────────────────────────────────────

    private Vector2 GetImageSize(int texW, int texH, int forceWidth, int forceHeight)
    {
        // Both explicitly set in HTML
        if (forceWidth > 0 && forceHeight > 0)
            return new Vector2(forceWidth, forceHeight);

        float aspect = (float)texW / texH;

        // Only width set — derive height
        if (forceWidth > 0)
            return new Vector2(forceWidth, forceWidth / aspect);

        // Only height set — derive width
        if (forceHeight > 0)
            return new Vector2(forceHeight * aspect, forceHeight);

        // Neither set — clamp to Inspector max
        if (texW > maxImageWidth) return new Vector2(maxImageWidth, maxImageWidth / aspect);
        if (texH > maxImageHeight) return new Vector2(maxImageHeight * aspect, maxImageHeight);
        return new Vector2(texW, texH);
    }

    // ── Static image loading (Resources) ───────────────────────────────────

    private IEnumerator LoadAndDisplayStatic(string filename, Transform container,
        int forceWidth = -1, int forceHeight = -1)
    {
        Sprite sprite = LoadSprite(filename);
        if (sprite != null)
            DisplaySprite(sprite, container, forceWidth, forceHeight);
        yield break;
    }

    private void DisplaySprite(Sprite sprite, Transform container,
        int forceWidth = -1, int forceHeight = -1)
    {
        if (imagePrefab == null || container == null) return;

        GameObject imgObj = Instantiate(imagePrefab, container);
        Image imgComp = imgObj.GetComponent<Image>();

        if (imgComp == null)
        {
            Debug.LogWarning("[ImageSystem] imagePrefab has no Image component!");
            Destroy(imgObj);
            return;
        }

        imgComp.sprite = sprite;
        imgComp.preserveAspect = (forceWidth <= 0 || forceHeight <= 0);

        RectTransform rt = imgComp.GetComponent<RectTransform>();
        rt.sizeDelta = GetImageSize(
            sprite.texture.width, sprite.texture.height, forceWidth, forceHeight);
    }

    private Sprite LoadSprite(string filename)
    {
        string nameNoExt = Path.GetFileNameWithoutExtension(filename);
        string key = nameNoExt.ToLower();

        if (spriteCache.TryGetValue(key, out Sprite cached))
            return cached;

        Sprite sprite = Resources.Load<Sprite>(nameNoExt);
        if (sprite != null)
        {
            spriteCache[key] = sprite;
            Debug.Log($"[ImageSystem] Loaded sprite: Resources/{nameNoExt}");
            return sprite;
        }

        Texture2D tex = Resources.Load<Texture2D>(nameNoExt);
        if (tex != null)
        {
            sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            spriteCache[key] = sprite;
            Debug.Log($"[ImageSystem] Loaded texture: Resources/{nameNoExt}");
            return sprite;
        }

        Debug.LogError(
            $"[ImageSystem] Could not find '{nameNoExt}' in Resources!\n" +
            $"  → Assets/Resources/{nameNoExt}.png (or .jpg)\n" +
            $"  → Texture Type must be 'Sprite (2D and UI)'"
        );
        return null;
    }

    // ── GIF loading (StreamingAssets) ───────────────────────────────────────

    private IEnumerator LoadAndDisplayGif(string nameNoExt, Transform container,
        int forceWidth = -1, int forceHeight = -1)
    {
        string gifPath = Path.Combine(Application.streamingAssetsPath, nameNoExt + ".gif");

        if (!File.Exists(gifPath))
        {
            Debug.LogError(
                $"[ImageSystem] GIF not found: {gifPath}\n" +
                $"  → Put '{nameNoExt}.gif' in Assets/StreamingAssets/"
            );
            yield break;
        }

        byte[] gifBytes = File.ReadAllBytes(gifPath);
        Debug.Log($"[ImageSystem] Loaded GIF '{nameNoExt}' ({gifBytes.Length} bytes)");

        List<UniGif.GifTexture> frames = null;
        int loopCount = 0;

        yield return StartCoroutine(
            UniGif.GetTextureListCoroutine(gifBytes,
                (textureList, loops, w, h) =>
                {
                    frames = textureList;
                    loopCount = loops;
                })
        );

        if (frames == null || frames.Count == 0)
        {
            Debug.LogError($"[ImageSystem] Failed to decode GIF '{nameNoExt}'");
            yield break;
        }

        if (imagePrefab == null) yield break;

        GameObject imgObj = Instantiate(imagePrefab, container);
        Image imgComp = imgObj.GetComponent<Image>();

        if (imgComp == null)
        {
            Destroy(imgObj);
            yield break;
        }

        imgComp.preserveAspect = (forceWidth <= 0 || forceHeight <= 0);

        // Size from first frame
        UniGif.GifTexture firstFrame = frames[0];
        RectTransform rt = imgComp.GetComponent<RectTransform>();
        rt.sizeDelta = GetImageSize(
            firstFrame.m_texture2d.width, firstFrame.m_texture2d.height,
            forceWidth, forceHeight);

        Coroutine c = StartCoroutine(AnimateGif(imgComp, frames, loopCount, imgObj));
        activeGifCoroutines.Add(c);
    }

    private IEnumerator AnimateGif(Image imgComp, List<UniGif.GifTexture> frames,
        int loopCount, GameObject imgObj)
    {
        int loops = 0;

        while (true)
        {
            for (int i = 0; i < frames.Count; i++)
            {
                if (imgObj == null || imgComp == null) yield break;

                UniGif.GifTexture frame = frames[i];
                Sprite frameSprite = Sprite.Create(
                    frame.m_texture2d,
                    new Rect(0, 0, frame.m_texture2d.width, frame.m_texture2d.height),
                    new Vector2(0.5f, 0.5f)
                );
                imgComp.sprite = frameSprite;

                yield return new WaitForSeconds(frame.m_delaySec > 0 ? frame.m_delaySec : 0.1f);
            }

            loops++;
            if (loopCount > 0 && loops >= loopCount) break;
        }
    }

    // ── Alignment ───────────────────────────────────────────────────────────

    private enum Alignment { Left, Right, Center }

    private Alignment GetAlignment(string attrs)
    {
        Match styleMatch = Regex.Match(attrs,
            @"style=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (styleMatch.Success)
        {
            string style = styleMatch.Groups[1].Value;
            if (Regex.IsMatch(style, @"float\s*:\s*left", RegexOptions.IgnoreCase)) return Alignment.Left;
            if (Regex.IsMatch(style, @"float\s*:\s*right", RegexOptions.IgnoreCase)) return Alignment.Right;
        }

        Match alignMatch = Regex.Match(attrs,
            @"align=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
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

    public Sprite GetSprite(string name) => LoadSprite(name);

    public bool IsGif(string filename) =>
        Path.GetExtension(filename).ToLower() == ".gif";

    public Coroutine StartGifOnImage(string nameNoExt, Image target)
    {
        return StartCoroutine(LoadAndPlayGifOnImage(nameNoExt, target));
    }

    private IEnumerator LoadAndPlayGifOnImage(string nameNoExt, Image target)
    {
        string gifPath = Path.Combine(Application.streamingAssetsPath, nameNoExt + ".gif");

        if (!File.Exists(gifPath))
        {
            Debug.LogError($"[ImageSystem] GIF not found for background: {gifPath}");
            yield break;
        }

        byte[] gifBytes = File.ReadAllBytes(gifPath);
        List<UniGif.GifTexture> frames = null;
        int loopCount = 0;

        yield return StartCoroutine(
            UniGif.GetTextureListCoroutine(gifBytes,
                (textureList, loops, w, h) =>
                {
                    frames = textureList;
                    loopCount = loops;
                })
        );

        if (frames == null || frames.Count == 0) yield break;

        Coroutine c = StartCoroutine(AnimateGif(target, frames, loopCount, target.gameObject));
        activeGifCoroutines.Add(c);
    }

    // ── Clear ───────────────────────────────────────────────────────────────

    public void ClearImages()
    {
        foreach (Coroutine c in activeGifCoroutines)
            if (c != null) StopCoroutine(c);
        activeGifCoroutines.Clear();

        ClearContainer(imageDisplayAreaLeft);
        ClearContainer(imageDisplayAreaRight);
        ClearContainer(imageDisplayAreaCenter);
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

[System.Serializable]
public class NamedSprite
{
    public string name;
    public Sprite sprite;
}