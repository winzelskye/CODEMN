using UnityEngine;
using System.Collections.Generic;

public class AttackListManager : MonoBehaviour
{
    [Header("Attacks")]
    public List<AttackPrefabEntry> attacks = new List<AttackPrefabEntry>();

    [Header("Player Action Buttons")]
    public GameObject fightButtons;
    public GameObject skillsButtons;
    public GameObject itemsButtons;

    private List<AttackData> availableAttacks;

    void OnEnable()
    {
        LoadAttacks();
    }

    void LoadAttacks()
    {
        var player = SaveLoadManager.Instance.LoadPlayer();
        if (player == null) return;

        availableAttacks = SaveLoadManager.Instance.GetNormalAttacks(player.selectedCharacter);

        for (int i = 0; i < attacks.Count; i++)
        {
            if (i < availableAttacks.Count)
            {
                attacks[i].button.gameObject.SetActive(true);
                if (attacks[i].buttonText != null)
                    attacks[i].buttonText.text = availableAttacks[i].attackName;

                int index = i;
                attacks[i].button.onClick.RemoveAllListeners();
                attacks[i].button.onClick.AddListener(() => SelectAttack(index));
            }
            else
            {
                attacks[i].button.gameObject.SetActive(false);
            }
        }
    }

    void SelectAttack(int index)
    {
        AttackData attack = availableAttacks[index];
        BattleManager.Instance.player.UseAttack(attack);

        foreach (var a in attacks)
            if (a.attackObject != null) a.attackObject.SetActive(false);

        if (attacks[index].attackObject != null)
            attacks[index].attackObject.SetActive(true);

        gameObject.SetActive(false);
    }

    public void ShowBattleUI()
    {
        if (fightButtons != null) fightButtons.SetActive(true);
        if (skillsButtons != null) skillsButtons.SetActive(true);
        if (itemsButtons != null) itemsButtons.SetActive(true);
    }
}