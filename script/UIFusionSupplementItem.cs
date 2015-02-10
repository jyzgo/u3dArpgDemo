using System;
using System.Collections.Generic;
using UnityEngine;

public class UIFusionSupplementItem : MonoBehaviour
{
    public UITexture texture;

    public UILabel countLabel;

    public UISprite qualitySlot;

    public UISprite hcIcon1;

    public UILabel hcLabel1;

    public UISprite strikeout;

    public UISprite hcIcon2;

    public UILabel hcLabel2;

    private ItemData _itemData;

    private InventoryHCWorth _inventoryHCWorth;

    void Awake()
    { 
        
    }

    public InventoryHCWorth InventoryHCWorth
    {
        set 
        {
            _inventoryHCWorth = value;
            Refresh();
        }
        get
        {
            return _inventoryHCWorth;
        }
    }

    public void Refresh()
    {
        _itemData = DataManager.Instance.ItemDataManager.GetItemData(_inventoryHCWorth.ItemId);
        texture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(_itemData.iconPath) as Texture;
        hcLabel1.text = (_inventoryHCWorth.OnePrice * _inventoryHCWorth.Count).ToString();
        countLabel.text = _inventoryHCWorth.Count.ToString();
        qualitySlot.spriteName = UIGlobalSettings.QualityNamesMap[(ItemQuality)_itemData.rareLevel];
        if (_inventoryHCWorth.Discount < 100)//item with discount
        {
            hcLabel2.text = (_inventoryHCWorth.DiscountPrice * _inventoryHCWorth.Count).ToString();
            hcLabel2.gameObject.SetActive(true);
            hcIcon2.gameObject.SetActive(true);
            strikeout.gameObject.SetActive(true);
        }
        else
        {
            hcLabel2.gameObject.SetActive(false);
            hcIcon2.gameObject.SetActive(false);
            strikeout.gameObject.SetActive(false);
        }
    }

    void OnPress(bool press)
    {
        if (press)
        {
            UIMessageBoxManager.Instance.ShowMessageBox(null, null, MB_TYPE.MB_FLOATING, null, _itemData);
        }
        else
        {
            UIMessageBoxManager.Instance.HideMessageBox(MB_TYPE.MB_FLOATING);
        }
    }
}
