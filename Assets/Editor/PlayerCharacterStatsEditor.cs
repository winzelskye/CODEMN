using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PlayerCharacterStatsEditor : EditorWindow
{
    private List<CharacterStats> characters = new List<CharacterStats>();
    private CharacterStats selectedCharacter;
    private int selectedCharacterIndex = -1;

    private Vector2 scrollPosition;
    private Vector2 characterListScroll;
    private bool showDebugSection = true;
    private bool showAttacksSection = true;
    private bool showSpecialSkillSection = true;
    private bool showAnimationsSection = true;

    [MenuItem("Tools/Player Character Stats Editor")]
    public static void ShowWindow()
    {
        PlayerCharacterStatsEditor window = GetWindow<PlayerCharacterStatsEditor>("Character Stats");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    private void OnEnable()
    {
        LoadAllCharacters();
    }

    private void LoadAllCharacters()
    {
        characters.Clear();
        string[] guids = AssetDatabase.FindAssets("t:CharacterStats");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterStats stats = AssetDatabase.LoadAssetAtPath<CharacterStats>(path);
            if (stats != null)
            {
                characters.Add(stats);
            }
        }

        if (characters.Count > 0 && selectedCharacter == null)
        {
            selectedCharacter = characters[0];
            selectedCharacterIndex = 0;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        // Title
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("PLAYER CHARACTER STATS EDITOR", titleStyle);

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        // Left Panel - Character List
        DrawCharacterList();

        // Right Panel - Character Editor
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width - 220));

        if (selectedCharacter != null)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawCharacterEditor();
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Select a character from the list or create a new one.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // Save Changes
        if (GUI.changed && selectedCharacter != null)
        {
            EditorUtility.SetDirty(selectedCharacter);
            AssetDatabase.SaveAssets();
        }
    }

    private void DrawCharacterList()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(200));

        EditorGUILayout.LabelField("Characters", EditorStyles.boldLabel);

        // Create New Character Button
        if (GUILayout.Button("+ Create New Character", GUILayout.Height(30)))
        {
            CreateNewCharacter();
        }

        EditorGUILayout.Space(5);

        // Character List
        characterListScroll = EditorGUILayout.BeginScrollView(characterListScroll, GUILayout.Height(position.height - 150));

        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i] == null) continue;

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = selectedCharacterIndex == i ? Color.cyan : Color.white;

            EditorGUILayout.BeginHorizontal("box");

            if (GUILayout.Button(characters[i].characterName, GUILayout.Height(40)))
            {
                selectedCharacter = characters[i];
                selectedCharacterIndex = i;
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Delete Character",
                    "Are you sure you want to delete " + characters[i].characterName + "?",
                    "Delete", "Cancel"))
                {
                    DeleteCharacter(i);
                }
            }

            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // Refresh Button
        if (GUILayout.Button("Refresh List"))
        {
            LoadAllCharacters();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawCharacterEditor()
    {
        // Basic Info Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("BASIC INFO", EditorStyles.boldLabel);

        selectedCharacter.characterName = EditorGUILayout.TextField("Character Name", selectedCharacter.characterName);
        selectedCharacter.characterSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", selectedCharacter.characterSprite, typeof(Sprite), false);
        selectedCharacter.attackType = EditorGUILayout.TextField("Attack Type", selectedCharacter.attackType);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        // Animations Section
        DrawAnimationsSection();
        EditorGUILayout.Space(5);

        // Stats Display
        DrawStatsSection();
        EditorGUILayout.Space(5);

        // Debug Section
        DrawDebugSection();
        EditorGUILayout.Space(5);

        // Attacks Section (Editable)
        DrawEditableAttacksSection();
        EditorGUILayout.Space(5);

        // Special Skill Section (Editable)
        DrawEditableSpecialSkillSection();
    }

    private void DrawAnimationsSection()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        showAnimationsSection = EditorGUILayout.Foldout(showAnimationsSection, "CHARACTER ANIMATIONS & SPRITES", true, EditorStyles.foldoutHeader);

        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            if (selectedCharacter.animationPhases == null)
            {
                selectedCharacter.animationPhases = new List<AnimationPhase>();
            }
            selectedCharacter.animationPhases.Add(new AnimationPhase
            {
                phaseName = "New Phase",
                description = "Animation description"
            });
        }
        EditorGUILayout.EndHorizontal();

        if (showAnimationsSection)
        {
            EditorGUILayout.Space(5);

            if (selectedCharacter.animationPhases == null)
            {
                selectedCharacter.animationPhases = new List<AnimationPhase>();
            }

            if (selectedCharacter.animationPhases.Count == 0)
            {
                EditorGUILayout.HelpBox("No animation phases yet. Click '+' to add one!", MessageType.Info);
            }

            for (int i = 0; i < selectedCharacter.animationPhases.Count; i++)
            {
                AnimationPhase phase = selectedCharacter.animationPhases[i];

                EditorGUILayout.BeginVertical("box");

                // Header with remove button
                EditorGUILayout.BeginHorizontal();
                GUIStyle phaseStyle = new GUIStyle(EditorStyles.boldLabel);
                phaseStyle.normal.textColor = Color.cyan;
                EditorGUILayout.LabelField("Phase " + (i + 1), phaseStyle);

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    selectedCharacter.animationPhases.RemoveAt(i);
                    GUI.backgroundColor = Color.white;
                    break;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                // Phase details
                phase.phaseName = EditorGUILayout.TextField("Phase Name", phase.phaseName);
                phase.description = EditorGUILayout.TextField("Description", phase.description);

                EditorGUILayout.Space(3);

                // Animation Clip
                phase.animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", phase.animationClip, typeof(AnimationClip), false);

                // Animator Controller
                phase.animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", phase.animatorController, typeof(RuntimeAnimatorController), false);

                EditorGUILayout.Space(3);

                // Sprite/Texture
                phase.phaseSprite = (Sprite)EditorGUILayout.ObjectField("Phase Sprite", phase.phaseSprite, typeof(Sprite), false);
                phase.spriteSheet = (Texture2D)EditorGUILayout.ObjectField("Sprite Sheet", phase.spriteSheet, typeof(Texture2D), false);

                EditorGUILayout.Space(3);

                // Animation settings
                phase.frameRate = EditorGUILayout.FloatField("Frame Rate", phase.frameRate);
                phase.loop = EditorGUILayout.Toggle("Loop Animation", phase.loop);

                // Quick access buttons
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                if (phase.animationClip != null)
                {
                    if (GUILayout.Button("Select Animation", GUILayout.Height(25)))
                    {
                        Selection.activeObject = phase.animationClip;
                        EditorGUIUtility.PingObject(phase.animationClip);
                    }
                }

                if (phase.animatorController != null)
                {
                    if (GUILayout.Button("Select Controller", GUILayout.Height(25)))
                    {
                        Selection.activeObject = phase.animatorController;
                        EditorGUIUtility.PingObject(phase.animatorController);
                    }
                }

                if (phase.spriteSheet != null)
                {
                    if (GUILayout.Button("Select Sprite Sheet", GUILayout.Height(25)))
                    {
                        Selection.activeObject = phase.spriteSheet;
                        EditorGUIUtility.PingObject(phase.spriteSheet);
                    }
                }

                EditorGUILayout.EndHorizontal();

                // Preview sprite if available
                if (phase.phaseSprite != null)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                    Rect spriteRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawPreviewTexture(spriteRect, phase.phaseSprite.texture);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            // Quick Setup Presets
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Quick Setup Presets", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Standard Phases", GUILayout.Height(30)))
            {
                AddStandardPhases();
            }
            if (GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Animations",
                    "Remove all animation phases?",
                    "Clear", "Cancel"))
                {
                    selectedCharacter.animationPhases.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void AddStandardPhases()
    {
        if (selectedCharacter.animationPhases == null)
        {
            selectedCharacter.animationPhases = new List<AnimationPhase>();
        }

        string[] standardPhases = { "Idle", "Walk", "Run", "Attack", "Hurt", "Death", "Jump", "Defend" };

        foreach (string phaseName in standardPhases)
        {
            bool exists = false;
            foreach (AnimationPhase phase in selectedCharacter.animationPhases)
            {
                if (phase.phaseName == phaseName)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                selectedCharacter.animationPhases.Add(new AnimationPhase
                {
                    phaseName = phaseName,
                    description = phaseName + " animation",
                    frameRate = 12f,
                    loop = phaseName != "Attack" && phaseName != "Hurt" && phaseName != "Death"
                });
            }
        }

        Debug.Log("Added standard animation phases!");
    }

    private void DrawStatsSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("CURRENT STATS", EditorStyles.boldLabel);

        // HP Bar
        EditorGUILayout.LabelField("HP", EditorStyles.boldLabel);
        float hpPercentage = selectedCharacter.maxHP > 0 ? (float)selectedCharacter.currentHP / selectedCharacter.maxHP : 0;
        Rect hpBarRect = GUILayoutUtility.GetRect(18, 20, GUILayout.ExpandWidth(true));
        EditorGUI.ProgressBar(hpBarRect, hpPercentage, selectedCharacter.currentHP + " / " + selectedCharacter.maxHP);

        EditorGUILayout.Space(5);

        // Bit Points
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Bit Points", EditorStyles.boldLabel, GUILayout.Width(100));
        GUIStyle bitPointStyle = new GUIStyle(EditorStyles.boldLabel);
        bitPointStyle.fontSize = 20;
        bitPointStyle.normal.textColor = Color.yellow;
        EditorGUILayout.LabelField(selectedCharacter.bitPoints.ToString(), bitPointStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Level
        EditorGUILayout.LabelField("Level: " + selectedCharacter.level);

        EditorGUILayout.EndVertical();
    }

    private void DrawDebugSection()
    {
        EditorGUILayout.BeginVertical("box");

        Color originalBgColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);

        showDebugSection = EditorGUILayout.Foldout(showDebugSection, "DEBUG MODE", true, EditorStyles.foldoutHeader);

        if (showDebugSection)
        {
            GUI.backgroundColor = originalBgColor;
            EditorGUILayout.Space(5);

            // HP Controls
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current HP", GUILayout.Width(100));
            selectedCharacter.currentHP = EditorGUILayout.IntSlider(selectedCharacter.currentHP, 0, selectedCharacter.maxHP);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max HP", GUILayout.Width(100));
            selectedCharacter.maxHP = EditorGUILayout.IntField(selectedCharacter.maxHP);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Level
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level", GUILayout.Width(100));
            selectedCharacter.level = EditorGUILayout.IntField(selectedCharacter.level);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Bit Points Controls
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bit Points", GUILayout.Width(100));
            selectedCharacter.bitPoints = EditorGUILayout.IntField(selectedCharacter.bitPoints);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+1 Bit", GUILayout.Height(25)))
            {
                selectedCharacter.bitPoints++;
            }
            if (GUILayout.Button("+5 Bits", GUILayout.Height(25)))
            {
                selectedCharacter.bitPoints += 5;
            }
            if (GUILayout.Button("Reset Bits", GUILayout.Height(25)))
            {
                selectedCharacter.bitPoints = 0;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Reset All Button
            GUI.backgroundColor = new Color(1f, 0.5f, 0f);
            if (GUILayout.Button("Reset All Stats", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Stats",
                    "Reset all stats for " + selectedCharacter.characterName + "?",
                    "Reset", "Cancel"))
                {
                    selectedCharacter.Initialize();
                }
            }
        }

        GUI.backgroundColor = originalBgColor;
        EditorGUILayout.EndVertical();
    }

    private void DrawEditableAttacksSection()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        showAttacksSection = EditorGUILayout.Foldout(showAttacksSection, "ATTACKS AVAILABLE", true, EditorStyles.foldoutHeader);

        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            if (selectedCharacter.attacksAvailable == null)
            {
                selectedCharacter.attacksAvailable = new List<Attack>();
            }
            selectedCharacter.attacksAvailable.Add(new Attack
            {
                name = "New Attack",
                bitCost = 0,
                damage = 10,
                description = "Description"
            });
        }
        EditorGUILayout.EndHorizontal();

        if (showAttacksSection)
        {
            EditorGUILayout.Space(5);

            if (selectedCharacter.attacksAvailable == null)
            {
                selectedCharacter.attacksAvailable = new List<Attack>();
            }

            for (int i = 0; i < selectedCharacter.attacksAvailable.Count; i++)
            {
                Attack attack = selectedCharacter.attacksAvailable[i];

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Attack " + (i + 1), EditorStyles.boldLabel);

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    selectedCharacter.attacksAvailable.RemoveAt(i);
                    GUI.backgroundColor = Color.white;
                    break;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                attack.name = EditorGUILayout.TextField("Name", attack.name);
                attack.damage = EditorGUILayout.IntField("Damage", attack.damage);
                attack.bitCost = EditorGUILayout.IntField("Bit Cost", attack.bitCost);
                attack.description = EditorGUILayout.TextField("Description", attack.description);

                // Test button
                bool canUse = selectedCharacter.bitPoints >= attack.bitCost;
                GUI.enabled = canUse;
                if (GUILayout.Button("Test Use Attack"))
                {
                    selectedCharacter.bitPoints -= attack.bitCost;
                    Debug.Log(selectedCharacter.characterName + " used " + attack.name + "!");
                }
                GUI.enabled = true;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawEditableSpecialSkillSection()
    {
        EditorGUILayout.BeginVertical("box");

        showSpecialSkillSection = EditorGUILayout.Foldout(showSpecialSkillSection, "SPECIAL SKILL", true, EditorStyles.foldoutHeader);

        if (showSpecialSkillSection)
        {
            EditorGUILayout.Space(5);

            if (selectedCharacter.specialSkill == null)
            {
                selectedCharacter.specialSkill = new SpecialSkill();
            }

            EditorGUILayout.BeginVertical("box");

            selectedCharacter.specialSkill.name = EditorGUILayout.TextField("Skill Name", selectedCharacter.specialSkill.name);
            selectedCharacter.specialSkill.bitCost = EditorGUILayout.IntField("Bit Cost", selectedCharacter.specialSkill.bitCost);
            selectedCharacter.specialSkill.effect = EditorGUILayout.TextField("Effect", selectedCharacter.specialSkill.effect);
            selectedCharacter.specialSkill.description = EditorGUILayout.TextField("Description", selectedCharacter.specialSkill.description);

            EditorGUILayout.Space(3);

            // Test button
            bool canUse = selectedCharacter.bitPoints >= selectedCharacter.specialSkill.bitCost;
            GUI.enabled = canUse;
            if (GUILayout.Button("Test Use Special Skill", GUILayout.Height(30)))
            {
                selectedCharacter.bitPoints -= selectedCharacter.specialSkill.bitCost;
                Debug.Log(selectedCharacter.characterName + " used " + selectedCharacter.specialSkill.name + "!");
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateNewCharacter()
    {
        string folderPath = "Assets/Scripts/Battle/PlayerCharacterStats";

        // Create folders if they don't exist
        if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
        {
            AssetDatabase.CreateFolder("Assets", "Scripts");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Scripts/Battle"))
        {
            AssetDatabase.CreateFolder("Assets/Scripts", "Battle");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/Battle", "PlayerCharacterStats");
        }

        CharacterStats newChar = CreateInstance<CharacterStats>();
        newChar.characterName = "New Character";
        newChar.maxHP = 100;
        newChar.currentHP = 100;
        newChar.level = 1;
        newChar.attackType = "Physical";
        newChar.bitPoints = 0;
        newChar.attacksAvailable = new List<Attack>();
        newChar.animationPhases = new List<AnimationPhase>();
        newChar.specialSkill = new SpecialSkill
        {
            name = "Special Skill",
            bitCost = 5,
            effect = "Effect description"
        };

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/NewCharacter.asset");
        AssetDatabase.CreateAsset(newChar, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        LoadAllCharacters();
        selectedCharacter = newChar;
        selectedCharacterIndex = characters.IndexOf(newChar);

        Debug.Log("New character created at: " + uniquePath);
    }

    private void DeleteCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;

        string path = AssetDatabase.GetAssetPath(characters[index]);
        AssetDatabase.DeleteAsset(path);

        characters.RemoveAt(index);

        if (selectedCharacterIndex == index)
        {
            selectedCharacter = characters.Count > 0 ? characters[0] : null;
            selectedCharacterIndex = characters.Count > 0 ? 0 : -1;
        }
        else if (selectedCharacterIndex > index)
        {
            selectedCharacterIndex--;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}