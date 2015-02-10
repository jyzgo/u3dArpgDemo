using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;
public class UIFusionSupplementPanel : MonoBehaviour
{
    public delegate void WantBuyItemBuyCompleteDelegate();
    public WantBuyItemBuyCompleteDelegate OnBuyCompleteHandler;

    public UIImageButton closeButton;

    public UIImageButton submitButton;

    public UILabel buyDescribeLabel;

    public UIFusionSupplementItem supplementItemSample;

    public UIGrid grid;

    public List<InventoryHCWorth> WantBuyItems;

    private List<UIFusionSupplementItem> _itemsCache = new List<UIFusionSupplementItem>();

    private List<UIFusionSupplementItem> _usingItemsList = new List<UIFusionSupplementItem>();

    private bool _priceGot;

    void Awake()
    {
        supplementItemSample.gameObject.SetActive(false);
        UIEventListener.Get(closeButton.gameObject).onClick = OnClosePanel;
        UIEventListener.Get(submitButton.gameObject).onClick = OnClickSubmitButton;
    }

    void OnEnable()
    {
        _priceGot = false;
        buyDescribeLabel.text = string.Format(Localization.Localize("IDS_MESSAGE_FUSION_HCCOSTCONDIRM"), 0);
        ConnectionManager.Instance.RegisterHandler(askPrice, true);
    }

    void askPrice()
    {
        ConnectionManager.Instance.SendACK(askPrice, true);
        NetworkManager.Instance.StoreQueryGoodsHCPrice(WantBuyItems, OnQureyHCPriceHandler);
    }

    void OnQureyHCPriceHandler(NetResponse response)
    {
        _priceGot = true;
        if (response.Succeeded)
        {
            StoreHCPriceQueryResponse sResponse = (StoreHCPriceQueryResponse)response;
            int totalCost = 0;
            foreach (InventoryHCWorth hcWorth in WantBuyItems)
            {
                hcWorth.OnePrice = sResponse.InventoryHCWorthMapping[hcWorth.ItemId].OnePrice;
                hcWorth.Discount = sResponse.InventoryHCWorthMapping[hcWorth.ItemId].Discount;
                hcWorth.DiscountPrice = sResponse.InventoryHCWorthMapping[hcWorth.ItemId].DiscountPrice;
                totalCost += hcWorth.Count * hcWorth.DiscountPrice;
                MakeSupplementItem(hcWorth);
            }
            grid.repositionNow = true;
            buyDescribeLabel.text = string.Format(Localization.Localize("IDS_MESSAGE_FUSION_HCCOSTCONDIRM"), totalCost);
        }
        else
        {
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "UIFusion");
        }
    }

    UIFusionSupplementItem MakeSupplementItem(InventoryHCWorth hcWorth)
    {
        GameObject go = null;
        if (_itemsCache.Count > 0)
        {
            go = _itemsCache[0].gameObject;
            _itemsCache.RemoveAt(0);
        }
        else
        {
            go = GameObject.Instantiate(supplementItemSample.gameObject) as GameObject;
            go.transform.parent = grid.transform;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = supplementItemSample.transform.localPosition;
        }
        UIFusionSupplementItem item = go.GetComponent<UIFusionSupplementItem>();
        item.InventoryHCWorth = hcWorth;
        go.SetActive(true);
        _usingItemsList.Add(item);
        return item;
    }

    void OnClickSubmitButton(GameObject go)
    {
        if (_priceGot)
        {
            NetworkManager.Instance.StoreBuyInventoryGoods(WantBuyItems, OnBuyItemsHandler);
        }
    }

    void OnBuyItemsHandler(NetResponse response)
    {
        if (response.Succeeded)
        {
            StoreInventoryItemsListBuyResponse sResponse = (StoreInventoryItemsListBuyResponse)response;
            sResponse.UpdateData.Broadcast();
            OnClosePanel(null);
            if (null != OnBuyCompleteHandler)
            {
                OnBuyCompleteHandler();
            }
        }
        else
        {
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "UIFusion");
        }
    }

    void OnClosePanel(GameObject go)
    {
        foreach(UIFusionSupplementItem item in _usingItemsList)
        {
            item.gameObject.SetActive(false);
            _itemsCache.Add(item);
        }
        _usingItemsList.Clear();
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        OnClosePanel(null);
    }
}
