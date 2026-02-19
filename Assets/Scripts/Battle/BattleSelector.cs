using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleSelector : MonoBehaviour
{
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    private void Start()
    {
        CreateBattleButtons();
    }

    private void CreateBattleButtons()
    {
        var levels = SaveLoadManager.Instance.GetAllLevels();

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var level in levels)
        {
            var levelRef = level;
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = level.levelName;

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = level.isUnlocked == 1;
                button.onClick.AddListener(() => OnLevelSelected(levelRef.id));
            }
        }
    }

    private void OnLevelSelected(int levelId)
    {
        BattleManager.Instance.currentLevelId = levelId;
        Debug.Log($"Loading level {levelId}");
        // Add scene loading here if needed
    }
}