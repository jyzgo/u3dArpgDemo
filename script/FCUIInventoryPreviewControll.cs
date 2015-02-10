using System;
using System.Collections.Generic;
using UnityEngine;

public class FCUIInventoryPreviewControll : MonoBehaviour
{
    public Transform targetTransform;

    public GameObject leftArrow;
    public GameObject rightArrow;

	private const float _rate = 3;
    private float _step;

    void Awake()
    {
        UIEventListener.Get(leftArrow).onPress = OnPressArrow;
		UIEventListener.Get(rightArrow).onPress = OnPressArrow;
    }

    void Start()
    {

    }

    void Update()
    {
		targetTransform.localEulerAngles += new Vector3(0, _step * _rate, 0);
    }

    void OnPressArrow(GameObject go, bool state)
    {
        if (go == leftArrow && state)
            _step = 1;
        else if (go == rightArrow && state)
            _step = -1;
        else
            _step = 0;
    }
}
