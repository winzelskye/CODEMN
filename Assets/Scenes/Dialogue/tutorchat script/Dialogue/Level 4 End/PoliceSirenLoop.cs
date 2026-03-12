using UnityEngine;

public class SceneLoopSFX : MonoBehaviour
{
    void Start()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = AudioSettings.Instance.sfxClips[7]; // Element 7 - police siren
        audioSource.loop = true;
        audioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        audioSource.Play();
    }
}