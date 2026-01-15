using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Sentence
{
    [TextArea(2, 4)]
    public string instructions;

    [TextArea(1, 5)]
    public string[] correctWords;

    [TextArea(1, 5)]
    public string[] availableWords;
}

public class ParagraphBuilder : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TextMeshProUGUI instructionsText;
    public TMPro.TextMeshProUGUI timerText;
    public Transform sentenceSlotsContainer;
    public Transform wordButtonsContainer;
    public GameObject slotPrefab;
    public GameObject wordButtonPrefab;
    public Button doneButton;
    public Canvas quizCanvas;

    [Header("Sentences")]
    public Sentence[] sentences;

    [Header("Randomization")]
    public bool randomizeSentenceOrder = true;

    [Header("Timer Settings")]
    public float timePenaltyForWrongAnswer = 5f;
    public bool countUp = true;
    public float startTime = 60f;
    public Color normalTimerColor = Color.white;
    public Color warningTimerColor = Color.yellow;
    public Color dangerTimerColor = Color.red;
    public float warningThreshold = 30f;
    public float dangerThreshold = 10f;

    private int currentSentenceIndex = 0;
    private List<WordSlot> currentSlots = new List<WordSlot>();
    private List<DraggableWord> currentWords = new List<DraggableWord>();
    private float timer = 0f;
    private bool isTimerRunning = false;
    private List<int> sentenceOrder = new List<int>();

    void Start()
    {
        if (doneButton != null)
            doneButton.onClick.AddListener(CheckAnswer);

        timer = countUp ? 0f : startTime;

        // Setup sentence order
        if (randomizeSentenceOrder)
        {
            for (int i = 0; i < sentences.Length; i++)
            {
                sentenceOrder.Add(i);
            }
            // Shuffle
            for (int i = 0; i < sentenceOrder.Count; i++)
            {
                int temp = sentenceOrder[i];
                int randomIndex = Random.Range(i, sentenceOrder.Count);
                sentenceOrder[i] = sentenceOrder[randomIndex];
                sentenceOrder[randomIndex] = temp;
            }
        }
        else
        {
            for (int i = 0; i < sentences.Length; i++)
            {
                sentenceOrder.Add(i);
            }
        }

        LoadSentence(0);
        isTimerRunning = true;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (countUp)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    timer = 0;
                    OnTimerExpired();
                    return;
                }
            }

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(timer / 60f);
                int seconds = Mathf.FloorToInt(timer % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

                if (!countUp)
                {
                    if (timer <= dangerThreshold)
                        timerText.color = dangerTimerColor;
                    else if (timer <= warningThreshold)
                        timerText.color = warningTimerColor;
                    else
                        timerText.color = normalTimerColor;
                }
                else
                {
                    timerText.color = normalTimerColor;
                }
            }
        }
    }

    void LoadSentence(int progressIndex)
    {
        if (progressIndex >= sentences.Length)
        {
            isTimerRunning = false;
            HideCanvas();
            return;
        }

        ClearCurrentSentence();

        int actualIndex = sentenceOrder[progressIndex];
        Sentence sentence = sentences[actualIndex];
        currentSentenceIndex = progressIndex;

        if (instructionsText != null)
            instructionsText.text = sentence.instructions;

        // Create slots
        foreach (string word in sentence.correctWords)
        {
            GameObject slotObj = Instantiate(slotPrefab, sentenceSlotsContainer);
            WordSlot slot = slotObj.GetComponent<WordSlot>();
            slot.correctWord = word;
            currentSlots.Add(slot);
        }

        // Create word buttons
        string[] shuffledWords = sentence.availableWords.OrderBy(x => Random.value).ToArray();

        foreach (string word in shuffledWords)
        {
            GameObject wordObj = Instantiate(wordButtonPrefab, wordButtonsContainer);

            // Position randomly
            RectTransform rect = wordObj.GetComponent<RectTransform>();
            RectTransform containerRect = wordButtonsContainer.GetComponent<RectTransform>();

            float padding = 80f;
            float randomX = Random.Range(-containerRect.rect.width / 2 + padding, containerRect.rect.width / 2 - padding);
            float randomY = Random.Range(-containerRect.rect.height / 2 + padding, containerRect.rect.height / 2 - padding);
            rect.anchoredPosition = new Vector2(randomX, randomY);

            // Set text
            DraggableWord draggable = wordObj.GetComponent<DraggableWord>();
            if (draggable != null)
            {
                draggable.SetText(word);
                currentWords.Add(draggable);
            }
        }
    }

    void ClearCurrentSentence()
    {
        foreach (WordSlot slot in currentSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        currentSlots.Clear();

        foreach (DraggableWord word in currentWords)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        currentWords.Clear();
    }

    public void CheckAnswer()
    {
        bool allCorrect = true;

        foreach (WordSlot slot in currentSlots)
        {
            if (!slot.IsCorrect())
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            currentSentenceIndex++;

            if (currentSentenceIndex >= sentences.Length)
            {
                HideCanvas();
            }
            else
            {
                LoadSentence(currentSentenceIndex);
            }
        }
        else
        {
            if (countUp)
            {
                timer += timePenaltyForWrongAnswer;
            }
            else
            {
                timer -= timePenaltyForWrongAnswer;
                if (timer < 0) timer = 0;
            }
        }
    }

    void OnTimerExpired()
    {
        isTimerRunning = false;
        HideCanvas();
    }

    void HideCanvas()
    {
        isTimerRunning = false;

        if (quizCanvas != null)
        {
            quizCanvas.gameObject.SetActive(false);
        }
    }
}