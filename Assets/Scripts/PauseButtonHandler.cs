using UnityEngine;

public class PauseButtonHandler : MonoBehaviour
{
    public void ReturnToGame()
    {
        PauseManager.Instance.ReturnToGame();
    }

    public void OpenSettings()
    {
        PauseManager.Instance.OpenSettings();
    }

    public void LoadMainMenu()
    {
        PauseManager.Instance.LoadMainMenu();
    }

    public void CloseSettings()
    {
        PauseManager.Instance.CloseSettings();
    }
}