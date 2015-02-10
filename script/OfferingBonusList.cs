using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class OfferingBonusList : MonoBehaviour
{
    public int hidenPosY;

    public int showPosY;

    public UIAnchor anchor;

    public OfferingGropSlot sampleBonusItem;

    public UIDraggablePanel bonusDraggablePanel;

    public UIPanel uiPanel;

    public UIGrid gridOfferingGroup;

    private OfferingGroup _offeringGroup;

    private Dictionary<int, OfferingGropSlot> _offeringGroupList = new Dictionary<int, OfferingGropSlot>();

    private float _showPercent = 0.00f;

    void Awake()
    {
        
    }

    void OnEnable()
    {
        gridOfferingGroup.gameObject.SetActive(true);
    }

    public void SetOfferingGroup(OfferingGroup offeringGroup, bool forceToRefresh = false)
    {
        _offeringGroup = offeringGroup;
        if (!forceToRefresh)
        {
            anchor.enabled = false;
            StopAllCoroutines();
            StartCoroutine(StepToHide());
        }
        else
        {
            Refresh();
        }
    }

    IEnumerator StepToHide()
    { 
        while(true)
        {
            float delta = hidenPosY - transform.localPosition.y;
            if (delta.AlmostEquals(0, 0.1f))
            {
                break;
            }
            float stepY = delta / 3;
            transform.transform.localPosition = new Vector3(transform.localPosition.x,
                transform.localPosition.y + stepY, transform.localPosition.z);
            yield return null;
        }
        Refresh();
        StartCoroutine(StepToShow());
    }

    IEnumerator StepToShow()
    {
        _showPercent = 0.00f;
        float originalPosY = transform.localPosition.y;
        while (true)
        {
            if (_showPercent > 1)
            {
                break;
            }
            _showPercent += 0.015f;
            float newPosY = iTween.easeOutElastic(originalPosY, showPosY, _showPercent);
            transform.transform.localPosition = new Vector3(transform.localPosition.x,
                newPosY, transform.localPosition.z);
            yield return null;
        }
    }

    void Refresh()
    {
        float originaly = uiPanel.clipRange.w / 2 - gridOfferingGroup.cellHeight / 2 - 10;
        uiPanel.clipRange = new Vector4(uiPanel.clipRange.x,
            -originaly,
            uiPanel.clipRange.z,
            uiPanel.clipRange.w);
        uiPanel.transform.localPosition = new Vector3(uiPanel.clipRange.x,
            originaly,
            uiPanel.transform.localPosition.z);
        bonusDraggablePanel.DisableSpring();
        for (int i = 0; i < _offeringGroup.groupList.Count; i++)
        {
            SingleGroup sg = _offeringGroup.groupList[i];
            ItemData itemData = DataManager.Instance.ItemDataManager.GetItemData(sg.item);

            if (null == itemData)
            {
                continue;
            }

            GameObject clone = null;
            if (_offeringGroupList.ContainsKey(i))
            {
                clone = _offeringGroupList[i].gameObject;
            }
            else
            {
                clone = NGUITools.AddChild(gridOfferingGroup.gameObject, sampleBonusItem.gameObject);
                clone.transform.localPosition = sampleBonusItem.transform.localPosition;
                clone.SetActive(true);
                _offeringGroupList[i] = clone.GetComponent<OfferingGropSlot>();
            }
            OfferingGropSlot slot = clone.GetComponent<OfferingGropSlot>();
            slot.ItemData = itemData;
        }

        UIGrid uigrid = gridOfferingGroup.GetComponent<UIGrid>();
        uigrid.repositionNow = true;
        anchor.enabled = true;
    }
}
