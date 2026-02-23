using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HTMLCodingSystem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField codeInputField;
    public TextMeshProUGUI outputPanel;
    public Button runButton;
    public Button clearButton;

    [Header("Output Panel Background")]
    public Image outputPanelBackground;
    public HTMLSimpleImageSystem imageSystem;

    [Header("Settings")]
    public Color errorColor = Color.red;
    public Color defaultBackgroundColor = Color.white;

    public System.Action<string> OnCodeExecuted;
    public System.Action<string> OnError;
    public System.Action OnSystemCleared;

    void Start()
    {
        runButton.onClick.AddListener(RunCode);
        clearButton.onClick.AddListener(ClearAll);

        // Disable rich text in INPUT field so tags show as plain text while coding
        if (codeInputField != null)
            codeInputField.richText = false;

        // Auto-add link handler to output panel if missing
        if (outputPanel != null && outputPanel.GetComponent<HTMLLinkHandler>() == null)
            outputPanel.gameObject.AddComponent<HTMLLinkHandler>();

        // Apply default background color immediately on start
        if (outputPanelBackground != null)
        {
            outputPanelBackground.sprite = null;
            outputPanelBackground.color = defaultBackgroundColor;
        }
    }

    public void RunCode()
    {
        string rawCode = codeInputField.text;

        if (string.IsNullOrWhiteSpace(rawCode))
        {
            OnError?.Invoke("No code to execute");
            outputPanel.text = "<color=#FF0000>Error: No code to execute</color>";
            return;
        }

        List<string> errors = ValidateHTML(rawCode);
        if (errors.Count > 0)
        {
            outputPanel.text = "<color=#FF0000><b>Syntax Errors:</b></color>\n" + string.Join("\n", errors);
            OnError?.Invoke(string.Join("; ", errors));
            return;
        }

        string normalizedCode = NormalizeHTML(rawCode);
        OnCodeExecuted?.Invoke(normalizedCode);
        ApplyBodyBackground(normalizedCode);

        string renderedOutput = ParseAndRenderHTML(normalizedCode);
        outputPanel.text = renderedOutput;
    }

    // ── Background ──────────────────────────────────────────────────────────

    private void ApplyBodyBackground(string html)
    {
        if (outputPanelBackground == null) return;

        Match bodyTagMatch = Regex.Match(html,
            @"<body[^>]*style=""([^""]*)""[^>]*>", RegexOptions.IgnoreCase);

        if (bodyTagMatch.Success)
        {
            string bodyStyle = bodyTagMatch.Groups[1].Value;

            // background-image: url('name')
            Match bgImageMatch = Regex.Match(bodyStyle,
                @"background-image:\s*url\(['""]?([^'"")\s]+)['""]?\)", RegexOptions.IgnoreCase);
            if (bgImageMatch.Success)
            {
                string imgName = bgImageMatch.Groups[1].Value.Trim();
                Sprite bgSprite = imageSystem != null ? imageSystem.GetSprite(imgName) : null;
                if (bgSprite != null)
                {
                    outputPanelBackground.sprite = bgSprite;
                    outputPanelBackground.color = Color.white;
                    outputPanelBackground.type = Image.Type.Sliced;
                    return;
                }
            }

            // background-color: value
            Match bgColorMatch = Regex.Match(bodyStyle,
                @"background-color:\s*([^;]+)", RegexOptions.IgnoreCase);
            if (bgColorMatch.Success)
            {
                string colorStr = ConvertColorName(bgColorMatch.Groups[1].Value.Trim());
                if (ColorUtility.TryParseHtmlString(colorStr, out Color parsedColor))
                {
                    outputPanelBackground.sprite = null;
                    outputPanelBackground.color = parsedColor;
                    return;
                }
            }
        }

        // Default
        outputPanelBackground.sprite = null;
        outputPanelBackground.color = defaultBackgroundColor;
    }

    // ── Validation ──────────────────────────────────────────────────────────

    private List<string> ValidateHTML(string html)
    {
        List<string> errors = new List<string>();

        if (!Regex.IsMatch(html, @"<html[^>]*>", RegexOptions.IgnoreCase))
            errors.Add("Missing <html> tag");
        if (!Regex.IsMatch(html, @"<body[^>]*>", RegexOptions.IgnoreCase))
            errors.Add("Missing <body> tag");

        Stack<string> tagStack = new Stack<string>();
        HashSet<string> voidTags = new HashSet<string>
            { "br", "hr", "img", "meta", "link", "input" };

        foreach (Match tag in Regex.Matches(html, @"<(/?)(\w+)([^>]*)(/?)>", RegexOptions.IgnoreCase))
        {
            bool isClosing = tag.Groups[1].Value == "/";
            string tagName = tag.Groups[2].Value.ToLower();
            bool isSelfClosing = tag.Groups[4].Value == "/";

            if (isSelfClosing || voidTags.Contains(tagName)) continue;

            if (isClosing)
            {
                if (tagStack.Count == 0)
                    errors.Add($"Unexpected closing tag </{tagName}>");
                else if (tagStack.Peek() != tagName)
                    errors.Add($"Mismatched tag: expected </{tagStack.Peek()}>, found </{tagName}>");
                else
                    tagStack.Pop();
            }
            else
            {
                tagStack.Push(tagName);
            }
        }

        foreach (string t in tagStack)
            errors.Add($"Unclosed tag: <{t}>");

        if (Regex.IsMatch(html, @"<\w+[^>]*$", RegexOptions.Multiline))
            errors.Add("Incomplete tag (missing closing >)");

        return errors;
    }

    // ── Clear ───────────────────────────────────────────────────────────────

    public void ClearAll()
    {
        codeInputField.text = "";
        if (outputPanel != null) outputPanel.text = "";
        if (outputPanelBackground != null)
        {
            outputPanelBackground.sprite = null;
            outputPanelBackground.color = defaultBackgroundColor;
        }
        OnSystemCleared?.Invoke();
    }

    // ── Normalise ───────────────────────────────────────────────────────────

    private string NormalizeHTML(string html)
    {
        html = Regex.Replace(html, @"\s+", " ").Trim();
        html = Regex.Replace(html, @"<\s+", "<");
        html = Regex.Replace(html, @"\s+>", ">");
        html = Regex.Replace(html, @"\s*=\s*([""'])", "=$1");
        return html;
    }

    // ── Parse & Render ──────────────────────────────────────────────────────

    private string ParseAndRenderHTML(string html)
    {
        try
        {
            string output = "";
            html = Regex.Replace(html, @"<!DOCTYPE[^>]*>", "", RegexOptions.IgnoreCase);

            Match titleMatch = Regex.Match(html, @"<title[^>]*>(.*?)</title>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (titleMatch.Success)
                output += $"<size=24><b>{titleMatch.Groups[1].Value}</b></size>\n\n";

            Match bodyMatch = Regex.Match(html, @"<body[^>]*>(.*?)</body>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string bodyContent = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;

            output += ParseHTMLTags(bodyContent);
            return output;
        }
        catch (System.Exception e)
        {
            OnError?.Invoke($"Parse error: {e.Message}");
            return $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>Parse Error: {e.Message}</color>";
        }
    }

    // ── Tag Parser ──────────────────────────────────────────────────────────

    private string ParseHTMLTags(string content)
    {
        string result = content;

        // Styled block tags first
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

        // Images — stripped here, HTMLSimpleImageSystem handles actual display
        result = Regex.Replace(result, @"<img[^>]*>", "", RegexOptions.IgnoreCase);

        // Links — TMP link tag so HTMLLinkHandler can detect clicks
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

        // Inline formatting
        result = Regex.Replace(result, @"<(b|strong)[^>]*>(.*?)</(b|strong)>", "<b>$2</b>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<(i|em)[^>]*>(.*?)</(i|em)>", "<i>$2</i>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<u[^>]*>(.*?)</u>", "<u>$1</u>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Line break / rule
        result = Regex.Replace(result, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<hr\s*/?>", "──────────────────\n", RegexOptions.IgnoreCase);

        // Lists
        result = Regex.Replace(result, @"<ul[^>]*>(.*?)</ul>", "\n$1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<ol[^>]*>(.*?)</ol>", "\n$1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<li[^>]*>(.*?)</li>", "• $1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Unstyled divs / spans
        result = Regex.Replace(result, @"<div[^>]*>(.*?)</div>", "$1\n",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<span[^>]*>(.*?)</span>", "$1",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Strip remaining unknown HTML — preserve TMP rich-text tags
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
            string border = borderMatch.Groups[1].Value;
            string borderColor = "#FFFFFF";
            Match bcm = Regex.Match(border,
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

    // ── Color helper ────────────────────────────────────────────────────────

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

    public string GetNormalizedCode(string rawCode) => NormalizeHTML(rawCode);
    public string ParseHTML(string html) => ParseAndRenderHTML(html);
}