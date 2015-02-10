using System;
using System.Collections.Generic;
using UnityEngine;
using FaustComm;

public class FCUIInventoryItemInfo : MonoBehaviour
{
    public GameObject otherLayout;
    public GameObject commonLayout;

    public UIImageButton equipButton;
    public UIImageButton sellButton;
    public UIImageButton fusionButton;
    public UIFightScore fightScore;
    public UISoftCurrency costSC;

    public FCUIInventoryTattooSlot tattooSlot;
    public FCUIInventorySlot slot;
    public UILabel itemNameLabel;

    public InventoryItemProperty itemPartValue;
    public InventoryItemProperty equipLevelValue;
    public InventoryItemProperty useLevelValue;
    public InventoryItemProperty fusionLevelValue;

    public InventoryItemProperty attribute1;
    public InventoryItemProperty attribute2;
    public InventoryItemProperty attribute3;
    public InventoryItemProperty attribute4;
    public InventoryItemProperty attribute5;
    public InventoryItemProperty attribute6;

    //about tattoo
    public UIImageButton rightButton;
    public UIImageButton otherSellButton;
    public UIImageButton middleButton;

    private ItemInventory _currentItem;

    private Dictionary<ItemSubType, string> _armorNamesMapping = new Dictionary<ItemSubType, string>()
    {
        {ItemSubType.helmet,      "IDS_NAME_EQUIPMENT_PART_HELMET"},
        {ItemSubType.shoulder,    "IDS_NAME_EQUIPMENT_PART_SHOULDER"},
        {ItemSubType.armpiece,    "IDS_NAME_EQUIPMENT_PART_ARMPIECE"},
        {ItemSubType.chest,       "IDS_NAME_EQUIPMENT_PART_CHEST"},
        {ItemSubType.belt,        "IDS_NAME_EQUIPMENT_PART_BELT"},
        {ItemSubType.leggings,    "IDS_NAME_EQUIPMENT_PART_LEGGINGS"},
        {ItemSubType.ring,        "IDS_NAME_EQUIPMENT_PART_RING"},
        {ItemSubType.necklace,    "IDS_NAME_EQUIPMENT_PART_NECKLACE"}
    };

    void Start()
    {
        UIEventListener.Get(fusionButton.gameObject).onClick = OnFusionButtonClick;
        UIEventListener.Get(equipButton.gameObject).onClick = OnEquipOnBody;
        UIEventListener.Get(sellButton.gameObject).onClick = OnClickSellItem;

        UIEventListener.Get(rightButton.gameObject).onClick = OnTattooDisenchantButtonClick;
        UIEventListener.Get(middleButton.gameObject).onClick = OnTattooEquipButtonClick;
        UIEventListener.Get(otherSellButton.gameObject).onClick = OnTattooSellButtonClick;
    }

    void OnEnable()
    {
        if (null != FCUIInventory.Instance)
        {
            _currentItem = FCUIInventory.Instance.CurrentSelectionItem;
            itemNameLabel.text = Localization.Localize(_currentItem.ItemData.nameIds);
            itemNameLabel.color = FCConst.RareColorMapping[(EnumRareLevel)_currentItem.ItemData.rareLevel];
            if (_currentItem.ItemData.type != ItemType.tattoo)
            {
                slot.Item = _currentItem;
            }
            equipLevelValue.Refresh(Localization.Localize("IDS_MESSAGE_ITEMINFO_ITEMLEVEL"), _currentItem.ItemData.level.ToString());
            useLevelValue.Refresh(Localization.Localize("IDS_MESSAGE_ITEMINFO_ITEMENABLELEVEL"), _currentItem.ItemData.enableLevel.ToString());
            fightScore.FS = _currentItem.FS;
            tattooSlot.gameObject.SetActive(false);
            slot.gameObject.SetActive(true);
            costSC.SC = _currentItem.CostSC;
            if (_currentItem.IsEquipment())
            {
                SitchOtherLayout(false);

                itemPartValue.Refresh(Localization.Localize("IDS_MESSAGE_INVENTORY_EQUIPPART"), Localization.Localize(Utils.GetItemPartNamesIDS(_currentItem.ItemData.type, _currentItem.ItemData.subType)));
                fusionLevelValue.Refresh(Localization.Localize("IDS_MESSAGE_ITEMINFO_ITEMFUSIONLEVEL"), _currentItem.CurrentFusionLevel().ToString());
                attribute1.Refresh(_currentItem, 0);
                attribute2.Refresh(_currentItem, 1);
                attribute3.Refresh(_currentItem, 2);
                attribute4.Clear();
                attribute5.Clear();
                attribute6.Clear();
                equipButton.label.text = Localization.Localize("IDS_BUTTON_GLOBAL_EQUIP");
                equipButton.gameObject.SetActive(true);
                fusionButton.label.text = Localization.Localize("IDS_BUTTON_GLOBAL_FUSION");
                fusionButton.gameObject.SetActive(true);

                itemPartValue.gameObject.SetActive(true);
                useLevelValue.gameObject.SetActive(true);
                fusionLevelValue.gameObject.SetActive(true);
                equipLevelValue.gameObject.SetActive(true);
                fightScore.gameObject.SetActive(true);

                //whether the equipment is on body
                if (_currentItem.IsEquiped())
                {
                    equipButton.GetComponent<UIImageButton>().isEnabled = false;
                    sellButton.GetComponent<UIImageButton>().isEnabled = false;
                }
                else
                {
                    equipButton.GetComponent<UIImageButton>().isEnabled = true;
                    sellButton.GetComponent<UIImageButton>().isEnabled = true;
                }
                //fusion
                if (null != _currentItem.NextFusionData)
                {
                    fusionButton.GetComponent<UIImageButton>().isEnabled = true;
                }
                else
                {
                    fusionButton.GetComponent<UIImageButton>().isEnabled = false;
                }
            }
            else//not equipment
            {
                SitchOtherLayout(true);

                itemPartValue.Refresh(Localization.Localize("IDS_MESSAGE_ITEMINFO_ITEMTYPE"), Localization.Localize(Utils.GetItemPartNamesIDS(_currentItem.ItemData.type, _currentItem.ItemData.subType)));
                attribute1.Refresh(Localization.Localize(_currentItem.ItemData.descriptionIds));
                attribute2.Clear();
                attribute3.Clear();
                attribute4.Clear();
                attribute5.Clear();
                attribute6.Clear();

                itemPartValue.gameObject.SetActive(true);
                equipLevelValue.gameObject.SetActive(true);
                useLevelValue.gameObject.SetActive(false);
                fusionLevelValue.gameObject.SetActive(false);
                fightScore.gameObject.SetActive(false);

                middleButton.gameObject.SetActive(true);
                otherSellButton.gameObject.SetActive(true);

                //tatoo
                if (_currentItem.ItemData.type == ItemType.tattoo)
                {
                    rightButton.isEnabled = true;
                    rightButton.gameObject.SetActive(true);
                    rightButton.label.text = Localization.Localize("IDS_BUTTON_TATTOO_TATTOODISENCHANT");

                    middleButton.gameObject.SetActive(true);
                    middleButton.label.text = Localization.Localize("IDS_BUTTON_GLOBAL_EQUIP");
                    tattooSlot.tattooItem = _currentItem;
                    tattooSlot.gameObject.SetActive(true);
                    slot.gameObject.SetActive(false);
                    useLevelValue.gameObject.SetActive(true);
                    fusionLevelValue.gameObject.SetActive(true);
                    TattooData td = DataManager.Instance.GetTattooData(_currentItem.ItemData.id);
                    useLevelValue.Refresh(Localization.Localize("IDS_MESSAGE_TATTOO_TATTOOBEARPOINT")
                        , td.bearingPoint.ToString());
                    fusionLevelValue.Refresh(Localization.Localize("IDS_MESSAGE_TATTOO_TATTOOPOSITION")
                        , Utils.GetTattooApplicablePositions(td.applicableParts));
                }//tribute
                else if(_currentItem.ItemData.type == ItemType.tribute)
                {
                    middleButton.gameObject.SetActive(false);

                    rightButton.gameObject.SetActive(true);
                    rightButton.label.text = Localization.Localize("IDS_BUTTON_HUD_TRIBUTE");
                }//recipe
                else if (_currentItem.ItemData.type == ItemType.recipe)
                {
                    middleButton.isEnabled = true;
                    middleButton.gameObject.SetActive(true);
                    middleButton.label.text = Localization.Localize("IDS_BUTTON_TATTOO_ACTIVATION");

                    rightButton.isEnabled = true;
                    rightButton.gameObject.SetActive(true);
                    rightButton.label.text = Localization.Localize("IDS_BUTTON_GLOBAL_EXCHANGE");
                }
                else
                {
                    middleButton.gameObject.SetActive(false);
                    rightButton.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnTattooDisenchantButtonClick(GameObject go)
    {
        if (_currentItem.ItemData.type == ItemType.recipe)
        {
            UIManager.Instance.CloseUI("FCUIInventory");
            UIManager.Instance.OpenUI("Tattoo", new System.Object[] { _currentItem.ItemData, "learn" });
        }
        else if(_currentItem.ItemData.type == ItemType.tattoo)
        {
            UIManager.Instance.CloseUI("FCUIInventory");
            UIManager.Instance.OpenUI("Tattoo", new System.Object[] { _currentItem.ItemData, "disenchant" });
        }
    }

    void OnTattooEquipButtonClick(GameObject go)
    {
        if (_currentItem.ItemData.type == ItemType.recipe)
        {
            UIManager.Instance.CloseUI("FCUIInventory");
            UIManager.Instance.OpenUI("Tattoo", new System.Object[] { _currentItem.ItemData });
        }
        else if (_currentItem.ItemData.type == ItemType.tribute)
        {
            UIManager.Instance.CloseUI("FCUIInventory");
            UIManager.Instance.OpenUI("FCUIOffering", _currentItem);
        }
        else if(_currentItem.ItemData.type == ItemType.tattoo)
        {
            UIManager.Instance.CloseUI("FCUIInventory");
            UIManager.Instance.OpenUI("Tattoo", new System.Object[] { _currentItem.ItemData, "equip" });
        }
    }

    void OnTattooSellButtonClick(GameObject go)
    {
        SellItem(_currentItem);
    }

    void SitchOtherLayout(bool isShow)
    {
        otherLayout.SetActive(isShow);
        commonLayout.SetActive(!isShow);
    }

    void OnClickSellItem(GameObject go)
    {
        SellItem(_currentItem);
    }

    void OnFusionButtonClick(GameObject go)
    {
        UIManager.Instance.CloseUI("FCUIInventory");
        UIManager.Instance.OpenUI("UIFusion", FCUIInventory.Instance.CurrentSelectionItem);
    }

    void OnEquipOnBody(GameObject go)
    {
        EquipOnBody(_currentItem);
    }

    #region equip item
    public static void EquipOnBody(ItemInventory item)
    {
        NetworkManager.Instance.InventoryOperation(item.Item_GUID, InventoryMenuItemOperationType.Equip, OnItemOperationHandler);
    }
    #endregion

    #region sell item
    private static Int64 _sellingItemGUID;

    public static void SellItem(ItemInventory item)
    {
        if (0 != _sellingItemGUID)
        {
            return;
        }
        _sellingItemGUID = item.Item_GUID;
        if ((item.ItemData.rareLevel != (int)ItemQuality.white &&
            item.ItemData.rareLevel != (int)ItemQuality.green) ||
            (null != item.CurrentFusionData && item.CurrentFusionData.fusionLevel > 0))
        {
            string sellAlert = String.Format(Localization.Localize("IDS_MESSAGE_INVENTORY_SELLEQUIPMENT"),
                item.ItemData.DisplayNameWithRareColor);
            UIMessageBoxManager.Instance.ShowMessageBox(sellAlert, "", MB_TYPE.MB_OKCANCEL, SellByAlertHandler);
        }
        else
        {
            SellItemById(item.Item_GUID);
        }
    }

    private static void SellByAlertHandler(ID_BUTTON buttonID)
    {
        if (buttonID == ID_BUTTON.ID_OK)
        {
            SellItemById(_sellingItemGUID);
        }
        else
        {
            _sellingItemGUID = 0;
        }
    }

    private static void SellItemById(Int64 guid)
    {
        NetworkManager.Instance.InventoryOperation(guid, InventoryMenuItemOperationType.Sell, OnItemOperationHandler);
    }

    private static void OnItemOperationHandler(NetResponse response)
    {
		if (response.Succeeded)
		{
            ItemInventory lastItem = PlayerInfo.Instance.PlayerInventory.GetItem(_sellingItemGUID);
            int lastIndex = FCUIInventory.Instance.itemsListContainer.GetItemIndex(lastItem);
			ItemOperationResponse itemResonse = (ItemOperationResponse)response;
			itemResonse.updateData.Broadcast();
            ItemInventory currentItem = FCUIInventory.Instance.itemsListContainer.GetItemAt(lastIndex);
            FCUIInventory.Instance.itemsListContainer.pageController.ForceSelectItem(currentItem);
		}
		else
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(response.errorCode), null, MB_TYPE.MB_OK, null);
		}
        _sellingItemGUID = 0;
    }
    #endregion

    
}
