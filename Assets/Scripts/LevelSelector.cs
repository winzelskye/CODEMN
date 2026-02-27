using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

[Serializable]
public class LevelEntry
{
    public Button button;
    public string convoScene;
}

public class LevelSelector : MonoBehaviour
{
    [Header("Levels")]
    public LevelEntry[] levels;

    [Header("Run Button")]
    public Button runButton;

    [Header("Shop Button")]
    public Button shopButton;

    [Header("Unlock Settings")]
    public bool unlockAllLevels = false;

    [Header("Selection Glow")]
    public Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f); // golden glow

    private int selectedLevelIndex = -1;
    private Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();

    void Start()
    {
        var levelData = SaveLoadManager.Instance.GetAllLevels();

        if (runButton != null)
            runButton.interactable = false;

        for (int i = 0; i < levels.Length; i++)
        {
            int index = i;
            bool isUnlocked = unlockAllLevels || (i < levelData.Count && levelData[i].isUnlocked == 1);
            levels[i].button.interactable = isUnlocked;
            levels[i].button.onClick.AddListener(() => SelectLevel(index));

            // Cache each button's original color
            var img = levels[i].button.GetComponent<Image>();
            if (img != null)
                originalColors[levels[i].button] = img.color;
        }

        if (runButton != null)
            runButton.onClick.AddListener(OnRunClicked);

        if (shopButton != null)
        {
            var level1 = levelData.Find(l => l.id == 1);
            bool level1Completed = level1 != null && level1.isCompleted == 1;
            shopButton.gameObject.SetActive(level1Completed);
        }
    }

    void SelectLevel(int index)
    {
        // Reset all buttons to their original colors
        foreach (var level in levels)
        {
            if (originalColors.TryGetValue(level.button, out Color original))
                SetButtonColor(level.button, original);
        }

        selectedLevelIndex = index;

        // Highlight selected button
        SetButtonColor(levels[index].button, selectedColor);

        if (runButton != null)
            runButton.interactable = true;

        Debug.Log($"Level {index + 1} selected");
    }

    void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    void OnRunClicked()
    {
        if (selectedLevelIndex == -1)
        {
            Debug.LogWarning("No level selected!");
            return;
        }

        var levelData = SaveLoadManager.Instance.GetAllLevels();
        int levelId = selectedLevelIndex < levelData.Count ? levelData[selectedLevelIndex].id : selectedLevelIndex + 1;

        var player = SaveLoadManager.Instance.LoadPlayer();
        if (player != null)
            SaveLoadManager.Instance.SavePlayer(player.playerName, player.selectedCharacter, levelId);
        else
            SaveLoadManager.Instance.SavePlayer("Player", "Esther", levelId);

        string sceneName = levels[selectedLevelIndex].convoScene;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"No scene set for level {selectedLevelIndex + 1}!");
            return;
        }

        SceneController sc = FindFirstObjectByType<SceneController>();
        if (sc != null) sc.ChangeScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }
}