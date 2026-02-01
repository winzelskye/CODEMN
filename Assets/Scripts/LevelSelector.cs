using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[] levelButtons;
    
    [Header("Scene Names")]
    public string[] sceneNames; // Add scene names in Inspector
    
    [Header("Unlock Settings")]
    public bool unlockAllLevels = true; // Toggle this in Inspector
    
    void Start()
    {
        // Setup each button
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i;
            
            // Add click listener to each button
            levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
            
            // Check if level is unlocked
            bool isUnlocked = CheckIfLevelUnlocked(levelIndex + 1);
            levelButtons[i].interactable = isUnlocked;
        }
    }
    
    bool CheckIfLevelUnlocked(int levelNumber)
    {
        // If unlockAllLevels is true, all levels are accessible
        if (unlockAllLevels) return true;
        
        // Level 1 is always unlocked
        if (levelNumber == 1) return true;
        
        // Check if previous level was completed
        return PlayerPrefs.GetInt("Level_" + (levelNumber - 1) + "_Completed", 0) == 1;
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < sceneNames.Length)
        {
            Debug.Log("Loading: " + sceneNames[levelIndex]);
            SceneManager.LoadScene(sceneNames[levelIndex]);
        }
    }
    
    // Call this when a level is completed
    public static void CompleteLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("Level_" + levelNumber + "_Completed", 1);
        PlayerPrefs.Save();
    }
}