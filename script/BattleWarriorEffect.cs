using UnityEngine;
using System.Collections;

public class BattleWarriorEffect : BattleCharEffect
{

    public Animation _effectAnimation = null;

    //the consistant effect
    public ParticleSystem[] _consistantEffectList = null;
    public ParticleSystem[] _consistantEffectList2 = null;	//2nd list of consistant effect
    public ParticleSystem[] _bornEffectList = null;
    public ParticleSystem[] _destroyEffectList = null;

    //end effect 0
    public ParticleSystem[] _endEffectList0 = null;
    public ParticleSystem[] _endEffectList1 = null;

    //basic mesh
    public Renderer[] _commonRenderers = null;
    public ParticleSystem[] _commonParticles = null;


    private int _currentIndex = 0;
    private bool _show = true;
    private Transform _thisTransform = null;

    Vector3 _lockTransform = Vector3.zero;
    bool _lockMe = false;

    void Awake()
    {

        //disable mesh render at beginning
        foreach (Renderer r in _commonRenderers)
        {
            r.enabled = false;
        }

    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //adjust the rotate
    void LateUpdate()
    {
        if (_thisTransform != null)
            _thisTransform.forward = Vector3.forward;

        if (_lockMe)
            _thisTransform.position = _lockTransform;
    }

    //prepare effect
    public override void PrepareEffect()
    {
        _thisTransform = gameObject.transform;

        if (_effectAnimation != null)
            _effectAnimation.Play("start");

        ResetEffect();
    }

    private void ResetEffect()
    {
        _currentIndex = 0;

        //stop all
        foreach (ParticleSystem p in _consistantEffectList)
        {
            if (p != null)
            {
                p.Stop();
                p.enableEmission = false;
                p.Clear();
            }
        }

        foreach (ParticleSystem p in _consistantEffectList2)
        {
            if (p != null)
            {
                p.Stop();
                p.enableEmission = false;
                p.Clear();
            }
        }

        foreach (ParticleSystem p in _bornEffectList)
        {
            if (p != null)
            {
                p.Stop();
                p.enableEmission = false;
                p.Clear();
            }
        }

        foreach (ParticleSystem p in _destroyEffectList)
        {
            if (p != null)
            {
                p.Stop();
                p.enableEmission = false;
                p.Clear();
            }
        }

        foreach (ParticleSystem p in _endEffectList0)
        {
            if (p != null)
            {
                p.Stop();
                p.enableEmission = false;
                p.Clear();
            }
        }

        foreach (ParticleSystem p in _endEffectList1)
        {
            if (p != null)
            {
                p.Stop();
                p.enableEmission = false;
                p.Clear();
            }
        }

    }

    //show monk passive effect
    //0 -- no effect
    //1 -- effect level 1
    //2 -- effect level 2
    //3 -- effect level 3	
    public override void ShowEffect(int effectIndex)
    {

        Assertion.Check(effectIndex >= 0);
        int lastIndex = _currentIndex;

        if (effectIndex == _currentIndex)
            return;
        else
            _currentIndex = effectIndex;

        //if it is hiding now, skip this show effect OP
        if (!_show)
            return;


        switch (effectIndex)
        {
            case 0:
                //stop all consistant
                foreach (ParticleSystem p in _consistantEffectList)
                {
                    if (p != null)
                    {
                        p.Stop();
                        p.enableEmission = false;
                        p.Clear();
                    }
                }

                foreach (ParticleSystem p in _consistantEffectList2)
                {
                    if (p != null)
                    {
                        p.Stop();
                        p.enableEmission = false;
                        p.Clear();
                    }
                }

                //close all born
                foreach (ParticleSystem p in _bornEffectList)
                {
                    if (p != null)
                    {
                        p.Stop();
                        p.enableEmission = false;
                    }
                }

                //begin destroy effect
                for (int i = 0; i < lastIndex; i++)
                {
                    if (_destroyEffectList[i] != null)
                    {
                        _destroyEffectList[i].Clear();
                        _destroyEffectList[i].enableEmission = true;
                        _destroyEffectList[i].startDelay = 0.001f;//fix a bug that position is wrong
                        _destroyEffectList[i].Play();
                    }
                }
                break;

            default:

                //start consistant 0
                if (_consistantEffectList[effectIndex - 1] != null)
                {
                    _consistantEffectList[effectIndex - 1].enableEmission = true;
                    _consistantEffectList[effectIndex - 1].startDelay = 0.001f;//fix a bug that position is wrong
                    _consistantEffectList[effectIndex - 1].Play();
                }

                if (_consistantEffectList2[effectIndex - 1] != null)
                {
                    _consistantEffectList2[effectIndex - 1].enableEmission = true;
                    _consistantEffectList2[effectIndex - 1].startDelay = 0.001f;//fix a bug that position is wrong
                    _consistantEffectList2[effectIndex - 1].Play();
                }

                //start born 0
                if (_bornEffectList[effectIndex - 1] != null)
                {
                    _bornEffectList[effectIndex - 1].enableEmission = true;
                    _bornEffectList[effectIndex - 1].startDelay = 0.001f;//fix a bug that position is wrong
                    _bornEffectList[effectIndex - 1].Play();
                }

                break;
        }
    }

    public override void Show(bool show)
    {
        _show = show;

        if (show)
        {
            //enable common renderers
            foreach (Renderer r in _commonRenderers)
            {
                r.enabled = true;
            }
            foreach (ParticleSystem p in _commonParticles)
            {
                if (p != null)
                {
                    p.enableEmission = true;
                    p.startDelay = 0.001f;//fix a bug that position is wrong
                    p.Play();
                }
            }


            //show current effects
            for (int i = 0; i < _currentIndex; i++)
            {
                if (_consistantEffectList[i] != null)
                {
                    _consistantEffectList[i].enableEmission = true;
                    _consistantEffectList[i].startDelay = 0.001f;//fix a bug that position is wrong
                    _consistantEffectList[i].Play();
                }

                if (_consistantEffectList2[i] != null)
                {
                    _consistantEffectList2[i].enableEmission = true;
                    _consistantEffectList2[i].startDelay = 0.001f;//fix a bug that position is wrong
                    _consistantEffectList2[i].Play();
                }
            }

        }
        else
        {
            //disable mesh renderers
            foreach (Renderer r in _commonRenderers)
            {
                r.enabled = false;
            }
            foreach (ParticleSystem p in _commonParticles)
            {
                if (p != null)
                {
                    p.Stop();
                    p.enableEmission = false;
                    p.Clear();
                }
            }


            //stop all consistant
            foreach (ParticleSystem p in _consistantEffectList)
            {
                if (p != null)
                {
                    p.Stop();
                    p.enableEmission = false;
                    p.Clear();
                }
            }

            foreach (ParticleSystem p in _consistantEffectList2)
            {
                if (p != null)
                {
                    p.Stop();
                    p.enableEmission = false;
                    p.Clear();
                }
            }
        }
    }


    //on/off some start mesh
    override public void ShowStartEffect(bool show)
    {
        foreach (Renderer r in _commonRenderers)
        {
            r.enabled = show;
        }

        if (show)
            foreach (ParticleSystem p in _commonParticles)
            {
                if (p != null)
                {
                    p.enableEmission = true;
                    p.startDelay = 0.001f;//fix a bug that position is wrong
                    p.Play();
                }
            }
        else
            foreach (ParticleSystem p in _commonParticles)
            {
                if (p != null)
                {
                    p.Stop();
                    p.enableEmission = false;
                    p.Clear();
                }
            }
    }

    //begin some special end effect
    override public void ShowSpecialEndEffect(int effectIndex)
    {
        //currently only support up to 2 end effects
        Assertion.Check(effectIndex >= 0);
        Assertion.Check(effectIndex < 2);

        ParticleSystem[] ps = null;

        if (effectIndex == 0)
            ps = _endEffectList0;
        else
            ps = _endEffectList1;

        foreach (ParticleSystem p in ps)
        {
            p.Clear();
            p.enableEmission = true;
            p.startDelay = 0.001f;//fix a bug that position is wrong
            p.Play();
        }
    }

    //reset location
    override public void ResetLocation(Vector3 location)
    {
        _lockTransform = location;
    }

    //lock me?
    override public void LockLocation(bool lockMe)
    {
        _lockMe = lockMe;
    }

}