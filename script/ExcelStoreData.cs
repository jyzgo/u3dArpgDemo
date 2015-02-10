using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


public class ExcelStoreData : AssetPostprocessor{

	public static string fileName = "Assets/DataTables/StoreData.xml";
	public static string sheetName = "store";
	public static string outFileName = "Assets/GlobalManagers/Data/StoreData/StoreDataList.asset";
	
	public static string sheetName1 = "monthCard";
	public static string outFileName1 = "Assets/GlobalManagers/Data/StoreData/MonthCard.asset";
			
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
	                                           string[] movedAssets, string[] movedFromPath)
	{
		if( CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets) )
		{
			Read();
			ReadMonthCard();
		}
	}
	
	private static bool CheckResModified(string[] files)
	{
		bool fileModified = false;
		foreach(string file in files)
		{
			if(file.Contains(fileName))
			{
				fileModified = true;	
				break;
			}
		}
		return fileModified;
	}
	
	
	[MenuItem("Tools/Import Data Table/Store Data")]
	public static void Read()
	{
		Workbook workbook = Workbook.CreatWorkbook(fileName,sheetName);
		Sheet sheet = workbook._sheet;
		if(sheet == null)
		{
			Debug.LogError("Can't find " + sheetName + " in " + fileName);
			return;
		}
		
		bool newFile = false;
		
		
		StoreDataList	dataList = null;
		
		UnityEngine.Object  oldFile = AssetDatabase.LoadAssetAtPath(outFileName,typeof(StoreDataList));
		if(oldFile == null)
		{
			newFile = true;	
			dataList = ScriptableObject.CreateInstance(typeof(StoreDataList)) as StoreDataList;
		}else{
			dataList = oldFile as StoreDataList;
		}
				
		dataList._dataList.Clear();
		for(int i = 1; i< sheet._rows.Count; i++)
		{
			Row data = sheet._rows[i];
			StoreData newData = new StoreData();
		
			newData._id =  data["id"]; 
			newData._displayNameIds =  data["displayNameIds"]; 
			newData._order =  GetInt(data["order"]); 
			newData._isVisibleOnStore =  GetBool(data["isVisibleOnStore"]); 
			newData._visibleLevelMin =  GetInt(data["visibleLevelMin"]);
			newData._visibleLevelMax =  GetInt(data["visibleLevelMax"]);
			newData._order =  GetInt(data["order"]); 
			newData._visibleStartTime =  data["visibleStartTime"]; 
			newData._visibleEndTime =  data["visibleEndTime"];
			newData._isRecommend =  GetBool(data["isRecommend"]); 
			newData._isNew =  GetBool(data["isNew"]); 
			newData._discount =  GetFloat(data["discount"]); 
			newData._discountStartTime =  data["discountStartTime"]; 
			newData._discountEndTime =  data["discountEndTime"]; 
			newData._storeType =  GetStoreItemType(data["storeType"]); 
			newData._storeIconName =  data["storeIconName"];
			
			newData._normalInfo._itemID =  data["itemID"];
			newData._normalInfo._count =  GetInt(data["count"]);
			newData._normalInfo._hardCurrency =  GetInt(data["hardCurrency"]); 
			newData._normalInfo._softCurrency =  GetInt(data["softCurrency"]); 
			newData._normalInfo._dissountHardCurrency =  GetInt(data["dissountHardCurrency"]); 
			newData._normalInfo._discountSoftCurrency =  GetInt(data["discountSoftCurrency"]); 
			
			newData._iapInfo._iapId =  data["iapId"];
			newData._iapInfo._iapIdAndroid =  data["iapIdAndroid"];
			newData._iapInfo._hcCount = GetInt(data["hcCount"]);
			newData._iapInfo._scCount = GetInt(data["scCount"]);
			newData._iapInfo._bonusCount = GetInt(data["bonusCount"]);
			newData._iapInfo._discountHcCount = GetInt(data["discountHcCount"]);
			newData._iapInfo._discountScCount = GetInt(data["discountScCount"]);
			newData._iapInfo._discountBonusCount = GetInt(data["discountBonusCount"]);
			newData._iapInfo._USD_price = GetInt(data["USD_price"]);
			
			dataList._dataList.Add(newData);
		}
		
		if(newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}else{
			EditorUtility.SetDirty(dataList);	
		}
		
	}
	
	
	public static void ReadMonthCard()
	{
		Workbook workbook = Workbook.CreatWorkbook(fileName,sheetName1);
		Sheet sheet = workbook._sheet;
		if(sheet == null)
		{
			Debug.LogError("Can't find " + sheetName1 + " in " + fileName);
			return;
		}
		
		bool newFile = false;
		
		
		MonthCardIAP dataList = null;

		UnityEngine.Object  oldFile = AssetDatabase.LoadAssetAtPath(outFileName1,typeof(MonthCardIAP));
		if(oldFile == null)
		{
			newFile = true;	
			dataList = ScriptableObject.CreateInstance(typeof(MonthCardIAP)) as MonthCardIAP;
		}else{
			dataList = oldFile as MonthCardIAP;
		}
				
	
		Row data = sheet._rows[1];
		dataList.iap_id = data["iap_id"]; 
		dataList.count = GetInt(data["count"]);
		
		if(newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName1);
		}else{
			EditorUtility.SetDirty(dataList);	
		}
		
	}
	
	

	
	public static StoreItemType GetStoreItemType(string data)
	{
		StoreItemType val = (StoreItemType) (System.Enum.Parse(typeof(StoreItemType),data));
		return val;
	}
	
	public static int GetInt(string data)
	{
		float val = 0;
		try{
			val = float.Parse(data);
		}catch(Exception e)
		{
			Debug.LogError(e.Message + "  data["+data+"]");
		}
		return Mathf.RoundToInt(val);	
	}
	
	public static float GetFloat(string data)
	{
		float val = 0;
		try{
			val = float.Parse(data);
		}catch(Exception e)
		{
			Debug.LogError(e.Message + "  data["+data+"]");
		}
		return val;	
	}
	
	public static bool GetBool(string data)
	{
		int val = 0;
		try{
			val = int.Parse(data);
		}catch(Exception e)
		{
			Debug.LogError(e.Message + "  data["+data+"]");
		}
		return val == 0 ? false : true;
	}
	
}
