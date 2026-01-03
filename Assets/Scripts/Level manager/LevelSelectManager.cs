using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelNode
    {
        public string levelName;
        public LevelType levelType;
        public bool isCompleted;
        public Vector2 position; // Position in 0-1 range (0.5, 0.5 is center)
    }

    public enum LevelType
    {
        Tutorial,
        Normal,
        Boss
    }

    [Header("Level Configuration")]
    public List<LevelNode> levels = new List<LevelNode>();

    [Header("Node Colors")]
    public Color tutorialColor = new Color(0.4f, 0.8f, 1f); // Light blue
    public Color normalColor = Color.white;
    public Color bossColor = new Color(1f, 0.3f, 0.3f); // Red
    public Color completedColor = new Color(0.3f, 1f, 0.3f); // Green
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f); // Gray

    [Header("Visual Settings")]
    public float nodeSize = 80f;
    public float lineThickness = 8f;
    public Color lineColor = Color.white;

    [Header("References")]
    public RectTransform pathContainer;

    void Start()
    {
        if (pathContainer == null)
        {
            Debug.LogError("PathContainer is not assigned!");
            return;
        }

        SetupDefaultLevels();
        CreateLevelPath();
    }

    void SetupDefaultLevels()
    {
        if (levels.Count == 0)
        {
            // Create branching path layout matching the reference image exactly
            // The pattern goes: center -> UP HIGH -> DOWN LOW -> UP HIGH -> center
            levels.Add(new LevelNode
            {
                levelName = "The Tutorial",
                levelType = LevelType.Tutorial,
                isCompleted = false,
                position = new Vector2(0.1f, 0.5f) // Left side, middle
            });

            levels.Add(new LevelNode
            {
                levelName = "Level 2",
                levelType = LevelType.Normal,
                isCompleted = false,
                position = new Vector2(0.3f, 0.75f) // Goes WAY UP
            });

            levels.Add(new LevelNode
            {
                levelName = "Level 3",
                levelType = LevelType.Normal,
                isCompleted = false,
                position = new Vector2(0.5f, 0.25f) // Goes WAY DOWN
            });

            levels.Add(new LevelNode
            {
                levelName = "Level 4",
                levelType = LevelType.Normal,
                isCompleted = false,
                position = new Vector2(0.7f, 0.7f) // Goes UP again
            });

            levels.Add(new LevelNode
            {
                levelName = "Boss Fight",
                levelType = LevelType.Boss,
                isCompleted = false,
                position = new Vector2(0.9f, 0.5f) // End right, middle
            });
        }
    }

    void CreateLevelPath()
    {
        // Create lines first (so they appear behind nodes)
        for (int i = 0; i < levels.Count - 1; i++)
        {
            CreateLine(i, i + 1);
        }

        // Create nodes
        for (int i = 0; i < levels.Count; i++)
        {
            CreateNode(i);
        }
    }

    void CreateNode(int index)
    {
        LevelNode level = levels[index];

        GameObject node = new GameObject($"Node_{index}_{level.levelName}");
        node.transform.SetParent(pathContainer, false);

        RectTransform rect = node.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(nodeSize, nodeSize);

        // Convert 0-1 position to canvas position
        Vector2 canvasSize = pathContainer.rect.size;
        Vector2 pos = new Vector2(
            (level.position.x - 0.5f) * canvasSize.x,
            (level.position.y - 0.5f) * canvasSize.y
        );
        rect.anchoredPosition = pos;

        // Create circle image
        Image img = node.AddComponent<Image>();
        img.sprite = CreateCircleSprite();
        img.color = GetNodeColor(level, index);

        // Add outline for better visibility
        Outline outline = node.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.5f);
        outline.effectDistance = new Vector2(3, -3);
    }

    void CreateLine(int fromIndex, int toIndex)
    {
        GameObject lineObj = new GameObject($"Line_{fromIndex}_to_{toIndex}");
        lineObj.transform.SetParent(pathContainer, false);

        RectTransform rect = lineObj.AddComponent<RectTransform>();
        Image img = lineObj.AddComponent<Image>();
        img.color = lineColor;

        // Calculate positions
        Vector2 canvasSize = pathContainer.rect.size;
        Vector2 fromPos = new Vector2(
            (levels[fromIndex].position.x - 0.5f) * canvasSize.x,
            (levels[fromIndex].position.y - 0.5f) * canvasSize.y
        );
        Vector2 toPos = new Vector2(
            (levels[toIndex].position.x - 0.5f) * canvasSize.x,
            (levels[toIndex].position.y - 0.5f) * canvasSize.y
        );

        // Position and rotate the line
        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rect.anchoredPosition = fromPos;
        rect.sizeDelta = new Vector2(distance, lineThickness);
        rect.pivot = new Vector2(0, 0.5f);
        rect.localRotation = Quaternion.Euler(0, 0, angle);

        // Make line appear behind nodes
        lineObj.transform.SetAsFirstSibling();
    }

    Color GetNodeColor(LevelNode level, int index)
    {
        // If completed, always green
        if (level.isCompleted)
            return completedColor;

        // If locked (previous level not completed)
        if (index > 0 && !levels[index - 1].isCompleted)
            return lockedColor;

        // Otherwise use level type color
        switch (level.levelType)
        {
            case LevelType.Tutorial:
                return tutorialColor;
            case LevelType.Boss:
                return bossColor;
            default:
                return normalColor;
        }
    }

    Sprite CreateCircleSprite()
    {
        int resolution = 128;
        Texture2D texture = new Texture2D(resolution, resolution);
        Color[] pixels = new Color[resolution * resolution];

        Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
        float radius = resolution / 2f - 2f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * resolution + x] = distance <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    // Call this to mark a level as completed
    public void CompleteLevel(int index)
    {
        if (index >= 0 && index < levels.Count)
        {
            levels[index].isCompleted = true;

            // Refresh the display
            foreach (Transform child in pathContainer)
            {
                Destroy(child.gameObject);
            }
            CreateLevelPath();
        }
    }
}