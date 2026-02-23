using UnityEngine;
using System.Collections.Generic;

public class DialogueNodeReimportSettings : ScriptableObject
{
    public List<string> triggerScenes = new List<string>();
    public List<string> dialogueNodeFolders = new List<string>()
    {
        "Assets/Scenes/Dialogue/tutorchat script/Dialogue/LEVEL1.Tutorial"
    };
}
