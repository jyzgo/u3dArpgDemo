using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FCUIInventorySlot : MonoBehaviour
{
    private static Dictionary<string, Texture> _iconTextureCache = new Dictionary<string, Texture>();
    public static Texture GetIconTexture(string texturePath)
    {
        Texture texture = null;
        if (!_iconTextureCache.ContainsKey(texturePath))
        {
            texture = InJoy.AssetBundles.AssetBundles.Load(texturePath) as Texture;
            _iconTextureCache.Add(texturePath, texture);
        }
        else
        {
            texture = _iconTextureCache[texturePath];
        }
        return texture;
    }

    public static bool FindIconTexture(string texturePath)
    {
        return _iconTextureCache.ContainsKey(texturePath);
    }

    private bool _isShowIconRightNow = true;
    public bool IsShowIconRightNow 
    {
        set 
        { 
            _isShowIconRightNow = value;
            if (!_isShowIconRightNow)
            {
                texture.enabled = false;
            }
        }
        get { return _isShowIconRightNow; }
    }

    private BoxCollider _boxCollider;
    public BoxCollider Collider
    {
        get 
        {
            if (null == _boxCollider)
                _boxCollider = GetComponent<BoxCollider>();
            return _boxCollider;
        }
    }

    public UISprite newIcon;
    public UISprite colorfulQuality;//can change color
    public UILabel stackLabel;
    public UILabel fsLabel;
    public UISprite fsLabelSpan;
    public GameObject selectionOuter;
    public UITexture texture;
    private string _texturePath;

    private ItemInventory _item;
    public ItemInventory Item {
        set 
        {
            _item = value;
            Refresh();
        }
        get{ return _item; } 
    }

    private bool _refreshComplete;
    public bool RefreshComplete
    {
        get 
        {
            return _refreshComplete;
        }
    }

    private ItemQuality _quality;
    public ItemQuality Quality
    {
        set
        { 
            _quality = value;
            if (null != colorfulQuality)
            {
                colorfulQuality.spriteName = UIGlobalSettings.QualityNamesMap[_quality];
            }
        }
        get { return _quality; }
    }

    private int _stack;
    public int Stack
    {
        set 
        {
            _stack = value;
            if (null != stackLabel)
            {
                stackLabel.gameObject.SetActive(null != Item && Item.ItemData.stack > 1);
                stackLabel.text = "x " + _stack.ToString();
            }
        }
        get { return _stack; }
    }

    private int _fs;
    public int Fs
    {
        set
        {
            _fs = value;
            if (null == Item || !Item.IsEquipment() || Item.IsEquiped())
            {
                if (null != fsLabelSpan)
                {
                    fsLabelSpan.gameObject.SetActive(false);
                }
                fsLabel.gameObject.SetActive(false);
            }
            else
            {
                if (null != fsLabelSpan)
                {
                    fsLabelSpan.gameObject.SetActive(true);
                    fsLabel.gameObject.SetActive(true);
                    int deltaFS = _fs;
                    ItemInventory equiptedItem = PlayerInfo.Instance.EquippedInventory.GetItem(Item.ItemData.type, Item.ItemData.subType);
                    if (null != equiptedItem)
                    {
                        deltaFS = _fs - equiptedItem.FS;
                    }
                    fsLabel.text = Mathf.Abs(deltaFS).ToString();
                    if (deltaFS > 0)
                    {
                        fsLabelSpan.spriteName = "261";
                        fsLabel.color = Color.green;
                    }
                    else if (deltaFS < 0)
                    {
                        fsLabelSpan.spriteName = "262";
                        fsLabel.color = Color.red;
                    }
                    else
                    {
                        fsLabelSpan.gameObject.SetActive(false);
                        fsLabel.gameObject.SetActive(false);
                    }
                }
            }

        }
        get { return _fs; }
    }

    void Awake()
    {

    }

    void Start()
    {
        Refresh();
        GodIgnoreMe();
    }

    void Update()
    {
    }

    void Refresh()
    {
        _refreshComplete = false;
        if (null != Item)
        {
            Stack = Item.Count;
            if(_texturePath != Item.ItemData.iconPath)
            {
                if (_isShowIconRightNow || FindIconTexture(Item.ItemData.iconPath))
                {
                    if (!texture.enabled)
                    {
                        texture.enabled = true;
                    }
                    texture.mainTexture = GetIconTexture(Item.ItemData.iconPath);
                    _refreshComplete = true;
                }
                _texturePath = Item.ItemData.iconPath;
            }
            Quality = (ItemQuality)Item.ItemData.rareLevel;
            texture.gameObject.SetActive(true);
            Fs = Item.FS;
            if (newIcon)
            {
                newIcon.gameObject.SetActive(Item.IsNew);
            }
        }
        else
        {
            Stack = 0;
            texture.mainTexture = null;
            _texturePath = null;
            Quality = ItemQuality.white;
            Fs = 0;
            texture.gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
    }

    public void RefreshIcon()
    {
        if (!texture.enabled)
        {
            texture.enabled = true;
        }
        texture.mainTexture = GetIconTexture(Item.ItemData.iconPath);
    }

    public bool TestTouch()
    {
        Vector3 touchPoint = UICamera.lastTouchPosition;
        Vector3 locationCenter = UICamera.currentCamera.WorldToScreenPoint(transform.position);
        if (Mathf.Abs(locationCenter.x - touchPoint.x) < Collider.size.x / 2
            &&
            Mathf.Abs(locationCenter.y - touchPoint.y) < Collider.size.y / 2
            )
        {
            if (null != Item && Item.IsNew)
            {
                Item.IsNew = false;
                Refresh();
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GodSelectedMe()
    {
        selectionOuter.SetActive(true);
    }

    public void GodIgnoreMe()
    {
        selectionOuter.SetActive(false);
    }
}