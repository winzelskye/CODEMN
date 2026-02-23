using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProceedButton : MonoBehaviour
{
    public GameObject confirmationDialog;
    public string sceneToLoad = "CharacterSelect"; // ← changed this
    public int sceneIndex = 0;
    public bool useSceneIndex = false;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnProceedButtonClicked);

        if (confirmationDialog == null)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == "ConfirmationDialog")
                {
                    confirmationDialog = current.gameObject;
                    break;
                }
                current = current.parent;
            }
        }
    }

    void OnProceedButtonClicked()
    {
        Debug.Log("Proceed button clicked - Resetting progress");

        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);

        ResetProgress();
        LoadScene();
    }

    void ResetProgress()
    {
        if (DatabaseManager.Instance != null && DatabaseManager.Instance.db != null)
        {
            DatabaseManager.Instance.db.Close();
            DatabaseManager.Instance.db = null;
        }

        string dbPath = Application.persistentDataPath + "/gamedata.db";
        if (System.IO.File.Exists(dbPath))
            System.IO.File.Delete(dbPath);

        Debug.Log("Progress has been reset!");
    }

    void LoadScene()
    {
        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(sceneToLoad);
        else
            SceneManager.LoadScene(sceneToLoad);
    }
}