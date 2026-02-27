using UnityEngine;
using System.Collections;

[System.Serializable]
public class NamedSFX
{
    public string name;
    public AudioClip clip;
}

[System.Serializable]
public class SceneMusicEntry
{
    public string sceneName;
    public AudioClip clip;
}

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Background Music")]
    public SceneMusicEntry[] sceneMusics;

    [Header("SFX Clips")]
    public AudioClip[] sfxClips;

    [Header("Panel SFX")]
    public AudioClip panelOpenSFX;
    public AudioClip panelCloseSFX;

    private static AudioSettings instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Apply saved volume on startup
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));
    }

    // --- Volume Controls ---

    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // --- Scene Music ---

    public void PlaySceneMusic(string sceneName)
    {
        foreach (SceneMusicEntry music in sceneMusics)
        {
            if (music.sceneName == sceneName)
            {
                musicSource.Stop();
                musicSource.clip = music.clip;
                musicSource.loop = true;
                musicSource.Play();
                return;
            }
        }
    }

    // --- Stop and Resume Music ---

    public void StopMusicForPanel()
    {
        if (panelOpenSFX != null)
            sfxSource.PlayOneShot(panelOpenSFX);
        musicSource.Pause();
    }

    public void ResumeMusicAfterPanel()
    {
        if (panelCloseSFX != null)
            sfxSource.PlayOneShot(panelCloseSFX);
        musicSource.UnPause();
    }

    // --- SFX ---

    public void PlaySFX(int index)
    {
        if (index < sfxClips.Length && sfxClips[index] != null)
            sfxSource.PlayOneShot(sfxClips[index]);
    }

    // --- Static Instance ---

    public static AudioSettings Instance => instance;
}