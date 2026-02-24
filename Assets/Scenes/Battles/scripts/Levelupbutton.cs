using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Player can only level up if their current level is below this number.")]
    public int maxLevel = 4;

    [Header("Optional")]
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(TryLevelUp);
    }

    public void TryLevelUp()
    {
        var player = SaveLoadManager.Instance.LoadPlayer();
        if (player == null) { Debug.LogWarning("LevelUpButton: No player found!"); return; }

        if (player.currentLevel < maxLevel)
        {
            // Complete current level → unlocks next level in LevelData + unlocks attacks
            SaveLoadManager.Instance.CompleteLevel(player.currentLevel);

            // Increment and save player level
            player.currentLevel += 1;
            SaveLoadManager.Instance.SavePlayer(player.playerName, player.selectedCharacter, player.currentLevel);

            Debug.Log($"Leveled up! Now level {player.currentLevel}. Next level unlocked.");
        }
        else
        {
            Debug.Log($"Already at max level {maxLevel}, nothing happened.");
        }
    }
}