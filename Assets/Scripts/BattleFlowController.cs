using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles battle flow transitions: Retry, Return to Menu, Reward Collection, Shop transition.
/// Integrates with BattleController to handle win/loss states.
/// </summary>
public class BattleFlowController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject rewardPanel;

    [Header("Reward Display")]
    [SerializeField] private TMPro.TextMeshProUGUI goldRewardText;
    [SerializeField] private TMPro.TextMeshProUGUI itemRewardText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "CODEMN(GAME)";
    [SerializeField] private string shopScene = "Shop"; // Create shop scene or use UI panel

    private BattleController battleController;
    private int battleGoldReward = 50; // Default reward, can be set per battle
    private string battleItemReward = ""; // Optional item reward

    private void Start()
    {
        battleController = FindObjectOfType<BattleController>();
        SetupButtons();
        HideAllPanels();
    }

    private void SetupButtons()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (returnToMenuButton != null)
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
    }

    private void HideAllPanels()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    /// <summary>Called when battle is won - show rewards and transition to shop</summary>
    public void OnBattleWon(int goldReward = 50, string itemReward = "")
    {
        Debug.Log($"Battle won! Gold: {goldReward}, Item: {itemReward}");

        battleGoldReward = goldReward;
        battleItemReward = itemReward;

        // Add rewards to player inventory
        CollectRewards(goldReward, itemReward);

        // Show reward panel
        ShowRewardPanel(goldReward, itemReward);

        // After showing rewards, transition to shop
        // Could add a delay or "Continue" button here
        Invoke(nameof(TransitionToShop), 2f); // Wait 2 seconds then go to shop
    }

    /// <summary>Called when battle is lost - show defeat screen</summary>
    public void OnBattleLost()
    {
        Debug.Log("Battle lost!");

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        // Show retry and return to menu buttons
        if (retryButton != null)
            retryButton.gameObject.SetActive(true);
        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(true);
    }

    private void CollectRewards(int gold, string itemId)
    {
        // Add gold to inventory
        var inventory = PlayerInventory.Instance;
        if (inventory != null)
        {
            inventory.AddGold(gold);
        }

        // Add item if specified
        if (!string.IsNullOrEmpty(itemId))
        {
            // Load item from Resources or ScriptableObject
            // For now, just log it
            Debug.Log($"Item reward: {itemId}");
        }

        // Save to GameData
        if (DataPersistenceManager.instance != null && DataPersistenceManager.instance != null)
        {
            // Gold is already added to PlayerInventory, which should persist
            DataPersistenceManager.instance.SaveGame();
        }
    }

    private void ShowRewardPanel(int gold, string item)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);

            if (goldRewardText != null)
                goldRewardText.text = $"Gold: +{gold}";

            if (itemRewardText != null)
            {
                if (!string.IsNullOrEmpty(item))
                    itemRewardText.text = $"Item: {item}";
                else
                    itemRewardText.text = "";
            }
        }
    }

    private void TransitionToShop()
    {
        Debug.Log("Transitioning to Shop...");

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToShop();
        }
        else
        {
            // If shop is a scene
            SceneManager.LoadScene(shopScene);
            // If shop is a UI panel, activate it instead
        }
    }

    public void OnRetryClicked()
    {
        Debug.Log("Retrying battle...");
        // Restart current battle scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnReturnToMenuClicked()
    {
        Debug.Log("Returning to main menu...");

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadScene(mainMenuScene);
        }
    }

    /// <summary>Set battle rewards (called before battle starts or from battle config)</summary>
    public void SetBattleRewards(int gold, string itemId = "")
    {
        battleGoldReward = gold;
        battleItemReward = itemId;
    }

    private void OnDestroy()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryClicked);
        if (returnToMenuButton != null)
            returnToMenuButton.onClick.RemoveListener(OnReturnToMenuClicked);
    }
}
