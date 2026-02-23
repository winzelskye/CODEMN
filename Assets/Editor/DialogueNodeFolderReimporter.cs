using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public class DialogueNodeManualFolderReimporter
{
    static DialogueNodeManualFolderReimporter()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            Debug.Log("[Manual Reimporter] Running before Play...");
            RunAll();
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        var settings = DialogueNodeReimportSettingsWindow.LoadOrCreateSettings();
        if (settings == null) return;

        if (settings.triggerScenes.Contains(scene.name))
        {
            Debug.Log($"[Manual Reimporter] Scene '{scene.name}' matched — running reimport...");
            RunAll();
        }
    }

    public static void ReimportDialogueNodeFolders(List<string> folders = null)
    {
        var settings = DialogueNodeReimportSettingsWindow.LoadOrCreateSettings();
        var foldersToUse = folders ?? settings?.dialogueNodeFolders ?? new List<string>();

        int reimportedCount = 0;
        foreach (string folder in foldersToUse)
        {
            if (string.IsNullOrEmpty(folder)) continue;

            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogError($"[Manual Reimporter] Folder does not exist: {folder}");
                continue;
            }

            Debug.Log($"[Manual Reimporter] Reimporting: {folder}");
            AssetDatabase.ImportAsset(folder, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            reimportedCount++;
        }

        if (reimportedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Manual Reimporter] Reimported {reimportedCount} folder(s).");
        }
        else
        {
            Debug.LogWarning("[Manual Reimporter] No folders reimported. Check paths in Configure Reimport Settings.");
        }
    }

    public static void RunAll()
    {
        var settings = DialogueNodeReimportSettingsWindow.LoadOrCreateSettings();
        ReimportDialogueNodeFolders(settings?.dialogueNodeFolders);
        DialogueNodeFolderReimporterDebug.DiagnoseDialogueNodes();
    }

    [MenuItem("Tools/Dialogue/Force Reimport DialogueNode Folders (Manual)")]
    private static void ManualReimport()
    {
        RunAll();
    }
}