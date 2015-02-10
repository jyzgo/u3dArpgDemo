using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerPropData
{
    public PlayerPropKey propKey;
    public string ids;
    public int fs;
}

[System.Serializable]
public class PlayerPropDataList :ScriptableObject
{
    public List<PlayerPropData> dataList = new List<PlayerPropData>();

    public PlayerPropData GetPlayerPropDataByProp(PlayerPropKey key)
    {
        PlayerPropData playerPropData = dataList.Find(delegate(PlayerPropData testData) 
        {
            return testData.propKey == key;
        });
        return playerPropData;
    }
}