using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private Button estherButton;
    [SerializeField] private Button michaelButton;
    [SerializeField] private Button confirmButton;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Level Select";

    private string selectedCharacter = "Esther";

    void Start()
    {
        // Auto-initialize managers if missing
        if (DatabaseManager.Instance == null)
        {
            GameObject managers = new GameObject("Managers");
            managers.AddComponent<DatabaseManager>();
            managers.AddComponent<SaveLoadManager>();
        }

        if (estherButton != null)
            estherButton.onClick.AddListener(() => HighlightCharacter("Esther"));
        if (michaelButton != null)
            michaelButton.onClick.AddListener(() => HighlightCharacter("Michael"));
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSelection);
    }

    private void HighlightCharacter(string characterName)
    {
        selectedCharacter = characterName;
        Debug.Log($"Highlighted: {characterName}");
    }

    private void ConfirmSelection()
    {
        if (DatabaseManager.Instance == null)
        {
            GameObject managers = new GameObject("Managers");
            var dbManager = managers.AddComponent<DatabaseManager>();
            managers.AddComponent<SaveLoadManager>();
            dbManager.InitDB(); // Force initialize immediately
        }
        else if (DatabaseManager.Instance.db == null)
        {
            DatabaseManager.Instance.InitDB(); // Force initialize if db is null
        }

        SaveLoadManager.Instance.SavePlayer("Player", selectedCharacter, 1);
        Debug.Log($"Confirmed: {selectedCharacter}");

        SceneController sceneController = FindFirstObjectByType<SceneController>();
        if (sceneController != null)
            sceneController.ChangeScene(nextSceneName);
        else
            SceneManager.LoadScene(nextSceneName);
    }

    public static string GetSelectedCharacter()
    {
        var player = SaveLoadManager.Instance.LoadPlayer();
        return player != null ? player.selectedCharacter : "Esther";
    }

    public static bool IsCharacterSelected(string characterName)
    {
        return GetSelectedCharacter() == characterName;
    }

    public static void ClearSelection()
    {
        SaveLoadManager.Instance.SavePlayer("Player", "Esther", 1);
    }
}