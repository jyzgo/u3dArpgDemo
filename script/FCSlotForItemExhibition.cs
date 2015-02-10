using System;
using System.Collections.Generic;
using UnityEngine;

public class FCSlotForItemExhibition : MonoBehaviour
{
    public UISprite colorfulQuality;//can change color
    public UILabel stackLabel;
    public UILabel fsLabel;
    public UISprite fsLabelSpan;//can resizeble
    public UITexture texture;
    public UILabel nameLabel;
    public UILabel partLabel;
    private string _texturePath;

    private int _stack;
    public int Stack
    {
        set
        {
            _stack = value;
            if (null != stackLabel)
            {
                stackLabel.text = _stack.ToString();
            }
            
        }
        get { return _stack; }
    }

    private ItemQuality _quality;
    public ItemQuality Quality
    {
        set
        {
            _quality = value;
            colorfulQuality.spriteName = UIGlobalSettings.QualityNamesMap[_quality];
        }
        get { return _quality; }
    }

    private int _fs;
    public int Fs
    {
        set
        {
            _fs = value;
            if (null != fsLabel || null != fsLabelSpan)
            {
                fsLabel.gameObject.SetActive(_fs != 0);
                fsLabel.text = _fs.ToString();
                float rectWidth = fsLabel.text.Length > 1 ?
                    10 * fsLabel.text.Length * 0.9f :
                    10 * fsLabel.text.Length * 1.5f;
                fsLabel.lineWidth = (int)rectWidth;
                fsLabel.lineHeight = 12;
                fsLabelSpan.gameObject.SetActive(_fs != 0);
                fsLabelSpan.transform.localScale = new Vector3(rectWidth, fsLabelSpan.transform.localScale.y, 1);
            }
        }
        get { return _fs; }
    }

    public void Refresh(string itemId, int count)
    {
        ItemData = DataManager.Instance.ItemDataManager.GetItemData(itemId);
        Quality = (ItemQuality)ItemData.rareLevel;
        texture.mainTexture = InJoy.AssetBundles.AssetBundles.Load(ItemData.iconPath) as Texture;
        Stack = count;
        Fs = ItemData.FS;

        if (null != nameLabel)
        {
            nameLabel.text = ItemData.DisplayNameWithRareColor;
        }
        if (null != partLabel)
        {
            partLabel.text = Localization.Localize(
                Utils.GetItemPartNamesIDS(ItemData.type,
                ItemData.subType));
            if (ItemData.type == ItemType.sc)
            {
                partLabel.text = "";
            }
        }
    }

    private ItemData _itemData;
    public ItemData ItemData
    {
        get
        {
            return _itemData;
        }
        set 
        {
            _itemData = value;
        }
    }

    void OnPress(bool press)
    {
        if(ItemData.type == ItemType.sc)
            return;
        if (press)
        {
            UIMessageBoxManager.Instance.ShowMessageBox(null, null, MB_TYPE.MB_FLOATING, null, ItemData);
        }
        else
        {
            UIMessageBoxManager.Instance.HideMessageBox(MB_TYPE.MB_FLOATING);
        }
    }
}
