using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    private Button settingsButton;

    private void Awake()
    {
        settingsButton = GetComponent<Button>();
    }

    private void Start()
    {
        settingsButton.onClick.AddListener(OpenSettings);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        AudioSettings.Instance?.StopMusicForPanel();
        Time.timeScale = 0f;
    }
}