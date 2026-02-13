using UnityEngine;
using UnityEditor;

/// <summary>
/// Manual folder reimporter - YOU specify which folders to reimport
/// Put this in Assets/Editor/ folder
/// </summary>
[InitializeOnLoad]
public class DialogueNodeManualFolderReimporter
{
    // ===== EDIT THESE PATHS TO YOUR DIALOGUENODE FOLDERS =====
    // Example: "Assets/Dialogue/Nodes"
    // Example: "Assets/ScriptableObjects/DialogueNodes"
    // Add as many folders as you need!
    private static string[] dialogueNodeFolders = new string[]
    {
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/LEVEL1.Tutorial",  // CHANGE THIS to your actual folder!
        // "Assets/AnotherFolder/Dialogue",  // Add more if needed
    };
    // ==========================================================

    static DialogueNodeManualFolderReimporter()
    {
        // Autoload - Reimport on Unity startup
        EditorApplication.delayCall += () =>
        {
            Debug.Log("[Manual Reimporter] Auto-reimporting DialogueNode folders on startup...");
            ReimportDialogueNodeFolders();
        };

        // Also reimport when entering Play mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("[Manual Reimporter] Reimporting DialogueNode folders before Play...");
            ReimportDialogueNodeFolders();
        }
    }

    private static void ReimportDialogueNodeFolders()
    {
        int reimportedCount = 0;

        foreach (string folder in dialogueNodeFolders)
        {
            // Skip if path not set up yet
            if (string.IsNullOrEmpty(folder) || folder.Contains("YOUR_FOLDER_PATH_HERE"))
            {
                Debug.LogWarning("[Manual Reimporter] Please edit the script and set your DialogueNode folder path!");
                continue;
            }

            // Check if folder exists
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogError($"[Manual Reimporter] Folder does not exist: {folder}");
                continue;
            }

            // Reimport the folder
            Debug.Log($"[Manual Reimporter] Reimporting folder: {folder}");
            AssetDatabase.ImportAsset(folder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            reimportedCount++;
        }

        if (reimportedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Manual Reimporter] Successfully reimported {reimportedCount} folder(s)!");
        }
        else
        {
            Debug.LogWarning("[Manual Reimporter] No folders were reimported. Please check your folder paths!");
        }
    }

    // Manual button
    [MenuItem("Tools/Dialogue/Force Reimport DialogueNode Folders (Manual)")]
    private static void ManualReimport()
    {
        ReimportDialogueNodeFolders();
        Debug.Log("[Manual Reimporter] Manual folder reimport complete!");
    }
}