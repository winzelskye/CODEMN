using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[InitializeOnLoad]
public class DialogueNodeFolderReimporterDebug
{
    static DialogueNodeFolderReimporterDebug()
    {
        // No auto-run — triggered by DialogueNodeManualFolderReimporter
    }

    public static void DiagnoseDialogueNodes()
    {
        Debug.Log("=== DIALOGUE NODE DIAGNOSTIC ===");

        string[] guids = AssetDatabase.FindAssets("t:DialogueNode");
        Debug.Log($"Found {guids.Length} assets with 't:DialogueNode'");

        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        int dialogueNodeCount = 0;

        foreach (string path in allAssets)
        {
            if (path.Contains("DialogueNode") || path.EndsWith(".asset"))
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj is DialogueNode node)
                {
                    dialogueNodeCount++;
                    Debug.Log($"  - DialogueNode: {node.name}, Character: {node.characterName}");
                }
            }
        }

        Debug.Log($"Total DialogueNodes found: {dialogueNodeCount}");
        Debug.Log("=== END DIAGNOSTIC ===");

        ClearConsole();
    }

    private static void ClearConsole()
    {
        Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.CoreModule");
        if (logEntries != null)
        {
            MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod?.Invoke(null, null);
        }
    }

    [MenuItem("Tools/Dialogue/Run DialogueNode Diagnostic")]
    private static void ManualDiagnose()
    {
        DiagnoseDialogueNodes();
    }
}