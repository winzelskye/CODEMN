using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSelector : MonoBehaviour
{
    [SerializeField] private BattleSceneController battleController;
    [SerializeField] private Transform buttonContainer; // Parent object for buttons
    [SerializeField] private GameObject buttonPrefab; // Button prefab

    private void Start()
    {
        CreateBattleButtons();
    }

    private void CreateBattleButtons()
    {
        if (battleController == null)
        {
            Debug.LogError("❌ BattleSceneController not assigned!");
            return;
        }

        int battleCount = battleController.GetBattleCount();

        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create button for each battle
        for (int i = 0; i < battleCount; i++)
        {
            int battleIndex = i; // Capture index for closure

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

            // Set button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"Battle {i + 1}";
            }

            // Add click listener
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnBattleButtonClicked(battleIndex));
            }
        }

        Debug.Log($"✅ Created {battleCount} battle buttons");
    }

    private void OnBattleButtonClicked(int battleIndex)
    {
        Debug.Log($"🎮 Loading battle {battleIndex + 1}");
        battleController.ChangeBattle(battleIndex);
    }
}