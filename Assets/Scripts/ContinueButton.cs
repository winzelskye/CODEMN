using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{

    public string gameSceneName = "Level Select";

    public void OnContinueClick()
    {
       
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