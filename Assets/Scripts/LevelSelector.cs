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
    public GameObject infoPanel;
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

    [Header("Auto Selection")]
    public int defaultSelectedLevel = 0;

    [Header("Selection Glow")]
    public Color selectedColor = new Color(0f, 1f, 1f, 1f); // #00FFFF

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

            originalColors[levels[i].button] = levels[i].button.colors.normalColor;

            // If locked, force black
            if (!isUnlocked)
                SetButtonColor(levels[i].button, Color.black);

            // Hide all panels initially
            if (levels[i].infoPanel != null)
                levels[i].infoPanel.SetActive(false);
        }

        if (runButton != null)
            runButton.onClick.AddListener(OnRunClicked);

        if (shopButton != null)
        {
            var level1 = levelData.Find(l => l.id == 1);
            bool level1Completed = level1 != null && level1.isCompleted == 1;
            shopButton.gameObject.SetActive(level1Completed);
        }

        AutoSelectLevel(levelData);
    }

    void AutoSelectLevel(List<LevelData> levelData)
    {
        // Option 1: Select the highest unlocked level
        int lastUnlockedIndex = -1;
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].button.interactable)
                lastUnlockedIndex = i;
        }

        if (lastUnlockedIndex != -1)
        {
            SelectLevel(lastUnlockedIndex);
            return;
        }

        // Option 2: Use the defaultSelectedLevel set in Inspector
        if (defaultSelectedLevel >= 0 && defaultSelectedLevel < levels.Length
            && levels[defaultSelectedLevel].button.interactable)
        {
            SelectLevel(defaultSelectedLevel);
            return;
        }

        // Option 3: Fall back to first unlocked level
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].button.interactable)
            {
                SelectLevel(i);
                return;
            }
        }
    }

    void SelectLevel(int index)
    {
        foreach (var level in levels)
        {
            if (originalColors.TryGetValue(level.button, out Color original))
                SetButtonColor(level.button, level.button.interactable ? original : Color.black);

            // Hide all panels
            if (level.infoPanel != null)
                level.infoPanel.SetActive(false);
        }

        selectedLevelIndex = index;
        SetButtonColor(levels[index].button, selectedColor);

        // Show selected panel
        if (levels[index].infoPanel != null)
            levels[index].infoPanel.SetActive(true);

        if (runButton != null)
            runButton.interactable = true;

        Debug.Log($"Level {index + 1} selected");
    }

    void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;

        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;

        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        cb.selectedColor = color;
        cb.disabledColor = Color.black;
        btn.colors = cb;
    }

    void OnRunClicked()
    {
        if (selectedLevelIndex == -1)
        {
            Debug.LogWarning("No level selected!");
            return;
        }

        var levelData = SaveLoadManager.Instance.GetAllLevels();
        int levelId = selectedLevelIndex < levelData.Count
            ? levelData[selectedLevelIndex].id
            : selectedLevelIndex + 1;

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