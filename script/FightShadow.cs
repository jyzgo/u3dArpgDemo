using UnityEngine;
using System.Collections;

public class FightShadow : MonoBehaviour
{

    public MeshRenderer _renderer;
    public Transform _referenceTransform;
    public float _duration;
    public float _departTime;
    public float _offset;
    public AnimationCurve _scaleAnimaiton;
    public AnimationCurve _alphaAnimation;

    float _timer = 0.0f;

    public void Show()
    {
        _timer = _duration;
        _renderer.enabled = true;
        transform.parent = _referenceTransform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = transform.localPosition + Vector3.up * _offset;
    }

    void Update()
    {
        if (_timer > 0.0f)
        {
            _timer -= Time.deltaTime;
            float percent = 1.0f - _timer / _duration;
            float scale = _scaleAnimaiton.Evaluate(percent);
            float alpha = _alphaAnimation.Evaluate(percent);
            transform.localScale = Vector3.one * scale;
            _renderer.sharedMaterial.SetFloat("_alpha", alpha);
            if (_timer <= _departTime)
            {
                transform.parent = null;
            }
            if (_timer <= 0.0f)
            {
                _renderer.enabled = false;
            }
        }
    }
}
