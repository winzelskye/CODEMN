using UnityEngine;

public class PanelAudio : MonoBehaviour
{
    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        AudioSettings.Instance.StopMusicForPanel();
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
        AudioSettings.Instance.ResumeMusicAfterPanel();
    }
}