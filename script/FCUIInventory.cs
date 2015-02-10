using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

/// <summary>
///     Inventory Display Layout Policy
/// </summary>
public enum InventoryDisplayLayoutPolicy
{
    NotAvilable,
    LeftPreviewRightItemsList,
    LeftPreviewRightItemInfo,
    LeftItemInfoRightItemsList,
    LeftHeroInfoRightItemsList,
    LeftHeroInfoRightItemInfo
}

public class FCUIInventory : MonoBehaviour
{
    private ItemInventory _itemInventory;
    public ItemInventory CurrentSelectionItem
    { 
        set
        {
            _itemInventory = value;
            if (null == value)
            {
                itemsListContainer.ClearSlection();
                heroPreviewContainer.ClearSelection();
                OnResumeDefaultLayout();
            }
            else
            {
                if (_itemInventory.IsEquiped())
                    Layout(InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo);
                else
                    Layout(InventoryDisplayLayoutPolicy.LeftItemInfoRightItemsList);
            }
        } 
        get
        {
            return _itemInventory;
        }
    }

    public GameObject leftAreaPoint;//mark left point.
    public GameObject rightAreaPoint;//mark right point.
    //4 containers
    public FCUIInventoryHeroPreview heroPreviewContainer;//display the hero and hero equipments.      (LayoutIndex 0)
    public FCUIInventoryHeroInfo heroInfoConatiner;//display the hero detail information.          (LayoutIndex 1)
    public FCUIInventoryItemInfo itemInfoContainer;//display the information of the selected item. (LayoutIndex 2)
    public FCUIInventoryItemList itemsListContainer;//display the list of items(can scroll).       (LayoutIndex 3)

    public GameObject cueSlot;//live for being clone to fill inventory.

    public GameObject storeButton;
    public FCUIInventoryItemTab tab;
    public GameObject closeButton;

    private InventoryDisplayLayoutPolicy _currentLayoutPolicy = InventoryDisplayLayoutPolicy.NotAvilable;
    public InventoryDisplayLayoutPolicy CurrentLayoutPolicy { get { return _currentLayoutPolicy; } }
    private Dictionary<int, MonoBehaviour> _containerMapping;
    //1 means show at left,2 means show at right,0 means hide.
    private Dictionary<InventoryDisplayLayoutPolicy, int> _layoutSettings = new Dictionary<InventoryDisplayLayoutPolicy, int> 
    { 
        {InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList,     1002},
        {InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo,      1020},
        {InventoryDisplayLayoutPolicy.LeftItemInfoRightItemsList,    0012},
        {InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemsList,    0102},
        {InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemInfo,     0120}
    };

    private bool _isFirstOpen = true;

    #region singleton
    private static FCUIInventory _instance;
    public static FCUIInventory Instance { get { return _instance; } }
    #endregion

    void Awake()
    {
        _instance = this;
        UIEventListener.Get(closeButton).onClick = OnClickClose;
        UIEventListener.Get(storeButton).onClick = OnClickStore;

        _containerMapping = new Dictionary<int, MonoBehaviour>
        {
            {0, heroPreviewContainer},
            {1, heroInfoConatiner},
            {2, itemInfoContainer},
            {3, itemsListContainer}
        };
        tab.SelectHandler = OnFilterTabButtonSelectHandler;

        UpdateInforResponseData.EquipMoveObserver += OnEquipMoveHandler;
        UpdateInforResponseData.ItemUpdateObserver += OnItemUpdateHandler;
        UpdateInforResponseData.PlayerPropUpdateObserver += OnPlayerPropUpdateHandler;
        UpdateInforResponseData.ItemNewAddObserver += OnNewItemsAdded;
        UpdateInforResponseData.ItemsCountUpdateObserver += OnItemsCountUpdateHandler;
    }

    public void OnInitialize()
    {
        OnInitializeWithCaller(InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList);
    }

    void OnInitializeWithCaller(InventoryDisplayLayoutPolicy policy)
    {
        TownHUD.Instance.TempHide();
        Layout(policy);
        CurrentSelectionItem = null;
    }

    public void Layout(InventoryDisplayLayoutPolicy displayLayout)
    {
        _currentLayoutPolicy = displayLayout;
        int displayArea = _layoutSettings[displayLayout];
        string displayStr = displayArea.ToString("D4");
        int containerLength = displayStr.Length;
        for (int index = containerLength - 1; index >= 0; --index)
        {
            MonoBehaviour mb = _containerMapping[index];
            string bit = "";
            bit = displayStr[index] + "";
            if (bit == "1")
            {
                if (mb.gameObject.activeInHierarchy)
                {
                    mb.SendMessage("OnEnable");
                }
                else
                {
                    mb.gameObject.SetActive(true);
                }
                if (mb == itemsListContainer)
                {
                    mb.transform.localPosition = leftAreaPoint.transform.localPosition +
                        rightAreaPoint.transform.parent.localPosition;
                }
                else
                {
                    mb.transform.parent = leftAreaPoint.transform;
                    mb.transform.localPosition = Vector3.zero;
                }
            }
            else if (bit == "2")
            {
                if (mb.gameObject.activeInHierarchy)
                {
                    mb.SendMessage("OnEnable");
                }
                else
                {
                    mb.gameObject.SetActive(true);
                }
                if (mb == itemsListContainer)
                {
                    mb.transform.localPosition = rightAreaPoint.transform.localPosition +
                        rightAreaPoint.transform.parent.localPosition;
                }
                else
                {
                    mb.transform.localPosition = Vector3.zero;
                    mb.transform.parent = rightAreaPoint.transform;
                }
            }
            else
            {
                mb.gameObject.SetActive(false);
            }
        }
    }

    #region show inventory item information
    #if UNITY_EDITOR
    void OnGUI()
    {
        if(null != CurrentSelectionItem)
        {
            if (GUI.Button(new Rect(Screen.width - 200, 50, 200, 50),
                new GUIContent(CurrentSelectionItem.Item_GUID.ToString()))
            )
            {
                TextEditor te = new TextEditor();
                te.content = new GUIContent(CurrentSelectionItem.Item_GUID.ToString());
                te.OnFocus();
                te.Copy();
            }
            if (GUI.Button(new Rect(Screen.width - 200, 0, 200, 50),
                new GUIContent(CurrentSelectionItem.ItemData.id))
                )
            {
                TextEditor te = new TextEditor();
                te.content = new GUIContent(CurrentSelectionItem.ItemData.id);
                te.OnFocus();
                te.Copy();
            }
        }
    }
    #endif
    #endregion

    void OnClickStore(GameObject go)
    {
        OnClickClose(go);
        UIManager.Instance.OpenUI("Store");
    }

    void OnClickClose(GameObject go)
    {
        UIManager.Instance.CloseUI("FCUIInventory");
    }

    /// <summary>
    ///     tab change,set currentItem null
    /// </summary>
    /// <param name="CategoryList"></param>
    void OnFilterTabButtonSelectHandler(List<ItemType> CategoryList)
    {
        if(_currentLayoutPolicy != InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList)
            OnResumeDefaultLayout();
        itemsListContainer.FilterItems(CategoryList);
        CurrentSelectionItem = null;
    }

    void OnEnable()
    {
        if (!_isFirstOpen)
        {
            itemsListContainer.FilterItemsBySameFilter();
        }
        _isFirstOpen = false;
    }

    void OnDisable()
    {
        heroPreviewContainer.UpdateTownModel();
        _currentLayoutPolicy = InventoryDisplayLayoutPolicy.NotAvilable;
        PlayerInfo.Instance.PlayerInventory.ClearNewItemInInventory();
        itemsListContainer.ClearNewIcon();

        int containerLength = 4;
        for (int index = containerLength - 1; index >= 0; --index)
        {
            MonoBehaviour mb = _containerMapping[index];
            mb.gameObject.SetActive(false);
        }
        TownHUD.Instance.ResumeShow();
    }

    void OnDestroy()
    {
        UpdateInforResponseData.EquipMoveObserver -= OnEquipMoveHandler;
        UpdateInforResponseData.ItemUpdateObserver -= OnItemUpdateHandler;
        UpdateInforResponseData.PlayerPropUpdateObserver -= OnPlayerPropUpdateHandler;
        UpdateInforResponseData.ItemNewAddObserver -= OnNewItemsAdded;
        UpdateInforResponseData.ItemsCountUpdateObserver -= OnItemsCountUpdateHandler;
    }

    #region do network handler

    void OnEquipMoveHandler(ItemMoveVo[] itemMoveOps)
    {
        if (!UIManager.Instance.IsUIOpened("FCUIInventory"))
        {
            return;
        }
        bool needUpdatePreview = false;
        //equip on or off
        foreach (ItemMoveVo moveVo in itemMoveOps)
        {
            ItemInventory item = PlayerInfo.Instance.FindItemInventoryById(moveVo.ItemGUID);
            if (null != item && item.IsAvatar())
                needUpdatePreview = true;
        }
        CurrentSelectionItem = null;
        OnResumeDefaultLayout();
        itemsListContainer.FilterItemsBySameFilter();
        if (needUpdatePreview)
            heroPreviewContainer.RefreshPreview(true);
        heroPreviewContainer.RefreshLabels();
    }

    void OnNewItemsAdded(ItemInventory[] items)
    {
        if (!UIManager.Instance.IsUIOpened("FCUIInventory"))
        {
            return;
        }
        itemsListContainer.FilterItemsBySameFilter();
    }

    void OnItemUpdateHandler(ItemAttributeUpdateVoList[] itemsUpdateItemAttribute)
    {
        if (!UIManager.Instance.IsUIOpened("FCUIInventory"))
        {
            return;
        }
        itemsListContainer.FilterItemsBySameFilter();
    }

    void OnPlayerPropUpdateHandler(PlayerProp[] props)
    {
        heroInfoConatiner.OnEnable();
        heroPreviewContainer.RefreshLabels();
        itemsListContainer.OnRefreshCapacity();
    }

	void OnItemsCountUpdateHandler(List<ItemCountVo> itemCountOps)
	{
        if (!UIManager.Instance.IsUIOpened("FCUIInventory"))
        {
            return;
        }
        bool isCountZero = false;
        foreach(ItemCountVo itemCoutVo in itemCountOps)
        {
            if (itemCoutVo.ItemCount == 0)
                isCountZero = true;
        }
        itemsListContainer.FilterItemsBySameFilter();

        if (isCountZero)
        {
            OnResumeDefaultLayout();
            CurrentSelectionItem = null;
        }
	}

    #endregion

    #region do different layout
    internal void OnSwitchPreviewToInfo()
    {
        if (_currentLayoutPolicy == InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList)
            Layout(InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemsList);
        else if (_currentLayoutPolicy == InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo)
            Layout(InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemInfo);
    }

    internal void OnSwitchInfoToPreview()
    {
        if (_currentLayoutPolicy == InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemsList)
            Layout(InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList);
        else if (_currentLayoutPolicy == InventoryDisplayLayoutPolicy.LeftHeroInfoRightItemInfo)
            Layout(InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo);
    }

    internal void OnSwithToItemInfo()
    {
        if (_currentLayoutPolicy == InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList)
            Layout(InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo);
        if (_currentLayoutPolicy == InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo)
            Layout(InventoryDisplayLayoutPolicy.LeftPreviewRightItemInfo);
    }

    internal void OnResumeDefaultLayout()
    {
        Layout(InventoryDisplayLayoutPolicy.LeftPreviewRightItemsList);
    }
    #endregion
}