using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OfferingItemSlot : MonoBehaviour
{
    public UITexture icon;
    public UILabel count;
    public string itemId;
    public int itemCount;
    public string itemIcon;
    public OfferingLevel level;
    public bool isCanUse;

    void OnPress(bool pressed)
    {
        if (pressed)
        {
            ItemData itemData = DataManager.Instance.GetItemData(itemId);
            UIMessageBoxManager.Instance.ShowMessageBox(null, null, MB_TYPE.MB_FLOATING, null, itemData);
        }
        else
        {
            UIMessageBoxManager.Instance.HideMessageBox(MB_TYPE.MB_FLOATING);
        }
    }
}
