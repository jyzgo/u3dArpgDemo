using UnityEngine;
using System.Collections;




public class testBulletDisplayer : BulletDisplayer
{
    public Renderer[] _renderers;
    public ParticleSystem[] _particles;
    public GameObject[] _endParticles;
    public GameObject[] _startObject;
    public float _startDuration = 0;

    public float _endDuration;

    void Awake()
    {
        foreach (MeshRenderer mr in _renderers)
        {
            if (mr != null)
            {
                Material newMat = Utils.CloneMaterial(mr.sharedMaterial);
                mr.sharedMaterial = newMat;
                mr.enabled = false;
            }
        }

        foreach (ParticleSystem p in _particles)
        {
            p.Clear();
            p.Stop();
        }

        foreach (GameObject r in _endParticles)
        {
            r.SetActive(false);
        }

        foreach (GameObject r in _startObject)
        {
            r.SetActive(true);
        }
    }

    public override void StartEffect()
    {
        bool particleEnabled = GameSettings.Instance.IsParticleEnabled();
        foreach (Renderer r in _renderers)
        {
            if (r.sharedMaterial != null)
                r.sharedMaterial.SetFloat("_startTime", Time.timeSinceLevelLoad);

            r.enabled = true;
            if (!particleEnabled)
            {
                break;
            }
        }

        foreach (ParticleSystem p in _particles)
        {
            p.Clear();
            p.enableEmission = true;
            p.startDelay = 0.001f;
            p.Play();

            if (!particleEnabled)
            {
                break;
            }
        }

        if (particleEnabled)
        {
            foreach (GameObject r in _startObject)
            {
                r.SetActive(true);
            }
        }
    }

    public override float EndEffect()
    {
        foreach (Renderer r in _renderers)
        {
            r.enabled = false;
        }
        foreach (ParticleSystem p in _particles)
        {
            p.Stop();
            p.enableEmission = false;
            if (p.name.EndsWith("_clear"))
            {
                p.Clear();
            }
        }

        if (GameSettings.Instance.IsParticleEnabled())
        {
            foreach (GameObject r in _endParticles)
            {
                r.SetActive(true);
            }
        }

        StartCoroutine(DisableEffect(_endParticles, _endDuration));
        StartCoroutine(DisableEffect(_startObject, _startDuration));
        float dt = Mathf.Max(_startDuration, _endDuration);
        return dt;
    }

    IEnumerator DisableEffect(GameObject[] renderers, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        foreach (GameObject r in renderers)
        {
            r.SetActive(false);
        }

    }

}
