using System;
using System.Collections.Generic;
using UnityEngine;

public class VIPLogo : MonoBehaviour, IRefreshable
{
    public UILabel vipNumber;

    private int _vipLevel;
    public int VipLevel
    {
        set { _vipLevel = value; Refresh(); }
        get { return _vipLevel; }
    }

    public void Refresh()
    {
        vipNumber.text = _vipLevel.ToString();
    }
}
