using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDynamicBackground : MonoBehaviour
{
    public GameObject leftDoor;

    public GameObject rightDoor;

    void OnEnable()
    {
        float rate = 1.00f * Screen.width / Screen.height ;
        if (rate >= 1.75)
        {
            leftDoor.SetActive(true);
            rightDoor.SetActive(true);
        }
        else
        {
            leftDoor.SetActive(false);
            rightDoor.SetActive(false);
        }
    }
}
