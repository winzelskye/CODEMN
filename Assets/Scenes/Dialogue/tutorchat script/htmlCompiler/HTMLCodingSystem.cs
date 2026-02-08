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

    [Header("Optional Rich Output")]
    public HTMLRichOutputRenderer richOutputRenderer; // Optional: for displaying actual images and clickable links

    [Header("Settings")]
    public Color errorColor = Color.red;

    // Events for external log system
    public System.Action<string> OnCodeExecuted;
    public System.Action<string> OnError;
    public System.Action OnSystemCleared;

    void Start()
    {
        runButton.onClick.AddListener(RunCode);
        clearButton.onClick.AddListener(ClearAll);
    }

    public void RunCode()
    {
        string rawCode = codeInputField.text;

        Debug.Log($"RAW CODE INPUT:\n{rawCode}");

        if (string.IsNullOrWhiteSpace(rawCode))
        {
            OnError?.Invoke("No code to execute");
            outputPanel.text = "<color=#FF0000>Error: No code to execute</color>";
            return;
        }

        // Validate HTML syntax first
        List<string> errors = ValidateHTML(rawCode);
        if (errors.Count > 0)
        {
            string errorMessage = "<color=#FF0000><b>Syntax Errors:</b></color>\n" + string.Join("\n", errors);
            outputPanel.text = errorMessage;
            OnError?.Invoke(string.Join("; ", errors));
            return;
        }

        // Normalize the code (remove extra spaces and newlines)
        string normalizedCode = NormalizeHTML(rawCode);

        Debug.Log($"NORMALIZED CODE:\n{normalizedCode}");

        // Notify external log system
        OnCodeExecuted?.Invoke(normalizedCode);

        // Parse and render HTML
        string renderedOutput = ParseAndRenderHTML(normalizedCode);

        Debug.Log($"FINAL OUTPUT:\n{renderedOutput}");

        // Use rich output renderer if available, otherwise use basic text output
        if (richOutputRenderer != null)
        {
            richOutputRenderer.RenderHTML(renderedOutput);
        }
        else
        {
            outputPanel.text = renderedOutput;
        }
    }

    private List<string> ValidateHTML(string html)
    {
        List<string> errors = new List<string>();

        // Check for required structure
        bool hasHtml = Regex.IsMatch(html, @"<html[^>]*>", RegexOptions.IgnoreCase);
        bool hasBody = Regex.IsMatch(html, @"<body[^>]*>", RegexOptions.IgnoreCase);

        if (!hasHtml)
        {
            errors.Add("Missing <html> tag");
        }
        if (!hasBody)
        {
            errors.Add("Missing <body> tag");
        }

        // Check for unclosed/incomplete tags
        Stack<string> tagStack = new Stack<string>();

        // Find all tags
        MatchCollection tags = Regex.Matches(html, @"<(/?)(\w+)([^>]*)(/?)>", RegexOptions.IgnoreCase);

        foreach (Match tag in tags)
        {
            bool isClosing = tag.Groups[1].Value == "/";
            string tagName = tag.Groups[2].Value.ToLower();
            string attributes = tag.Groups[3].Value;
            bool isSelfClosing = tag.Groups[4].Value == "/";

            // Self-closing tags (ignore)
            if (isSelfClosing || tagName == "br" || tagName == "hr" || tagName == "img" || tagName == "meta" || tagName == "link")
                continue;

            if (isClosing)
            {
                // Closing tag
                if (tagStack.Count == 0)
                {
                    errors.Add($"Unexpected closing tag </{tagName}>");
                }
                else if (tagStack.Peek().ToLower() != tagName)
                {
                    errors.Add($"Mismatched closing tag: expected </{tagStack.Peek()}>, found </{tagName}>");
                }
                else
                {
                    tagStack.Pop();
                }
            }
            else
            {
                // Opening tag
                tagStack.Push(tagName);
            }
        }

        // Check for unclosed tags
        if (tagStack.Count > 0)
        {
            foreach (string unclosedTag in tagStack)
            {
                errors.Add($"Unclosed tag: <{unclosedTag}>");
            }
        }

        // Check for incomplete tags (tags without closing >)
        if (Regex.IsMatch(html, @"<\w+[^>]*$", RegexOptions.Multiline))
        {
            errors.Add("Incomplete tag found (missing closing >)");
        }

        return errors;
    }

    public void ClearAll()
    {
        codeInputField.text = "";
        outputPanel.text = "";
        OnSystemCleared?.Invoke();
    }

    private string NormalizeHTML(string html)
    {
        // Remove extra whitespace OUTSIDE of tags
        // First, collapse multiple spaces/newlines into single spaces
        html = Regex.Replace(html, @"\s+", " ");

        // Trim leading/trailing whitespace
        html = html.Trim();

        // Remove spaces after opening < and before closing >
        html = Regex.Replace(html, @"<\s+", "<");
        html = Regex.Replace(html, @"\s+>", ">");

        // Remove spaces around = BUT preserve spaces inside quoted attribute values
        html = Regex.Replace(html, @"\s*=\s*([""'])", "=$1");

        return html;
    }

    private string ParseAndRenderHTML(string html)
    {
        try
        {
            // Convert HTML to displayable text
            string output = "";

            // Remove doctype
            html = Regex.Replace(html, @"<!DOCTYPE[^>]*>", "", RegexOptions.IgnoreCase);

            // Extract title
            Match titleMatch = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase);
            if (titleMatch.Success)
            {
                output += $"<size=24><b>{titleMatch.Groups[1].Value}</b></size>\n\n";
            }

            // Extract body content
            Match bodyMatch = Regex.Match(html, @"<body[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string bodyContent = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;

            // Parse common HTML tags
            output += ParseHTMLTags(bodyContent);

            return output;
        }
        catch (System.Exception e)
        {
            OnError?.Invoke($"Parse error: {e.Message}");
            return $"<color=#{ColorUtility.ToHtmlStringRGB(errorColor)}>Parse Error</color>";
        }
    }

    private string ParseHTMLTags(string content)
    {
        string result = content;

        Debug.Log($"ParseHTMLTags INPUT: {content}");

        // FIRST: Parse all styled tags
        // Handle <p style="...">
        var pMatches = Regex.Matches(result, @"<p\s+style=""([^""]+)""[^>]*>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        Debug.Log($"Found {pMatches.Count} <p> tags with style");

        result = Regex.Replace(result,
            @"<p\s+style=""([^""]+)""[^>]*>(.*?)</p>",
            match => {
                Debug.Log($"Matched P tag: style='{match.Groups[1].Value}' content='{match.Groups[2].Value}'");
                string styleAttr = match.Groups[1].Value;
                string innerContent = match.Groups[2].Value;
                return ApplyStyles(innerContent, styleAttr, "p");
            },
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Handle <div style="...">
        var divMatches = Regex.Matches(result, @"<div\s+style=""([^""]+)""[^>]*>(.*?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        Debug.Log($"Found {divMatches.Count} <div> tags with style");

        result = Regex.Replace(result,
            @"<div\s+style=""([^""]+)""[^>]*>(.*?)</div>",
            match => {
                Debug.Log($"Matched DIV tag: style='{match.Groups[1].Value}' content='{match.Groups[2].Value}'");
                string styleAttr = match.Groups[1].Value;
                string innerContent = match.Groups[2].Value;
                return ApplyStyles(innerContent, styleAttr, "div");
            },
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Handle <span style="...">
        result = Regex.Replace(result,
            @"<span\s+style=""([^""]+)""[^>]*>(.*?)</span>",
            match => {
                string styleAttr = match.Groups[1].Value;
                string innerContent = match.Groups[2].Value;
                return ApplyStyles(innerContent, styleAttr, "span");
            },
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        // Handle <h1-h6 style="...">
        for (int i = 1; i <= 6; i++)
        {
            result = Regex.Replace(result,
                $@"<h{i}\s+style=""([^""]+)""[^>]*>(.*?)</h{i}>",
                match => {
                    string styleAttr = match.Groups[1].Value;
                    string innerContent = match.Groups[2].Value;
                    return ApplyStyles(innerContent, styleAttr, $"h{i}");
                },
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        Debug.Log($"After styled tags: {result}");

        // Headers WITHOUT styles (h1-h6)
        for (int i = 1; i <= 6; i++)
        {
            int size = 30 - (i * 3);
            result = Regex.Replace(result, $@"<h{i}[^>]*>(.*?)</h{i}>",
                $"<size={size}><b>$1</b></size>\n", RegexOptions.IgnoreCase);
        }

        // Paragraph WITHOUT styles
        result = Regex.Replace(result, @"<p[^>]*>(.*?)</p>", "$1\n\n", RegexOptions.IgnoreCase);

        // Bold
        result = Regex.Replace(result, @"<b[^>]*>(.*?)</b>", "<b>$1</b>", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<strong[^>]*>(.*?)</strong>", "<b>$1</b>", RegexOptions.IgnoreCase);

        // Italic
        result = Regex.Replace(result, @"<i[^>]*>(.*?)</i>", "<i>$1</i>", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<em[^>]*>(.*?)</em>", "<i>$1</i>", RegexOptions.IgnoreCase);

        // Underline
        result = Regex.Replace(result, @"<u[^>]*>(.*?)</u>", "<u>$1</u>", RegexOptions.IgnoreCase);

        // Line break
        result = Regex.Replace(result, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

        // Horizontal rule
        result = Regex.Replace(result, @"<hr\s*/?>", "───────────────────\n", RegexOptions.IgnoreCase);

        // Lists
        result = Regex.Replace(result, @"<ul[^>]*>(.*?)</ul>", "\n$1\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<ol[^>]*>(.*?)</ol>", "\n$1\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<li[^>]*>(.*?)</li>", "• $1\n", RegexOptions.IgnoreCase);

        // Divs and spans WITHOUT styles (just extract content)
        result = Regex.Replace(result, @"<div[^>]*>(.*?)</div>", "$1\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<span[^>]*>(.*?)</span>", "$1", RegexOptions.IgnoreCase);

        // Links - make them clickable using TMP link tags
        result = Regex.Replace(result, @"<a[^>]*href=[""']([^""']+)[""'][^>]*>(.*?)</a>",
            match => {
                string url = match.Groups[1].Value;
                string linkText = match.Groups[2].Value;
                // If rich renderer is available, use link tags, otherwise show URL
                return $"<color=#00BFFF><u>{linkText}</u></color> <color=#808080>({url})</color>";
            },
            RegexOptions.IgnoreCase);

        // Images - support both src and alt attributes
        result = Regex.Replace(result, @"<img[^>]*src=[""']([^""']+)[""']([^>]*alt=[""']([^""']+)[""'])?[^>]*>",
            match => {
                string src = match.Groups[1].Value;
                string alt = match.Groups[3].Success ? match.Groups[3].Value : "image";

                // Check if it's a file path or URL
                if (src.StartsWith("http://") || src.StartsWith("https://") || src.StartsWith("www."))
                {
                    return $"<color=#FFA500>[🖼️ Image: {alt}]</color>\n<color=#808080><i>{src}</i></color>";
                }
                else
                {
                    return $"<color=#FFA500>[🖼️ {alt}]</color> <color=#808080>({src})</color>";
                }
            },
            RegexOptions.IgnoreCase);

        // Remove remaining HTML tags BUT NOT TextMeshPro tags
        // Only remove tags that aren't TMP rich text tags (color, size, b, i, u, link)
        result = Regex.Replace(result, @"<(?!(color|/color|size|/size|b|/b|i|/i|u|/u|link|/link)[\s=>])[^>]+>", "", RegexOptions.IgnoreCase);

        // Clean up excessive newlines
        result = Regex.Replace(result, @"\n{3,}", "\n\n");

        Debug.Log($"ParseHTMLTags OUTPUT: {result}");

        return result.Trim();
    }

    private string ApplyStyles(string content, string styleAttr, string tagName)
    {
        string result = content;
        string openTags = "";
        string closeTags = "";

        // Parse color
        Match colorMatch = Regex.Match(styleAttr, @"color:\s*([^;]+)", RegexOptions.IgnoreCase);
        if (colorMatch.Success)
        {
            string color = colorMatch.Groups[1].Value.Trim();
            color = ConvertColorName(color);
            // Try both formats - TMP might be picky
            openTags += $"<color={color}>";
            closeTags = "</color>" + closeTags;
        }

        // Parse font-size
        Match sizeMatch = Regex.Match(styleAttr, @"font-size:\s*(\d+)", RegexOptions.IgnoreCase);
        if (sizeMatch.Success)
        {
            string size = sizeMatch.Groups[1].Value;
            openTags += $"<size={size}>";
            closeTags = "</size>" + closeTags;
        }

        // Apply default formatting for certain tags
        if (tagName.ToLower().StartsWith("h"))
        {
            // Headers should be bold by default
            openTags += "<b>";
            closeTags = "</b>" + closeTags;
        }

        // Parse border (visual representation)
        Match borderMatch = Regex.Match(styleAttr, @"border:\s*([^;]+)", RegexOptions.IgnoreCase);
        if (borderMatch.Success)
        {
            string border = borderMatch.Groups[1].Value.Trim();
            // Extract border color if specified
            string borderColor = "#FFFFFF";
            Match borderColorMatch = Regex.Match(border, @"(red|blue|green|yellow|black|white|orange|purple|pink|gray|#[0-9a-fA-F]{6})", RegexOptions.IgnoreCase);
            if (borderColorMatch.Success)
            {
                borderColor = ConvertColorName(borderColorMatch.Groups[1].Value);
            }

            // Get text color from style if specified
            Match textColorMatch = Regex.Match(styleAttr, @"color:\s*([^;]+)", RegexOptions.IgnoreCase);
            string textColor = "";
            string textColorClose = "";
            if (textColorMatch.Success)
            {
                string txtCol = ConvertColorName(textColorMatch.Groups[1].Value.Trim());
                textColor = $"<color={txtCol}>";
                textColorClose = "</color>";
            }

            // Create visual border with colored box characters
            string topBorder = $"<color={borderColor}>┌──────────────────┐</color>";
            string bottomBorder = $"<color={borderColor}>└──────────────────┘</color>";

            result = $"{topBorder}\n{textColor}{content}{textColorClose}\n{bottomBorder}";
            return openTags + result + closeTags + "\n";
        }

        // Add newline for block elements
        string lineBreak = (tagName.ToLower() == "p" || tagName.ToLower() == "div") ? "\n\n" : "";

        // DEBUG: Log what we're generating
        string finalOutput = openTags + result + closeTags + lineBreak;
        Debug.Log($"Style Applied - Tag: {tagName}, Styles: {styleAttr}, Output: {finalOutput}");

        return finalOutput;
    }

    private string ConvertColorName(string color)
    {
        // Convert common CSS color names to Unity color format
        color = color.ToLower().Trim();

        // Already a hex color - ensure it has # prefix
        if (color.StartsWith("#"))
            return color;

        // Common color names - TextMeshPro needs hex format
        switch (color)
        {
            case "red": return "#FF0000";
            case "blue": return "#0000FF";
            case "green": return "#00FF00";
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
            case "lime": return "#00FF00";
            case "maroon": return "#800000";
            case "navy": return "#000080";
            default: return "#FFFFFF"; // Default to white if color not recognized
        }
    }

    // Public method to get normalized code (useful for your log system)
    public string GetNormalizedCode(string rawCode)
    {
        return NormalizeHTML(rawCode);
    }

    // Public method to parse HTML without running (useful for validation)
    public string ParseHTML(string html)
    {
        return ParseAndRenderHTML(html);
    }
}