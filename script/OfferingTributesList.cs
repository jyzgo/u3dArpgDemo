using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class OfferingTributesList : MonoBehaviour
{
    public delegate void TributeClickDelegate(OfferingItemSlot slot);
    public TributeClickDelegate OnTributeClickHandler;

    public int offsetY = 85;

    public UIPanel uiPanel;

    public UIGrid gridOfferingItem;

    public OfferingItemSlot[] tributesSlots;

    public GameObject cloneOfferingItem;

    private OfferingData _currentOfferingData;

    float _stepPercent = 0;

    void Awake()
    {
        HideAllSlots();
    }

    void HideAllSlots()
    {
        foreach (OfferingItemSlot slot in tributesSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    void ShowAllSlots()
    {
        foreach (OfferingItemSlot slot in tributesSlots)
        {
            slot.gameObject.SetActive(true);
        }
    }

    public void Refresh(OfferingData offeringData, bool useTweenEffect = true)
    {
        uiPanel.gameObject.SetActive(true);
        _currentOfferingData = offeringData;
        if (!useTweenEffect)
        {
            DoRefresh();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(StepToHide());
        }
    }

    IEnumerator StepToHide()
    {
        _stepPercent = 0.00f;
        gridOfferingItem.transform.localPosition = new Vector3(gridOfferingItem.transform.localPosition.x,
            0,
            gridOfferingItem.transform.localPosition.z);
        float originalY = gridOfferingItem.transform.localPosition.y;
        while (_stepPercent < 1)
        {
            float posY = iTween.linear(originalY, offsetY, _stepPercent);
            gridOfferingItem.transform.localPosition = new Vector3(
                gridOfferingItem.transform.localPosition.x,
                posY,
                gridOfferingItem.transform.localPosition.z
                );
            _stepPercent += 0.05f;
            yield return null;
        }
        DoRefresh();
        StartCoroutine(StepToShow());
    }

    IEnumerator StepToShow()
    {
        _stepPercent = 0.00f;
        ShowAllSlots();
        while (_stepPercent < 1)
        {
            float posY = iTween.easeOutElastic(-offsetY, 0, _stepPercent);
            gridOfferingItem.transform.localPosition = new Vector3(
                gridOfferingItem.transform.localPosition.x,
                posY,
                gridOfferingItem.transform.localPosition.z
                );
            _stepPercent += 0.02f;
            yield return null;
        }
    }

    void DoRefresh()
    {
        for (int i = 0; i < _currentOfferingData.costItemList.Count; i++)
        {
            string itemId = _currentOfferingData.costItemList[i];
            ItemData itemData = DataManager.Instance.ItemDataManager.GetItemData(itemId);
            int itemCount = PlayerInfo.Instance.PlayerInventory.GetItemCount(itemId);

            OfferingItemSlot slot = tributesSlots[i];
            UIEventListener.Get(slot.gameObject).onClick = OnClickOfferingItem;
            slot.icon.mainTexture = InJoy.AssetBundles.AssetBundles.Load(itemData.iconPath) as Texture2D;
            slot.count.text = itemCount.ToString();
            slot.itemId = itemId;
            slot.itemCount = itemCount;
            slot.itemIcon = itemData.iconPath;
            slot.level = _currentOfferingData.level;
            slot.isCanUse = (itemCount > 0);
        }
    }

    void OnClickOfferingItem(GameObject go)
    {
        if (null != OnTributeClickHandler)
        {
            OnTributeClickHandler(go.GetComponent<OfferingItemSlot>());
        }
    }

    public int GetTributesCount()
    { 
        int count = 0;
        for (int i = 0; i < tributesSlots.Length; i++ )
        {
            count += tributesSlots[i].itemCount;
        }
        return count;
    }

    public int GetTributesCount(OfferingData offeringData)
    {
        int sum = 0;
        PlayerInfo pInfo = PlayerInfo.Instance;
        foreach (string itemId in offeringData.costItemList)
        {
            sum += pInfo.PlayerInventory.GetItemCount(itemId);
        }
        return sum;
    }

    public string GetRandomItemId()
    {
        List<OfferingItemSlot> richSlots = new List<OfferingItemSlot>();
        foreach (OfferingItemSlot slot in tributesSlots)
        {
            if (slot.itemCount > 0)
            {
                richSlots.Add(slot);  
            }
        }
        int count = richSlots.Count;
        int randomIndex = UnityEngine.Random.Range(0, count);
        return richSlots[randomIndex].itemId;
    }

    void OnEnable()
    {
        uiPanel.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        uiPanel.gameObject.SetActive(false);
    }
}
