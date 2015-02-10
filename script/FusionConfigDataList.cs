using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class FusionConfigData
{
	public ItemSubType _part;
	public float _factor;
}


public class FusionConfigDataList : ScriptableObject {
	public List<FusionConfigData> _dataList = new List<FusionConfigData>();	
}
