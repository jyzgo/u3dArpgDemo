using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentFSData
{
    public AIHitParams prop;
    public string ids;
    public int fs;
    public int sc;
}

public class EquipmentFSDataList : ScriptableObject
{
    public List<EquipmentFSData> dataList = new List<EquipmentFSData>();

    public EquipmentFSData GetFSDataByAttribute(AIHitParams attribute)
    {
        EquipmentFSData fsData = dataList.Find(delegate(EquipmentFSData testData)
        {
            return testData.prop == attribute;
        });
        return fsData;
    }
}