using UnityEngine;
using System.IO;

public class CharacterManager : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterStats character1;
    public CharacterStats character2;

    [Header("Current Character")]
    public CharacterStats currentCharacter;

    [Header("Save Settings")]
    public string saveFileName = "character_save.json";

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
        Debug.Log("Selected character: " + character.characterName);
    }

    public CharacterStats GetCharacter1() => character1;
    public CharacterStats GetCharacter2() => character2;
    public CharacterStats GetCurrentCharacter() => currentCharacter;

    // JSON Save/Load for current character
    public void SaveCurrentCharacter()
    {
        if (currentCharacter != null)
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            currentCharacter.SaveToJson(path);
            Debug.Log("Saved current character: " + currentCharacter.characterName);
        }
        else
        {
            Debug.LogWarning("No current character to save!");
        }
    }

    public void LoadCurrentCharacter()
    {
        if (currentCharacter != null)
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            currentCharacter.LoadFromJson(path);
            Debug.Log("Loaded current character: " + currentCharacter.characterName);
        }
        else
        {
            Debug.LogWarning("No current character to load to!");
        }
    }

    public void SaveCharacter(CharacterStats character, string fileName)
    {
        if (character != null)
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            character.SaveToJson(path);
            Debug.Log("Saved character: " + character.characterName + " to " + path);
        }
    }

    public void LoadCharacter(CharacterStats character, string fileName)
    {
        if (character != null)
        {
            string path = Path.Combine(Application.persistentDataPath, fileName);
            character.LoadFromJson(path);
            Debug.Log("Loaded character: " + character.characterName + " from " + path);
        }
    }

    public void SaveAllCharacters()
    {
        if (character1 != null)
        {
            SaveCharacter(character1, "character1.json");
        }
        if (character2 != null)
        {
            SaveCharacter(character2, "character2.json");
        }
        Debug.Log("Saved all characters");
    }

    public void LoadAllCharacters()
    {
        if (character1 != null)
        {
            LoadCharacter(character1, "character1.json");
        }
        if (character2 != null)
        {
            LoadCharacter(character2, "character2.json");
        }
        Debug.Log("Loaded all characters");
    }

    public string GetSavePath()
    {
        return Application.persistentDataPath;
    }
}