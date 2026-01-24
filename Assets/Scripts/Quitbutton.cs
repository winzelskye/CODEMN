using UnityEngine;
using UnityEngine.UI;

public class QuitManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject confirmationPanel;
    public Button quitButton;
    public Button yesButton;
    public Button noButton;

    void Start()
    {
        // Hide confirmation panel at start
        confirmationPanel.SetActive(false);

        // Add button listeners
        quitButton.onClick.AddListener(ShowConfirmation);
        yesButton.onClick.AddListener(QuitGame);
        noButton.onClick.AddListener(CancelQuit);
    }

    void ShowConfirmation()
    {
        confirmationPanel.SetActive(true);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void CancelQuit()
    {
        confirmationPanel.SetActive(false);
    }
}