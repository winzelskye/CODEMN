using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnitBase : MonoBehaviour
{

    private Animator animator;
    private Action onAnimationComplete;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnAnimationComplete()
    {
        onAnimationComplete();
    }

    public void PlayAnimation(string AnimationName)
    {
        animator.SetTrigger(AnimationName);
    }

    public void PlayForcedAnimation(string AnimationName, Action onAnimationComplete)
    {
        PlayAnimation(AnimationName);
        this.onAnimationComplete = onAnimationComplete;
    }
}