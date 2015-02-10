using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


public class ExcelPropertyWeight : AssetPostprocessor{

	public static string fileName = "Assets/DataTables/EquipmentUpgrade.xml";
	public static string sheetName = "mage_Weight";
	public static string outFileFolder = "Assets/GlobalManagers/Data/Weight/Mage/";
	public static string outFileName = "Assets/GlobalManagers/Data/Weight/MageWeightList.asset";
	
	public static string sheetName1 = "warrior_Weight";
	public static string outFileFolder1 = "Assets/GlobalManagers/Data/Weight/Warrior/";
	public static string outFileName1 = "Assets/GlobalManagers/Data/Weight/WarriorWeightList.asset";
	
	public static string sheetName2 = "monk_Weight";
	public static string outFileFolder2 = "Assets/GlobalManagers/Data/Weight/Monk/";
	public static string outFileName2 = "Assets/GlobalManagers/Data/Weight/MonkWeightList.asset";
			
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
	                                           string[] movedAssets, string[] movedFromPath)
	{
		if( CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets) )
		{
			ReadWeight(sheetName, outFileFolder, outFileName);
			ReadWeight(sheetName1, outFileFolder1, outFileName1);
			ReadWeight(sheetName2, outFileFolder2, outFileName2);
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
	
	public static void ReadWeight(string sheetName, string outFileFolder, string outFileName)
	{
		Workbook workbook = Workbook.CreatWorkbook(fileName,sheetName);
		Sheet sheet = workbook._sheet;
		if(sheet == null)
		{
			Debug.LogError("Can't find " + sheetName + " in " + fileName);
			return;
		}
		
		bool newFile = false;
		
		
		HeroEquipmentWeight	dataList = null;
		
		UnityEngine.Object  oldFile = AssetDatabase.LoadAssetAtPath(outFileName,typeof(HeroEquipmentWeight));
		if(oldFile == null)
		{
			newFile = true;	
			dataList = ScriptableObject.CreateInstance(typeof(HeroEquipmentWeight)) as HeroEquipmentWeight;
		}else{
			dataList = oldFile as HeroEquipmentWeight;
		}
				
		
		dataList._weaponWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._helmWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._shoulderWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._beltWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._armpieceWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._leggingsWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._necklaceWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		dataList._ringWeight = ScriptableObject.CreateInstance<PropertyWeightDataList>(); 
		
		for(int i = 1; i< sheet._rows.Count; i++)
		{
			Row data = sheet._rows[i];
			
			PropertyWeightData newData = new PropertyWeightData();
			newData._type = GetHitParam(data["WeaponType"]);
			newData._weight = GetInt(data["WeaponWeight"]);
			if(newData._type != AIHitParams.None && newData._weight>0)
			dataList._weaponWeight._dataList.Add(newData);
			
			PropertyWeightData newData1 = new PropertyWeightData();
			newData1._type = GetHitParam(data["HelmType"]);
			newData1._weight = GetInt(data["HelmWeight"]);
			if(newData1._type != AIHitParams.None && newData1._weight>0)
			dataList._helmWeight._dataList.Add(newData1);
			
			PropertyWeightData newData2 = new PropertyWeightData();
			newData2._type = GetHitParam(data["ShoulderType"]);
			newData2._weight = GetInt(data["ShoulderWeight"]);
			if(newData2._type != AIHitParams.None && newData2._weight>0)
			dataList._shoulderWeight._dataList.Add(newData2);
			
			PropertyWeightData newData3 = new PropertyWeightData();
			newData3._type = GetHitParam(data["BeltType"]);
			newData3._weight = GetInt(data["BeltWeight"]);
			if(newData3._type != AIHitParams.None && newData3._weight>0)
			dataList._beltWeight._dataList.Add(newData3);
			
			PropertyWeightData newData4 = new PropertyWeightData();
			newData4._type = GetHitParam(data["ArmpieceType"]);
			newData4._weight = GetInt(data["ArmpieceWeight"]);
			if(newData4._type != AIHitParams.None && newData4._weight>0)
			dataList._armpieceWeight._dataList.Add(newData4);
			
			PropertyWeightData newData5 = new PropertyWeightData();
			newData5._type = GetHitParam(data["LeggingsType"]);
			newData5._weight = GetInt(data["LeggingsWeight"]);
			if(newData5._type != AIHitParams.None && newData5._weight>0)
			dataList._leggingsWeight._dataList.Add(newData5);
			
			PropertyWeightData newData6 = new PropertyWeightData();
			newData6._type = GetHitParam(data["NecklaceType"]);
			newData6._weight = GetInt(data["NecklaceWeight"]);
			if(newData6._type != AIHitParams.None && newData6._weight>0)
			dataList._necklaceWeight._dataList.Add(newData6);
			
			PropertyWeightData newData7 = new PropertyWeightData();
			newData7._type = GetHitParam(data["RingType"]);
			newData7._weight = GetInt(data["RingWeight"]);
			if(newData7._type != AIHitParams.None && newData7._weight>0)
			dataList._ringWeight._dataList.Add(newData7);
		}
		
		CreateWeightScriptObject(dataList._weaponWeight, outFileFolder, "weaponWeight");
		CreateWeightScriptObject(dataList._helmWeight, outFileFolder, "helmWeight");
		CreateWeightScriptObject(dataList._shoulderWeight, outFileFolder, "shoulderWeight");
		CreateWeightScriptObject(dataList._armpieceWeight, outFileFolder, "armpieceWeight");
		CreateWeightScriptObject(dataList._beltWeight, outFileFolder, "beltWeight");
		CreateWeightScriptObject(dataList._leggingsWeight, outFileFolder, "leggingsWeight");
		CreateWeightScriptObject(dataList._necklaceWeight, outFileFolder, "necklaceWeight");
		CreateWeightScriptObject(dataList._ringWeight, outFileFolder, "ringWeight");
		
		
		
		if(newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}else{
			EditorUtility.SetDirty(dataList);	
		}
	}
	
	
	
	public static void CreateWeightScriptObject(PropertyWeightDataList obj, string outFileFolder, string name)
	{
		string path	 = outFileFolder + name + ".asset";
		AssetDatabase.DeleteAsset(path);
		AssetDatabase.CreateAsset(obj,path);
	}

	
	
	public static void AddEquip(List<String> list, string equipId)
	{
		if(equipId == null)
		{
			return;
		}
		
		if(equipId.Length < 2)
		{
			return;
		}
		
		list.Add(equipId);
	}
	
	public static AIHitParams GetHitParam(string data)
	{
		AIHitParams val = (AIHitParams) (System.Enum.Parse(typeof(AIHitParams),data));
		return val;
	}
	
	
	public static FC_AC_FACTIOH_TYPE GetFaction(string data)
	{
		FC_AC_FACTIOH_TYPE val = (FC_AC_FACTIOH_TYPE) (System.Enum.Parse(typeof(FC_AC_FACTIOH_TYPE),data));
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
