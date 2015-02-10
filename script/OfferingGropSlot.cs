using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OfferingGropSlot:MonoBehaviour
{
    public FCSlotForItemExhibition exhibitionItem;

    public UITexture icon;

    private ItemData _itemData;
    public ItemData ItemData
    {
        set 
        {
            _itemData = value;
            Refresh();
        }
        get 
        {
            return _itemData;
        }
    }

    void Refresh()
    {
        exhibitionItem.Refresh(_itemData.id, 0);
    }
}
