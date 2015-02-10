using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class TattooDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/tattoo.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Tattoo/TattooDataList.asset";

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

	[MenuItem("Tools/Import Data Table/Tattoo Data")]
	public static void Read()
	{
		bool newFile = false;

		TattooDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(TattooDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(TattooDataList)) as TattooDataList;
		}
		else
		{
			dataList = oldFile as TattooDataList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			TattooData newData = new TattooData();

			newData.tattooID = ht2["tattoo"] as string;
			newData.recipeID = ht2["recipe"] as string;
			newData.suiteID = ht2["suiteId"] as string;
			newData.ord = (int)ht2["ord"];

			List<EnumTattooPart> parts = new List<EnumTattooPart>();

			int partsNum = (int)ht2["tattooPart"];

			int shiftTime = 0;

			while (partsNum != 0)
			{
				if ((partsNum & 1) != 0)
				{
					parts.Add((EnumTattooPart)shiftTime);
				}

				partsNum = partsNum >> 1;

				shiftTime++;
			}

			newData.applicableParts = parts;

			newData.level = (int)ht2["tattooLevel"];
			newData.bearingPoint = (int)ht2["bearingPoint"];
			newData.learnHC = (int)ht2["learnCostHC"];
			newData.hcCost = (int)ht2["costHC"];
			newData.scCost = (int)ht2["costSC"];

			newData.materials = new List<MatAmountMapping>();

			for (int i = 1; i < 6; i++)
			{
				string fieldMat = "material" + i.ToString();
				string fieldCount = "count" + i.ToString();

				string materialName = ht2[fieldMat] as string;

				if (string.IsNullOrEmpty(materialName))
				{
					break;
				}

				newData.materials.Add(new MatAmountMapping(materialName, (int)ht2[fieldCount]));
			}

			dataList.dataList.Add(newData);
		}

		dataList.Sort();

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Tattoo data imported OK. {0} records.", dataList.dataList.Count));
	}
}


public class TattooExchangeDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/tattoo_exchange.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Tattoo/TattooExchangeDataList.asset";

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

	[MenuItem("Tools/Import Data Table/Tattoo Exchange Data")]
	public static void Read()
	{
		bool newFile = false;

		TattooExchangeDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(TattooExchangeDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(TattooExchangeDataList)) as TattooExchangeDataList;
		}
		else
		{
			dataList = oldFile as TattooExchangeDataList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		Hashtable ht = FCJson.jsonDecode(jsonStr) as Hashtable;

		foreach (System.Object obj in ht.Values)
		{
			Hashtable ht2 = obj as Hashtable;

			TattooExchangeData newData = new TattooExchangeData();

			newData.id = ht2["item"] as string;
	
			newData.costSC = (int)ht2["costSC"];

			newData.materials = new List<MatAmountMapping>();

			for (int i = 1; i < 6; i++)
			{
				string fieldMat = "material" + i.ToString();
				string fieldCount = "count" + i.ToString();

				string materialName = ht2[fieldMat] as string;

				if (string.IsNullOrEmpty(materialName))
				{
					break;
				}

				newData.materials.Add(new MatAmountMapping(materialName, (int)ht2[fieldCount]));
			}

			dataList.dataList.Add(newData);
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Tattoo Exchange data imported OK. {0} records.", dataList.dataList.Count));
	}
}