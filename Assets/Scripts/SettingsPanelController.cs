using UnityEngine;

public class SettingsPanelController : MonoBehaviour
{
    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true);
    }
}