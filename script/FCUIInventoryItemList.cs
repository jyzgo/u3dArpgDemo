using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FaustComm;

public class FCUIInventoryItemList : MonoBehaviour
{
    public FCUIInventorySlotPageController pageController;

    public UIImageButton IncrementButton;

    public UILabel capacityLabelMax;//capacity of inventory

    public UILabel capacityLabelCurrent;

    public UISprite capacityProgress;

    private GameObject _currentSelectSlot;

    private List<ItemType> _currentFilterTypeList;

    private List<ItemInventory> _filterList;

    private Dictionary<ItemInventory, GameObject> _totalFCUIInventorySlots = new Dictionary<ItemInventory, GameObject>();

    private Dictionary<InventoryMenuItemOperationType, string> _operationMapping = new Dictionary<InventoryMenuItemOperationType, string>()
    {
        {InventoryMenuItemOperationType.Information,    "IDS_BUTTON_INVENTORY_INFO"},
        {InventoryMenuItemOperationType.Fusion,         "IDS_BUTTON_GLOBAL_FUSION"},
        {InventoryMenuItemOperationType.Sell,           "IDS_BUTTON_INVENTORY_SELL"},
        {InventoryMenuItemOperationType.Equip,          "IDS_BUTTON_GLOBAL_EQUIP"}
    };

    void Awake()
    {
    }

    void Update()
    {
        
    }

    void Start()
    {
        pageController.ItemSelectionHandler = OnSelectedItem;
        UIEventListener.Get(IncrementButton.gameObject).onClick = OnClickIncrementButton;
        FilterItems();
    }

    void OnSelectedItem(ItemInventory item)
    {
        FCUIInventory.Instance.CurrentSelectionItem = item;
    }

    public void FilterItemsBySameFilter()
    {
        FilterItems(_currentFilterTypeList, false);
    }

    public void FilterItems(List<ItemType> itemTypeList = null, bool backToFirstPage = true)
    {
        Debug.Log("FilterItems");
        _currentFilterTypeList = itemTypeList;
        List<ItemInventory> list = PlayerInfo.Instance.PlayerInventory.itemList;
        _filterList = new List<ItemInventory>();
        foreach (ItemInventory item in list)
        {
            bool isContains = false;
            if (itemTypeList == null || itemTypeList.Count == 0)
                isContains = true;
            else
            {
                foreach (ItemType type in itemTypeList)
                {
                    if (item.ItemData.type == type)
                    {
                        isContains = true;
                        break;
                    }
                }
            }
            if (isContains)
                _filterList.Add(item);
        }

        _filterList.Sort(delegate(ItemInventory left, ItemInventory right)
        {
            return ItemInventory.SortCompare(left, right);
        });
        pageController.SelectedItem = null;
        pageController.Show(_filterList, backToFirstPage);
        OnRefreshCapacity();
    }

    public int GetItemIndex(ItemInventory item)
    {
        return _filterList.IndexOf(item);
    }

    public ItemInventory GetItemAt(int index)
    {
        if (_filterList.Count <= index || index < 0)
            return null;
        return _filterList[index];
    }
    
    public void OnRefreshCapacity()
    {
        capacityLabelCurrent.text = PlayerInfo.Instance.PlayerInventory.Count.ToString();
        capacityLabelMax.text = PlayerInfo.Instance.InventoryCount.ToString();
        decimal percent = (decimal)PlayerInfo.Instance.PlayerInventory.Count /
            (decimal)PlayerInfo.Instance.InventoryCount;
        capacityProgress.fillAmount = (float)percent;
        if (PlayerInfo.Instance.InventoryCount <
            (int)DataManager.Instance.CurGlobalConfig.getConfig("bagMax"))
        {
            IncrementButton.gameObject.SetActive(true);
        }
        else
        {
            IncrementButton.gameObject.SetActive(false);
        }
    }

    public void ClearSlection()
    {
        if (null != _currentSelectSlot)
        {
            _currentSelectSlot.GetComponent<FCUIInventorySlot>().GodIgnoreMe();
            _currentSelectSlot = null;
        }
        if (null != pageController)
        {
            pageController.ClearSelection();
        }
    }

    public static void OnClickIncrementButton(GameObject go = null)
    {
        GlobalConfig gc = DataManager.Instance.CurGlobalConfig;
        if (PlayerInfo.Instance.InventoryCount < (int)gc.getConfig("bagMax"))
        {
            int openCount = (int)gc.getConfig("bagIncrement");
            int costHC = (int)gc.getConfig("bagIncrementHC");
            string message = string.Format(Localization.Localize("IDS_MESSAGE_INVENTORYINCREASESPACE"), costHC, openCount);
            UIMessageBoxManager.Instance.ShowMessageBox(message, "", MB_TYPE.MB_OKCANCEL, OnSureToIncrement);
        }
        else
        {
            UIMessageBoxManager.Instance.ShowMessageBox(Localization.Localize("IDS_INVENTORY_INCREMENT_MAX"), "", MB_TYPE.MB_OK, null);
        }
    }

    public static void OnSureToIncrement(ID_BUTTON buttonID)
    {
        if (buttonID == ID_BUTTON.ID_OK)
        {
            NetworkManager.Instance.IncrementInventory(OnIncrementCallback);
        }
    }

    public static void OnIncrementCallback(NetResponse msg)
    {
        IncrementInventoryResponse response = (IncrementInventoryResponse)msg;
        if (response.Succeeded)
        {
            response.data.Broadcast();
        }
        else
        {
            UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "FCUIInventory");
        }
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        ClearSlection();
    }

    public void ClearNewIcon()
    {
        pageController.RefreshMainPage();
    }
}
