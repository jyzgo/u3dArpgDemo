using System;
using System.Collections.Generic;
using UnityEngine;

public class UIFusionEquipment : MonoBehaviour
{
    public UITexture texture;
    public UISprite colorfulBoard;

    private ItemInventory _item;
    public ItemInventory Item
    {
        set { _item = value; RefreshTexture(); }
        get { return _item; }
    }

    void RefreshTexture()
    {
        texture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(_item.ItemData.iconPath) as Texture2D;
        colorfulBoard.spriteName = UIGlobalSettings.QualityNamesMap[(ItemQuality)_item.ItemData.rareLevel];
    }
}
