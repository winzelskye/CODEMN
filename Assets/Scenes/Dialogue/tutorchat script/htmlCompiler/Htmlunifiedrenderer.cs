using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unified HTML renderer — text and images flow together in a single vertical layout,
/// just like a real browser. No separate image container needed.
///
/// SETUP:
///   1. Create a ScrollView in your Canvas
///   2. Inside Viewport, create a Content object with:
///        - Vertical Layout Group (Control Child Size: Width ON, Height ON. Force Expand: Width ON, Height OFF)
///        - Content Size Fitter (Vertical Fit: Preferred Size)
///   3. Assign the Content object to the "contentContainer" field below
///   4. Assign text/image prefabs
///   5. Attach HTMLCodingSystem separately and assign it here
///
/// IMAGE FILES:
///   Static (.png/.jpg) → Assets/Resources/
///   Animated (.gif)    → Assets/StreamingAssets/
/// </summary>
public class HTMLUnifiedRenderer : MonoBehaviour
{
    [Header("References")]
    public HTMLCodingSystem htmlSystem;

    [Header("Output Layout")]
    [Tooltip("The Content object inside your ScrollView Viewport")]
    public RectTransform contentContainer;

    [Header("Output Panel Background")]
    public Image outputPanelBackground;

    [Header("Prefabs")]
    [Tooltip("Prefab with TextMeshProUGUI component — set preferred width to stretch")]
    public GameObject textPrefab;
    [Tooltip("Prefab with Image component")]
    public GameObject imagePrefab;

    [Header("Settings")]
    public Color defaultBackgroundColor = Color.white;
    public int defaultFontSize = 16;
    public int maxImageWidth = 400;
    public int maxImageHeight = 300;
    public Color errorColor = Color.red;
    public Color linkColor = new Color(0f, 0.75f, 1f);

    // GIF tracking
    private List<Coroutine> activeGifCoroutines = new List<Coroutine>();
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Start()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted += OnHTMLExecuted;
            htmlSystem.OnSystemCleared += ClearOutput;
        }
        else
            Debug.LogWarning("[Renderer] htmlSystem not assigned!");

        if (contentContainer == null)
            Debug.LogWarning("[Renderer] contentContainer not assigned!");

        if (outputPanelBackground != null)
        {
            outputPanelBackground.sprite = null;
            outputPanelBackground.color = defaultBackgroundColor;
        }
    }

    // ── Entry point ─────────────────────────────────────────────────────────

    void OnHTMLExecuted(string html)
    {
        ClearOutput();
        ApplyBodyBackground(html);
        StartCoroutine(RenderHTML(html));
    }

    // ── Background ──────────────────────────────────────────────────────────

    private void ApplyBodyBackground(string html)
    {
        if (outputPanelBackground == null) return;

        Match bodyMatch = Regex.Match(html,
            @"<body[^>]*style=""([^""]*)""[^>]*>", RegexOptions.IgnoreCase);

        if (bodyMatch.Success)
        {
            string style = bodyMatch.Groups[1].Value;

            Match bgImg = Regex.Match(style,
                @"background-image:\s*url\(['""]?([^'"")\s]+)['""]?\)", RegexOptions.IgnoreCase);
            if (bgImg.Success)
            {
                string fname = bgImg.Groups[1].Value.Trim();
                if (Path.GetExtension(fname).ToLower() == ".gif")
                {
                    // Animated GIF background
                    outputPanelBackground.color = Color.white;
                    StartCoroutine(LoadGifOntoImage(
                        Path.GetFileNameWithoutExtension(fname),
                        outputPanelBackground, -1, -1));
                    return;
                }
                else
                {
                    Sprite s = LoadSprite(fname);
                    if (s != null)
                    {
                        outputPanelBackground.sprite = s;
                        outputPanelBackground.color = Color.white;
                        outputPanelBackground.type = Image.Type.Sliced;
                        return;
                    }
                }
            }

            Match bgCol = Regex.Match(style,
                @"background-color:\s*([^;]+)", RegexOptions.IgnoreCase);
            if (bgCol.Success)
            {
                string hex = ConvertColorName(bgCol.Groups[1].Value.Trim());
                if (ColorUtility.TryParseHtmlString(hex, out Color c))
                {
                    outputPanelBackground.sprite = null;
                    outputPanelBackground.color = c;
                    return;
                }
            }
        }

        outputPanelBackground.sprite = null;
        outputPanelBackground.color = defaultBackgroundColor;
    }

    // ── Main renderer ───────────────────────────────────────────────────────

    private IEnumerator RenderHTML(string html)
    {
        html = Regex.Replace(html, @"<!DOCTYPE[^>]*>", "", RegexOptions.IgnoreCase);

        // Extract body content
        Match bodyMatch = Regex.Match(html,
            @"<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        string body = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;

        // Extract title
        Match titleMatch = Regex.Match(html,
            @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (titleMatch.Success)
            CreateTextBlock($"<size=24><b>{titleMatch.Groups[1].Value}</b></size>");

        // Split body into segments (text runs and img tags)
        yield return StartCoroutine(RenderSegments(body));

        // Force layout rebuild so scrolling works
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer);
    }

    // ── Segment splitter ────────────────────────────────────────────────────

    private IEnumerator RenderSegments(string html)
    {
        // Split on <img> tags — everything else is a text block
        string[] parts = Regex.Split(html,
            @"(<img[^>]*>)", RegexOptions.IgnoreCase);

        foreach (string part in parts)
        {
            if (string.IsNullOrWhiteSpace(part)) continue;

            if (Regex.IsMatch(part, @"^<img", RegexOptions.IgnoreCase))
            {
                // Image segment
                yield return StartCoroutine(RenderImageTag(part));
            }
            else
            {
                // Text segment — parse HTML tags into TMP rich text
                string parsed = ParseTextTags(part);
                if (!string.IsNullOrWhiteSpace(parsed))
                    CreateTextBlock(parsed);
            }
        }
    }

    // ── Text block creation ─────────────────────────────────────────────────

    private void CreateTextBlock(string tmpText)
    {
        if (textPrefab == null || contentContainer == null) return;

        GameObject obj = Instantiate(textPrefab, contentContainer);
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Destroy(obj); return; }

        tmp.text = tmpText.Trim();
        tmp.fontSize = defaultFontSize;
        tmp.richText = true;

        // Make sure it stretches full width
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1f);

        // Add link handler for clickable links
        if (obj.GetComponent<HTMLLinkHandler>() == null)
            obj.AddComponent<HTMLLinkHandler>();
    }

    // ── Image rendering ─────────────────────────────────────────────────────

    private IEnumerator RenderImageTag(string imgTag)
    {
        Match srcMatch = Regex.Match(imgTag,
            @"src=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (!srcMatch.Success) yield break;

        string filename = srcMatch.Groups[1].Value.Trim();
        string ext = Path.GetExtension(filename).ToLower();
        int w = GetIntAttr(imgTag, "width");
        int h = GetIntAttr(imgTag, "height");

        // Create image object in the unified content container
        if (imagePrefab == null || contentContainer == null) yield break;

        GameObject imgObj = Instantiate(imagePrefab, contentContainer);
        Image imgComp = imgObj.GetComponent<Image>();
        if (imgComp == null) { Destroy(imgObj); yield break; }

        imgComp.preserveAspect = (w <= 0 || h <= 0);

        // Handle alignment via a wrapper with HorizontalLayoutGroup
        RectTransform imgRt = imgComp.GetComponent<RectTransform>();

        if (ext == ".gif")
        {
            string nameNoExt = Path.GetFileNameWithoutExtension(filename);
            // Load first frame to get dimensions, then animate
            yield return StartCoroutine(LoadGifOntoImage(nameNoExt, imgComp, w, h));
        }
        else
        {
            Sprite sprite = LoadSprite(filename);
            if (sprite == null) { Destroy(imgObj); yield break; }

            imgComp.sprite = sprite;
            imgRt.sizeDelta = GetImageSize(
                sprite.texture.width, sprite.texture.height, w, h);
        }

        // Apply horizontal alignment
        ApplyImageAlignment(imgTag, imgObj);
    }

    private void ApplyImageAlignment(string attrs, GameObject imgObj)
    {
        string alignment = GetAlignmentString(attrs);
        RectTransform rt = imgObj.GetComponent<RectTransform>();

        switch (alignment)
        {
            case "left":
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case "right":
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            default: // center
                rt.anchorMin = new Vector2(0.5f, 1);
                rt.anchorMax = new Vector2(0.5f, 1);
                rt.pivot = new Vector2(0.5f, 1);
                break;
        }
    }

    private string GetAlignmentString(string attrs)
    {
        Match styleMatch = Regex.Match(attrs,
            @"style=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (styleMatch.Success)
        {
            string style = styleMatch.Groups[1].Value;
            if (Regex.IsMatch(style, @"float\s*:\s*left", RegexOptions.IgnoreCase)) return "left";
            if (Regex.IsMatch(style, @"float\s*:\s*right", RegexOptions.IgnoreCase)) return "right";
        }

        Match alignMatch = Regex.Match(attrs,
            @"align=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        if (alignMatch.Success)
            return alignMatch.Groups[1].Value.ToLower();

        return "center";
    }

    // ── Tag parser (text only, no img) ──────────────────────────────────────

    private string ParseTextTags(string content)
    {
        string result = content;

        // Styled tags
        result = Regex.Replace(result,
            @"<p\s+style=""([^""]+)""[^>]*>(.*?)</p>",
            m => ApplyStyles(m.Groups[2].Value, m.Groups[1].Value, "p"),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        result = Regex.Replace(result,
            @"<div\s+style=""([^""]+)""[^>]*>(.*?)</div>",
            m => ApplyStyles(m.Groups[2].Value, m.Groups[1].Value, "div"),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        result = Regex.Replace(result,
            @"<span\s+style=""([^""]+)""[^>]*>(.*?)</span>",
            m => ApplyStyles(m.Groups[2].Value, m.Groups[1].Value, "span"),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        for (int i = 1; i <= 6; i++)
        {
            int idx = i;
            result = Regex.Replace(result,
                $@"<h{idx}\s+style=""([^""]+)""[^>]*>(.*?)</h{idx}>",
                m => ApplyStyles(m.Groups[2].Value, m.Groups[1].Value, $"h{idx}"),
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        // Links
        result = Regex.Replace(result,
            @"<a[^>]*href=[""']([^""']+)[""'][^>]*>(.*?)</a>",
            m =>
            {
                string url = m.Groups[1].Value.Trim();
                string text = m.Groups[2].Value;
                if (!url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                    url = "https://" + url;
                return $"<link=\"{url}\"><color=#00BFFF><u>{text}</u></color></link>";
            },
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Unstyled headers
        for (int i = 1; i <= 6; i++)
        {
            int size = 30 - (i * 3);
            result = Regex.Replace(result,
                $@"<h{i}[^>]*>(.*?)</h{i}>",
                $"<size={size}><b>$1</b></size>\n",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        // Paragraphs
        result = Regex.Replace(result, @"<p[^>]*>(.*?)</p>", "$1\n\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Inline
        result = Regex.Replace(result, @"<(b|strong)[^>]*>(.*?)</(b|strong)>", "<b>$2</b>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<(i|em)[^>]*>(.*?)</(i|em)>", "<i>$2</i>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<u[^>]*>(.*?)</u>", "<u>$1</u>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // br / hr
        result = Regex.Replace(result, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<hr\s*/?>", "──────────────────\n", RegexOptions.IgnoreCase);

        // Lists
        result = Regex.Replace(result, @"<ul[^>]*>(.*?)</ul>", "\n$1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<ol[^>]*>(.*?)</ol>", "\n$1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<li[^>]*>(.*?)</li>", "• $1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Divs / spans unstyled
        result = Regex.Replace(result, @"<div[^>]*>(.*?)</div>", "$1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<span[^>]*>(.*?)</span>", "$1",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Strip remaining unknown tags, keep TMP tags
        result = Regex.Replace(result,
            @"<(?!\/?(?:color|size|b|i|u|s|mark|sup|sub|link)[\s=>\/])[^>]+>",
            "", RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"\n{3,}", "\n\n");
        return result.Trim();
    }

    // ── Style applicator ────────────────────────────────────────────────────

    private string ApplyStyles(string content, string styleAttr, string tagName)
    {
        Match borderMatch = Regex.Match(styleAttr, @"border:\s*([^;]+)", RegexOptions.IgnoreCase);
        if (borderMatch.Success)
        {
            string borderColor = "#FFFFFF";
            Match bcm = Regex.Match(borderMatch.Groups[1].Value,
                @"(red|blue|green|yellow|black|white|orange|purple|pink|gray|grey|#[0-9a-fA-F]{3,6})",
                RegexOptions.IgnoreCase);
            if (bcm.Success) borderColor = ConvertColorName(bcm.Groups[1].Value);
            string inner = ApplyInlineStyles(content, styleAttr, tagName);
            return $"<color={borderColor}>┌──────────────────┐</color>\n{inner}\n<color={borderColor}>└──────────────────┘</color>\n";
        }

        string result = ApplyInlineStyles(content, styleAttr, tagName);
        bool isBlock = tagName == "p" || tagName == "div" || tagName.StartsWith("h");
        return result + (isBlock ? "\n\n" : "");
    }

    private string ApplyInlineStyles(string content, string styleAttr, string tagName)
    {
        string open = "", close = "";

        Match colorMatch = Regex.Match(styleAttr, @"(?<![a-z-])color:\s*([^;]+)", RegexOptions.IgnoreCase);
        if (colorMatch.Success)
        {
            string c = ConvertColorName(colorMatch.Groups[1].Value.Trim());
            open += $"<color={c}>"; close = "</color>" + close;
        }

        Match sizeMatch = Regex.Match(styleAttr, @"font-size:\s*(\d+)", RegexOptions.IgnoreCase);
        if (sizeMatch.Success)
        {
            open += $"<size={sizeMatch.Groups[1].Value}>"; close = "</size>" + close;
        }

        if (Regex.IsMatch(styleAttr, @"font-weight:\s*bold", RegexOptions.IgnoreCase))
        {
            open += "<b>"; close = "</b>" + close;
        }

        if (Regex.IsMatch(styleAttr, @"font-style:\s*italic", RegexOptions.IgnoreCase))
        {
            open += "<i>"; close = "</i>" + close;
        }

        if (tagName.Length == 2 && tagName[0] == 'h' && char.IsDigit(tagName[1]))
        {
            open += "<b>"; close = "</b>" + close;
        }

        return open + content + close;
    }

    // ── GIF loader ──────────────────────────────────────────────────────────

    private IEnumerator LoadGifOntoImage(string nameNoExt, Image target, int forceW, int forceH)
    {
        string gifPath = Path.Combine(Application.streamingAssetsPath, nameNoExt + ".gif");

        if (!File.Exists(gifPath))
        {
            Debug.LogError($"[Renderer] GIF not found: {gifPath}");
            yield break;
        }

        byte[] bytes = File.ReadAllBytes(gifPath);
        List<UniGif.GifTexture> frames = null;
        int loopCount = 0;

        yield return StartCoroutine(
            UniGif.GetTextureListCoroutine(bytes,
                (list, loops, w, h) => { frames = list; loopCount = loops; }));

        if (frames == null || frames.Count == 0) yield break;

        // Set size from first frame if this is an img tag (not background)
        if (target.gameObject != outputPanelBackground?.gameObject)
        {
            RectTransform rt = target.GetComponent<RectTransform>();
            rt.sizeDelta = GetImageSize(
                frames[0].m_texture2d.width, frames[0].m_texture2d.height,
                forceW, forceH);
        }

        Coroutine c = StartCoroutine(AnimateGif(target, frames, loopCount));
        activeGifCoroutines.Add(c);
    }

    private IEnumerator AnimateGif(Image target, List<UniGif.GifTexture> frames, int loopCount)
    {
        int loops = 0;
        while (true)
        {
            foreach (UniGif.GifTexture frame in frames)
            {
                if (target == null) yield break;
                target.sprite = Sprite.Create(
                    frame.m_texture2d,
                    new Rect(0, 0, frame.m_texture2d.width, frame.m_texture2d.height),
                    new Vector2(0.5f, 0.5f));
                yield return new WaitForSeconds(frame.m_delaySec > 0 ? frame.m_delaySec : 0.1f);
            }
            loops++;
            if (loopCount > 0 && loops >= loopCount) break;
        }
    }

    // ── Static sprite loader ─────────────────────────────────────────────────

    private Sprite LoadSprite(string filename)
    {
        string nameNoExt = Path.GetFileNameWithoutExtension(filename);
        string key = nameNoExt.ToLower();

        if (spriteCache.TryGetValue(key, out Sprite cached)) return cached;

        Sprite sprite = Resources.Load<Sprite>(nameNoExt);
        if (sprite != null) { spriteCache[key] = sprite; return sprite; }

        Texture2D tex = Resources.Load<Texture2D>(nameNoExt);
        if (tex != null)
        {
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            spriteCache[key] = sprite;
            return sprite;
        }

        Debug.LogError($"[Renderer] Image not found in Resources: {nameNoExt}");
        return null;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private Vector2 GetImageSize(int texW, int texH, int forceW, int forceH)
    {
        if (forceW > 0 && forceH > 0) return new Vector2(forceW, forceH);
        float aspect = (float)texW / texH;
        if (forceW > 0) return new Vector2(forceW, forceW / aspect);
        if (forceH > 0) return new Vector2(forceH * aspect, forceH);
        if (texW > maxImageWidth) return new Vector2(maxImageWidth, maxImageWidth / aspect);
        if (texH > maxImageHeight) return new Vector2(maxImageHeight * aspect, maxImageHeight);
        return new Vector2(texW, texH);
    }

    private int GetIntAttr(string attrs, string attrName)
    {
        Match m = Regex.Match(attrs, $@"{attrName}=[""']?(\d+)[""']?", RegexOptions.IgnoreCase);
        return (m.Success && int.TryParse(m.Groups[1].Value, out int val)) ? val : -1;
    }

    private string ConvertColorName(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return "#FFFFFF";
        color = color.ToLower().Trim();
        if (color.StartsWith("#")) return color;
        switch (color)
        {
            case "red": return "#FF0000";
            case "blue": return "#0000FF";
            case "green": return "#008000";
            case "lime": return "#00FF00";
            case "yellow": return "#FFFF00";
            case "orange": return "#FFA500";
            case "purple": return "#800080";
            case "pink": return "#FFC0CB";
            case "black": return "#000000";
            case "white": return "#FFFFFF";
            case "gray":
            case "grey": return "#808080";
            case "cyan": return "#00FFFF";
            case "magenta": return "#FF00FF";
            case "maroon": return "#800000";
            case "navy": return "#000080";
            case "teal": return "#008080";
            case "silver": return "#C0C0C0";
            default: return "#FFFFFF";
        }
    }

    // ── Clear ────────────────────────────────────────────────────────────────

    public void ClearOutput()
    {
        foreach (Coroutine c in activeGifCoroutines)
            if (c != null) StopCoroutine(c);
        activeGifCoroutines.Clear();

        if (contentContainer != null)
            foreach (Transform child in contentContainer)
                Destroy(child.gameObject);

        if (outputPanelBackground != null)
        {
            outputPanelBackground.sprite = null;
            outputPanelBackground.color = defaultBackgroundColor;
        }
    }

    void OnDestroy()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted -= OnHTMLExecuted;
            htmlSystem.OnSystemCleared -= ClearOutput;
        }
    }
}