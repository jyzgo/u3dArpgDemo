using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class PropertyWeightData
{
	public AIHitParams _type;
	public int _weight;
}


public class PropertyWeightDataList : ScriptableObject {
	public List<PropertyWeightData> _dataList = new List<PropertyWeightData>();	
}
