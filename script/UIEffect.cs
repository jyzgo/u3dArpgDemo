using System;
using System.Collections.Generic;
using UnityEngine;

public class UIEffect : MonoBehaviour
{
    public delegate void PlayEffectCompleteDelegate();
    public PlayEffectCompleteDelegate OnPlayEffectComplete;

    public float duration;

    public int order;

    public bool loop;

    private bool _isPlaying;
    public bool IsPlaying
    {
        get 
        {
            return _isPlaying;
        }
    }

    private Vector3 _originalPosition;

    private float _startTime;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        _originalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        _startTime = UnityEngine.Time.realtimeSinceStartup;
        _isPlaying = true;
        foreach (ParticleSystem ps in PSList)
        {
            ps.Play(true);
        }
    }

    void OnDisable()
    {

    }

    void Update()
    {
        float deltaTime = Time.realtimeSinceStartup - _startTime;
        if (duration <= deltaTime)
        {
            _isPlaying = false;
            if (null != OnPlayEffectComplete)
            {
                OnPlayEffectComplete();
            }
            if (!loop)
            {
                gameObject.transform.localPosition = _originalPosition;
                gameObject.SetActive(false);

                foreach (ParticleSystem pSystem in PSList)
                {
                    pSystem.Stop(true);
                }
            }
        }
    }

    private List<ParticleSystem> _psList;
    public List<ParticleSystem> PSList
    {
        get 
        {
            if (null == _psList)
            {
                ParticleSystem[] ps = gameObject.GetComponentsInChildren<ParticleSystem>();
                _psList = new List<ParticleSystem>(ps);
                ParticleSystem p = gameObject.GetComponent<ParticleSystem>();
                if (null != p)
                {
                    _psList.Add(p);
                }
            }
            return _psList;
        }
    }
}
