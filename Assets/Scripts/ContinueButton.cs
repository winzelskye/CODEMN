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
        Debug.Log("Continue button clicked!");
        

        // Load saved game or continue scene
        // You can load a specific save file or the last played level here
        SceneManager.LoadScene("SavedGame");

        // Or load the last saved scene index
        // SceneManager.LoadScene(PlayerPrefs.GetInt("LastLevel", 1));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}