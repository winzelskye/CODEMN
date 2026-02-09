using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

/// <summary>
/// Manages level progression with sublevel tracking.
/// Handles: "Have you finished the previous sublevel?" and "Have you finished all the sublevels?" checks.
/// </summary>
public class LevelProgressionManager : MonoBehaviour, IDataPersistence
{
    [System.Serializable]
    public class LevelConfig
    {
        public string levelName; // "Level_1", "Level_2", etc.
        public int totalSublevels;
        public string battleSceneName; // Scene to load for this level
        public bool isTutorial;
    }

    [Header("Level Configuration")]
    [SerializeField] private List<LevelConfig> levelConfigs = new List<LevelConfig>();

    [Header("Scene Names")]
    [SerializeField] private string battleSceneName = "BattleTemplate";
    [SerializeField] private string mainMenuSceneName = "CODEMN(GAME)";

    private GameData gameData;

    private void Start()
    {
        // Register with DataPersistenceManager
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.LoadGame();
        }

        SetupDefaultLevels();
    }

    private void SetupDefaultLevels()
    {
        if (levelConfigs.Count == 0)
        {
            // Default level configuration
            levelConfigs.Add(new LevelConfig { levelName = "Level_1", totalSublevels = 3, battleSceneName = "BattleTemplate", isTutorial = false });
            levelConfigs.Add(new LevelConfig { levelName = "Level_2", totalSublevels = 3, battleSceneName = "BattleTemplate", isTutorial = false });
            levelConfigs.Add(new LevelConfig { levelName = "Level_3", totalSublevels = 3, battleSceneName = "BattleTemplate", isTutorial = false });
            levelConfigs.Add(new LevelConfig { levelName = "Level_4", totalSublevels = 3, battleSceneName = "BattleTemplate", isTutorial = false });
            levelConfigs.Add(new LevelConfig { levelName = "Tutorial", totalSublevels = 1, battleSceneName = "BattleTemplate", isTutorial = true });
        }
    }

    /// <summary>Check if a level can be accessed (previous level completed)</summary>
    public bool CanAccessLevel(string levelName)
    {
        if (gameData == null) return false;

        // Tutorials are always accessible
        var config = GetLevelConfig(levelName);
        if (config != null && config.isTutorial)
            return true;

        // Level 1 is always accessible
        if (levelName == "Level_1")
            return true;

        // Check if previous level is completed
        int levelNumber = ExtractLevelNumber(levelName);
        if (levelNumber <= 1) return true;

        string previousLevel = $"Level_{levelNumber - 1}";
        return IsLevelCompleted(previousLevel);
    }

    /// <summary>Check if previous sublevel is finished</summary>
    public bool HasFinishedPreviousSublevel(string levelName, int sublevelIndex)
    {
        if (gameData == null) return true; // Default to true if no data

        // First sublevel is always accessible
        if (sublevelIndex == 0)
            return true;

        string previousSublevelKey = $"{levelName}_Sublevel_{sublevelIndex - 1}";
        return gameData.sublevelsCompleted.ContainsKey(previousSublevelKey) &&
               gameData.sublevelsCompleted[previousSublevelKey];
    }

    /// <summary>Check if all sublevels in a level are completed</summary>
    public bool HasFinishedAllSublevels(string levelName)
    {
        if (gameData == null) return false;

        var config = GetLevelConfig(levelName);
        if (config == null) return false;

        // Check all sublevels are completed
        for (int i = 0; i < config.totalSublevels; i++)
        {
            string sublevelKey = $"{levelName}_Sublevel_{i}";
            if (!gameData.sublevelsCompleted.ContainsKey(sublevelKey) ||
                !gameData.sublevelsCompleted[sublevelKey])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Mark a sublevel as completed</summary>
    public void CompleteSublevel(string levelName, int sublevelIndex)
    {
        if (gameData == null)
        {
            Debug.LogError("GameData is null. Cannot complete sublevel.");
            return;
        }

        string sublevelKey = $"{levelName}_Sublevel_{sublevelIndex}";
        gameData.sublevelsCompleted[sublevelKey] = true;

        // Update current sublevel index
        if (!gameData.currentSublevelIndex.ContainsKey(levelName))
        {
            gameData.currentSublevelIndex[levelName] = 0;
        }
        gameData.currentSublevelIndex[levelName] = Mathf.Max(
            gameData.currentSublevelIndex[levelName],
            sublevelIndex + 1
        );

        // Check if all sublevels are done
        if (HasFinishedAllSublevels(levelName))
        {
            CompleteLevel(levelName);
        }

        // Save progress
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.SaveGame();
        }

        Debug.Log($"Completed {sublevelKey}");
    }

    /// <summary>Mark a level as completed</summary>
    public void CompleteLevel(string levelName)
    {
        if (gameData == null) return;

        gameData.levelsCompleted[levelName] = true;
        gameData.currentLevel = levelName;

        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.SaveGame();
        }

        Debug.Log($"Level {levelName} completed!");
    }

    /// <summary>Check if a level is completed</summary>
    public bool IsLevelCompleted(string levelName)
    {
        if (gameData == null) return false;
        return gameData.levelsCompleted.ContainsKey(levelName) &&
               gameData.levelsCompleted[levelName];
    }

    /// <summary>Get current sublevel index for a level</summary>
    public int GetCurrentSublevelIndex(string levelName)
    {
        if (gameData == null) return 0;
        if (!gameData.currentSublevelIndex.ContainsKey(levelName))
            return 0;
        return gameData.currentSublevelIndex[levelName];
    }

    /// <summary>Load a level/sublevel (transition to battle)</summary>
    public void LoadLevel(string levelName, int sublevelIndex = -1)
    {
        // Check if level can be accessed
        if (!CanAccessLevel(levelName))
        {
            Debug.LogWarning($"Cannot access {levelName} - previous level not completed");
            return;
        }

        // If sublevel specified, check previous sublevel
        if (sublevelIndex >= 0 && !HasFinishedPreviousSublevel(levelName, sublevelIndex))
        {
            Debug.LogWarning($"Cannot access {levelName} Sublevel {sublevelIndex} - previous sublevel not completed");
            return;
        }

        // Get battle scene name
        var config = GetLevelConfig(levelName);
        string sceneToLoad = config != null ? config.battleSceneName : battleSceneName;

        // Transition to battle
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToBattle(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    /// <summary>Proceed to next level after completing all sublevels</summary>
    public void ProceedToNextLevel(string currentLevelName)
    {
        int levelNumber = ExtractLevelNumber(currentLevelName);
        string nextLevel = $"Level_{levelNumber + 1}";

        if (CanAccessLevel(nextLevel))
        {
            LoadLevel(nextLevel, 0); // Start from first sublevel
        }
        else
        {
            Debug.Log("All levels completed or next level not unlocked yet.");
            // Could return to level selection or show completion screen
            ReturnToLevelSelection();
        }
    }

    public void ReturnToLevelSelection()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToLevelSelection();
        }
        else
        {
            SceneManager.LoadScene("Level Select");
        }
    }

    public void ReturnToMainMenu()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private LevelConfig GetLevelConfig(string levelName)
    {
        return levelConfigs.Find(c => c.levelName == levelName);
    }

    private int ExtractLevelNumber(string levelName)
    {
        // Extract number from "Level_X" format
        if (levelName.StartsWith("Level_"))
        {
            string numberStr = levelName.Substring(6);
            if (int.TryParse(numberStr, out int number))
                return number;
        }
        return 0;
    }

    // IDataPersistence implementation
    public void LoadData(GameData data)
    {
        this.gameData = data;
    }

    public void SaveData(ref GameData data)
    {
        // Data is already updated through CompleteSublevel/CompleteLevel methods
        // This method is called by DataPersistenceManager
    }
}
