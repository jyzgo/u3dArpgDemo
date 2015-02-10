using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


public class CreateStoreFile : AssetPostprocessor{

	public static string fileName = "Assets/GlobalManagers/Data/StoreData/StoreDataConfig.asset";
	public static string sheetName = "store";
	public static string outFileName = "/GlobalManagers/Data/StoreData/StoreData.txt";
	public static string outFileName1 = "/GlobalManagers/Data/StoreData/StoreData_encrypt.txt";
			
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
	                                           string[] movedAssets, string[] movedFromPath)
	{
		if( CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets) )
		{
			Read();
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
		/*
		StoreDataConfig storeDataConfig = AssetDatabase.LoadAssetAtPath(fileName,typeof(StoreDataConfig)) as StoreDataConfig;
		string path = Application.dataPath+ outFileName;
		StoreDataManager.SaveToFile(storeDataConfig, path, false);
		
		string path1 = Application.dataPath+ outFileName1;
		StoreDataManager.SaveToFile(storeDataConfig, path1, true);
		*/
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
