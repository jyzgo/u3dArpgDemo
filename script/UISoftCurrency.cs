using System;
using System.Collections.Generic;
using UnityEngine;

public class UISoftCurrency : MonoBehaviour
{
    public UILabel scLabel;

    private int _sc;
    public int SC
    {
        set
        {
            _sc = value;
            scLabel.text = _sc.ToString();
        }
        get
        {
            return _sc;
        }
    }
}
