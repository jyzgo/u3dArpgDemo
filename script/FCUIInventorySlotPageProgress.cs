using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


[RequireComponent(typeof(UISprite))]
public class FCUIInventorySlotPageProgress : MonoBehaviour
{
    public UISprite substratum;

    private bool _isInited = false;

    private UISprite _uiSprite;
    protected UISprite uiSprite
    {
        get 
        {
            if (null == _uiSprite)
            { 
                _uiSprite = GetComponent<UISprite>();
            }
            return _uiSprite;
        }
    }

    private Vector3 _basePosition = Vector3.zero;
    protected Vector3 basePosition
    {
        get 
        {
            if (Vector3.zero == _basePosition)
            {
                _basePosition = transform.localPosition;
            }
            return _basePosition;
        }
    }

    void Start()
    {
        uiSprite.enabled = false;
        substratum.enabled = false;
    }

    void Update()
    {
    }

    public void SepPagePercent(int currentPageIndex, int totalPagesCount)
    {
        if (totalPagesCount > 0)
        {
            float pageHeight = uiSprite.transform.localScale.y / totalPagesCount;
            uiSprite.fillAmount = 1.000f / totalPagesCount;
            uiSprite.transform.localPosition = new Vector3(basePosition.x,
                basePosition.y - pageHeight * currentPageIndex, basePosition.z);
        }
        uiSprite.enabled = totalPagesCount > 1;
        substratum.enabled = totalPagesCount > 1;
        if (_isInited == false)
        {
            _isInited = true;
            uiSprite.enabled = false;
            substratum.enabled = false;
        }
        else
        {
            StopAllCoroutines();
            if (uiSprite.alpha != 1 && substratum.alpha != 1)
            {
                StartCoroutine(StepToHide());
            }
            else
            {
                StartCoroutine(StepToShow());
            }
        }
    }

    IEnumerator StepToShow()
    {
        uiSprite.alpha = 0;
        substratum.alpha = 0;
        float beginTime = Time.realtimeSinceStartup;
        const float showTime = 0.3f;
        while (true)
        {
            float now = Time.realtimeSinceStartup;
            float delta = now - beginTime;
            if (delta <= showTime)
            {
                float percent = delta / showTime * 1.00f;
                uiSprite.alpha = percent;
                substratum.alpha = percent;
                yield return null;
            }
            else
            {
                break;
            }
        }
        uiSprite.alpha = 1;
        substratum.alpha = 1;
        StartCoroutine(StepToHide());
    }

    IEnumerator StepToHide()
    {
        uiSprite.alpha = 0.99f;
        substratum.alpha = 0.99f;
        yield return new WaitForSeconds(1.0f);
        float beginTime = Time.realtimeSinceStartup;
        const float hideTime = 0.3f;
        while (true)
        {
            float now = Time.realtimeSinceStartup;
            float delta = now - beginTime;
            if (delta <= hideTime)
            {
                float percent = delta / hideTime * 1.00f;
                uiSprite.alpha = 1 - percent;
                substratum.alpha = 1 - percent;
                yield return null;
            }
            else
            {
                break;
            }
        }
        uiSprite.alpha = 1;
        substratum.alpha = 1;
        uiSprite.enabled = false;
        substratum.enabled = false;
    }
}
