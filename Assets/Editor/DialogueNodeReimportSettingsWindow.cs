using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DialogueNodeReimportSettingsWindow : EditorWindow
{
    private DialogueNodeReimportSettings settings;
    private Vector2 sceneScrollPos;
    private Vector2 folderScrollPos;

    [MenuItem("Tools/Dialogue/Configure Reimport Settings")]
    public static void OpenWindow()
    {
        GetWindow<DialogueNodeReimportSettingsWindow>("Dialogue Reimport Settings");
    }

    public static DialogueNodeReimportSettings LoadOrCreateSettings()
    {
        string path = "Assets/Editor/DialogueNodeReimportSettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<DialogueNodeReimportSettings>(path);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<DialogueNodeReimportSettings>();
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[Reimport Settings] Created new settings asset at: " + path);
        }
        return settings;
    }

    void OnEnable()
    {
        settings = LoadOrCreateSettings();
    }

    void OnGUI()
    {
        if (settings == null)
        {
            EditorGUILayout.HelpBox("Settings not found. Click below to create.", MessageType.Warning);
            if (GUILayout.Button("Create Settings"))
                settings = LoadOrCreateSettings();
            return;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Dialogue Node Reimport Settings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Reimport + diagnostic will run on Play and whenever any listed scene is loaded.", MessageType.Info);
        EditorGUILayout.Space(10);

        // ── Trigger Scenes ──
        EditorGUILayout.LabelField("Trigger Scenes", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Add scene names (no .unity extension) that should trigger reimport when loaded.", MessageType.None);

        sceneScrollPos = EditorGUILayout.BeginScrollView(sceneScrollPos, GUILayout.Height(200));
        for (int i = 0; i < settings.triggerScenes.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            settings.triggerScenes[i] = EditorGUILayout.TextField(settings.triggerScenes[i]);
            if (GUILayout.Button("✕", GUILayout.Width(25)))
            {
                settings.triggerScenes.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ Add Scene"))
            settings.triggerScenes.Add("");

        EditorGUILayout.Space(10);

        // ── Dialogue Node Folders ──
        EditorGUILayout.LabelField("Dialogue Node Folders", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Folders to reimport (e.g. Assets/Dialogue/Nodes).", MessageType.None);

        folderScrollPos = EditorGUILayout.BeginScrollView(folderScrollPos, GUILayout.Height(150));
        for (int i = 0; i < settings.dialogueNodeFolders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            settings.dialogueNodeFolders[i] = EditorGUILayout.TextField(settings.dialogueNodeFolders[i]);
            if (GUILayout.Button("Browse", GUILayout.Width(55)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected))
                {
                    if (selected.StartsWith(Application.dataPath))
                        selected = "Assets" + selected.Substring(Application.dataPath.Length);
                    settings.dialogueNodeFolders[i] = selected;
                }
            }
            if (GUILayout.Button("✕", GUILayout.Width(25)))
            {
                settings.dialogueNodeFolders.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ Add Folder"))
            settings.dialogueNodeFolders.Add("");

        EditorGUILayout.Space(15);

        if (GUILayout.Button("Save Settings", GUILayout.Height(30)))
        {
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Reimport Settings] Settings saved!");
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Run Reimport + Diagnostic Now", GUILayout.Height(25)))
        {
            DialogueNodeManualFolderReimporter.ReimportDialogueNodeFolders(settings.dialogueNodeFolders);
            DialogueNodeFolderReimporterDebug.DiagnoseDialogueNodes();
        }
    }
}