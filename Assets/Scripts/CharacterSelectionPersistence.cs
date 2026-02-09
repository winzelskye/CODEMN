using UnityEngine;

/// <summary>
/// Persists selected character into GameData when SaveGame is called.
/// Add to CharacterSelect scene so DataPersistenceManager can write selectedCharacterName.
/// </summary>
public class CharacterSelectionPersistence : MonoBehaviour, IDataPersistence
{
    public void LoadData(GameData data)
    {
        if (data == null || string.IsNullOrEmpty(data.selectedCharacterName)) return;
        PlayerPrefs.SetString("SelectedCharacter", data.selectedCharacterName);
        PlayerPrefs.Save();
    }

    public void SaveData(ref GameData data)
    {
        if (data == null) return;
        data.selectedCharacterName = CharacterSelectionManager.GetSelectedCharacter();
    }
}
