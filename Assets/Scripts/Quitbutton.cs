using UnityEngine;
using UnityEngine.UI;

public class QuitManager : MonoBehaviour
{
    [Header("UI References")]
    public Button quitButton;

    void Start()
    {
        quitButton.onClick.AddListener(QuitGame);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}