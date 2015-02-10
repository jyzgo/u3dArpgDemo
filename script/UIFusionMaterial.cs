using System;
using System.Collections.Generic;
using UnityEngine;

public class UIFusionMaterial : MonoBehaviour
{
    public UILabel materialCountLabel;

    public FCSlotForItemExhibition exhibitionItem;

    private ItemData _itemData;

    private string _itemId;
    public string ItemId 
    { 
        set
        {
            _itemId = value;
            _itemData = DataManager.Instance.ItemDataManager.GetItemData(_itemId);
            RefreshTexture();
        }
        get{return _itemId;}
    }

    private int _needCount;
    public int NeedCount
    {
        set
        {
            _needCount = value;
        }
    }

    private int _currentCount;
    public int CurrentCount
    {
        set
        { 
            _currentCount = value;
            RefreshCount();
        }
    }

    void RefreshTexture()
    {
        exhibitionItem.Refresh(_itemData.id, _currentCount);
    }

    void RefreshCount()
    {
        if(_currentCount < _needCount)
            materialCountLabel.text = "[ff0000]" + _currentCount + "/" + _needCount + "[-]";
        else
            materialCountLabel.text = "[00ff00]" + _currentCount + "/" + _needCount + "[-]";
    }
}
