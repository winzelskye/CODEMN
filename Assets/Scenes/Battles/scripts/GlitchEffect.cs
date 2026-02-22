using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    public float glitchDuration = 0.5f;
    public float glitchIntensity = 10f;
    public int glitchFlashes = 5;

    [Header("References")]
    public RawImage glitchOverlay;

    private bool isGlitching = false;

    void Start()
    {
        if (glitchOverlay != null)
            glitchOverlay.gameObject.SetActive(false);
    }

    public void TriggerGlitch()
    {
        if (!isGlitching)
            StartCoroutine(GlitchCoroutine());
    }

    private IEnumerator GlitchCoroutine()
    {
        isGlitching = true;

        if (glitchOverlay != null)
            glitchOverlay.gameObject.SetActive(true);

        float elapsed = 0f;
        float flashInterval = glitchDuration / glitchFlashes;

        while (elapsed < glitchDuration)
        {
            if (glitchOverlay != null)
            {
                float offsetX = Random.Range(-glitchIntensity, glitchIntensity);
                float offsetY = Random.Range(-glitchIntensity, glitchIntensity);
                glitchOverlay.uvRect = new Rect(
                    offsetX / Screen.width,
                    offsetY / Screen.height,
                    1f, 1f
                );

                glitchOverlay.color = new Color(
                    Random.Range(0.8f, 1f),
                    Random.Range(0f, 0.3f),
                    Random.Range(0f, 0.3f),
                    Random.Range(0.1f, 0.3f)
                );
            }

            elapsed += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        if (glitchOverlay != null)
        {
            glitchOverlay.uvRect = new Rect(0, 0, 1, 1);
            glitchOverlay.gameObject.SetActive(false);
        }

        isGlitching = false;
    }
}