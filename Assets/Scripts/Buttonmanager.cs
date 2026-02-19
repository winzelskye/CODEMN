using UnityEngine;
using UnityEngine.SceneManagement;

public class Makebutton : MonoBehaviour
{
    [SerializeField] private GameObject confirmationDialog;

    public void OnNewGameClick()
    {
        Debug.Log("New Game");
        // Show confirmation panel first
        if (confirmationDialog != null)
            confirmationDialog.SetActive(true);
        else
            SceneManager.LoadScene("CharacterSelect");
    }
}