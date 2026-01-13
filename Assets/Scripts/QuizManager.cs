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
    public float timePenaltyForWrongAnswer = 5f; // New: Time penalty in seconds

    [Header("Questions")]
    public List<QuizQuestion> questions = new List<QuizQuestion>();

    [Header("Quiz Settings")]
    public bool randomizeQuestions = true;
    public bool caseSensitiveAnswers = false;
    public float feedbackDisplayTime = 1.5f;
    public KeyCode submitKey = KeyCode.Return;
    public bool hideCanvasOnCorrectAnswer = true; // New: Hide canvas when answer is correct
    public bool stayOnQuestionUntilCorrect = true; // New: Don't move to next question if wrong

    private List<QuizQuestion> currentQuestions;
    private int currentQuestionIndex;
    private int score;
    private float timeRemaining;
    private bool quizActive;
    private bool waitingForNextQuestion;

    void Start()
    {
        StartQuiz();
    }

    void Update()
    {
        if (!quizActive) return;

        // Timer countdown
        timeRemaining -= Time.deltaTime;
        UpdateTimerDisplay();

        if (timeRemaining <= 0)
        {
            EndQuizByTimeout();
            return;
        }

        // Submit answer with Enter key
        if (Input.GetKeyDown(submitKey) && !waitingForNextQuestion)
        {
            SubmitAnswer();
        }
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

        // Setup questions
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

        // Check answer
        bool isCorrect = caseSensitiveAnswers
            ? userAnswer == correctAnswer
            : userAnswer.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            score++;
            ShowFeedback("Correct!", Color.green);

            // Hide canvas on correct answer
            if (hideCanvasOnCorrectAnswer && quizCanvas != null)
            {
                quizCanvas.SetActive(false);
                quizActive = false;
            }
            else
            {
                UpdateScoreDisplay();
                waitingForNextQuestion = true;
                answerInputField.interactable = false;
                Invoke(nameof(NextQuestion), feedbackDisplayTime);
            }
        }
        else
        {
            // Apply time penalty for wrong answer
            timeRemaining -= timePenaltyForWrongAnswer;
            if (timeRemaining < 0) timeRemaining = 0;

            ShowFeedback($"Wrong! Try again (-{timePenaltyForWrongAnswer}s)", Color.red);

            if (stayOnQuestionUntilCorrect)
            {
                // Stay on same question - just clear the input and let them try again
                waitingForNextQuestion = true;
                answerInputField.interactable = false;
                Invoke(nameof(RetryQuestion), feedbackDisplayTime);
            }
            else
            {
                // Move to next question even if wrong
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
        // Clear input and let player try the same question again
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

            // Change color when time is low
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
        {
            quizCanvas.SetActive(false);
        }
        else
        {
            if (questionText != null)
                questionText.text = "Time's Up!";

            if (answerInputField != null)
                answerInputField.interactable = false;

            ShowFinalScore();
        }
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

    // Public method to restart quiz
    public void RestartQuiz()
    {
        CancelInvoke();
        StartQuiz();
    }

    // Public method to add time
    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
        if (timeRemaining < 0) timeRemaining = 0;
    }
}