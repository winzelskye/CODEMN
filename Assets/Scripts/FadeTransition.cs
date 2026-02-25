using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Instance;

    public Image fadeImage;
    public float fadeDuration = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Start transparent
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeOutThenLoad(sceneName));
    }

    IEnumerator FadeOutThenLoad(string sceneName)
    {
        // STEP 1: FADE TO BLACK (transparent → black)
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = timer / fadeDuration; // 0 → 1
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1); // Fully black

        // STEP 2: LOAD THE SCENE
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);

        // Wait one frame for scene to load
        yield return null;

        // STEP 3: FADE FROM BLACK (black → transparent)
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = 1 - (timer / fadeDuration); // 1 → 0
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0); // Fully transparent
    }
}