using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum ImagePosition { Left, Center, Right }

public class MChoiceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questionText;

    [Header("Question Images (one per position)")]
    [SerializeField] private Image imageLeft;
    [SerializeField] private Image imageCenter;
    [SerializeField] private Image imageRight;

    [Header("Answer Buttons")]
    [SerializeField] private Button buttonA;
    [SerializeField] private Button buttonB;
    [SerializeField] private Button buttonC;
    [SerializeField] private Button buttonD;

    [Header("Button Labels")]
    [SerializeField] private TextMeshProUGUI labelA;
    [SerializeField] private TextMeshProUGUI labelB;
    [SerializeField] private TextMeshProUGUI labelC;
    [SerializeField] private TextMeshProUGUI labelD;

    [Header("Button Colors")]
    [SerializeField] private Color defaultColor = new Color(0.2f, 0.2f, 0.3f, 1f);
    [SerializeField] private Color correctColor = new Color(0.1f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color wrongColor = new Color(0.7f, 0.1f, 0.1f, 1f);

    [Header("Questions")]
    [SerializeField] private List<MChoiceEntry> questions = new List<MChoiceEntry>();

    [Header("Heal Settings")]
    [SerializeField] private int healAmount = 15;

    // ── Components ────────────────────────────────────────────
    private MChoiceTimer mChoiceTimer;

    // ── Runtime ───────────────────────────────────────────────
    private bool answered = false;
    private Button correctButton = null;
    private Sprite currentSprite = null;
    private ImagePosition currentImagePosition = ImagePosition.Center;

    // ─────────────────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        mChoiceTimer = GetComponent<MChoiceTimer>();
        if (mChoiceTimer == null)
            Debug.LogError("MChoiceUI: MChoiceTimer not found on this GameObject!");
    }

    private void OnEnable()
    {
        HideAllImages();
        StartCoroutine(InitNextFrame());
    }

    private IEnumerator InitNextFrame()
    {
        yield return null;

        if (questions == null || questions.Count == 0)
        {
            Debug.LogWarning("MChoiceUI: No questions configured!");
            Close(false, 0);
            yield break;
        }

        LoadRandomQuestion();
    }

    private void OnDisable()
    {
        mChoiceTimer?.StopTimer();
        StopAllCoroutines();
    }

    // ─────────────────────────────────────────────────────────
    // INTERNAL
    // ─────────────────────────────────────────────────────────

    private void LoadRandomQuestion()
    {
        answered = false;
        correctButton = null;
        currentSprite = null;
        currentImagePosition = ImagePosition.Center;

        MChoiceEntry entry = questions[Random.Range(0, questions.Count)];

        if (questionText != null)
            questionText.text = entry.question;

        currentSprite = entry.questionSprite;
        currentImagePosition = entry.imagePosition;

        HideAllImages();

        // Build shuffled answer list
        List<string> answers = new List<string>(entry.wrongAnswers) { entry.correctAnswer };
        Shuffle(answers);

        Button[] buttons = { buttonA, buttonB, buttonC, buttonD };
        TextMeshProUGUI[] labels = { labelA, labelB, labelC, labelD };

        ResetButtonColors();

        for (int i = 0; i < buttons.Length; i++)
        {
            string answer = answers[i];
            bool isCorrect = answer == entry.correctAnswer;

            if (labels[i] != null) labels[i].text = answer;
            if (isCorrect) correctButton = buttons[i];

            buttons[i].onClick.RemoveAllListeners();
            Button captured = buttons[i];
            bool capCorrect = isCorrect;
            buttons[i].onClick.AddListener(() => OnAnswerSelected(captured, capCorrect));
        }

        SetButtonsInteractable(true);
        mChoiceTimer?.StartTimer(OnTimeUp);
    }

    private void OnAnswerSelected(Button pressed, bool isCorrect)
    {
        if (answered) return;
        answered = true;

        mChoiceTimer?.StopTimer();
        SetButtonsInteractable(false);

        SetButtonColor(correctButton, correctColor);
        if (!isCorrect) SetButtonColor(pressed, wrongColor);

        // Show the correct position image only on correct answer
        if (isCorrect && currentSprite != null)
            ShowImage(currentImagePosition, currentSprite);

        StartCoroutine(CloseAfterDelay(isCorrect, isCorrect ? healAmount : 0, 1.5f));
    }

    private void OnTimeUp()
    {
        if (answered) return;
        answered = true;

        SetButtonsInteractable(false);
        SetButtonColor(correctButton, correctColor);
        HideAllImages();

        StartCoroutine(CloseAfterDelay(false, 0, 1.5f));
    }

    private void ShowImage(ImagePosition pos, Sprite sprite)
    {
        HideAllImages();

        Image target = pos switch
        {
            ImagePosition.Left => imageLeft,
            ImagePosition.Right => imageRight,
            _ => imageCenter
        };

        if (target != null)
        {
            target.sprite = sprite;
            target.gameObject.SetActive(true);
        }
    }

    private void HideAllImages()
    {
        if (imageLeft != null) imageLeft.gameObject.SetActive(false);
        if (imageCenter != null) imageCenter.gameObject.SetActive(false);
        if (imageRight != null) imageRight.gameObject.SetActive(false);
    }

    private IEnumerator CloseAfterDelay(bool correct, int heal, float delay)
    {
        yield return new WaitForSeconds(delay);
        Close(correct, heal);
    }

    private void Close(bool correct, int heal)
    {
        gameObject.SetActive(false);

        if (correct && heal > 0)
            BattleManager.Instance.player.TakeDamage(-heal);

        BattleManager.Instance.OnPlayerAttackResult(correct, false);

        FindFirstObjectByType<AttackListManager>()?.ShowBattleUI();
        FindFirstObjectByType<SkillListManager>()?.ShowBattleUI();
    }

    // ─────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────

    private void SetButtonsInteractable(bool state)
    {
        if (buttonA != null) buttonA.interactable = state;
        if (buttonB != null) buttonB.interactable = state;
        if (buttonC != null) buttonC.interactable = state;
        if (buttonD != null) buttonD.interactable = state;
    }

    private void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    private void ResetButtonColors()
    {
        SetButtonColor(buttonA, defaultColor);
        SetButtonColor(buttonB, defaultColor);
        SetButtonColor(buttonC, defaultColor);
        SetButtonColor(buttonD, defaultColor);
    }

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

[System.Serializable]
public class MChoiceEntry
{
    [TextArea(1, 10)]
    public string question = "Your question here?";

    [Tooltip("Optional image — only appears when the correct answer is selected.")]
    public Sprite questionSprite;

    [Tooltip("Which image slot to show the sprite in when correct.")]
    public ImagePosition imagePosition = ImagePosition.Center;

    public string correctAnswer = "Correct answer";

    [Tooltip("Provide exactly 3 wrong answers.")]
    public List<string> wrongAnswers = new List<string> { "Wrong 1", "Wrong 2", "Wrong 3" };
}