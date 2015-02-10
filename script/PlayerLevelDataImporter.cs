using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;


public class PlayerLevelDataImporter : AssetPostprocessor
{
	private const string fileName = "Assets/DataTables/player_level_config.json";
	private const string outputFileName = "Assets/GlobalManagers/Data/PlayerLevelData/PlayerLevelDataList.asset";

	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
											   string[] movedAssets, string[] movedFromPath)
	{
		if (CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets))
		{
			Read();
		}
	}

	private static bool CheckResModified(string[] files)
	{
		bool fileModified = false;
		foreach (string file in files)
		{
			if (file.Contains(fileName))
			{
				fileModified = true;
				break;
			}
		}
		return fileModified;
	}


	[MenuItem("Tools/Import Data Table/Player Level Data")]
	public static void Read()
	{
		bool newFile = false;

		PlayerLevelDataList newList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(PlayerLevelDataList));
		if (oldFile == null)
		{
			newFile = true;
			newList = ScriptableObject.CreateInstance(typeof(PlayerLevelDataList)) as PlayerLevelDataList;
		}
		else
		{
			newList = oldFile as PlayerLevelDataList;
		}

		newList._dataList.Clear();
		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable data = obj as Hashtable;

			PlayerLevelData newData = new PlayerLevelData();

			newData._level = (int)data["level"];
			newData._exp = (int)data["exp"];
			newData.bearPoint = (int)data["totem_Point"];
			//newData._reviveHc = (int)data["revive_hc"];

			newData._mage_attack = (int)data["mage_Attack"];
			newData._mage_defense = (int)data["mage_Defense"];
			newData._mage_hp = (int)data["mage_Hp"];
			newData._mage_crit = (float)data["mage_Crit"];
			newData._mage_crit_damage = (float)data["mage_CritDamage"];

			newData._warrior_attack = (int)data["warrior_Attack"];
			newData._warrior_defense = (int)data["warrior_Defense"];
			newData._warrior_hp = (int)data["warrior_Hp"];
			newData._warrior_crit = (float)data["warrior_Crit"];
			newData._warrior_crit_damage = (float)data["warrior_CritDamage"];

			newList._dataList.Add(newData);
		}

        newList._dataList.Sort(OnCompare);

		if (newFile)
		{
			AssetDatabase.CreateAsset(newList, outputFileName);
		}
		else
		{
			EditorUtility.SetDirty(newList);
		}
		Debug.Log(string.Format("Player level config data successfully imported. {0} records.", newList._dataList.Count));
	}


    private static int OnCompare(PlayerLevelData data1, PlayerLevelData data2)
    {
        if (data1._level > data2._level)
        {
            return 1;
        }
        else if (data1._level == data2._level)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
}
