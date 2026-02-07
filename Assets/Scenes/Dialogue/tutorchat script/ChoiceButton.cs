using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceButton : MonoBehaviour
{
    public TextMeshProUGUI choiceText;
    private Button button;
    private DialogueChoice choice;
    private DialogueManager manager;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnChoiceSelected);
        }
        else
        {
            Debug.LogError("ChoiceButton needs a Button component!");
        }
    }

    public void Setup(DialogueChoice dialogueChoice, DialogueManager dialogueManager)
    {
        if (choiceText == null)
        {
            Debug.LogError("ChoiceText is not assigned in ChoiceButton!");
            return;
        }

        choice = dialogueChoice;
        manager = dialogueManager;
        choiceText.text = dialogueChoice.choiceText;
    }

    void OnChoiceSelected()
    {
        if (manager != null && choice != null)
        {
            manager.OnPlayerChoice(choice);
        }
    }
}