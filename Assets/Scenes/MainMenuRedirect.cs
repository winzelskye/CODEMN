using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuRedirect : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("CODEMN(GAME)");
    }
}