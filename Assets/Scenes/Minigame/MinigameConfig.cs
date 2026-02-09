using UnityEngine;

/// <summary>
/// Configuration for a single QTE minigame (duration, key to press, display text).
/// </summary>
[System.Serializable]
public class MinigameConfig
{
    [Tooltip("Time in seconds the player has to press the key.")]
    public float duration = 3f;

    [Tooltip("Key the player must press to succeed.")]
    public KeyCode keyToPress = KeyCode.Space;

    [Tooltip("Optional display text for the prompt (e.g. 'Press SPACE!').")]
    public string promptText = "Press SPACE!";
}
