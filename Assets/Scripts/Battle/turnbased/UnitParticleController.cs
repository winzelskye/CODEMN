using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitParticleController : MonoBehaviour
{
    ParticleSystem currentParticleSystem;

    public IEnumerator PlayParticle(string name, Action onParticleFinish)
    {
        switch (name)
        {
            case "Heal":
                currentParticleSystem = transform.Find(name).GetComponent<ParticleSystem>();
                if (currentParticleSystem != null)
                {
                    currentParticleSystem.Play();
                }
                break;
            case "Death":
                currentParticleSystem = transform.Find(name).GetComponent<ParticleSystem>();
                if (currentParticleSystem != null)
                {
                    currentParticleSystem.Play();
                }
                break;
        }
        yield return new WaitForSeconds(currentParticleSystem.main.duration + currentParticleSystem.main.startLifetime.constantMax);
        onParticleFinish();
    }
}