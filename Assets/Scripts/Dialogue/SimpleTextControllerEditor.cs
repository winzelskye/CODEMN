#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(SimpleTextController))]
public class SimpleTextControllerEditor : Editor
{
    private SimpleTextController controller;
    private ReorderableList messageList;
    private bool showMessages = true;
    private bool showConditionTester = true;

    private Color headerColor = new Color(0.3f, 0.5f, 0.7f);

    private void OnEnable()
    {
        controller = (SimpleTextController)target;
        SetupMessageList();
    }

    private void SetupMessageList()
    {
        SerializedProperty messagesProp = serializedObject.FindProperty("messages");

        messageList = new ReorderableList(serializedObject, messagesProp, true, true, true, true);

        messageList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Messages", EditorStyles.boldLabel);
        };

        messageList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = messagesProp.GetArrayElementAtIndex(index);
            rect.y += 2;

            SerializedProperty nameProp = element.FindPropertyRelative("messageName");
            SerializedProperty textProp = element.FindPropertyRelative("text");
            SerializedProperty effectProp = element.FindPropertyRelative("textEffect");
            SerializedProperty triggerProp = element.FindPropertyRelative("nextMessageTrigger");
            SerializedProperty isSpecialProp = element.FindPropertyRelative("isSpecialMessage");
            SerializedProperty isFinalProp = element.FindPropertyRelative("isFinalText");
            SerializedProperty triggerConditionProp = element.FindPropertyRelative("triggerCondition");

            string headerText = $"[{index}] {nameProp.stringValue}";

            // Add special/final indicators
            if (isSpecialProp.boolValue)
            {
                headerText += " [SPECIAL]";
            }
            if (isFinalProp.boolValue)
            {
                headerText += " [FINAL]";
            }

            // Add trigger condition if not Always
            TriggerCondition triggerCondition = (TriggerCondition)triggerConditionProp.enumValueIndex;
            if (triggerCondition != TriggerCondition.Always)
            {
                headerText += $" [{triggerCondition}]";
            }

            NextMessageTrigger trigger = (NextMessageTrigger)triggerProp.enumValueIndex;
            headerText += $" [{trigger}]";

            TextEffect effect = (TextEffect)effectProp.enumValueIndex;
            if (effect != TextEffect.None)
            {
                headerText += $" [Effect: {effect}]";
            }

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(headerRect, headerText, EditorStyles.boldLabel);

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            string preview = textProp.stringValue;
            if (preview.Length > 60)
                preview = preview.Substring(0, 60) + "...";

            Rect previewRect = new Rect(rect.x + 15, rect.y, rect.width - 15, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(previewRect, preview, EditorStyles.miniLabel);
        };

        messageList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight * 2 + 10;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawHeader();
        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("textComponent"));
        EditorGUILayout.Space(10);

        DrawMessagesSection();
        EditorGUILayout.Space(10);

        DrawDisplaySettings();
        EditorGUILayout.Space(10);

        DrawConditionTester();
        EditorGUILayout.Space(10);

        if (Application.isPlaying)
        {
            DrawRuntimeControls();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private new void DrawHeader()
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.backgroundColor = headerColor;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Simple Text Controller", headerStyle);
        EditorGUILayout.LabelField("Advanced Message System", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.EndVertical();
    }

    private void DrawMessagesSection()
    {
        showMessages = EditorGUILayout.BeginFoldoutHeaderGroup(showMessages, "Messages");

        if (showMessages)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            messageList.DoLayoutList();

            EditorGUILayout.Space(5);

            if (messageList.index >= 0 && messageList.index < serializedObject.FindProperty("messages").arraySize)
            {
                DrawSelectedMessageDetails(messageList.index);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawSelectedMessageDetails(int index)
    {
        SerializedProperty messagesProp = serializedObject.FindProperty("messages");
        SerializedProperty message = messagesProp.GetArrayElementAtIndex(index);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Editing Message [{index}]", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(message.FindPropertyRelative("messageName"), new GUIContent("Message Name"));
        EditorGUILayout.PropertyField(message.FindPropertyRelative("text"), new GUIContent("Text Content"));

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Display Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(message.FindPropertyRelative("showOnStart"), new GUIContent("Show on Start"));
        EditorGUILayout.PropertyField(message.FindPropertyRelative("useTypewriter"));

        EditorGUILayout.Space(5);

        DrawDisplayDurationSection(message);

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Message Type", EditorStyles.boldLabel);

        SerializedProperty isSpecialProp = message.FindPropertyRelative("isSpecialMessage");
        SerializedProperty triggerButtonProp = message.FindPropertyRelative("triggerButton");
        SerializedProperty isFinalProp = message.FindPropertyRelative("isFinalText");

        EditorGUILayout.PropertyField(isSpecialProp, new GUIContent("Is Special Message"));

        if (isSpecialProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("Special messages don't appear in the normal sequence. They only show when their trigger button is clicked. After dismissing, it returns to the last normal message.", MessageType.Info);
            EditorGUILayout.PropertyField(triggerButtonProp, new GUIContent("Trigger Button"));

            if (triggerButtonProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠ No button assigned! Drag a UI Button from your scene here.", MessageType.Warning);
            }
            else
            {
                UnityEngine.UI.Button btn = triggerButtonProp.objectReferenceValue as UnityEngine.UI.Button;
                EditorGUILayout.HelpBox($"✓ This message will show when '{btn.name}' is clicked.", MessageType.None);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(isFinalProp, new GUIContent("Is Final Text"));
        if (isFinalProp.boolValue)
        {
            EditorGUILayout.HelpBox("This message will be the last one shown. After it's dismissed, the text will hide and messages will stop.", MessageType.Info);
        }

        EditorGUILayout.Space(5);

        DrawTriggerConditionSection(message);

        EditorGUILayout.Space(5);

        DrawTriggerSection(message);

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(message.FindPropertyRelative("textEffect"));

        TextEffect effect = (TextEffect)message.FindPropertyRelative("textEffect").enumValueIndex;
        if (effect != TextEffect.None)
        {
            EditorGUILayout.PropertyField(message.FindPropertyRelative("effectDuration"));
            EditorGUILayout.Space(3);
            DrawEffectSettings(message, effect);
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Styling", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(message.FindPropertyRelative("useCustomColor"));
        if (message.FindPropertyRelative("useCustomColor").boolValue)
        {
            EditorGUILayout.PropertyField(message.FindPropertyRelative("textColor"));
        }

        EditorGUILayout.PropertyField(message.FindPropertyRelative("useCustomFontSize"));
        if (message.FindPropertyRelative("useCustomFontSize").boolValue)
        {
            EditorGUILayout.PropertyField(message.FindPropertyRelative("fontSize"));
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(message.FindPropertyRelative("onMessageShown"), new GUIContent("On Message Shown"));

        EditorGUI.indentLevel--;
    }

    private void DrawDisplayDurationSection(SerializedProperty message)
    {
        EditorGUILayout.LabelField("Display Duration", EditorStyles.boldLabel);

        SerializedProperty useDisplayDuration = message.FindPropertyRelative("useDisplayDuration");
        SerializedProperty displayDuration = message.FindPropertyRelative("displayDuration");

        EditorGUILayout.PropertyField(useDisplayDuration, new GUIContent("Use Display Duration"));

        if (useDisplayDuration.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(displayDuration, new GUIContent("Duration (seconds)"));
            EditorGUILayout.HelpBox("The message will be shown for this duration before accepting input to advance. Useful for creating pauses between messages.", MessageType.Info);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawTriggerConditionSection(SerializedProperty message)
    {
        EditorGUILayout.LabelField("Trigger Condition", EditorStyles.boldLabel);

        SerializedProperty triggerCondition = message.FindPropertyRelative("triggerCondition");
        EditorGUILayout.PropertyField(triggerCondition, new GUIContent("When to Show"));

        TriggerCondition condition = (TriggerCondition)triggerCondition.enumValueIndex;

        switch (condition)
        {
            case TriggerCondition.Always:
                EditorGUILayout.HelpBox("This message will always show in sequence.", MessageType.Info);
                break;
            case TriggerCondition.OnTurnStart:
                EditorGUILayout.HelpBox("This message only shows when 'TurnStart' condition is active.", MessageType.Info);
                break;
            case TriggerCondition.OnTurnEnd:
                EditorGUILayout.HelpBox("This message only shows when 'TurnEnd' condition is active.", MessageType.Info);
                break;
            case TriggerCondition.OnHit:
                EditorGUILayout.HelpBox("This message only shows when 'Hit' condition is active.", MessageType.Info);
                break;
            case TriggerCondition.OnLowHealth:
                EditorGUILayout.HelpBox("This message only shows when 'LowHealth' condition is active.", MessageType.Info);
                break;
            case TriggerCondition.OnPhaseChange:
                EditorGUILayout.HelpBox("This message only shows when 'PhaseChange' condition is active.", MessageType.Info);
                break;
        }
    }

    private void DrawTriggerSection(SerializedProperty message)
    {
        EditorGUILayout.LabelField("Next Message Trigger", EditorStyles.boldLabel);

        SerializedProperty useCustomTrigger = message.FindPropertyRelative("useCustomTrigger");
        SerializedProperty triggerProp = message.FindPropertyRelative("nextMessageTrigger");
        SerializedProperty autoDelay = message.FindPropertyRelative("autoAdvanceDelay");

        EditorGUILayout.PropertyField(useCustomTrigger, new GUIContent("Use Custom Trigger"));

        if (useCustomTrigger.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(triggerProp, new GUIContent("Trigger Type"));

            NextMessageTrigger trigger = (NextMessageTrigger)triggerProp.enumValueIndex;
            if (trigger == NextMessageTrigger.Auto)
            {
                EditorGUILayout.PropertyField(autoDelay, new GUIContent("Auto Advance Delay"));
            }

            EditorGUILayout.HelpBox(GetTriggerDescription(trigger), MessageType.Info);

            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.HelpBox("Using global trigger setting", MessageType.Info);
        }
    }

    private string GetTriggerDescription(NextMessageTrigger trigger)
    {
        switch (trigger)
        {
            case NextMessageTrigger.PressEnter: return "Player must press Enter/Return to advance";
            case NextMessageTrigger.PressSpace: return "Player must press Spacebar to advance";
            case NextMessageTrigger.PressAnyKey: return "Player can press any key to advance";
            case NextMessageTrigger.MouseClick: return "Player must left-click to advance";
            case NextMessageTrigger.RightClick: return "Player must right-click to advance";
            case NextMessageTrigger.Button: return "Must call NextMessage() manually (e.g., from UI button)";
            case NextMessageTrigger.Auto: return "Automatically advances after delay";
            default: return "";
        }
    }

    private void DrawEffectSettings(SerializedProperty message, TextEffect effect)
    {
        SerializedProperty settings = message.FindPropertyRelative("effectSettings");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Effect Settings", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        switch (effect)
        {
            case TextEffect.Wave:
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("waveSpeed"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("waveIntensity"));
                break;
            case TextEffect.Shake:
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("shakeIntensity"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("shakeSpeed"));
                break;
            case TextEffect.Rainbow:
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("rainbowSpeed"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("rainbowSaturation"));
                break;
            case TextEffect.Pulse:
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("pulseSpeed"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("pulseIntensity"));
                break;
            case TextEffect.Bounce:
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("bounceSpeed"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("bounceHeight"));
                break;
            case TextEffect.Fade:
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("fadeSpeed"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("fadeMin"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("fadeMax"));
                break;
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawDisplaySettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Display Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("useTypewriter"));

        if (serializedObject.FindProperty("useTypewriter").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("typewriterSpeed"));
        }

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Global Next Message Trigger", EditorStyles.boldLabel);
        SerializedProperty globalTrigger = serializedObject.FindProperty("globalNextTrigger");
        EditorGUILayout.PropertyField(globalTrigger, new GUIContent("Default Trigger"));

        EditorGUILayout.HelpBox(GetTriggerDescription((NextMessageTrigger)globalTrigger.enumValueIndex), MessageType.Info);

        EditorGUILayout.EndVertical();
    }

    private void DrawConditionTester()
    {
        showConditionTester = EditorGUILayout.BeginFoldoutHeaderGroup(showConditionTester, "Condition Tester");

        if (showConditionTester)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("activeConditions"));

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Quick Condition Setters:", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set TurnStart"))
                {
                    controller.SetCondition("TurnStart", true);
                }
                if (GUILayout.Button("Set TurnEnd"))
                {
                    controller.SetCondition("TurnEnd", true);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Hit"))
                {
                    controller.SetCondition("Hit", true);
                }
                if (GUILayout.Button("Set LowHealth"))
                {
                    controller.SetCondition("LowHealth", true);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set PhaseChange"))
                {
                    controller.SetCondition("PhaseChange", true);
                }
                if (GUILayout.Button("Clear All"))
                {
                    controller.ClearAllConditions();
                }
                EditorGUILayout.EndHorizontal();

                List<string> active = controller.GetActiveConditions();
                if (active.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Active Conditions:", EditorStyles.boldLabel);
                    foreach (string condition in active)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        EditorGUILayout.LabelField("✓ " + condition, EditorStyles.miniLabel);
                        if (GUILayout.Button("Clear", GUILayout.Width(50)))
                        {
                            controller.ClearCondition(condition);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test conditions", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawRuntimeControls()
    {
        GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("▶ Start Messages"))
        {
            controller.StartMessages();
        }

        if (GUILayout.Button("⏭ Next Message"))
        {
            controller.NextMessage();
        }

        if (GUILayout.Button("⏹ Stop"))
        {
            controller.StopMessages();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Test Message Index:", GUILayout.Width(120));
        int testIndex = EditorGUILayout.IntField(messageList.index, GUILayout.Width(50));
        if (GUILayout.Button("Show"))
        {
            controller.ShowMessage(testIndex);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
#endif