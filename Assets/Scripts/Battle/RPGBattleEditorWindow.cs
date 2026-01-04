#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class RPGBattleEditorWindow : EditorWindow
{
    private BattleCollection battleCollection = new BattleCollection();
    private Vector2 scrollPosition;
    private string savePath = "Assets/Battles/battles.json";

    private int selectedBattleIndex = -1;
    private enum EditorTab { Overview, Enemy, Dialogue, Minigames, Phases, Distractions, Settings }
    private EditorTab currentTab = EditorTab.Overview;

    private bool[] dialogueFoldouts;
    private bool[] attackFoldouts;
    private bool[] phaseFoldouts;
    private bool[] distractionFoldouts;

    [MenuItem("Tools/RPG Battle Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<RPGBattleEditorWindow>("Battle Editor");
        window.minSize = new Vector2(750, 550);
    }

    private void OnEnable()
    {
        LoadBattles();
    }

    private void OnGUI()
    {
        DrawToolbar();
        GUILayout.Space(10);

        if (selectedBattleIndex >= 0 && selectedBattleIndex < battleCollection.battles.Count)
        {
            DrawBattleEditor();
        }
        else
        {
            DrawBattleList();
        }
    }

    // ===================================================
    // TOOLBAR
    // ===================================================
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("← Battles", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            selectedBattleIndex = -1;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("New Battle", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            AddBattle();
        }

        if (GUILayout.Button("Save JSON", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            SaveBattles();
        }

        if (GUILayout.Button("Load JSON", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            LoadBattles();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Save Path:", GUILayout.Width(70));
        savePath = EditorGUILayout.TextField(savePath, EditorStyles.toolbarTextField);
        EditorGUILayout.EndHorizontal();
    }

    // ===================================================
    // BATTLE LIST VIEW
    // ===================================================
    private void DrawBattleList()
    {
        EditorGUILayout.LabelField($"Total Battles: {battleCollection.battles.Count}", EditorStyles.boldLabel);
        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < battleCollection.battles.Count; i++)
        {
            BattleData battle = battleCollection.battles[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label($"{i + 1}. {battle.battleName}", EditorStyles.boldLabel);
            GUILayout.Label($"Enemy: {battle.enemy.enemyName} | Minigames: {battle.attacks.Count} | Distractions: {battle.distractions.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Edit", GUILayout.Width(60)))
            {
                selectedBattleIndex = i;
                currentTab = EditorTab.Overview;
            }

            if (GUILayout.Button("Duplicate", GUILayout.Width(70)))
            {
                DuplicateBattle(i);
            }

            if (GUILayout.Button("▲", GUILayout.Width(25)) && i > 0)
            {
                MoveBattle(i, i - 1);
            }

            if (GUILayout.Button("▼", GUILayout.Width(25)) && i < battleCollection.battles.Count - 1)
            {
                MoveBattle(i, i + 1);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Battle", $"Delete '{battle.battleName}'?", "Yes", "No"))
                {
                    battleCollection.battles.RemoveAt(i);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        if (battleCollection.battles.Count == 0)
        {
            GUILayout.Space(50);
            EditorGUILayout.HelpBox("No battles created yet. Click 'New Battle' to start!", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    // ===================================================
    // BATTLE EDITOR VIEW
    // ===================================================
    private void DrawBattleEditor()
    {
        BattleData battle = battleCollection.battles[selectedBattleIndex];

        // Tab buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentTab == EditorTab.Overview, "Overview", EditorStyles.toolbarButton))
            currentTab = EditorTab.Overview;
        if (GUILayout.Toggle(currentTab == EditorTab.Enemy, "Enemy", EditorStyles.toolbarButton))
            currentTab = EditorTab.Enemy;
        if (GUILayout.Toggle(currentTab == EditorTab.Dialogue, "Dialogue", EditorStyles.toolbarButton))
            currentTab = EditorTab.Dialogue;
        if (GUILayout.Toggle(currentTab == EditorTab.Minigames, "Minigames", EditorStyles.toolbarButton))
            currentTab = EditorTab.Minigames;
        if (GUILayout.Toggle(currentTab == EditorTab.Phases, "Phases", EditorStyles.toolbarButton))
            currentTab = EditorTab.Phases;
        if (GUILayout.Toggle(currentTab == EditorTab.Distractions, "Distractions", EditorStyles.toolbarButton))
            currentTab = EditorTab.Distractions;
        if (GUILayout.Toggle(currentTab == EditorTab.Settings, "Settings", EditorStyles.toolbarButton))
            currentTab = EditorTab.Settings;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        switch (currentTab)
        {
            case EditorTab.Overview:
                DrawOverviewTab(battle);
                break;
            case EditorTab.Enemy:
                DrawEnemyTab(battle);
                break;
            case EditorTab.Dialogue:
                DrawDialogueTab(battle);
                break;
            case EditorTab.Minigames:
                DrawMinigamesTab(battle);
                break;
            case EditorTab.Phases:
                DrawPhasesTab(battle);
                break;
            case EditorTab.Distractions:
                DrawDistractionsTab(battle);
                break;
            case EditorTab.Settings:
                DrawSettingsTab(battle);
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    // ===================================================
    // OVERVIEW TAB
    // ===================================================
    private void DrawOverviewTab(BattleData battle)
    {
        EditorGUILayout.LabelField("Battle Overview", EditorStyles.boldLabel);
        GUILayout.Space(10);

        battle.battleName = EditorGUILayout.TextField("Battle Name", battle.battleName);
        battle.description = EditorGUILayout.TextField("Description", battle.description, GUILayout.Height(60));

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Quick Stats", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Enemy", battle.enemy.enemyName);
        EditorGUILayout.IntField("Enemy HP", battle.enemy.maxHealth);
        EditorGUILayout.IntField("Player HP", battle.basePlayerHealth);
        EditorGUILayout.IntField("Dialogue Entries", battle.dialogues.Count);
        EditorGUILayout.IntField("Total Minigames", battle.attacks.Count);
        EditorGUILayout.IntField("Battle Phases", battle.phases.Count);
        EditorGUILayout.IntField("Distractions", battle.distractions.Count);
        EditorGUILayout.Toggle("Boss Battle", battle.isBossBattle);
        EditorGUILayout.TextField("Battle ID", battle.id);
        EditorGUI.EndDisabledGroup();
    }

    // ===================================================
    // ENEMY TAB
    // ===================================================
    private void DrawEnemyTab(BattleData battle)
    {
        EditorGUILayout.LabelField("Enemy Configuration", EditorStyles.boldLabel);
        GUILayout.Space(10);

        battle.enemy.enemyName = EditorGUILayout.TextField("Enemy Name", battle.enemy.enemyName);
        battle.enemy.enemySprite = (Sprite)EditorGUILayout.ObjectField("Enemy Sprite", battle.enemy.enemySprite, typeof(Sprite), false);
        battle.enemy.animatorController = EditorGUILayout.TextField("Animator Controller Path", battle.enemy.animatorController);
        EditorGUILayout.HelpBox("Put animator in Resources folder. Example: 'Animators/EnemyAnimator'", MessageType.Info);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
        battle.enemy.maxHealth = EditorGUILayout.IntField("Max Health", battle.enemy.maxHealth);
        battle.enemy.attack = EditorGUILayout.IntSlider("Attack", battle.enemy.attack, 1, 100);
        battle.enemy.defense = EditorGUILayout.IntSlider("Defense", battle.enemy.defense, 1, 100);
        battle.enemy.speed = EditorGUILayout.IntSlider("Speed", battle.enemy.speed, 1, 100);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);
        battle.enemy.enemyColor = EditorGUILayout.ColorField("Color Tint", battle.enemy.enemyColor);
        battle.enemy.scale = EditorGUILayout.Vector3Field("Scale", battle.enemy.scale);
        battle.enemy.battlePosition = EditorGUILayout.Vector3Field("Battle Position", battle.enemy.battlePosition);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Damage Effects", EditorStyles.boldLabel);
        battle.enemy.enableDamageTint = EditorGUILayout.Toggle("Enable Damage Tint", battle.enemy.enableDamageTint);
        if (battle.enemy.enableDamageTint)
        {
            EditorGUI.indentLevel++;
            battle.enemy.damageTintColor = EditorGUILayout.ColorField("Tint Color", battle.enemy.damageTintColor);
            battle.enemy.damageTintDuration = EditorGUILayout.Slider("Tint Duration", battle.enemy.damageTintDuration, 0.1f, 1f);
            EditorGUI.indentLevel--;
        }

        battle.enemy.enableDamageShake = EditorGUILayout.Toggle("Enable Damage Shake", battle.enemy.enableDamageShake);
        if (battle.enemy.enableDamageShake)
        {
            EditorGUI.indentLevel++;
            battle.enemy.damageShakeIntensity = EditorGUILayout.Slider("Shake Intensity", battle.enemy.damageShakeIntensity, 0.1f, 2f);
            battle.enemy.damageShakeDuration = EditorGUILayout.Slider("Shake Duration", battle.enemy.damageShakeDuration, 0.1f, 1f);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(20);

        if (battle.enemy.enemySprite != null)
        {
            EditorGUILayout.LabelField("Sprite Preview:");
            GUILayout.Label(battle.enemy.enemySprite.texture, GUILayout.Width(150), GUILayout.Height(150));
        }
    }

    // ===================================================
    // DIALOGUE TAB (UPDATED)
    // ===================================================
    private void DrawDialogueTab(BattleData battle)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Dialogue System", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Dialogue", GUILayout.Width(120)))
        {
            battle.dialogues.Add(new DialogueEntry
            {
                turnNumber = battle.dialogues.Count + 1,
                dialogueText = "* New dialogue line...",
                characterName = battle.enemy.enemyName,
                triggerCondition = "OnTurnStart"
            });
            UpdateDialogueFoldouts(battle);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Dialogues appear at specific turns or when conditions are met. Use * at the start like Undertale!", MessageType.Info);

        GUILayout.Space(10);

        UpdateDialogueFoldouts(battle);

        if (GUILayout.Button("Sort by Turn Number"))
        {
            battle.dialogues.Sort((a, b) => a.turnNumber.CompareTo(b.turnNumber));
        }

        GUILayout.Space(5);

        for (int i = 0; i < battle.dialogues.Count; i++)
        {
            DialogueEntry dialogue = battle.dialogues[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            string previewText = dialogue.dialogueText;
            if (previewText.Length > 40) previewText = previewText.Substring(0, 40) + "...";

            dialogueFoldouts[i] = EditorGUILayout.Foldout(dialogueFoldouts[i],
                $"Turn {dialogue.turnNumber}: {previewText}", true);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("▲", GUILayout.Width(25)) && i > 0)
            {
                var temp = battle.dialogues[i];
                battle.dialogues[i] = battle.dialogues[i - 1];
                battle.dialogues[i - 1] = temp;
            }

            if (GUILayout.Button("▼", GUILayout.Width(25)) && i < battle.dialogues.Count - 1)
            {
                var temp = battle.dialogues[i];
                battle.dialogues[i] = battle.dialogues[i + 1];
                battle.dialogues[i + 1] = temp;
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                battle.dialogues.RemoveAt(i);
                UpdateDialogueFoldouts(battle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (dialogueFoldouts[i])
            {
                EditorGUI.indentLevel++;

                dialogue.turnNumber = EditorGUILayout.IntField("Turn Number", dialogue.turnNumber);
                dialogue.characterName = EditorGUILayout.TextField("Character Name", dialogue.characterName);

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Dialogue Text:", EditorStyles.boldLabel);
                dialogue.dialogueText = EditorGUILayout.TextArea(dialogue.dialogueText, GUILayout.Height(80));

                GUILayout.Space(5);

                dialogue.characterPortrait = (Sprite)EditorGUILayout.ObjectField("Portrait", dialogue.characterPortrait, typeof(Sprite), false);

                // NEW: Custom dialogue box field
                dialogue.customDialogueBox = (Sprite)EditorGUILayout.ObjectField("Custom Dialogue Box", dialogue.customDialogueBox, typeof(Sprite), false);
                EditorGUILayout.HelpBox("Leave empty to use character's default box from DialogueDisplay presets", MessageType.Info);

                dialogue.displayDuration = EditorGUILayout.Slider("Display Duration", dialogue.displayDuration, 1f, 10f);

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Trigger Settings:", EditorStyles.boldLabel);

                string[] triggers = new string[] { "OnTurnStart", "OnTurnEnd", "OnHit", "OnLowHealth", "OnPhaseChange", "Always" };
                int triggerIndex = Array.IndexOf(triggers, dialogue.triggerCondition);
                if (triggerIndex < 0) triggerIndex = 0;

                triggerIndex = EditorGUILayout.Popup("Trigger Condition", triggerIndex, triggers);
                dialogue.triggerCondition = triggers[triggerIndex];

                if (dialogue.triggerCondition == "OnLowHealth")
                {
                    dialogue.healthThreshold = EditorGUILayout.IntSlider("Health Threshold %", dialogue.healthThreshold, 0, 100);
                }

                GUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Preview:", EditorStyles.miniLabel);
                GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
                previewStyle.wordWrap = true;
                EditorGUILayout.LabelField(dialogue.dialogueText, previewStyle);
                EditorGUILayout.EndVertical();

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        if (battle.dialogues.Count == 0)
        {
            EditorGUILayout.HelpBox("No dialogue entries yet. Add dialogue to make your battle more engaging!", MessageType.Info);
        }
    }

    // ===================================================
    // MINIGAMES TAB
    // ===================================================
    private void DrawMinigamesTab(BattleData battle)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Minigame Attacks", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Minigame", GUILayout.Width(120)))
        {
            battle.attacks.Add(new MinigameAttack
            {
                attackName = "New Minigame",
                minigameType = "Quiz",
                damageOnFail = 10,
                timeLimit = 30f
            });
            UpdateAttackFoldouts(battle);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Create minigames that the player must complete. Failure to finish in time causes damage!", MessageType.Info);

        GUILayout.Space(10);

        UpdateAttackFoldouts(battle);

        for (int i = 0; i < battle.attacks.Count; i++)
        {
            MinigameAttack attack = battle.attacks[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            attackFoldouts[i] = EditorGUILayout.Foldout(attackFoldouts[i],
                $"{attack.attackName} - {attack.minigameType} ({attack.timeLimit}s | {attack.damageOnFail} DMG)", true);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                battle.attacks.RemoveAt(i);
                UpdateAttackFoldouts(battle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (attackFoldouts[i])
            {
                EditorGUI.indentLevel++;

                attack.attackName = EditorGUILayout.TextField("Attack Name", attack.attackName);

                string[] types = new string[] { "Quiz", "DragDrop", "CodeBlocks", "Custom" };
                int typeIndex = Array.IndexOf(types, attack.minigameType);
                if (typeIndex < 0) typeIndex = 0;
                typeIndex = EditorGUILayout.Popup("Minigame Type", typeIndex, types);
                attack.minigameType = types[typeIndex];

                attack.timeLimit = EditorGUILayout.Slider("Time Limit (seconds)", attack.timeLimit, 5f, 120f);
                attack.damageOnFail = EditorGUILayout.IntSlider("Damage on Fail", attack.damageOnFail, 1, 50);
                attack.description = EditorGUILayout.TextField("Description", attack.description, GUILayout.Height(40));

                GUILayout.Space(10);

                // Type-specific settings
                if (attack.minigameType == "Quiz")
                {
                    EditorGUILayout.LabelField("Quiz Settings", EditorStyles.boldLabel);

                    EditorGUILayout.LabelField("Questions (one per line):");
                    string questionsText = string.Join("\n", attack.questions);
                    questionsText = EditorGUILayout.TextArea(questionsText, GUILayout.Height(60));
                    attack.questions = new List<string>(questionsText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

                    EditorGUILayout.LabelField("Correct Answers (one per line, matching order of questions):");
                    string answersText = string.Join("\n", attack.correctAnswers);
                    answersText = EditorGUILayout.TextArea(answersText, GUILayout.Height(40));
                    attack.correctAnswers = new List<string>(answersText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

                    EditorGUILayout.LabelField("Wrong Answers (comma-separated per line, one line per question):");
                    EditorGUILayout.HelpBox("Example:\nRed,Green,Yellow\nParis,Tokyo,Berlin", MessageType.None);
                    string wrongText = string.Join("\n", attack.wrongAnswers);
                    wrongText = EditorGUILayout.TextArea(wrongText, GUILayout.Height(40));
                    attack.wrongAnswers = new List<string>(wrongText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

                    attack.shuffleOptions = EditorGUILayout.Toggle("Shuffle Options", attack.shuffleOptions);
                }
                else if (attack.minigameType == "DragDrop")
                {
                    EditorGUILayout.LabelField("Drag & Drop Settings", EditorStyles.boldLabel);
                    attack.dragDropItemCount = EditorGUILayout.IntSlider("Number of Items", attack.dragDropItemCount, 2, 10);
                    attack.dragDropTheme = EditorGUILayout.TextField("Theme/Objective", attack.dragDropTheme);
                    EditorGUILayout.HelpBox("Example: 'Sort numbers ascending' or 'Match colors to names'", MessageType.Info);
                }
                else if (attack.minigameType == "CodeBlocks")
                {
                    EditorGUILayout.LabelField("Code Blocks Settings", EditorStyles.boldLabel);

                    EditorGUILayout.LabelField("Code Blocks (one per line):");
                    string blocksText = string.Join("\n", attack.codeBlocks);
                    blocksText = EditorGUILayout.TextArea(blocksText, GUILayout.Height(80));
                    attack.codeBlocks = new List<string>(blocksText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

                    attack.correctOrder = EditorGUILayout.TextField("Correct Order (e.g. 0,1,2,3)", attack.correctOrder);
                    EditorGUILayout.HelpBox("Enter the indices in correct order. 0 = first line, 1 = second line, etc.", MessageType.Info);
                    attack.shuffleOptions = EditorGUILayout.Toggle("Shuffle Blocks", attack.shuffleOptions);
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
                attack.attackIcon = (Sprite)EditorGUILayout.ObjectField("Icon", attack.attackIcon, typeof(Sprite), false);
                attack.themeColor = EditorGUILayout.ColorField("Theme Color", attack.themeColor);

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Difficulty Scaling", EditorStyles.boldLabel);
                attack.timeReduction = EditorGUILayout.Slider("Time Reduction Per Use", attack.timeReduction, 0f, 10f);
                EditorGUILayout.HelpBox($"Each time this minigame is used, time limit reduces by {attack.timeReduction}s", MessageType.Info);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        if (battle.attacks.Count == 0)
        {
            EditorGUILayout.HelpBox("No minigames created yet. Add minigames to challenge the player!", MessageType.Info);
        }
    }

    // ===================================================
    // PHASES TAB
    // ===================================================
    private void DrawPhasesTab(BattleData battle)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Battle Phases", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Phase", GUILayout.Width(100)))
        {
            battle.phases.Add(new BattlePhase { phaseName = "New Phase", healthThreshold = 50 });
            UpdatePhaseFoldouts(battle);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Phases trigger when enemy reaches specific HP thresholds. Change sprite, animation, and music!", MessageType.Info);
        GUILayout.Space(5);

        UpdatePhaseFoldouts(battle);

        for (int i = 0; i < battle.phases.Count; i++)
        {
            BattlePhase phase = battle.phases[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            phaseFoldouts[i] = EditorGUILayout.Foldout(phaseFoldouts[i],
                $"{phase.phaseName} @ {phase.healthThreshold}% HP", true);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                battle.phases.RemoveAt(i);
                UpdatePhaseFoldouts(battle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (phaseFoldouts[i])
            {
                EditorGUI.indentLevel++;

                phase.phaseName = EditorGUILayout.TextField("Phase Name", phase.phaseName);
                phase.healthThreshold = EditorGUILayout.IntSlider("HP Threshold %", phase.healthThreshold, 0, 100);
                phase.phaseSpeed = EditorGUILayout.Slider("Speed Multiplier", phase.phaseSpeed, 0.5f, 3f);
                phase.timeReduction = EditorGUILayout.Slider("Minigame Time Reduction", phase.timeReduction, 0f, 20f);
                EditorGUILayout.HelpBox($"All minigames will have {phase.timeReduction}s less time in this phase", MessageType.Info);

                GUILayout.Space(5);
                phase.dialogueLine = EditorGUILayout.TextField("Dialogue", phase.dialogueLine, GUILayout.Height(40));

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Visual Changes", EditorStyles.boldLabel);

                phase.changeSprite = EditorGUILayout.Toggle("Change Sprite", phase.changeSprite);
                if (phase.changeSprite)
                {
                    EditorGUI.indentLevel++;
                    phase.phaseSprite = (Sprite)EditorGUILayout.ObjectField("Phase Sprite", phase.phaseSprite, typeof(Sprite), false);
                    phase.phaseTintColor = EditorGUILayout.ColorField("Phase Tint Color", phase.phaseTintColor);
                    EditorGUI.indentLevel--;
                }

                phase.changeAnimatorState = EditorGUILayout.Toggle("Change Animation", phase.changeAnimatorState);
                if (phase.changeAnimatorState)
                {
                    EditorGUI.indentLevel++;
                    phase.animatorStateName = EditorGUILayout.TextField("Animator State Name", phase.animatorStateName);
                    EditorGUILayout.HelpBox("Example: 'Phase2Idle' or 'Enraged'", MessageType.None);
                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(5);
                phase.changeMusic = EditorGUILayout.Toggle("Change Music", phase.changeMusic);
                if (phase.changeMusic)
                {
                    phase.phaseMusic = (AudioClip)EditorGUILayout.ObjectField("Phase Music", phase.phaseMusic, typeof(AudioClip), false);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }

    // ===================================================
    // DISTRACTIONS TAB
    // ===================================================
    private void DrawDistractionsTab(BattleData battle)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Distraction Mechanics", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Distraction", GUILayout.Width(130)))
        {
            battle.distractions.Add(new DistractionMechanic
            {
                mechanicName = "New Distraction",
                mechanicType = "ScreenGlitch",
                isEnabled = true,
                activationChance = 50f,
                duration = 2f
            });
            UpdateDistractionFoldouts(battle);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Distractions make minigames harder! They can glitch the screen, show popups, shake, etc.", MessageType.Info);

        GUILayout.Space(10);

        UpdateDistractionFoldouts(battle);

        for (int i = 0; i < battle.distractions.Count; i++)
        {
            DistractionMechanic distraction = battle.distractions[i];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            distractionFoldouts[i] = EditorGUILayout.Foldout(distractionFoldouts[i],
                $"{distraction.mechanicName} - {distraction.mechanicType} ({distraction.activationChance}%)", true);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                battle.distractions.RemoveAt(i);
                UpdateDistractionFoldouts(battle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if (distractionFoldouts[i])
            {
                EditorGUI.indentLevel++;

                distraction.mechanicName = EditorGUILayout.TextField("Name", distraction.mechanicName);

                string[] types = new string[] { "ScreenGlitch", "ImagePopup", "ScreenShake", "ColorFlash", "FakeError", "Custom" };
                int typeIndex = Array.IndexOf(types, distraction.mechanicType);
                if (typeIndex < 0) typeIndex = 0;
                typeIndex = EditorGUILayout.Popup("Type", typeIndex, types);
                distraction.mechanicType = types[typeIndex]; distraction.isEnabled = EditorGUILayout.Toggle("Enabled", distraction.isEnabled);
                distraction.activationChance = EditorGUILayout.Slider("Activation Chance %", distraction.activationChance, 0f, 100f);
                distraction.duration = EditorGUILayout.Slider("Duration (seconds)", distraction.duration, 0.5f, 10f);
                distraction.description = EditorGUILayout.TextField("Description", distraction.description, GUILayout.Height(40));

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

                if (distraction.mechanicType == "ImagePopup")
                {
                    distraction.distractionImage = (Sprite)EditorGUILayout.ObjectField("Popup Image", distraction.distractionImage, typeof(Sprite), false);
                }

                if (distraction.mechanicType == "ColorFlash")
                {
                    distraction.flashColor = EditorGUILayout.ColorField("Flash Color", distraction.flashColor);
                }

                if (distraction.mechanicType == "ScreenGlitch" || distraction.mechanicType == "ScreenShake")
                {
                    distraction.intensity = EditorGUILayout.Slider("Intensity", distraction.intensity, 0f, 1f);
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField("Timing Settings", EditorStyles.boldLabel);
                distraction.delayBeforeActivation = EditorGUILayout.Slider("Delay Before Activation", distraction.delayBeforeActivation, 0f, 30f);
                distraction.triggerOnce = EditorGUILayout.Toggle("Trigger Once Only", distraction.triggerOnce);
                EditorGUILayout.HelpBox("If unchecked, can trigger multiple times during one minigame", MessageType.None);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        if (battle.distractions.Count == 0)
        {
            EditorGUILayout.HelpBox("No distractions added yet. Add distractions to make minigames more challenging!", MessageType.Info);
        }
    }

    // ===================================================
    // SETTINGS TAB
    // ===================================================
    private void DrawSettingsTab(BattleData battle)
    {
        EditorGUILayout.LabelField("Battle Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);

        battle.isBossBattle = EditorGUILayout.Toggle("Boss Battle", battle.isBossBattle);
        battle.backgroundScene = EditorGUILayout.TextField("Background Scene", battle.backgroundScene);
        battle.battleMusic = (AudioClip)EditorGUILayout.ObjectField("Battle Music", battle.battleMusic, typeof(AudioClip), false);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Player Settings", EditorStyles.boldLabel);
        battle.basePlayerHealth = EditorGUILayout.IntField("Base Player Health", battle.basePlayerHealth);
        battle.globalTimeModifier = EditorGUILayout.Slider("Global Time Modifier", battle.globalTimeModifier, 0.25f, 3f);
        EditorGUILayout.HelpBox($"All minigame times will be multiplied by {battle.globalTimeModifier}x", MessageType.Info);

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Rewards", EditorStyles.boldLabel);
        battle.experienceReward = EditorGUILayout.IntField("Experience", battle.experienceReward);
        battle.goldReward = EditorGUILayout.IntField("Gold", battle.goldReward);

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Item Drops (comma-separated):", EditorStyles.miniLabel);
        string itemList = string.Join(", ", battle.itemDrops);
        itemList = EditorGUILayout.TextField(itemList);
        battle.itemDrops = new List<string>(itemList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        GUILayout.Space(20);

        EditorGUILayout.LabelField("Advanced", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Battle ID", battle.id);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Generate New ID"))
        {
            if (EditorUtility.DisplayDialog("Generate New ID", "Create a new unique ID?", "Yes", "No"))
            {
                battle.id = Guid.NewGuid().ToString();
            }
        }
    }

    // ===================================================
    // HELPER METHODS
    // ===================================================
    private void AddBattle()
    {
        battleCollection.battles.Add(new BattleData());
        selectedBattleIndex = battleCollection.battles.Count - 1;
        currentTab = EditorTab.Overview;
    }

    private void DuplicateBattle(int index)
    {
        var original = battleCollection.battles[index];
        var json = JsonUtility.ToJson(original);
        var duplicate = JsonUtility.FromJson<BattleData>(json);
        duplicate.id = Guid.NewGuid().ToString();
        duplicate.battleName += " (Copy)";
        battleCollection.battles.Insert(index + 1, duplicate);
    }

    private void MoveBattle(int from, int to)
    {
        var battle = battleCollection.battles[from];
        battleCollection.battles.RemoveAt(from);
        battleCollection.battles.Insert(to, battle);
        if (selectedBattleIndex == from) selectedBattleIndex = to;
    }

    private void UpdateDialogueFoldouts(BattleData battle)
    {
        if (dialogueFoldouts == null || dialogueFoldouts.Length != battle.dialogues.Count)
            dialogueFoldouts = new bool[battle.dialogues.Count];
    }

    private void UpdateAttackFoldouts(BattleData battle)
    {
        if (attackFoldouts == null || attackFoldouts.Length != battle.attacks.Count)
            attackFoldouts = new bool[battle.attacks.Count];
    }

    private void UpdatePhaseFoldouts(BattleData battle)
    {
        if (phaseFoldouts == null || phaseFoldouts.Length != battle.phases.Count)
            phaseFoldouts = new bool[battle.phases.Count];
    }

    private void UpdateDistractionFoldouts(BattleData battle)
    {
        if (distractionFoldouts == null || distractionFoldouts.Length != battle.distractions.Count)
            distractionFoldouts = new bool[battle.distractions.Count];
    }

    private void SaveBattles()
    {
        string json = JsonUtility.ToJson(battleCollection, true);
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(savePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Battles saved to {savePath}");
        EditorUtility.DisplayDialog("Success", $"Battles saved!\n{savePath}", "OK");
    }

    private void LoadBattles()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            battleCollection = JsonUtility.FromJson<BattleCollection>(json);
            Debug.Log($"Loaded {battleCollection.battles.Count} battles");
        }
        else
        {
            battleCollection = new BattleCollection();
        }
    }
}
#endif