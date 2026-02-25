using UnityEngine;
using System.Collections.Generic;

public class DialogueNodeReimportSettings : ScriptableObject
{
    public List<string> triggerScenes = new List<string>();
    public List<string> dialogueNodeFolders = new List<string>()
    {
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/LEVEL1.Tutorial",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/L1TutEnd",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 1 Start",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 1 End",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 2 Start",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 2 End",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 3 Start",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 3 End",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 4 Start",
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/Level 4 End"

    };
}
