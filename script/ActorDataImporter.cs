using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;


public class ActorDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/character_data.json";
	public static string sheetName = "data";
	public static string outFileName = "Assets/GlobalManagers/Data/Characters/AcDataList.asset";

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


	[MenuItem("Tools/Import Data Table/Actor Data")]
	public static void Read()
	{
		AcDataList acDataList;

		bool newFile = false;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(AcDataList));
		if (oldFile == null)
		{
			newFile = true;
			acDataList = ScriptableObject.CreateInstance(typeof(AcDataList)) as AcDataList;
		}
		else
		{
			acDataList = oldFile as AcDataList;
		}

		acDataList._dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable data = obj as Hashtable;

			AcData newData = new AcData();

			newData.id = data["id"] as string;

			newData.characterId = data["characterId"] as string;
			newData.classIds = data["class"] as string;
			newData.nameIds = data["nameIds"] as string;
			newData.faction = (FC_AC_FACTIOH_TYPE)Enum.Parse(typeof(FC_AC_FACTIOH_TYPE), data["faction"] as string);
			newData.eliteType = (FC_ELITE_TYPE)Enum.Parse(typeof(FC_ELITE_TYPE), data["type"] as string);
			newData.isPlayer = (int)data["isPlayer"] == 1;
			newData.speed = (float)data["speed"];
			newData.angleSpeed = (float)data["angleSpeed"];
			newData.hp = (int)data["hp"];
			//newData._energy = (int)data["energy"];
			//newData._vitality = (int)data["vitality"];
			newData.level = (int)data["level"];
			newData.thresholdMax = (int)data["thresholdMax"];
			newData.physicalAttack = (float)(int)data["physicalAttack"];
			newData.physicalDefense = (float)(int)data["physicalDefense"];
			newData.fireAttack = (float)(int)data["fireAttack"];
			newData.fireResist = (float)(int)data["fireResist"];
			newData.poisonAttack = (float)(int)data["poisonAttack"];
			newData.poisonResist = (float)(int)data["poisonResist"];
			newData.lightningAttack = (float)(int)data["lightingAttack"];
			newData.lightningResist = (float)(int)data["lightingResist"];
			newData.iceAttack = (float)(int)data["iceAttack"];
			newData.iceResist = (float)(int)data["iceResist"];
			newData.critRate = (float)data["critRate"];
			newData.critDamage = (float)data["critDamage"];

			newData.equipList.Clear();
			AddEquip(newData.equipList, data["equip1"] as string);
			AddEquip(newData.equipList, data["equip2"] as string);
			AddEquip(newData.equipList, data["equip3"] as string);
			AddEquip(newData.equipList, data["equip4"] as string);
			AddEquip(newData.equipList, data["equip5"] as string);
			AddEquip(newData.equipList, data["equip6"] as string);
			AddEquip(newData.equipList, data["equip7"] as string);
			AddEquip(newData.equipList, data["equip8"] as string);


			newData.lootXp = (int)data["lootXp"];

			acDataList._dataList.Add(newData);
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(acDataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(acDataList);
		}
		Debug.Log(string.Format("Character data successfully imported. {0} records.", acDataList._dataList.Count));
	}

	public static void AddEquip(List<EquipmentIdx> list, string equipId)
	{
		if (equipId == null)
		{
			return;
		}

		if (equipId.Length < 2)
		{
			return;
		}

		EquipmentIdx eIdx = new EquipmentIdx();
		eIdx._id = equipId;
		eIdx._evolutionLevel = 0;
		list.Add(eIdx);
	}
}
