using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Enhanced character selection flow matching the flowchart:
/// Character Selection → Character Information → Select Character? → Character Selected → Level Selection
/// </summary>
public class CharacterSelectionFlowController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject characterSelectionPanel;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private GameObject confirmationPanel;

    [Header("Character Info Display")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDescriptionText;
    [SerializeField] private Image characterPortraitImage;

    [Header("Confirmation Buttons")]
    [SerializeField] private Button confirmSelectButton;
    [SerializeField] private Button cancelSelectButton;
    [SerializeField] private Button backToSelectionButton;

    [Header("Character Buttons")]
    [SerializeField] private Button estherButton;
    [SerializeField] private Button michaelButton;

    [Header("Scene Names")]
    [SerializeField] private string levelSelectScene = "Level Select";

    private string currentlyPreviewingCharacter;
    private CharacterSelectionManager characterSelectionManager;

    private void Start()
    {
        characterSelectionManager = FindObjectOfType<CharacterSelectionManager>();
        if (characterSelectionManager == null)
        {
            characterSelectionManager = gameObject.AddComponent<CharacterSelectionManager>();
        }

        SetupUI();
        SetupButtons();
    }

    private void SetupUI()
    {
        // Show character selection, hide info and confirmation
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(true);
        if (characterInfoPanel != null)
            characterInfoPanel.SetActive(false);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    private void SetupButtons()
    {
        if (estherButton != null)
            estherButton.onClick.AddListener(() => ShowCharacterInfo("Esther"));

        if (michaelButton != null)
            michaelButton.onClick.AddListener(() => ShowCharacterInfo("Michael"));

        if (confirmSelectButton != null)
            confirmSelectButton.onClick.AddListener(OnConfirmSelection);

        if (cancelSelectButton != null)
            cancelSelectButton.onClick.AddListener(OnCancelSelection);

        if (backToSelectionButton != null)
            backToSelectionButton.onClick.AddListener(OnBackToSelection);
    }

    private void ShowCharacterInfo(string characterName)
    {
        currentlyPreviewingCharacter = characterName;
        Debug.Log($"Showing info for: {characterName}");

        // Update character info display
        if (characterNameText != null)
            characterNameText.text = characterName;

        if (characterDescriptionText != null)
        {
            // Set description based on character
            if (characterName == "Esther")
                characterDescriptionText.text = "Esther - A skilled warrior with balanced stats.";
            else if (characterName == "Michael")
                characterDescriptionText.text = "Michael - A powerful fighter with high damage.";
            else
                characterDescriptionText.text = $"Information about {characterName}";
        }

        // Show character info panel
        if (characterInfoPanel != null)
            characterInfoPanel.SetActive(true);

        // Hide character selection panel
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(false);
    }

    /// <summary>Call from the main "Select" button to confirm current character and go to Level Selection.</summary>
    public void ConfirmSelection()
    {
        OnConfirmSelection();
    }

    private void OnConfirmSelection()
    {
        Debug.Log($"Confirming selection: {currentlyPreviewingCharacter}");

        // Save character selection
        if (characterSelectionManager != null)
        {
            characterSelectionManager.SelectCharacter(currentlyPreviewingCharacter);
        }
        else
        {
            PlayerPrefs.SetString("SelectedCharacter", currentlyPreviewingCharacter);
            PlayerPrefs.Save();
        }

        // Persist to GameData via IDataPersistence (CharacterSelectionPersistence in scene)
        if (DataPersistenceManager.instance != null)
            DataPersistenceManager.instance.SaveGame();

        // Transition to Level Selection
        OnCharacterSelected();
    }

    private void OnCancelSelection()
    {
        Debug.Log("Canceling selection - returning to character selection");
        OnBackToSelection();
    }

    private void OnBackToSelection()
    {
        // Hide info and confirmation panels
        if (characterInfoPanel != null)
            characterInfoPanel.SetActive(false);
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        // Show character selection panel
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(true);

        currentlyPreviewingCharacter = null;
    }

    private void OnCharacterSelected()
    {
        Debug.Log($"Character selected: {currentlyPreviewingCharacter} - Transitioning to Level Selection");

        // Save current scene
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.SaveCurrentScene();
            GameFlowManager.Instance.GoToLevelSelection();
        }
        else
        {
            SceneManager.LoadScene(levelSelectScene);
        }
    }

    // Public method to show confirmation panel (can be called from UI)
    public void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (estherButton != null)
            estherButton.onClick.RemoveAllListeners();
        if (michaelButton != null)
            michaelButton.onClick.RemoveAllListeners();
        if (confirmSelectButton != null)
            confirmSelectButton.onClick.RemoveAllListeners();
        if (cancelSelectButton != null)
            cancelSelectButton.onClick.RemoveAllListeners();
        if (backToSelectionButton != null)
            backToSelectionButton.onClick.RemoveAllListeners();
    }
}
