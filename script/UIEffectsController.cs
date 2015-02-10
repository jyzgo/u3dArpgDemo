using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class UIEffectsController : MonoBehaviour
{
    public delegate void UIEffetPlayCompleteDelegate();
    public UIEffetPlayCompleteDelegate OnPlayEffectCompleteHandler;

    public UIEffect[] effects;

    private List<UIEffect> _effectsList;
    public List<UIEffect> EffectsList
    {
        get
        {
            if (null == _effectsList)
            {
                _effectsList = new List<UIEffect>(effects);
            }
            return _effectsList;
        }
    }

    private bool _isPlaying;
    public bool IsPlaying
    {
        get 
        {
            return _isPlaying;
        }
    }

    void Awake()
    {
    }

    public void Play()
    {
        EffectsList.Sort(delegate(UIEffect leftEffect, UIEffect rightEffect)
        {
            return leftEffect.order - rightEffect.order;
        });
        StartCoroutine(StepShowEffect());
        _isPlaying = true;
    }

    public void Stop()
    {
        foreach (UIEffect effect in EffectsList)
        {
            effect.gameObject.SetActive(false);
        }
        OnPlayComplete();
    }

    IEnumerator StepShowEffect()
    { 
        for(int i = 0,count = EffectsList.Count; i < count; ++i)
        {
            UIEffect eff = EffectsList[i];
            eff.gameObject.SetActive(true);
            while (eff.IsPlaying)
            {
                if (i + 1 < count && eff.order == EffectsList[i + 1].order)
                {
                    EffectsList[i + 1].gameObject.SetActive(true);
                    i++;
                }
                else
                {
                    yield return null;
                }
            }
        }
        OnPlayComplete();
    }

    void OnPlayComplete()
    {
        if (null != OnPlayEffectCompleteHandler)
        {
            OnPlayEffectCompleteHandler();
        }
        _isPlaying = false;
    }
}
