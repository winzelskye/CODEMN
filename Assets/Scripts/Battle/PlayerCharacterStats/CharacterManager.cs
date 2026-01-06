using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterStats character1;
    public CharacterStats character2;

    [Header("Current Character")]
    public CharacterStats currentCharacter;

    private void Start()
    {
        InitializeCharacters();
        SelectCharacter(character1);
    }

    private void InitializeCharacters()
    {
        if (character1 != null) character1.Initialize();
        if (character2 != null) character2.Initialize();
    }

    public void SelectCharacter(CharacterStats character)
    {
        currentCharacter = character;
        Debug.Log($"Selected character: {character.characterName}");
    }

    public void OnMinigameSuccess()
    {
        // Call this when player successfully finishes a minigame
        if (currentCharacter != null)
        {
            currentCharacter.AddBitPoints(1);
            Debug.Log($"Bit Points increased! Current: {currentCharacter.bitPoints}");
        }
    }

    public CharacterStats GetCharacter1() => character1;
    public CharacterStats GetCharacter2() => character2;
    public CharacterStats GetCurrentCharacter() => currentCharacter;
}