using UnityEngine;
using UnityEngine.SceneManagement;

public class Makebutton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // This method will be called by the button click event
    public void OnNewGameClick()
    {
        Debug.Log("New Game");

        // Alternative: Load scene by build index (0 is usually the first scene)
        // SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {

    }
}