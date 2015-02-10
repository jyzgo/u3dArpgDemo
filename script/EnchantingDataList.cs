using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class EnchantingData{
	public int _owerType;
	
	public string _typeIds;
	public int _index;
	public AIHitParams _propertyType;
	public int _costHc;
	public int _min;
	public int _max;
}


public class EnchantingDataList : ScriptableObject {
	public List<EnchantingData> _dataList = new List<EnchantingData>();	
}
