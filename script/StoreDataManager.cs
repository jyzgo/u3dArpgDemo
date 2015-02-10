using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class StoreDataManager : MonoBehaviour {
	
	public StoreDataConfig _storeDataConfig;
	public MonthCardIAP _monthCardIAP;

	private static StoreDataManager _instance = null;
	public static StoreDataManager Instance
	{
		get
		{
			return _instance;
		}
	}
	
	private bool _isReady = true;

	
	private StoreDataList _curStoreDataList = null;
	
	void OnDestroy()
	{
		_instance = null;
	}
	
	void Awake()
	{
		_instance = this;
		_isReady = true;
    }
	
    public bool IsReady { get { return _isReady; } }
	

    public StoreDataList GetStoreDataListByCurrentLanguage()
    {
		if(_curStoreDataList == null)
		{
			UseOriginalDataList();
		}
		return _curStoreDataList;
    }
	
	public StoreData GetStoreData(string Id)
	{		
		StoreDataList storeDataList = GetStoreDataListByCurrentLanguage();
       
		foreach (StoreData sd in storeDataList._dataList)
		{
			if (sd._id == Id)
			{
				return sd;
			}
		}
		
		return null;
	}
	

	public void UseOriginalDataList()
	{
		_curStoreDataList = _storeDataConfig._globalStoreDataList;
	}
}
