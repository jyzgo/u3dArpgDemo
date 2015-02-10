using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using InJoy.Utils;
using System.IO;


public class ExcelSkillFusion : AssetPostprocessor{

	public static string fileName = "Assets/DataTables/fusion.json";
	public static string sheetName = "#fusion";
	public static string outFileName = "Assets/GlobalManagers/Data/EquipmentUpgrade/FusionList.asset";
			
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
	
	
	[MenuItem("Tools/Import Data Table/Fusion Data")]
	public static void Read()
	{
        bool isNew = false;
        object old = AssetDatabase.LoadAssetAtPath(outFileName, typeof(FusionDataList));
        FusionDataList fusionList;
        if (null == old)
        {
            isNew = true;
            fusionList = ScriptableObject.CreateInstance(typeof(FusionDataList)) as FusionDataList;
        }
        else
        {
            fusionList = old as FusionDataList;
        }

        fusionList.FusionList.Clear();
        string jsonStr = File.ReadAllText(fileName);
        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;
        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable row = obj as Hashtable;
            FusionData fusion = new FusionData();
            fusion.id           = (string)row["id"];
            fusion.itemLevel    = (int)row["itemLevel"];
            fusion.itemType     = (ItemType)(int)row["itemType"];
            fusion.fusionLevel  = (int)row["fusionLevel"];
            fusion.increaseData = (float)row["increaseData"];
            fusion.cost         = (int)row["cost"];
            fusion.material1    = (string)row["material1"];
            fusion.material2    = (string)row["material2"];
            fusion.material3    = (string)row["material3"];
            fusion.materialCount1 = (int)row["materialcount1"];
            fusion.materialCount2 = (int)row["materialcount2"];
            fusion.materialCount3 = (int)row["materialcount3"];
            fusionList.FusionList.Add(fusion);
        }
        if (isNew)
        {
            AssetDatabase.CreateAsset(fusionList, outFileName);
        }
        else
        {
            EditorUtility.SetDirty(fusionList);
        }
        Debug.Log("fusion Data List import complete!(" + outFileName + ")");
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
