#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QuizManager))]
public class QuizManagerEditor : Editor
{
    private SerializedProperty questionsProp;
    private SerializedProperty questionTextProp;
    private SerializedProperty answerInputProp;
    private SerializedProperty timerTextProp;
    private SerializedProperty canvasProp;
    private SerializedProperty scoreTextProp;
    private SerializedProperty feedbackTextProp;
    private SerializedProperty totalTimeProp;
    private SerializedProperty hideOnTimeoutProp;
    private SerializedProperty timePenaltyProp;
    private SerializedProperty randomizeProp;
    private SerializedProperty caseSensitiveProp;
    private SerializedProperty feedbackTimeProp;
    private SerializedProperty submitKeyProp;
    private SerializedProperty hideCanvasOnCorrectProp;
    private SerializedProperty stayOnQuestionProp;

    private void OnEnable()
    {
        questionsProp = serializedObject.FindProperty("questions");
        questionTextProp = serializedObject.FindProperty("questionText");
        answerInputProp = serializedObject.FindProperty("answerInputField");
        timerTextProp = serializedObject.FindProperty("timerText");
        canvasProp = serializedObject.FindProperty("quizCanvas");
        scoreTextProp = serializedObject.FindProperty("scoreText");
        feedbackTextProp = serializedObject.FindProperty("feedbackText");
        totalTimeProp = serializedObject.FindProperty("totalQuizTime");
        hideOnTimeoutProp = serializedObject.FindProperty("hideCanvasOnTimeout");
        timePenaltyProp = serializedObject.FindProperty("timePenaltyForWrongAnswer");
        randomizeProp = serializedObject.FindProperty("randomizeQuestions");
        caseSensitiveProp = serializedObject.FindProperty("caseSensitiveAnswers");
        feedbackTimeProp = serializedObject.FindProperty("feedbackDisplayTime");
        submitKeyProp = serializedObject.FindProperty("submitKey");
        hideCanvasOnCorrectProp = serializedObject.FindProperty("hideCanvasOnCorrectAnswer");
        stayOnQuestionProp = serializedObject.FindProperty("stayOnQuestionUntilCorrect");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Quiz Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // UI References Section
        EditorGUILayout.LabelField("UI References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(questionTextProp, new GUIContent("Question Text"));
        EditorGUILayout.PropertyField(answerInputProp, new GUIContent("Answer Input Field"));
        EditorGUILayout.PropertyField(timerTextProp, new GUIContent("Timer Text"));
        EditorGUILayout.PropertyField(canvasProp, new GUIContent("Quiz Canvas"));
        EditorGUILayout.PropertyField(scoreTextProp, new GUIContent("Score Text"));
        EditorGUILayout.PropertyField(feedbackTextProp, new GUIContent("Feedback Text"));
        EditorGUILayout.Space();

        // Timer Settings Section
        EditorGUILayout.LabelField("Timer Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(totalTimeProp, new GUIContent("Total Time (seconds)"));
        EditorGUILayout.PropertyField(hideOnTimeoutProp, new GUIContent("Hide Canvas on Timeout"));
        EditorGUILayout.PropertyField(timePenaltyProp, new GUIContent("Time Penalty for Wrong Answer"));
        EditorGUILayout.Space();

        // Quiz Settings Section
        EditorGUILayout.LabelField("Quiz Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(randomizeProp, new GUIContent("Randomize Questions"));
        EditorGUILayout.PropertyField(caseSensitiveProp, new GUIContent("Case Sensitive Answers"));
        EditorGUILayout.PropertyField(feedbackTimeProp, new GUIContent("Feedback Display Time"));
        EditorGUILayout.PropertyField(submitKeyProp, new GUIContent("Submit Key"));
        EditorGUILayout.PropertyField(hideCanvasOnCorrectProp, new GUIContent("Hide Canvas on Correct Answer"));
        EditorGUILayout.PropertyField(stayOnQuestionProp, new GUIContent("Stay on Question Until Correct"));
        EditorGUILayout.Space();

        // Questions Section
        EditorGUILayout.LabelField("Questions", EditorStyles.boldLabel);

        // Add/Remove buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Question"))
        {
            questionsProp.arraySize++;
            SerializedProperty newQuestion = questionsProp.GetArrayElementAtIndex(questionsProp.arraySize - 1);
            newQuestion.FindPropertyRelative("question").stringValue = "New Question";
            newQuestion.FindPropertyRelative("correctAnswer").stringValue = "";
            newQuestion.FindPropertyRelative("wrongAnswers").arraySize = 0;
        }
        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog("Clear Questions",
                "Are you sure you want to delete all questions?", "Yes", "No"))
            {
                questionsProp.arraySize = 0;
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Display questions
        for (int i = 0; i < questionsProp.arraySize; i++)
        {
            SerializedProperty question = questionsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            question.isExpanded = EditorGUILayout.Foldout(question.isExpanded,
                $"Question {i + 1}: {question.FindPropertyRelative("question").stringValue}", true);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                questionsProp.DeleteArrayElementAtIndex(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            if (question.isExpanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(question.FindPropertyRelative("question"),
                    new GUIContent("Question Text"));
                EditorGUILayout.PropertyField(question.FindPropertyRelative("correctAnswer"),
                    new GUIContent("Correct Answer"));

                SerializedProperty wrongAnswers = question.FindPropertyRelative("wrongAnswers");
                EditorGUILayout.PropertyField(wrongAnswers, new GUIContent("Wrong Answers (Optional)"), true);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif