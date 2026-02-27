using UnityEngine;

public class SceneMusicTrigger : MonoBehaviour
{
    public AudioClip music;

    void Start()
    {
        if (AudioSettings.Instance == null) return;
        AudioSettings.Instance.musicSource.Stop();
        AudioSettings.Instance.musicSource.clip = music;
        AudioSettings.Instance.musicSource.loop = true;
        AudioSettings.Instance.musicSource.Play();
    }
}