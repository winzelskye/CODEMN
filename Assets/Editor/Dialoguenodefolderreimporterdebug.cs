using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// DEBUG VERSION - Shows detailed info about what it finds
/// </summary>
[InitializeOnLoad]
public class DialogueNodeFolderReimporterDebug
{
    static DialogueNodeFolderReimporterDebug()
    {
        EditorApplication.delayCall += () =>
        {
            Debug.Log("[DEBUG Reimporter] Running diagnostics...");
            DiagnoseDialogueNodes();
        };
    }

    private static void DiagnoseDialogueNodes()
    {
        Debug.Log("=== DIALOGUE NODE DIAGNOSTIC ===");

        // Try multiple search methods
        Debug.Log("Method 1: Searching for 't:DialogueNode'...");
        string[] guids1 = AssetDatabase.FindAssets("t:DialogueNode");
        Debug.Log($"Found {guids1.Length} assets with 't:DialogueNode'");

        Debug.Log("Method 2: Searching for 't:ScriptableObject'...");
        string[] guids2 = AssetDatabase.FindAssets("t:ScriptableObject");
        Debug.Log($"Found {guids2.Length} ScriptableObjects total");

        Debug.Log("Method 3: Searching for all assets...");
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        int dialogueNodeCount = 0;

        foreach (string path in allAssets)
        {
            if (path.Contains("DialogueNode") || path.EndsWith(".asset"))
            {
                Debug.Log($"Found potential DialogueNode asset: {path}");

                // Try to load it
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj != null)
                {
                    Debug.Log($"  - Type: {obj.GetType().Name}");

                    if (obj is DialogueNode)
                    {
                        dialogueNodeCount++;
                        DialogueNode node = obj as DialogueNode;
                        Debug.Log($"  - IS A DIALOGUENODE! Name: {node.name}, Character: {node.characterName}");
                    }
                }
            }
        }

        Debug.Log($"Total DialogueNode assets found by manual search: {dialogueNodeCount}");
        Debug.Log("=== END DIAGNOSTIC ===");
    }

    [MenuItem("Tools/Dialogue/Run DialogueNode Diagnostic")]
    private static void ManualDiagnose()
    {
        DiagnoseDialogueNodes();
    }
}