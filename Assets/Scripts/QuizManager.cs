using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

[System.Serializable]
public class QuizQuestion
{
    public string question;
    public string correctAnswer;
    [TextArea(2, 4)]
    public string[] wrongAnswers;
}

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public TMP_InputField answerInputField;
    public TextMeshProUGUI timerText;
    public GameObject quizCanvas;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI feedbackText;

    [Header("Timer Settings")]
    public float totalQuizTime = 60f;
    public bool hideCanvasOnTimeout = true;
    public float timePenaltyForWrongAnswer = 5f;

    [Header("Questions")]
    public List<QuizQuestion> questions = new List<QuizQuestion>();

    [Header("Quiz Settings")]
    public bool randomizeQuestions = true;
    public bool caseSensitiveAnswers = false;
    public float feedbackDisplayTime = 1.5f;
    public KeyCode submitKey = KeyCode.Return;
    public bool hideCanvasOnCorrectAnswer = true;
    public bool stayOnQuestionUntilCorrect = true;

    private List<QuizQuestion> currentQuestions;
    private int currentQuestionIndex;
    private int score;
    private float timeRemaining;
    private bool quizActive;
    private bool waitingForNextQuestion;
    private bool initialized = false; // ← new

    void Start()
    {
        // First time setup — guaranteed to run after Inspector values are assigned
        initialized = true;
        RestartQuiz();
    }

    void OnEnable()
    {
        // Only re-randomize and restart if already initialized
        // Prevents running before questions are assigned
        if (initialized)
            RestartQuiz();
    }

    void Update()
    {
        if (!quizActive) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            EndQuizByTimeout();
            return;
        }

        if (Input.GetKeyDown(submitKey) && !waitingForNextQuestion)
            SubmitAnswer();
    }

    public void StartQuiz()
    {
        if (questions.Count == 0)
        {
            Debug.LogError("No questions added to quiz!");
            return;
        }

        score = 0;
        currentQuestionIndex = 0;
        timeRemaining = totalQuizTime;
        quizActive = true;
        waitingForNextQuestion = false;

        if (quizCanvas != null)
            quizCanvas.SetActive(true);

        if (randomizeQuestions)
            currentQuestions = questions.OrderBy(x => Random.value).ToList();
        else
            currentQuestions = new List<QuizQuestion>(questions);

        DisplayQuestion();
        UpdateScoreDisplay();
        UpdateTimerDisplay();
    }

    void DisplayQuestion()
    {
        if (currentQuestionIndex >= currentQuestions.Count)
        {
            EndQuiz();
            return;
        }

        QuizQuestion q = currentQuestions[currentQuestionIndex];
        questionText.text = q.question;

        if (answerInputField != null)
        {
            answerInputField.text = "";
            answerInputField.interactable = true;
            answerInputField.ActivateInputField();
        }

        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void SubmitAnswer()
    {
        if (waitingForNextQuestion || !quizActive) return;
        if (answerInputField == null) return;

        string userAnswer = answerInputField.text.Trim();
        if (string.IsNullOrEmpty(userAnswer)) return;

        QuizQuestion q = currentQuestions[currentQuestionIndex];
        string correctAnswer = q.correctAnswer;

        bool isCorrect = caseSensitiveAnswers
            ? userAnswer == correctAnswer
            : userAnswer.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            score++;
            ShowFeedback("Correct!", Color.green);
            quizActive = false;

            if (hideCanvasOnCorrectAnswer && quizCanvas != null)
                quizCanvas.SetActive(false);

            FindFirstObjectByType<AttackListManager>()?.ShowBattleUI();
            FindFirstObjectByType<SkillListManager>()?.ShowBattleUI();
            BattleManager.Instance.OnPlayerAttackResult(true, false);
        }
        else
        {
            timeRemaining -= timePenaltyForWrongAnswer;
            if (timeRemaining < 0) timeRemaining = 0;

            ShowFeedback($"Wrong! Try again (-{timePenaltyForWrongAnswer}s)", Color.red);

            if (stayOnQuestionUntilCorrect)
            {
                waitingForNextQuestion = true;
                answerInputField.interactable = false;
                Invoke(nameof(RetryQuestion), feedbackDisplayTime);
            }
            else
            {
                UpdateScoreDisplay();
                waitingForNextQuestion = true;
                answerInputField.interactable = false;
                Invoke(nameof(NextQuestion), feedbackDisplayTime);
            }
        }
    }

    void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }

    void RetryQuestion()
    {
        waitingForNextQuestion = false;

        if (answerInputField != null)
        {
            answerInputField.text = "";
            answerInputField.interactable = true;
            answerInputField.ActivateInputField();
        }

        if (feedbackText != null)
            feedbackText.text = "";
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        waitingForNextQuestion = false;

        if (answerInputField != null)
            answerInputField.interactable = true;

        DisplayQuestion();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (timeRemaining <= 10f)
                timerText.color = Color.red;
            else if (timeRemaining <= 30f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }
    }

    void EndQuizByTimeout()
    {
        quizActive = false;

        if (hideCanvasOnTimeout && quizCanvas != null)
            quizCanvas.SetActive(false);
        else
        {
            if (questionText != null)
                questionText.text = "Time's Up!";
            if (answerInputField != null)
                answerInputField.interactable = false;
            ShowFinalScore();
        }

        FindFirstObjectByType<AttackListManager>()?.ShowBattleUI();
        FindFirstObjectByType<SkillListManager>()?.ShowBattleUI();
        BattleManager.Instance.OnPlayerAttackResult(false, false);
    }

    void EndQuiz()
    {
        quizActive = false;

        if (questionText != null)
            questionText.text = "Quiz Complete!";
        if (answerInputField != null)
            answerInputField.interactable = false;

        ShowFinalScore();
    }

    void ShowFinalScore()
    {
        float percentage = (float)score / currentQuestions.Count * 100;
        if (feedbackText != null)
        {
            feedbackText.text = $"Final Score: {score}/{currentQuestions.Count} ({percentage:F0}%)";
            feedbackText.color = Color.white;
        }
    }

    public void RestartQuiz()
    {
        CancelInvoke();
        StartQuiz();
    }

    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
        if (timeRemaining < 0) timeRemaining = 0;
    }
}