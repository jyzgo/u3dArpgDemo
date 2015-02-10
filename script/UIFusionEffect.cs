using System;
using System.Collections.Generic;
using UnityEngine;

public class UIFusionEffect : MonoBehaviour
{
    public AnimationCurve effectstatus;

    public int sortIndex;

    public delegate void PlayEffectCompleteDelegate();
    public PlayEffectCompleteDelegate OnPlayEffectComplete;

    public bool done;

    private float _startTime;

    void Start()
    {
    }

    void OnEnable()
    {
        _startTime = UnityEngine.Time.realtimeSinceStartup;
        done = false;
    }

    void Update()
    {
        float deltaTime = Time.realtimeSinceStartup - _startTime;
        float result = effectstatus.Evaluate(deltaTime);
        if (result >= 1)
        {
            if (null != OnPlayEffectComplete)
            {
                OnPlayEffectComplete();
            }
            done = true;
        }
    }
}
