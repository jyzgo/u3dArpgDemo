using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;


public class ExcelSocket : AssetPostprocessor{

	public static string fileName = "Assets/DataTables/EquipmentUpgrade.xml";
	public static string sheetName = "socket";
	public static string outFileName = "Assets/GlobalManagers/Data/EquipmentUpgrade/SocketList.asset";
			
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
	
	
	[MenuItem("Tools/Import Data Table/Socketing data")]
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
		
		
		SocketDataList	dataList = null;
		
		UnityEngine.Object  oldFile = AssetDatabase.LoadAssetAtPath(outFileName,typeof(SocketDataList));
		if(oldFile == null)
		{
			newFile = true;	
			dataList = ScriptableObject.CreateInstance(typeof(SocketDataList)) as SocketDataList;
		}else{
			dataList = oldFile as SocketDataList;
		}
				
		dataList._dataList.Clear();
		for(int i = 1; i< sheet._rows.Count; i++)
		{
			Row data = sheet._rows[i];
			SocketData newData = new SocketData();
		
			newData._level = GetInt(data["level"]);
			newData._rareLevel = GetInt(data["rareLevel"]);
						
			for(int k = 1; k<=2; k++)
			{
				int sc = GetInt(data["socketSc"+k]);
				int hc = GetInt(data["socketHc"+k]);
				string itemId = data["socketItemId"+k];
				
				if(itemId.Length>1)
				{
					SocketNeed need = new SocketNeed();
					need._itemId = itemId;
					need._sc = sc;
					need._hc = hc;
					newData._need.Add(need);
				}
			}
			
			dataList._dataList.Add(newData);
		}
		
		if(newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}else{
			EditorUtility.SetDirty(dataList);	
		}
		
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
