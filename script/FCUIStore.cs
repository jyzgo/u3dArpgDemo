using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;
using System.Collections;

public class FCUIStore : MonoBehaviour
{
    public UITabButtonsGroup tabButtonsGroup;

    public UIImageButton closeButton;

    public UIImageButton backButton;

    public UIGrid grid;

    public UIPanel uiPanel;

    public UIDraggablePanel uiDraggablePanel;

    public FCUIStoreItem templateStoreItem;

    private int _currentSlectedIndex = -1;

    private FC_StoreData _currentStoreData;

    private StoreSCExchangeData _currentSCExchangeData;

    private List<FC_StoreData> _goodsOnSellDataList;

    private List<StoreSCExchangeData> _storeSCExchangeDataList;

    private Dictionary<int, FCUIStoreItem> _cacheBuyItems = new Dictionary<int, FCUIStoreItem>();

    private Dictionary<int, FCUIStoreItem> _cacheExchangeItems = new Dictionary<int, FCUIStoreItem>();

    private Dictionary<int, FCUIStoreItem> _cacheHCItems = new Dictionary<int, FCUIStoreItem>();

    private float _lastGetListTime = -60;

    void Awake()
    {
        UIEventListener.Get(backButton.gameObject).onClick = OnClickCloseButton;
        UIEventListener.Get(closeButton.gameObject).onClick = OnClickCloseButton;
        tabButtonsGroup.OnSelection = OnTabSelection;
        templateStoreItem.gameObject.SetActive(false);
    }

	public void OnInitialize()
    {
        OnInitializeWithCaller(0);
    }

    public void OnInitializeWithCaller(int tabIndex)
    {
        TownHUD.Instance.TempHide();
        tabButtonsGroup.ChangeSelection(tabIndex);
    }

    void OnDisable()
    {
        TownHUD.Instance.ResumeShow();
    }

    void OnStoreGetListHandler(NetResponse response)
    {
        if (response.Succeeded)
        {
            StoreListResponse sResponse = (StoreListResponse)response;
            _goodsOnSellDataList = sResponse.GoodsOnSellDataList;
            _storeSCExchangeDataList = sResponse.ExchangeSCDataList;
            int currentSelectedIndex = _currentSlectedIndex;
            _currentSlectedIndex = -1;
            OnTabSelection(currentSelectedIndex);
        }
    }

    void Start()
    {
        Reposition();
    }

    void Reposition()
    {
        UIRoot root = NGUITools.FindInParents<UIRoot>(gameObject);
        float designWidth = Screen.width * root.GetPixelSizeAdjustment(Screen.height);
        designWidth *= 0.95f;
        float originalX = designWidth / 2 - grid.cellWidth / 2 - 15;
        uiPanel.clipRange = new Vector4(originalX,
            uiPanel.clipRange.y,
            designWidth,
            uiPanel.clipRange.w);
        uiPanel.transform.localPosition = new Vector3(-originalX,
            uiPanel.transform.localPosition.y,
            uiPanel.transform.localPosition.z);
    }

    void SortAndRename(List<FCUIStoreItem> items)
    {
        uiDraggablePanel.DisableSpring();
        items.Sort(delegate(FCUIStoreItem left, FCUIStoreItem right)
		{
            int leftWeight = 0;
            int rightWeight = 0;
            if (null != left.StoreData)
            { 
                leftWeight = left.StoreData.IsDiscount ? left.StoreData.order + 100000 : left.StoreData.order;
                rightWeight = right.StoreData.IsDiscount ? right.StoreData.order + 100000 : right.StoreData.order;
            }
            else if(null != left.StoreSCExchangeData)
            {
                leftWeight = left.StoreSCExchangeData.Type;
                rightWeight = right.StoreSCExchangeData.Type;
            }
            return leftWeight - rightWeight;
        });
        for(int i = 0,count = items.Count; i < count; ++i)
        {
            FCUIStoreItem item = items[i];
            item.name = i.ToString("000") + "(just4sort)";
        }
        grid.repositionNow = true;
        Reposition();
    }

    void HideAllItems()
    {
        foreach (FCUIStoreItem item in _cacheBuyItems.Values)
        {
            item.gameObject.SetActive(false);
        }
        foreach (FCUIStoreItem item in _cacheExchangeItems.Values)
        {
            item.gameObject.SetActive(false);
        }
        foreach(FCUIStoreItem item in _cacheHCItems.Values)
        {
            item.gameObject.SetActive(false);
        }
    }

    FCUIStoreItem MakeFCUIStoreItem(FC_StoreData storeData)
    {
        FCUIStoreItem item = null;
        if (_cacheBuyItems.ContainsKey(storeData.id))
        {
            item = _cacheBuyItems[storeData.id];
        }
        else
        {
            GameObject go = GameObject.Instantiate(templateStoreItem.gameObject) as GameObject;
            go.transform.parent = grid.transform;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = templateStoreItem.transform.localPosition;
            item = go.GetComponent<FCUIStoreItem>();
            item.OnBuyButtonClickHandler = OnBuyButtonClickHandler;
            _cacheBuyItems[storeData.id] = item;
        }
        item.StoreData = storeData;
        item.gameObject.SetActive(true);
        return item;
    }

    FCUIStoreItem MakeFCUIStoreItem(int hcIndex)
    {
        FCUIStoreItem item = null;
        if (_cacheHCItems.ContainsKey(hcIndex))
        {
            item = _cacheHCItems[hcIndex];
        }
        else
        {
            GameObject go = GameObject.Instantiate(templateStoreItem.gameObject) as GameObject;
            go.transform.parent = grid.transform;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = templateStoreItem.transform.localPosition;
            go.SetActive(true);
            item = go.GetComponent<FCUIStoreItem>();
            item.OnClickBuyHCButton = OnClickHCBuy;
            _cacheHCItems[hcIndex] = item;
        }
        item.RefreshHCBuy(hcIndex);
        item.gameObject.SetActive(true);
        return item;
    }

    FCUIStoreItem MakeFCUIStoreItem(StoreSCExchangeData storeSCExchangeData)
    {
        FCUIStoreItem item = null;
        if (_cacheExchangeItems.ContainsKey(storeSCExchangeData.Type))
        {
            item = _cacheExchangeItems[storeSCExchangeData.Type];
        }
        else
        {
            GameObject go = GameObject.Instantiate(templateStoreItem.gameObject) as GameObject;
            go.transform.parent = grid.transform;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = templateStoreItem.transform.localPosition;
            go.SetActive(true);
            item = go.GetComponent<FCUIStoreItem>();
            item.OnSCExchangeClickHandler = OnClickSCExchangeButtonHandler;
            _cacheExchangeItems[storeSCExchangeData.Type] = item;
        }
        item.StoreSCExchangeData = storeSCExchangeData;
        item.gameObject.SetActive(true);
        return item;
    }

    void OnTabSelection(int selectIndex)
    {
        if (_currentSlectedIndex == selectIndex)
        {
            return;
        }
        _currentSlectedIndex = selectIndex;
        HideAllItems();
        float currentTime = Time.realtimeSinceStartup;
        float deltaTime = currentTime - _lastGetListTime;
        if (deltaTime > 60)
        {
            NetworkManager.Instance.StoreGetList(OnStoreGetListHandler);
            _lastGetListTime = currentTime;
        }
        else
        {
            List<FCUIStoreItem> items = new List<FCUIStoreItem>();
            switch (selectIndex)
            {
                case 0://store item sell.
                    if (null != _goodsOnSellDataList)
                    {
                        foreach (FC_StoreData storeData in _goodsOnSellDataList)
                        {
                            items.Add(MakeFCUIStoreItem(storeData));
                        }
                        SortAndRename(items);
                    }
                    break;
                case 1:
                    for (int i = 0; i < 6; ++i)
                    {
                        items.Add(MakeFCUIStoreItem(i));
                    }
                    grid.repositionNow = true;
                    uiDraggablePanel.RestrictWithinBounds(false);
                    break;
                case 2:
                    if (null != _storeSCExchangeDataList)
                    {
                        foreach (StoreSCExchangeData storeSCExchangeData in _storeSCExchangeDataList)
                        {
                            items.Add(MakeFCUIStoreItem(storeSCExchangeData));
                        }
                        SortAndRename(items);
                    }
                    break;
            }
        }
    }

    void OnClickSCExchangeButtonHandler(StoreSCExchangeData storeSCExchangeData)
    {
        if (storeSCExchangeData.RemainCount < 1)
        {
            string hint = Localization.Localize("IDS_MESSAGE_STORE_EXCHANGEFULL");
            UIMessageBoxManager.Instance.ShowMessageBox(hint, "", MB_TYPE.MB_OK, null);
        }
        else
        {
            _currentSCExchangeData = storeSCExchangeData;
            NetworkManager.Instance.StoreSCExchange(_currentSCExchangeData.Type, OnStoreSCExchangeHandler);
        }
    }

    void OnBuyButtonClickHandler(FC_StoreData storeData)
    {
        _currentStoreData = storeData;
        int price = storeData.price;
        if (storeData.IsDiscount)
        {
            price = storeData.discountPrice;
        }
        string hint = null;
        if (!_currentStoreData.IsSC && price > PlayerInfo.Instance.HardCurrency)
        {
            hint = Localization.Localize("IDS_MESSAGE_GLOBAL_HC_NOTENOUGH");
        }
        else if (_currentStoreData.IsSC && price > PlayerInfo.Instance.SoftCurrency)
        {
            hint = Localization.Localize("IDS_MESSAGE_GLOBAL_NOTENOUGHSC");
        }
        if (!string.IsNullOrEmpty(hint))
        {
            UIMessageBoxManager.Instance.ShowMessageBox(hint, "", MB_TYPE.MB_OK, OnBuyNotEnoughHandler);
        }
        else
        {
            if (!storeData.IsSC)
            {
                hint = string.Format(Localization.instance.Get("IDS_MESSAGE_GLOBAL_BUY_CONFIRM_HC"),
                price,
                Localization.Localize(storeData.displayNameIds) + " * " + storeData.count);
            }
            else
            {
                hint = string.Format(Localization.instance.Get("IDS_MESSAGE_GLOBAL_BUY_CONFIRM_SC"),
                price,
                Localization.Localize(storeData.displayNameIds) + " * " + storeData.count);
            }
			UIMessageBoxManager.Instance.ShowMessageBox(hint, "", MB_TYPE.MB_OKCANCEL, OnClickBuyCallBack);
        }
    }

    void OnStoreBuyHandler(NetResponse response)
    {
        StoreBuyResponse sResponse = (StoreBuyResponse)response;
        if (sResponse.Succeeded)
        {
            sResponse.UpdateData.Broadcast();
        }
        else
        {
            string errorString = Utils.GetErrorIDS(response.errorCode);
            UIMessageBoxManager.Instance.ShowMessageBox(errorString, "", MB_TYPE.MB_OK, null);
        }
    }

    void OnStoreSCExchangeHandler(NetResponse response)
    {
        if (response.Succeeded)
        {
            StoreSCExchangeResponse sResponse = (StoreSCExchangeResponse)response;
            sResponse.UpdateData.Broadcast();
            FCUIStoreItem item = _cacheExchangeItems[sResponse.UpdateExchangeData.Type];
            item.StoreSCExchangeData.Count = sResponse.UpdateExchangeData.Count;
            item.StoreSCExchangeData.SC = sResponse.UpdateExchangeData.SC;
            item.StoreSCExchangeData.CoseHC = sResponse.UpdateExchangeData.CoseHC;
            item.StoreSCExchangeData.CountMax = sResponse.UpdateExchangeData.CountMax;
            item.RefreshStoreSCExchangeData();
        }
        else
        {
            string errorString = Utils.GetErrorIDS(response.errorCode);
            UIMessageBoxManager.Instance.ShowMessageBox(errorString, "", MB_TYPE.MB_OK, null);
        }
    }

    void OnClickBuyCallBack(ID_BUTTON idButton)
    {
        if (idButton == ID_BUTTON.ID_OK)
        {
            NetworkManager.Instance.StoreBuy(_currentStoreData.id, OnStoreBuyHandler);
        }
    }

    void OnBuyNotEnoughHandler(ID_BUTTON idButton)
    {
        if (idButton == ID_BUTTON.ID_OK)
        {
            if (!_currentStoreData.IsSC && _currentStoreData.price > PlayerInfo.Instance.HardCurrency)
            {
                tabButtonsGroup.ChangeSelection(1);
            }
            else if (_currentStoreData.IsSC && _currentStoreData.price > PlayerInfo.Instance.SoftCurrency)
            {
                tabButtonsGroup.ChangeSelection(2);
            }
        }
    }

    void OnClickHCBuy()
    {
        UIMessageBoxManager.Instance.ShowMessageBox("comming soon...", "", MB_TYPE.MB_OK, null);
    }

    void OnClickCloseButton(GameObject go)
    {
        UIManager.Instance.CloseUI("Store");
    }
}
