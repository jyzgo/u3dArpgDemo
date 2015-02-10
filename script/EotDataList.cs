using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using InJoy.RuntimeDataProtection;


[Serializable]
public class EotData
{
    public string eotID;
    public List<Eot> eotList = new List<Eot>();
 
}


public class EotDataList : ScriptableObject
{
    public List<EotData> dataList = new List<EotData>();
}
