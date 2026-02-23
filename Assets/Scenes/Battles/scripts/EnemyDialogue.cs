using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public enum TurnLineCondition
{
    None,
    OnEnemyDamaged
}

[System.Serializable]
public class EnemyDialogueLine
{
    [TextArea(2, 4)]
    public string line;
    public TurnLineCondition condition = TurnLineCondition.None;
}

public class EnemyDialogue : MonoBehaviour
{
    [Header("Enemy Text Color")]
    public Color enemyTextColor = Color.white;

    [Header("Pre-Battle Lines (plays before fight)")]
    public List<EnemyDialogueLine> preBattleLines = new List<EnemyDialogueLine>();

    [Header("Turn Lines (one per turn, no loop)")]
    public List<EnemyDialogueLine> turnLines = new List<EnemyDialogueLine>();

    [Header("Low Health Lines (when player HP is high)")]
    public List<EnemyDialogueLine> lowHealthLines = new List<EnemyDialogueLine>();

    [Header("Hit Reaction Lines (when enemy is hit)")]
    public List<EnemyDialogueLine> hitReactionLines = new List<EnemyDialogueLine>();

    private int currentTurnLineIndex = 0;
    private int currentHitLineIndex = 0;
    private int currentLowHealthLineIndex = 0;

    public string GetNextTurnLine(bool enemyTookDamage)
    {
        if (currentTurnLineIndex >= turnLines.Count) return "";

        while (currentTurnLineIndex < turnLines.Count)
        {
            EnemyDialogueLine line = turnLines[currentTurnLineIndex];
            bool conditionMet = line.condition == TurnLineCondition.None ||
                               (line.condition == TurnLineCondition.OnEnemyDamaged && enemyTookDamage);

            if (conditionMet)
            {
                string text = $"\"{line.line}\"";
                currentTurnLineIndex++;
                return text;
            }
            else
            {
                currentTurnLineIndex++;
            }
        }
        return "";
    }

    public string GetNextHitLine()
    {
        if (hitReactionLines.Count == 0) return "";
        if (currentHitLineIndex >= hitReactionLines.Count) return "";
        string line = $"\"{hitReactionLines[currentHitLineIndex].line}\"";
        currentHitLineIndex++;
        return line;
    }

    public string GetNextLowHealthLine()
    {
        if (lowHealthLines.Count == 0) return "";
        if (currentLowHealthLineIndex >= lowHealthLines.Count) return "";
        string line = $"\"{lowHealthLines[currentLowHealthLineIndex].line}\"";
        currentLowHealthLineIndex++;
        return line;
    }

    public string GetNextPreBattleLine(int index)
    {
        if (index >= preBattleLines.Count) return "";
        return $"\"{preBattleLines[index].line}\"";
    }
}