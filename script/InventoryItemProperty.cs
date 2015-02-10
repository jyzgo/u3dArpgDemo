using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class InventoryItemProperty : MonoBehaviour
{
    public void Refresh(ItemInventory _currentItem, int attriIndex)
    { 
        UILabel label = GetComponent<UILabel>();
        switch (attriIndex)
        { 
            case 0:
                label.text = _currentItem.GetFusionAttribute0(true);
                break;
            case 1:
                label.text = _currentItem.GetFusionAttribute1(true);
                break;
            case 2:
                label.text = _currentItem.GetFusionAttribute2(true);
                break;
        }
    }

    public void Refresh(string lb, string value)
    {
        UILabel label = GetComponent<UILabel>();
        label.text = lb + " : [BEAA82]" + value + "[-]";
    }

    public void Refresh(string text)
    {
        UILabel label = GetComponent<UILabel>();
        label.text = text;
    }

    public void Clear()
    {
        UILabel label = GetComponent<UILabel>();
        label.text = "";
    }
}
