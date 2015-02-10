using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InJoy.Utils;

public class ExcelGlobalData : AssetPostprocessor {

public static string fileName = "Assets/DataTables/global_config.json";
	public static string sheetName = "#gameconst";
	public static string outFileName = "Assets/GlobalManagers/Data/GlobalDatas.asset";
	
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
	                                           string[] movedAssets, string[] movedFromPath)
	{
		if( CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets) )
		{
			ReadGlobalDatas(sheetName, outFileName);
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
	
	
	
	public static void ReadGlobalDatas(string sheetName, string outFileName)
	{
		bool newFile = false;
		
		GlobalConfig dataList = null;
		
		UnityEngine.Object  oldFile = AssetDatabase.LoadAssetAtPath(outFileName,typeof(GlobalConfig));
		if(oldFile == null)
		{
			newFile = true;	
			dataList = ScriptableObject.CreateInstance(typeof(GlobalConfig)) as GlobalConfig;
		}else{
			dataList = oldFile as GlobalConfig;
		}
		
		dataList._intDatas.Clear();
		dataList._boolDatas.Clear();
		dataList._floatDatas.Clear();
		dataList._stringDatas.Clear();
        
        string jsonStr = File.ReadAllText(fileName);
        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable data = obj as Hashtable;
			string type = data["type"].ToString();
			if(type == "int")
			{
				IntConfigData newData = new IntConfigData();
				newData._id = data["name"].ToString();
				newData._type = data["type"].ToString();
				newData._value = System.Convert.ToInt32(data["intValue"].ToString());
				dataList._intDatas.Add(newData);
			}
			else if(type == "float")
			{
				FloatConfigData newData = new FloatConfigData();
				newData._id = data["name"].ToString();
				newData._type = data["type"].ToString();
				dataList._floatDatas.Add(newData);
				newData._value = (float)System.Convert.ToDouble(data["floatValue"].ToString());
			}
			else if(type == "bool")
			{
				BoolConfigData newData = new BoolConfigData();
				newData._id = data["name"].ToString();
				newData._type = data["type"].ToString();
				dataList._boolDatas.Add(newData);
				newData._value = System.Convert.ToInt32(data["boolValue"].ToString()) == 1;
			}
			else
			{
				StringConfigData newData = new StringConfigData();
				newData._id = data["name"].ToString();
				newData._type = data["type"].ToString();
				dataList._stringDatas.Add(newData);
				newData._value = data["stringValue"].ToString();
			}
		}
		
		if(newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}else{
			EditorUtility.SetDirty(dataList);	
		}

        Debug.Log("complete!");
	}
	
}
