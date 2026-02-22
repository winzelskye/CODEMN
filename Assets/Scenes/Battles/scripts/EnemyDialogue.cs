using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyDialogueLine
{
    [TextArea(2, 4)]
    public string line;
}

public class EnemyDialogue : MonoBehaviour
{
    [Header("Pre-Battle Lines (plays before fight)")]
    public List<EnemyDialogueLine> preBattleLines = new List<EnemyDialogueLine>();

    [Header("Turn Lines (one per turn, loops if runs out)")]
    public List<EnemyDialogueLine> turnLines = new List<EnemyDialogueLine>();

    [Header("Low Health Lines (when player HP is high)")]
    public List<EnemyDialogueLine> lowHealthLines = new List<EnemyDialogueLine>();

    [Header("Hit Reaction Lines (when enemy is hit)")]
    public List<EnemyDialogueLine> hitReactionLines = new List<EnemyDialogueLine>();

    private int currentTurnLineIndex = 0;
    private int currentHitLineIndex = 0;
    private int currentLowHealthLineIndex = 0;

    public string GetNextTurnLine()
    {
        if (turnLines.Count == 0) return "";
        string line = turnLines[currentTurnLineIndex].line;
        currentTurnLineIndex = (currentTurnLineIndex + 1) % turnLines.Count;
        return line;
    }

    public string GetNextHitLine()
    {
        if (hitReactionLines.Count == 0) return "";
        string line = hitReactionLines[currentHitLineIndex].line;
        currentHitLineIndex = (currentHitLineIndex + 1) % hitReactionLines.Count;
        return line;
    }

    public string GetNextLowHealthLine()
    {
        if (lowHealthLines.Count == 0) return "";
        string line = lowHealthLines[currentLowHealthLineIndex].line;
        currentLowHealthLineIndex = (currentLowHealthLineIndex + 1) % lowHealthLines.Count;
        return line;
    }
}