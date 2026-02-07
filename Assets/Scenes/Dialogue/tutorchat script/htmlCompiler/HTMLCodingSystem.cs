using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class HTMLCodingSystem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField codeInputField;
    public TextMeshProUGUI outputPanel;
    public Button runButton;
    public Button clearButton;

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

        if (string.IsNullOrWhiteSpace(rawCode))
        {
            OnError?.Invoke("No code to execute");
            return;
        }

        // Normalize the code (remove extra spaces and newlines)
        string normalizedCode = NormalizeHTML(rawCode);

        // Notify external log system
        OnCodeExecuted?.Invoke(normalizedCode);

        // Parse and render HTML
        string renderedOutput = ParseAndRenderHTML(normalizedCode);
        outputPanel.text = renderedOutput;
    }

    public void ClearAll()
    {
        codeInputField.text = "";
        outputPanel.text = "";
        OnSystemCleared?.Invoke();
    }

    private string NormalizeHTML(string html)
    {
        // Remove extra whitespace while preserving single spaces
        html = Regex.Replace(html, @"\s+", " ");
        // Trim leading/trailing whitespace
        html = html.Trim();
        // Remove spaces around < and >
        html = Regex.Replace(html, @"\s*<\s*", "<");
        html = Regex.Replace(html, @"\s*>\s*", ">");
        // Remove spaces around =
        html = Regex.Replace(html, @"\s*=\s*", "=");

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

        // Parse tags with style attributes first (before generic tag parsing)
        result = ParseStyledTags(result);

        // Headers (h1-h6)
        for (int i = 1; i <= 6; i++)
        {
            int size = 30 - (i * 3);
            result = Regex.Replace(result, $@"<h{i}[^>]*>(.*?)</h{i}>",
                $"<size={size}><b>$1</b></size>\n", RegexOptions.IgnoreCase);
        }

        // Paragraph
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

        // Divs and spans (just extract content)
        result = Regex.Replace(result, @"<div[^>]*>(.*?)</div>", "$1\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result = Regex.Replace(result, @"<span[^>]*>(.*?)</span>", "$1", RegexOptions.IgnoreCase);

        // Links
        result = Regex.Replace(result, @"<a[^>]*href=[""'](.*?)[""'][^>]*>(.*?)</a>",
            "<color=blue><u>$2</u></color> ($1)", RegexOptions.IgnoreCase);

        // Images (just show alt text)
        result = Regex.Replace(result, @"<img[^>]*alt=[""'](.*?)[""'][^>]*>",
            "[Image: $1]", RegexOptions.IgnoreCase);

        // Remove remaining HTML tags
        result = Regex.Replace(result, @"<[^>]+>", "");

        // Clean up excessive newlines
        result = Regex.Replace(result, @"\n{3,}", "\n\n");

        return result.Trim();
    }

    private string ParseStyledTags(string content)
    {
        // Match any opening tag with style attribute
        MatchCollection matches = Regex.Matches(content, @"<(\w+)[^>]*style=[""']([^""']*)[""'][^>]*>(.*?)</\1>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            string tagName = match.Groups[1].Value;
            string styleAttr = match.Groups[2].Value;
            string innerContent = match.Groups[3].Value;

            string replacement = ApplyStyles(innerContent, styleAttr);
            content = content.Replace(match.Value, replacement);
        }

        return content;
    }

    private string ApplyStyles(string content, string styleAttr)
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

        // Parse border (visual representation)
        Match borderMatch = Regex.Match(styleAttr, @"border:\s*([^;]+)", RegexOptions.IgnoreCase);
        if (borderMatch.Success)
        {
            string border = borderMatch.Groups[1].Value.Trim();
            // Extract border color if specified
            string borderColor = "white";
            Match borderColorMatch = Regex.Match(border, @"(red|blue|green|yellow|black|white|orange|purple|pink|gray|#[0-9a-fA-F]{6})", RegexOptions.IgnoreCase);
            if (borderColorMatch.Success)
            {
                borderColor = ConvertColorName(borderColorMatch.Groups[1].Value);
            }

            // Create visual border using box drawing characters
            result = $"<color={borderColor}>┌─────────────┐</color>\n{content}\n<color={borderColor}>└─────────────┘</color>";
            return openTags + result + closeTags;
        }

        return openTags + result + closeTags;
    }

    private string ConvertColorName(string color)
    {
        // Convert common CSS color names to Unity color format
        color = color.ToLower().Trim();

        // Already a hex color
        if (color.StartsWith("#"))
            return color;

        // Common color names
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
            default: return color; // Return as-is if not recognized
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