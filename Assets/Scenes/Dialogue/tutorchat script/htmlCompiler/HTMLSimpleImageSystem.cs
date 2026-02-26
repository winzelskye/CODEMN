using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Handles ALL image display — static images and animated GIFs.
/// NO Layout Groups used — each image is manually positioned via anchoredPosition.
///
/// SETUP:
///   imageDisplayArea should be a plain RectTransform with NO Layout Group on it.
///   The container itself should be top-anchored and stretch full width.
///
/// STATIC IMAGES: Put in Assets/Resources/, Texture Type = Sprite (2D and UI)
/// ANIMATED GIFS: Put in Assets/StreamingAssets/
///
/// ALIGNMENT per image:
///   <img src="nerd.png">                    default = center
///   <img src="nerd.png" align="left">
///   <img src="nerd.png" align="right">
///   <img src="nerd.png" style="float:left">
///   <img src="nerd.png" style="float:right">
///
/// SIZE:
///   <img src="nerd.png" width="200" height="150">
///   <img src="nerd.png" width="200">         height auto
///   <img src="nerd.png" height="150">        width auto
/// </summary>
public class HTMLSimpleImageSystem : MonoBehaviour
{
    [Header("References")]
    public HTMLCodingSystem htmlSystem;

    [Header("Image Display Area")]
    [Tooltip("Plain RectTransform — NO Layout Group. Top-anchored, full width.")]
    public RectTransform imageDisplayArea;

    [Header("Settings")]
    public GameObject imagePrefab;
    public int maxImageWidth = 400;
    public int maxImageHeight = 300;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
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

        if (imageDisplayArea == null)
            Debug.LogWarning("[ImageSystem] imageDisplayArea not assigned!");

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
            if (!srcMatch.Success) { Debug.LogWarning("[ImageSystem] <img> missing src."); continue; }

            string filename = srcMatch.Groups[1].Value.Trim();
            Alignment align = GetAlignment(attrs);
            string ext = Path.GetExtension(filename).ToLower();
            int w = GetIntAttr(attrs, "width");
            int h = GetIntAttr(attrs, "height");

            if (imageDisplayArea == null) { Debug.LogWarning("[ImageSystem] No imageDisplayArea."); continue; }

            if (ext == ".gif")
            {
                string nameNoExt = Path.GetFileNameWithoutExtension(filename);
                Coroutine c = StartCoroutine(LoadAndDisplayGif(nameNoExt, align, w, h));
                activeGifCoroutines.Add(c);
            }
            else
            {
                StartCoroutine(LoadAndDisplayStatic(filename, align, w, h));
            }
        }
    }

    // ── Row-based placement ─────────────────────────────────────────────────
    // The VLG controls vertical stacking of rows.
    // Each image gets its own full-width row (added to VLG).
    // Inside that row, the image is anchored left/center/right freely —
    // the VLG never touches children of the row, only the row itself.

    private RectTransform CreateRow(float height)
    {
        GameObject row = new GameObject("ImgRow", typeof(RectTransform));
        row.transform.SetParent(imageDisplayArea, false);

        RectTransform rowRt = row.GetComponent<RectTransform>();

        // Stretch full width inside VLG, fixed height
        rowRt.anchorMin = new Vector2(0, 0.5f);
        rowRt.anchorMax = new Vector2(1, 0.5f);
        rowRt.sizeDelta = new Vector2(0, height);
        rowRt.pivot = new Vector2(0.5f, 0.5f);

        // Tell VLG how tall this row is
        LayoutElement le = row.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;
        le.flexibleWidth = 1;

        return rowRt;
    }

    private void PlaceImage(RectTransform imgRt, Vector2 size, Alignment alignment)
    {
        // Create a full-width row in the VLG for this image
        RectTransform rowRt = CreateRow(size.y);

        // Parent the image into the row
        imgRt.SetParent(rowRt, false);

        // Now anchor the image inside the row — VLG won't touch it here
        imgRt.sizeDelta = size;

        switch (alignment)
        {
            case Alignment.Left:
                imgRt.anchorMin = new Vector2(0, 0.5f);
                imgRt.anchorMax = new Vector2(0, 0.5f);
                imgRt.pivot = new Vector2(0, 0.5f);
                imgRt.anchoredPosition = Vector2.zero;
                break;

            case Alignment.Right:
                imgRt.anchorMin = new Vector2(1, 0.5f);
                imgRt.anchorMax = new Vector2(1, 0.5f);
                imgRt.pivot = new Vector2(1, 0.5f);
                imgRt.anchoredPosition = Vector2.zero;
                break;

            default: // Center
                imgRt.anchorMin = new Vector2(0.5f, 0.5f);
                imgRt.anchorMax = new Vector2(0.5f, 0.5f);
                imgRt.pivot = new Vector2(0.5f, 0.5f);
                imgRt.anchoredPosition = Vector2.zero;
                break;
        }
    }

    // ── Size calculation ────────────────────────────────────────────────────

    private Vector2 GetImageSize(int texW, int texH, int forceWidth, int forceHeight)
    {
        if (forceWidth > 0 && forceHeight > 0)
            return new Vector2(forceWidth, forceHeight);

        float aspect = (float)texW / texH;

        if (forceWidth > 0) return new Vector2(forceWidth, forceWidth / aspect);
        if (forceHeight > 0) return new Vector2(forceHeight * aspect, forceHeight);

        if (texW > maxImageWidth) return new Vector2(maxImageWidth, maxImageWidth / aspect);
        if (texH > maxImageHeight) return new Vector2(maxImageHeight * aspect, maxImageHeight);
        return new Vector2(texW, texH);
    }

    // ── Attribute helpers ───────────────────────────────────────────────────

    private int GetIntAttr(string attrs, string attrName)
    {
        Match m = Regex.Match(attrs, $@"{attrName}=[""']?(\d+)[""']?", RegexOptions.IgnoreCase);
        if (m.Success && int.TryParse(m.Groups[1].Value, out int val)) return val;
        return -1;
    }

    // ── Static image loading ────────────────────────────────────────────────

    private IEnumerator LoadAndDisplayStatic(string filename, Alignment alignment,
        int forceWidth = -1, int forceHeight = -1)
    {
        Sprite sprite = LoadSprite(filename);
        if (sprite != null)
            DisplaySprite(sprite, alignment, forceWidth, forceHeight);
        yield break;
    }

    private void DisplaySprite(Sprite sprite, Alignment alignment,
        int forceWidth = -1, int forceHeight = -1)
    {
        if (imagePrefab == null || imageDisplayArea == null) return;

        // Instantiate WITHOUT a parent so the VLG doesn't touch it yet
        GameObject imgObj = Instantiate(imagePrefab);
        Image imgComp = imgObj.GetComponent<Image>();

        if (imgComp == null)
        {
            Debug.LogWarning("[ImageSystem] imagePrefab has no Image component!");
            Destroy(imgObj);
            return;
        }

        imgComp.sprite = sprite;
        imgComp.preserveAspect = (forceWidth <= 0 || forceHeight <= 0);

        Vector2 size = GetImageSize(sprite.texture.width, sprite.texture.height, forceWidth, forceHeight);
        RectTransform imgRt = imgObj.GetComponent<RectTransform>();
        imgRt.sizeDelta = size;

        // PlaceImage creates the row in the VLG and parents the image inside it
        PlaceImage(imgRt, size, alignment);
    }

    private Sprite LoadSprite(string filename)
    {
        string nameNoExt = Path.GetFileNameWithoutExtension(filename);
        string key = nameNoExt.ToLower();

        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

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
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            spriteCache[key] = sprite;
            Debug.Log($"[ImageSystem] Loaded texture: Resources/{nameNoExt}");
            return sprite;
        }

        Debug.LogError(
            $"[ImageSystem] Could not find '{nameNoExt}' in Resources!\n" +
            $"  → Assets/Resources/{nameNoExt}.png  (Texture Type: Sprite 2D and UI)");
        return null;
    }

    // ── GIF loading ─────────────────────────────────────────────────────────

    private IEnumerator LoadAndDisplayGif(string nameNoExt, Alignment alignment,
        int forceWidth = -1, int forceHeight = -1)
    {
        string gifPath = Path.Combine(Application.streamingAssetsPath, nameNoExt + ".gif");

        if (!File.Exists(gifPath))
        {
            Debug.LogError($"[ImageSystem] GIF not found: {gifPath}");
            yield break;
        }

        byte[] gifBytes = File.ReadAllBytes(gifPath);
        List<UniGif.GifTexture> frames = null;
        int loopCount = 0;

        yield return StartCoroutine(
            UniGif.GetTextureListCoroutine(gifBytes,
                (list, loops, w, h) => { frames = list; loopCount = loops; }));

        if (frames == null || frames.Count == 0)
        {
            Debug.LogError($"[ImageSystem] Failed to decode GIF '{nameNoExt}'");
            yield break;
        }

        if (imagePrefab == null) yield break;

        // Instantiate WITHOUT parent so VLG doesn't grab it before row is ready
        GameObject imgObj = Instantiate(imagePrefab);
        Image imgComp = imgObj.GetComponent<Image>();
        if (imgComp == null) { Destroy(imgObj); yield break; }

        imgComp.preserveAspect = (forceWidth <= 0 || forceHeight <= 0);

        UniGif.GifTexture first = frames[0];
        Vector2 size = GetImageSize(first.m_texture2d.width, first.m_texture2d.height, forceWidth, forceHeight);

        RectTransform imgRt = imgObj.GetComponent<RectTransform>();
        imgRt.sizeDelta = size;

        PlaceImage(imgRt, size, alignment);

        Coroutine c = StartCoroutine(AnimateGif(imgComp, frames, loopCount, imgObj));
        activeGifCoroutines.Add(c);
    }

    private IEnumerator AnimateGif(Image imgComp, List<UniGif.GifTexture> frames,
        int loopCount, GameObject imgObj)
    {
        int loops = 0;
        while (true)
        {
            foreach (UniGif.GifTexture frame in frames)
            {
                if (imgObj == null || imgComp == null) yield break;
                imgComp.sprite = Sprite.Create(
                    frame.m_texture2d,
                    new Rect(0, 0, frame.m_texture2d.width, frame.m_texture2d.height),
                    new Vector2(0.5f, 0.5f));
                yield return new WaitForSeconds(frame.m_delaySec > 0 ? frame.m_delaySec : 0.1f);
            }
            loops++;
            if (loopCount > 0 && loops >= loopCount) break;
        }
    }

    // ── Alignment detection ─────────────────────────────────────────────────

    private enum Alignment { Left, Right, Center }

    private Alignment GetAlignment(string attrs)
    {
        Match styleMatch = Regex.Match(attrs, @"style=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (styleMatch.Success)
        {
            string style = styleMatch.Groups[1].Value;
            if (Regex.IsMatch(style, @"float\s*:\s*left", RegexOptions.IgnoreCase)) return Alignment.Left;
            if (Regex.IsMatch(style, @"float\s*:\s*right", RegexOptions.IgnoreCase)) return Alignment.Right;
        }

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

    // ── Public API ──────────────────────────────────────────────────────────

    public Sprite GetSprite(string name) => LoadSprite(name);
    public bool IsGif(string filename) => Path.GetExtension(filename).ToLower() == ".gif";

    public Coroutine StartGifOnImage(string nameNoExt, Image target)
        => StartCoroutine(LoadAndPlayGifOnImage(nameNoExt, target));

    private IEnumerator LoadAndPlayGifOnImage(string nameNoExt, Image target)
    {
        string gifPath = Path.Combine(Application.streamingAssetsPath, nameNoExt + ".gif");
        if (!File.Exists(gifPath)) { Debug.LogError($"[ImageSystem] GIF not found: {gifPath}"); yield break; }

        byte[] gifBytes = File.ReadAllBytes(gifPath);
        List<UniGif.GifTexture> frames = null;
        int loopCount = 0;

        yield return StartCoroutine(
            UniGif.GetTextureListCoroutine(gifBytes,
                (list, loops, w, h) => { frames = list; loopCount = loops; }));

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

        if (imageDisplayArea != null)
            foreach (Transform child in imageDisplayArea)
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