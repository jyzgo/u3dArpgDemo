using UnityEngine;
using System.Collections;

public class ModelAnimInstance : EffectInstance
{

    public Animation[] _animations;

    public MeshRenderer[] _renderers;

    public ParticleSystem[] _particles;

    public override void Awake()
    {

        base.Awake();

        foreach (MeshRenderer mr in _renderers)
        {
            if (mr != null)
            {
                Material newMat = Utils.CloneMaterial(mr.sharedMaterial);
                mr.sharedMaterial = newMat;
                mr.enabled = false;
            }
        }
    }

    public override void BeginEffect()
    {
        if (effectStarted)
            return;
        else
            effectStarted = true;


        base.BeginEffect();

        foreach (Animation a in _animations)
        {
            a.Play("start");
        }
        foreach (MeshRenderer mr in _renderers)
        {
            mr.enabled = true;
            mr.sharedMaterial.SetFloat("_startTime", Time.timeSinceLevelLoad);
            // if particle is disabled, play 1st emitter only.
            if (!GameSettings.Instance.IsParticleEnabled())
            {
                break;
            }
        }
        foreach (ParticleSystem ps in _particles)
        {
            ps.Clear();
            ps.enableEmission = true;
            ps.startDelay = 0.001f;
            ps.Play();
            // if particle is disabled, play 1st emitter only.
            if (!GameSettings.Instance.IsParticleEnabled())
            {
                break;
            }
        }
    }

    public override void FinishEffect(bool force)
    {
        //I will consider force, discard non-force finish
        if (_considerForce && !force)
            return;

        effectStarted = false;

        foreach (Animation a in _animations)
        {
            a.Stop();
        }
        foreach (MeshRenderer mr in _renderers)
        {
            mr.enabled = false;
        }

        foreach (ParticleSystem ps in _particles)
        {
            ps.Stop();
            ps.enableEmission = false;
        }
    }
}
