using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Character Selection")]
    [SerializeField] private Button estherButton;
    [SerializeField] private Button michaelButton;

    private const string SELECTED_CHARACTER_KEY = "SelectedCharacter";

    private void Start()
    {
        // Setup button listeners
        if (estherButton != null)
            estherButton.onClick.AddListener(() => SelectCharacter("Esther"));

        if (michaelButton != null)
            michaelButton.onClick.AddListener(() => SelectCharacter("Michael"));
    }

    public void SelectCharacter(string characterName)
    {
        // Save selected character to PlayerPrefs
        PlayerPrefs.SetString(SELECTED_CHARACTER_KEY, characterName);
        PlayerPrefs.Save();

        Debug.Log($"Character selected: {characterName}");

        // Optional: Load the next scene or notify other systems
        // SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Get the currently selected character name
    /// </summary>
    public static string GetSelectedCharacter()
    {
        return PlayerPrefs.GetString(SELECTED_CHARACTER_KEY, "Esther"); // Default to Esther
    }

    /// <summary>
    /// Check if a specific character is selected
    /// </summary>
    public static bool IsCharacterSelected(string characterName)
    {
        return GetSelectedCharacter() == characterName;
    }

    /// <summary>
    /// Clear the saved character selection
    /// </summary>
    public static void ClearSelection()
    {
        PlayerPrefs.DeleteKey(SELECTED_CHARACTER_KEY);
        PlayerPrefs.Save();
    }
}