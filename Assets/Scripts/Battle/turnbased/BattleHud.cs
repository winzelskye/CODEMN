using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHud : MonoBehaviour
{
    public TMP_Text battleText;

    public void ChooseText()
    {
        battleText.text = "Choose action!";
    }

    public void UsedText(string unitName, string actionName)
    {
        battleText.text = unitName + " uses " + actionName + "!";
    }

    public void DamageText(string unitName, int damage)
    {
        battleText.text = unitName + " took " + damage + " damage!";
    }

    public IEnumerator HealText(string unitName, int heal)
    {
        yield return new WaitForSeconds(1.5f);
        battleText.text = unitName + " healed " + heal + " HP!";
    }

    public void ManaText(int mana)
    {
        battleText.text = "You need " + mana + " for spell!";
    }

    public void EndText(bool won)
    {
        if (won) battleText.text = "You WON!\nPress Restart to start again!";
        else battleText.text = "You LOST!\nPress Restart to start again!";
    }
}