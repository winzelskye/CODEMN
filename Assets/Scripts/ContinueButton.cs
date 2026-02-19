using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    public string gameSceneName = "Level Select";

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            var player = SaveLoadManager.Instance.LoadPlayer();
            btn.interactable = player != null;
        }
    }

    public void OnContinueClick()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(gameSceneName);
        else
            SceneManager.LoadScene(gameSceneName);

        Debug.Log("Loading scene: " + gameSceneName);
    }

    public void LoadBattles()
    {
        SceneManager.LoadScene("Battles");
    }

    public void LoadLevelSelect()
    {
        SceneManager.LoadScene("Level Select");
    }

    public void LoadLevelEditor()
    {
        SceneManager.LoadScene("LevelEditor");
    }

    public void LoadSampleScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}