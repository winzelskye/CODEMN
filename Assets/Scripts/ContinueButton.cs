using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // This method will be called by the Continue button click event
    public void OnContinueClick()
    {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        Debug.Log("Continue button clicked!");
        

        // Load saved game or continue scene
        // You can load a specific save file or the last played level here
        SceneManager.LoadScene("SavedGame");

        // Or load the last saved scene index
        // SceneManager.LoadScene(PlayerPrefs.GetInt("LastLevel", 1));
=======
=======
>>>>>>> Stashed changes
        // Load saved game data
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.LoadGame();
        }

        // Use GameFlowManager if available
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToLevelSelection();
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
        
        Debug.Log("Loading scene: " + gameSceneName);
>>>>>>> Stashed changes
    }

    // Update is called once per frame
    void Update()
    {
    }
}