using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(UIPanel))]
public class OfferingShow : MonoBehaviour
{
    public delegate void OfferingRewardCompleteDelegate();
    public OfferingRewardCompleteDelegate OnComplete;

    public UIPanel uiPanel;

    public UILabel itemName;
    public UILabel money;
    public FCSlotForItemExhibition slot;

    private ItemData _itemData;

    private bool _isPlaying;
    private const float LIMIT_TIME = 0.5f;

    public void Refresh(ItemData itemData, int sc)
    {
        _itemData = itemData;
        itemName.text = Localization.instance.Get(_itemData.nameIds);
        money.text = sc.ToString();
        if (itemData.type == ItemType.sc)
        {
            slot.gameObject.SetActive(false);
        }
        else
        {
            slot.gameObject.SetActive(true);
            slot.Refresh(_itemData.id, 1);
        }
    }

    public void Play()
    {
        gameObject.SetActive(true);
        _isPlaying = true;
        StartCoroutine(StepToShow());
    }

    IEnumerator StepToShow()
    {
        float startTime = UnityEngine.Time.realtimeSinceStartup;
        uiPanel.alpha = 0;
        while (true)
        {
            float playingTime = UnityEngine.Time.realtimeSinceStartup - startTime;
            float percent = playingTime * 1.000f / LIMIT_TIME;
            uiPanel.alpha = percent;
            if (uiPanel.alpha >= 1)
            {
                break;
            }
            yield return null;
        }
        
        StartCoroutine(StepToHide());
    }

    IEnumerator StepToHide()
    {
        yield return new WaitForSeconds(1.5f);
        float startTime = UnityEngine.Time.realtimeSinceStartup;
        
        while (true)
        {
            float playingTime = UnityEngine.Time.realtimeSinceStartup - startTime;
            float percent = playingTime * 1.000f / LIMIT_TIME;
            uiPanel.alpha = 1 - percent;
            if (uiPanel.alpha <= 0)
            {
                break;
            }
            yield return null;
        }
        uiPanel.alpha = 1;
        gameObject.SetActive(false);
        _isPlaying = false;
        OnPlayComplete();
    }

    public void OnPlayComplete()
    { 
        if(null != OnComplete)
        {
            OnComplete();
        }
    }
}
