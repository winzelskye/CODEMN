using UnityEngine;
using UnityEngine.UI;

public class BlinkImage : MonoBehaviour
{
    public Image targetImage;
    public float switchInterval = 0.5f;

    private bool isBlue = true;

    private void Start()
    {
        InvokeRepeating(nameof(ToggleColor), 0f, switchInterval);
    }

    void ToggleColor()
    {
        targetImage.color = isBlue ? new Color(0f, 0f, 1f, 6f / 255f) : new Color(1f, 0f, 0f, 6f / 255f);
        isBlue = !isBlue;
    }
}