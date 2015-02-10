using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public enum OfferingLevel
{
    Junior,
    Middle,
    Senior
}

[Serializable]
public class OfferingData
{
    #region from excel
    public string id;
    public OfferingLevel level;
    public string nameIds;
    public string desIds;
    public int isVisible;
    public int levelMin;
    public int levelMax;
    public int costHC;
    public List<string> costItemList = new List<string>();
    public string displayGroup;
    public int hitMoney;
    #endregion
}