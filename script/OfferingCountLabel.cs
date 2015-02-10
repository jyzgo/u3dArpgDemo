using System;
using System.Collections.Generic;
using UnityEngine;

public class OfferingCountLabel : MonoBehaviour
{
    public UILabel countLabel;

    private int _count;
    public int Count
    {
        set
        {
            _count = value;
            Refresh();
        }
        get
        {
            return _count;
        }
    }

    void Refresh()
    {
        countLabel.text = _count.ToString();
        gameObject.SetActive(_count > 0);
    }
}
