using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class FusionData
{
    public string id;
    public int itemLevel;
    public ItemType itemType;
    public int fusionLevel;
    public float increaseData;
    public int cost;
    public string material1;
    public int materialCount1;
    public string material2;
    public int materialCount2;
    public string material3;
    public int materialCount3;

    override public string ToString()
    {
        return material1 + "/" + material2 + "/" + material3;
    }
}


public class FusionDataList : ScriptableObject {
    public List<FusionData> FusionList = new List<FusionData>();

    private Dictionary<string, FusionData> _fusionDataMapping;
    public Dictionary<string, FusionData> FusionDataMapping 
    { 
        get
        {
            if (null == _fusionDataMapping)
            {
                _fusionDataMapping = new Dictionary<string, FusionData>();
                foreach (FusionData fusion in FusionList)
                {
                    _fusionDataMapping.Add(fusion.id, fusion);
                }
            }
            return _fusionDataMapping;
        }
    }
}
