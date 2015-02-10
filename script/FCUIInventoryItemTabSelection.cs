using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCUIInventoryItemTabSelection : MonoBehaviour
{
    public GameObject leftTop;
    public GameObject leftBottom;
    public GameObject rightTop;
    public GameObject rightBottom;

    public float floatMax = 3.0f;
    private float _currentStep;
    private bool _isIncrement;

    private Vector3 _v3LeftTop;
    private Vector3 _v3LeftBottom;
    private Vector3 _v3RightTop;
    private Vector3 _v3RightBottom;

    void Awake()
    {
        _v3LeftTop      = leftTop.transform.localPosition;
        _v3LeftBottom   = leftBottom.transform.localPosition;
        _v3RightTop     = rightTop.transform.localPosition;
        _v3RightBottom  = rightBottom.transform.localPosition;
    }

    void Start()
    {
    }

    void OnEnable()
    {
        StartCoroutine(StepToTween());
    }

    IEnumerator StepToTween()
    {
        while(true)
        {
            if (Mathf.Abs(_currentStep) >= floatMax)
            {
                _isIncrement = !_isIncrement;
            }
            _currentStep = _isIncrement ? _currentStep + 0.2f : _currentStep - 0.2f;
            leftTop.transform.localPosition = _v3LeftTop + new Vector3(-_currentStep, _currentStep, 0);
            leftBottom.transform.localPosition = _v3LeftBottom + new Vector3(-_currentStep, -_currentStep, 0);
            rightTop.transform.localPosition = _v3RightTop + new Vector3(_currentStep, _currentStep, 0);
            rightBottom.transform.localPosition = _v3RightBottom + new Vector3(_currentStep, -_currentStep, 0);
            yield return null;
        }
    }
}
