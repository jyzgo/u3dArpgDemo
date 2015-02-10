using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class VitPriceDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/store_buy_vitality.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Store/vit_prices.asset";

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

	public static void Read()
	{
		bool newFile = false;

		VitPriceList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(VitPriceList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(VitPriceList)) as VitPriceList;
		}
		else
		{
			dataList = oldFile as VitPriceList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			VitPrice newData = new VitPrice();

			newData.buyTimes = (int)ht2["buyTime"];
			newData.hcCost = (int)ht2["costHc"];

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
		Debug.Log(string.Format("Vitality prices imported OK. {0} records.", dataList.dataList.Count));
	}
}
