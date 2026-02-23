using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Handles animated GIF backgrounds set via the body tag.
/// Works alongside HTMLSimpleImageSystem which handles <img> GIFs.
///
/// USAGE IN HTML:
///   <body style="background-image: url('confusedjohn.gif')">
///
/// SETUP:
///   1. Place .gif files in Assets/StreamingAssets/ (keep .gif extension, no renaming)
///   2. Attach this to your HTMLSystem GameObject
///   3. Assign outputPanelBackground and htmlSystem in Inspector
/// </summary>
public class HTMLGifBackground : MonoBehaviour
{
    [Header("References")]
    public HTMLCodingSystem htmlSystem;
    public Image outputPanelBackground;
    public HTMLSimpleImageSystem imageSystem;

    [Header("Settings")]
    public Color defaultBackgroundColor = Color.white;

    private Coroutine gifCoroutine;

    void Start()
    {
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted += OnHTMLExecuted;
            htmlSystem.OnSystemCleared += StopGif;
        }
        else
            Debug.LogWarning("[GifBackground] htmlSystem not assigned!");
    }

    void OnHTMLExecuted(string html)
    {
        StopGif();

        Match bodyMatch = Regex.Match(html,
            @"<body[^>]*style=""([^""]*)""[^>]*>", RegexOptions.IgnoreCase);

        if (!bodyMatch.Success) return;

        string bodyStyle = bodyMatch.Groups[1].Value;

        Match bgMatch = Regex.Match(bodyStyle,
            @"background-image:\s*url\(['""]?([^'"")\s]+)['""]?\)", RegexOptions.IgnoreCase);

        if (!bgMatch.Success) return;

        string filename = bgMatch.Groups[1].Value.Trim();
        string ext = Path.GetExtension(filename).ToLower();

        if (ext != ".gif") return; // Non-GIF backgrounds handled by HTMLCodingSystem

        string nameNoExt = Path.GetFileNameWithoutExtension(filename);

        if (outputPanelBackground != null && imageSystem != null)
        {
            outputPanelBackground.color = Color.white;
            gifCoroutine = imageSystem.StartGifOnImage(nameNoExt, outputPanelBackground);
        }
        else
        {
            Debug.LogWarning("[GifBackground] outputPanelBackground or imageSystem not assigned!");
        }
    }

    public void StopGif()
    {
        if (gifCoroutine != null)
        {
            StopCoroutine(gifCoroutine);
            gifCoroutine = null;
        }

        if (outputPanelBackground != null)
        {
            outputPanelBackground.sprite = null;
            outputPanelBackground.color = defaultBackgroundColor;
        }
    }

    void OnDestroy()
    {
        StopGif();
        if (htmlSystem != null)
        {
            htmlSystem.OnCodeExecuted -= OnHTMLExecuted;
            htmlSystem.OnSystemCleared -= StopGif;
        }
    }
}