using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FCWorldmapLevelPathPoint : MonoBehaviour
{
    public void Born()
    {
        gameObject.SetActive(true);
        StartCoroutine(StepToTweenAlpha());
    }

    IEnumerator StepToTweenAlpha()
    {
        UISprite sprite = GetComponent<UISprite>();
        sprite.alpha = 0;
        while (sprite.alpha < 1)
        {
            sprite.alpha += 0.05f;
            yield return null;
        }
    }
}
