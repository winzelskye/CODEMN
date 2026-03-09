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

    private string selectedCharacter = null; // Changed from "Esther" to null

    void Start()
    {
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
        {
            confirmButton.onClick.AddListener(ConfirmSelection);
            confirmButton.interactable = false; // Disabled on start
        }
    }

    private void HighlightCharacter(string characterName)
    {
        selectedCharacter = characterName;
        Debug.Log($"Highlighted: {characterName}");

        // Enable confirm button now that a character is selected
        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    private void ConfirmSelection()
    {
        if (selectedCharacter == null) return; // Extra safety guard

        if (DatabaseManager.Instance == null)
        {
            GameObject managers = new GameObject("Managers");
            var dbManager = managers.AddComponent<DatabaseManager>();
            managers.AddComponent<SaveLoadManager>();
            dbManager.InitDB();
        }
        else if (DatabaseManager.Instance.db == null)
        {
            DatabaseManager.Instance.InitDB();
        }

        SaveLoadManager.Instance.SavePlayer(selectedCharacter, selectedCharacter, 0);
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
        SaveLoadManager.Instance.SavePlayer("Esther", "Esther", 0);
    }
}