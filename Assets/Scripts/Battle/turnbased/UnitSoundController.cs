using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSoundController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    AudioClip attackSound;
    [SerializeField]
    AudioClip deathSound;
    [SerializeField]
    AudioClip healSound;

    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(string name)
    {
        switch (name)
        {
            case "Attack":
                if (attackSound != null) audioSource.PlayOneShot(attackSound);
                break;
            case "Heal":
                if (healSound != null) audioSource.PlayOneShot(healSound);
                break;
            case "Death":
                if (deathSound != null) audioSource.PlayOneShot(deathSound);
                break;
        }
    }
}
