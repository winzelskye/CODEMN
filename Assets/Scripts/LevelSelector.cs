using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[] levelButtons;

    [Header("Scene Names")]
    public string[] sceneNames;

    [Header("Unlock Settings")]
    public bool unlockAllLevels = false;

    void Start()
    {
        var levels = SaveLoadManager.Instance.GetAllLevels();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i;

            levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));

            if (unlockAllLevels)
            {
                levelButtons[i].interactable = true;
            }
            else
            {
                // Check SQLite for unlock status
                bool isUnlocked = i < levels.Count && levels[i].isUnlocked == 1;
                levelButtons[i].interactable = isUnlocked;
            }
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < sceneNames.Length)
        {
            // Save current level to player record
            var player = SaveLoadManager.Instance.LoadPlayer();
            if (player != null)
                SaveLoadManager.Instance.SavePlayer(player.playerName, player.selectedCharacter, levelIndex + 1);

            Debug.Log("Loading: " + sceneNames[levelIndex]);

            SceneController sceneController = FindFirstObjectByType<SceneController>();
            if (sceneController != null)
                sceneController.ChangeScene(sceneNames[levelIndex]);
            else
                SceneManager.LoadScene(sceneNames[levelIndex]);
        }
    }
}