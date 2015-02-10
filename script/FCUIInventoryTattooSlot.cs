using System;
using System.Collections.Generic;
using UnityEngine;

public class FCUIInventoryTattooSlot : MonoBehaviour
{
    public UITexture iconTexture;

    private ItemInventory _tattooItem;
    public ItemInventory tattooItem
    {
        set 
        {
            _tattooItem = value;
            Refresh();
        }
    }

    void Start()
    { 
        
    }

    void Refresh()
    {
        if (_tattooItem != null)
        {
            iconTexture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(_tattooItem.ItemData.iconPath) as Texture;
        }
        else
        {
            iconTexture.mainTexture = null;
        }
    }
}
