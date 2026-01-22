using UnityEngine;

public class OverlayController : MonoBehaviour
{
    void Start()
    {
        // Hide at the start
        gameObject.SetActive(false);
    }

    public void ShowOverlay()
    {
        gameObject.SetActive(true);
    }

    public void HideOverlay()
    {
        gameObject.SetActive(false);
    }
}