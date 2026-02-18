using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public GameObject fadeCanvasPrefab;

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneCoroutine(sceneName));
    }

    IEnumerator ChangeSceneCoroutine(string sceneName)
    {
        // Spawn fade if needed
        if (FadeTransition.Instance == null && fadeCanvasPrefab != null)
        {
            Instantiate(fadeCanvasPrefab);
            yield return null; // Wait one frame for it to initialize
        }

        // Now load with fade
        if (FadeTransition.Instance != null)
        {
            FadeTransition.Instance.LoadSceneWithFade(sceneName);
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}