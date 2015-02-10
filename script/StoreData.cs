using UnityEngine;
using System;
using System.Collections;
using System.IO;

[Serializable]
public class IAPInfo
{	
	public string _iapId = "";			//ID used by apple store
	public string _iapIdAndroid = "";	//ID used by androit market
	
	public int _bonusCount = 0;
	public int _hcCount = 0;
	public int _scCount = 0;
	
	public int _discountBonusCount = 0;
	public int _discountHcCount = 0;
	public int _discountScCount = 0;	
	
	public int _USD_price;
	
	public void SaveTo(Hashtable saveObject)
	{
		saveObject["_iapId"] = _iapId;
		saveObject["_iapIdAndroid"] = _iapIdAndroid;
		saveObject["_hcCount"] = _hcCount;
		saveObject["_scCount"] = _scCount;
		saveObject["_bonusCount"] = _bonusCount;
		saveObject["_discountHcCount"] = _discountHcCount;
		saveObject["_discountScCount"] = _discountScCount;
		saveObject["_discountBonusCount"] = _discountBonusCount;
	}
	
	public void LoadFrom(Hashtable saveObject)
	{		
		_iapId = (string)saveObject["_iapId"];
		_iapIdAndroid = (string)saveObject["_iapIdAndroid"];
		_hcCount = (int)(double)saveObject["_hcCount"];
		_scCount = (int)(double)saveObject["_scCount"];
		_bonusCount = (int)(double)saveObject["_bonusCount"] ;
		_discountHcCount = (int)(double)saveObject["_discountHcCount"];
		_discountScCount = (int)(double)saveObject["_discountScCount"];
		_discountBonusCount = (int)(double)saveObject["_discountBonusCount"];
	}
}

[Serializable]
public class NormalInfo
{	
	public string _itemID = "";
	public int _count = 0;
	
	public int _softCurrency = 0;
	public int _hardCurrency = 0;
	
	public int _discountSoftCurrency = 0;
	public int _dissountHardCurrency = 0;
	

	
	public void SaveTo(Hashtable saveObject)
	{
		saveObject["_hardCurrency"] = _hardCurrency;
		saveObject["_softCurrency"] = _softCurrency;
		saveObject["_dissountHardCurrency"] = _dissountHardCurrency;
		saveObject["_discountSoftCurrency"] = _discountSoftCurrency;
		saveObject["_itemID"] = _itemID;
		saveObject["_count"] = _count;
	}
	
	public void LoadFrom(Hashtable saveObject)
	{		
		_itemID = (string)saveObject["_itemID"];
		_count = (int)(double)saveObject["_count"];
		_hardCurrency = (int)(double)saveObject["_hardCurrency"] ;
		_softCurrency = (int)(double)saveObject["_softCurrency"];
		_discountSoftCurrency = (int)(double)saveObject["_discountSoftCurrency"];
		_dissountHardCurrency = (int)(double)saveObject["_dissountHardCurrency"];
	}
	
}

[Serializable]
public class Cost
{
	public int _softCurrency = 0;
	public int _hardCurrency = 0;
}


[Serializable]
public class ItemPack
{
	public string _itemID;
	public int _count;
}


public enum StoreItemType
{
	POTION,
	TOKEN,
	GRID,
	HC,
	VANITY,
	MAX,
}

[Serializable]
public class StoreData 
{
	public string		_id; //id
	public string 		_displayNameIds; //name ids
	
	public int 			_order = 0;	//we use this value sort
	public bool 		_isVisibleOnStore = true;  //if show this instore
	
	public int 			_visibleLevelMin = 0;
	public int 			_visibleLevelMax = 0;

	
	public string  		_visibleStartTime = "0";  //if the value != "0", it's a time limit item. we just show it in time limit. start time
	public string 		_visibleEndTime = "0";  //end time
	

	public bool			_isRecommend = false; // if a recommend item
	public bool 		_isNew = false;  // if a new item
	
	public float 		_discount = 0;  // if the value > 0 ,we will do a off sale.  
	public string 		_discountStartTime = "0"; //discount start time
	public string 		_discountEndTime = "0"; //discount end time
		
	public StoreItemType _storeType = StoreItemType.POTION; //store type
	public string _storeIconName;//icon name in store atlas
		
	public NormalInfo _normalInfo = new NormalInfo(); //sell other  
	public IAPInfo 		_iapInfo = new IAPInfo(); //sell IAP
	
	
	public bool IsCurrency
	{
		get{return _storeType == StoreItemType.HC ||_storeType == StoreItemType.TOKEN;}	
	}

	public bool IsIAP
	{
		get{
	 		return _storeType == StoreItemType.HC;
		}
	}
	
	public bool IsMonthCard
	{
		get{
			return _iapInfo._iapId == StoreDataManager.Instance._monthCardIAP.iap_id;	
		}
	}
	
	public void GetIAPSellInfo(out int hcCount, out int scCount, out int bonusCount)
	{
		if(IsOnSale())
		{
			hcCount = _iapInfo._discountHcCount;
			scCount = _iapInfo._discountScCount;
			bonusCount = _iapInfo._discountBonusCount;
		}else{
			hcCount = _iapInfo._hcCount;
			scCount = _iapInfo._scCount;
			bonusCount = _iapInfo._bonusCount;
		}
	}
	

	public bool IsOnSale()
	{
		DateTime dtNow = TimeUtils.GetPSTDateTime();
		if(_discount > 0.001f)
		{
			if(TimeUtils.IsBetweenTime(dtNow,_discountStartTime,_discountEndTime))
			{
				return true;
			}
		}
		return false;
	}
}