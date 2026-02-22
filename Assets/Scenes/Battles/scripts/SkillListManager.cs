using UnityEngine;
using System.Collections.Generic;

public class SkillListManager : MonoBehaviour
{
    [Header("Skills")]
    public List<AttackPrefabEntry> skills = new List<AttackPrefabEntry>();

    [Header("Player Action Buttons")]
    public GameObject fightButtons;
    public GameObject skillsButtons;
    public GameObject itemsButtons;

    private List<AttackData> availableSkills;

    void OnEnable()
    {
        LoadSkills();
    }

    void Update()
    {
        if (availableSkills == null) return;
        if (BattleManager.Instance == null || BattleManager.Instance.player == null) return;

        float currentBP = BattleManager.Instance.player.bitpoints;
        for (int i = 0; i < skills.Count; i++)
        {
            if (i < availableSkills.Count)
            {
                skills[i].button.interactable = currentBP >= availableSkills[i].bitpointCost;
                // Update button text to show current BP vs cost
                if (skills[i].buttonText != null)
                    skills[i].buttonText.text = $"{availableSkills[i].attackName} ({(int)currentBP}/{availableSkills[i].bitpointCost} BP)";
            }
        }
    }

    void LoadSkills()
    {
        var player = SaveLoadManager.Instance.LoadPlayer();
        if (player == null) return;

        availableSkills = SaveLoadManager.Instance.GetSkills(player.selectedCharacter);
        Debug.Log($"Found {availableSkills.Count} skills, Player BP: {BattleManager.Instance.player.bitpoints}");

        for (int i = 0; i < skills.Count; i++)
        {
            if (i < availableSkills.Count)
            {
                skills[i].button.gameObject.SetActive(true);

                float currentBP = BattleManager.Instance.player.bitpoints;
                if (skills[i].buttonText != null)
                    skills[i].buttonText.text = $"{availableSkills[i].attackName} ({(int)currentBP}/{availableSkills[i].bitpointCost} BP)";

                skills[i].button.interactable = currentBP >= availableSkills[i].bitpointCost;

                int index = i;
                skills[i].button.onClick.RemoveAllListeners();
                skills[i].button.onClick.AddListener(() => SelectSkill(index));
            }
            else
            {
                skills[i].button.gameObject.SetActive(false);
            }
        }
    }

    void SelectSkill(int index)
    {
        AttackData skill = availableSkills[index];

        if (BattleManager.Instance.player.bitpoints < skill.bitpointCost)
        {
            Debug.Log("Not enough bitpoints!");
            return;
        }

        BattleManager.Instance.player.AddBitpoints(-skill.bitpointCost);
        BattleManager.Instance.player.UseAttack(skill);

        foreach (var s in skills)
            if (s.attackObject != null) s.attackObject.SetActive(false);

        if (skills[index].attackObject != null)
            skills[index].attackObject.SetActive(true);

        gameObject.SetActive(false);
    }

    public void ShowBattleUI()
    {
        if (fightButtons != null) fightButtons.SetActive(true);
        if (skillsButtons != null) skillsButtons.SetActive(true);
        if (itemsButtons != null) itemsButtons.SetActive(true);
    }
}