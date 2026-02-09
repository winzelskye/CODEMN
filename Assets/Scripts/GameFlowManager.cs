using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central coordinator for game flow and scene transitions.
/// Manages game state and handles connector points from flowcharts.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public enum GameState
    {
        MainMenu,
        CharacterSelection,
        LevelSelection,
        Battle,
        Shop,
        Settings
    }

    private GameState currentState;
    private string lastSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        currentState = GameState.MainMenu;
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"GameFlowManager: State changed to {newState}");
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>Transition to Character Selection (Connector A from flowchart)</summary>
    public void GoToCharacterSelection()
    {
        SetState(GameState.CharacterSelection);
        SceneManager.LoadScene("CharacterSelect");
    }

    /// <summary>Transition to Level Selection</summary>
    public void GoToLevelSelection()
    {
        SetState(GameState.LevelSelection);
        SceneManager.LoadScene("Level Select");
    }

    /// <summary>Transition to Battle (Connector B from flowchart)</summary>
    public void GoToBattle(string battleSceneName = "BattleTemplate")
    {
        SetState(GameState.Battle);
        SceneManager.LoadScene(battleSceneName);
    }

    /// <summary>Transition to Shop (Connector C from flowchart)</summary>
    public void GoToShop()
    {
        SetState(GameState.Shop);
        // Shop might be a scene or UI panel - adjust as needed
        SceneManager.LoadScene("Shop"); // Create shop scene or use UI panel
    }

    /// <summary>Return to Main Menu</summary>
    public void ReturnToMainMenu()
    {
        SetState(GameState.MainMenu);
        SceneManager.LoadScene("CODEMN(GAME)");
    }

    /// <summary>Save current scene name for Continue functionality</summary>
    public void SaveCurrentScene()
    {
        lastSceneName = SceneManager.GetActiveScene().name;
        if (DataPersistenceManager.instance != null)
        {
            DataPersistenceManager.instance.SaveGame();
        }
    }
}
