using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CharacterSpriteSet
{
    public string characterName;
    public List<GameObject> spriteObjects = new List<GameObject>();
}

public class CharacterSpriteController : MonoBehaviour
{
    [Header("Character Sprite Sets")]
    [SerializeField] private List<CharacterSpriteSet> characterSpriteSets = new List<CharacterSpriteSet>();

    [Header("Settings")]
    [SerializeField] private bool applyOnStart = true;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyCharacterSprites();
        }
    }

    /// <summary>
    /// Show/hide sprites based on selected character
    /// </summary>
    public void ApplyCharacterSprites()
    {
        string selectedCharacter = CharacterSelectionManager.GetSelectedCharacter();
        ApplyCharacterSprites(selectedCharacter);
    }

    /// <summary>
    /// Show/hide sprites for a specific character
    /// </summary>
    public void ApplyCharacterSprites(string characterName)
    {
        foreach (CharacterSpriteSet spriteSet in characterSpriteSets)
        {
            bool shouldShow = spriteSet.characterName == characterName;

            foreach (GameObject spriteObj in spriteSet.spriteObjects)
            {
                if (spriteObj != null)
                {
                    spriteObj.SetActive(shouldShow);
                }
            }

            if (shouldShow)
            {
                Debug.Log($"Showing sprites for: {characterName}");
            }
        }
    }

    /// <summary>
    /// Add a sprite object to a character's set at runtime
    /// </summary>
    public void AddSpriteToCharacter(string characterName, GameObject spriteObject)
    {
        CharacterSpriteSet targetSet = characterSpriteSets.Find(set => set.characterName == characterName);

        if (targetSet != null)
        {
            if (!targetSet.spriteObjects.Contains(spriteObject))
            {
                targetSet.spriteObjects.Add(spriteObject);
            }
        }
        else
        {
            CharacterSpriteSet newSet = new CharacterSpriteSet
            {
                characterName = characterName,
                spriteObjects = new List<GameObject> { spriteObject }
            };
            characterSpriteSets.Add(newSet);
        }
    }

    /// <summary>
    /// Hide all character sprites
    /// </summary>
    public void HideAllSprites()
    {
        foreach (CharacterSpriteSet spriteSet in characterSpriteSets)
        {
            foreach (GameObject spriteObj in spriteSet.spriteObjects)
            {
                if (spriteObj != null)
                {
                    spriteObj.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Get the sprite set for a specific character
    /// </summary>
    public List<GameObject> GetCharacterSprites(string characterName)
    {
        CharacterSpriteSet spriteSet = characterSpriteSets.Find(set => set.characterName == characterName);
        return spriteSet?.spriteObjects ?? new List<GameObject>();
    }
}