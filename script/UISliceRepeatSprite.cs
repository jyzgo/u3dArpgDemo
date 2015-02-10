using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UISliceRepeatSprite : MonoBehaviour
{
    public int totalWidth;

    public bool useScreenPercent;

    public float screenPercent = 0.5f;

    public UISprite headSprite;

    public UISprite repeatSprite;

    public UISprite tailSprite;

    void Start()
    {
    }

    void Update()
    {
        if (useScreenPercent)
        { 
            UIRoot root = NGUITools.FindInParents<UIRoot>(gameObject);
            float designWidth = Screen.width * root.GetPixelSizeAdjustment(Screen.height);
            totalWidth = (int)(designWidth * screenPercent);
        }
        headSprite.transform.localPosition = new Vector3(1, headSprite.transform.localPosition.y, headSprite.transform.localPosition.z);
        float headAndTailWidth = headSprite.transform.localScale.x + tailSprite.transform.localScale.x;
        float repeatWidth = totalWidth - headAndTailWidth;
        if (repeatWidth > 0)
        {
            repeatSprite.transform.localScale = new Vector3(repeatWidth,
                repeatSprite.transform.localScale.y, repeatSprite.transform.localScale.z);
            tailSprite.transform.localPosition = new Vector3(repeatWidth - 2,
                tailSprite.transform.localPosition.y, tailSprite.transform.localPosition.z);
        }
    }
}
