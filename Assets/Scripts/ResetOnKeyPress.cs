using UnityEngine;

public class ResetOnKeyPress : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgress();
        }
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
        {
            System.IO.File.Delete(dbPath);
            Debug.Log("Progress reset!");
        }

        // Reload the scene to reinitialize everything
        UnityEngine.SceneManagement.SceneManager.LoadScene("CODEMN(GAME)");
    }
}