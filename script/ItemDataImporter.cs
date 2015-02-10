using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;


public class ItemDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/item.json";

	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
											   string[] movedAssets, string[] movedFromPath)
	{
		if (CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets))
		{
			ReadJson();
		}
	}

	private static void ReadJson()
	{
		string[] files = new string[]
		{
			"weapon",   //item type name
			"Assets/GlobalManagers/Data/ItemData/WeaponItemDataList.asset",		//output file name
	
			"armor",
			"Assets/GlobalManagers/Data/ItemData/ArmorItemDataList.asset",
	
			"ornament",
			"Assets/GlobalManagers/Data/ItemData/OrnamentItemDataList.asset",

			"vanity",
			"Assets/GlobalManagers/Data/ItemData/VanityItemDataList.asset",

			"material",
			"Assets/GlobalManagers/Data/ItemData/MaterialItemDataList.asset",

			"tribute",
			"Assets/GlobalManagers/Data/ItemData/TributeItemDataList.asset",

			"other",
			"Assets/GlobalManagers/Data/ItemData/OtherItemDataList.asset",
		};

		ItemDataList[] lists = new ItemDataList[files.Length / 2];

		Dictionary<string, ItemDataList> dict = new Dictionary<string, ItemDataList>();

		for (int i = 0; i < files.Length / 2; i++)
		{
			string itemTypeName = files[i * 2];

			string outputFileName = files[i * 2 + 1];

			ItemDataList itemDataList = null;

			UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(ItemDataList));

			if (oldFile == null)
			{
				itemDataList = ScriptableObject.CreateInstance(typeof(ItemDataList)) as ItemDataList;
			}
			else
			{
				itemDataList = oldFile as ItemDataList;
			}

			itemDataList._dataList.Clear();

			dict.Add(itemTypeName, itemDataList);
		}


		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			ItemData itemdata = new ItemData();

			itemdata.id = ht2["itemId"] as string;

			itemdata.type = (ItemType)(int)ht2["type"];

			itemdata.subType = (ItemSubType)(int)ht2["subType"];

			itemdata.enableLevel = (int)ht2["enableLevel"];
			itemdata.roleID = (int)ht2["class"];
			itemdata.level = (int)ht2["level"];

			itemdata.nameIds = (string)ht2["name"];
			itemdata.descriptionIds = (string)ht2["description"];

			itemdata.rareLevel = (int)ht2["rareLevel"];

			itemdata.iconPath = (string)ht2["icon"];
			itemdata.instance = (string)ht2["instance"];

			itemdata.sellCount = (float)ht2["sellCount"];
			itemdata.stack = (int)ht2["stack"];

			itemdata.attrId0 = (AIHitParams)(int)ht2["attributeId0"];
			itemdata.attrValue0 = (float)ht2["value0"];

			itemdata.attrId1 = (AIHitParams)(int)ht2["attributeId1"];
			itemdata.attrValue1 = (float)ht2["value1"];

			itemdata.attrId2 = (AIHitParams)(int)ht2["attributeId2"];
			itemdata.attrValue2 = (float)ht2["value2"];

			string typeName = itemdata.type.ToString();

			if (dict.ContainsKey(typeName))
			{
				dict[typeName]._dataList.Add(itemdata);
			}
			else  //all other types are put into file "other"
			{
				dict["other"]._dataList.Add(itemdata);

				Debug.LogWarning(string.Format("ItemType {0} not found in dictionary, item {1} put into file \"other\".", typeName, itemdata.id));
			}
		}

		for (int i = 0; i < files.Length / 2; i++)
		{
			string itemTypeName = files[i * 2];

			string outputFileName = files[i * 2 + 1];

			if (!File.Exists(outputFileName))	//new file
			{
				AssetDatabase.CreateAsset(dict[itemTypeName], outputFileName);
			}
			else
			{
				EditorUtility.SetDirty(dict[itemTypeName]);
			}
			Debug.Log(string.Format("{0} records imported to {1}", dict[itemTypeName]._dataList.Count, outputFileName));
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
}
