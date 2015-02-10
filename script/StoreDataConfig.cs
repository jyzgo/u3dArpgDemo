using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using InJoy.Utils;

[System.Serializable]
public class StoreDataForLanuage
{
	public string _language;
	public StoreDataList _storeDataList = ScriptableObject.CreateInstance<StoreDataList>();
	
	
	public void SaveTo(Hashtable saveObject)
	{
		saveObject["_language"] = _language;
	}
	
	public void LoadFrom(Hashtable saveObject)
	{
		_language = (string)saveObject["_language"];
	}
	
}

public class StoreDataConfig : ScriptableObject 
{
    public StoreDataList _globalStoreDataList = ScriptableObject.CreateInstance<StoreDataList>();
	public List<StoreDataForLanuage> _storeDataForLanuage = new List<StoreDataForLanuage>();
}
