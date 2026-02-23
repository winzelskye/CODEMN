using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GifBackground : MonoBehaviour
{
    [Header("References")]
    public Image backgroundImage;

    [Header("GIF Settings")]
    public string gifFileName; // e.g. "mybg.gif"

    private List<UniGif.GifTexture> gifTextures;
    private Coroutine playCoroutine;

    void Start()
    {
        StartCoroutine(LoadGif());
    }

    IEnumerator LoadGif()
    {
        string path = Path.Combine(Application.streamingAssetsPath, gifFileName);
        byte[] bytes = File.ReadAllBytes(path);

        yield return StartCoroutine(UniGif.GetTextureListCoroutine(
            bytes,
            (textures, loops, width, height) =>
            {
                gifTextures = textures;
            }
        ));

        if (gifTextures != null && gifTextures.Count > 0)
            playCoroutine = StartCoroutine(PlayGif());
        else
            Debug.LogWarning($"[GifBackground] Failed to load: {gifFileName}");
    }

    IEnumerator PlayGif()
    {
        int frame = 0;
        while (true)
        {
            var gifFrame = gifTextures[frame];
            backgroundImage.sprite = Sprite.Create(
                gifFrame.m_texture2d,
                new Rect(0, 0, gifFrame.m_texture2d.width, gifFrame.m_texture2d.height),
                new Vector2(0.5f, 0.5f)
            );
            yield return new WaitForSeconds(gifFrame.m_delaySec);
            frame = (frame + 1) % gifTextures.Count;
        }
    }

    void OnDestroy()
    {
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);
    }
}