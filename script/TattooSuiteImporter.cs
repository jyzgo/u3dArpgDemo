using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class TattooSuiteImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/tattoo_suite.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Tattoo/TattooSuiteDataList.asset";

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

	[MenuItem("Tools/Import Data Table/Tattoo Suite Data")]
	public static void Read()
	{
		bool newFile = false;

		TattooSuiteDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(TattooSuiteDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(TattooSuiteDataList)) as TattooSuiteDataList;
		}
		else
		{
			dataList = oldFile as TattooSuiteDataList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			TattooSuiteData newData = new TattooSuiteData();

			newData.suiteID = ht2["tattooSuite"] as string;
			newData.nameIDS = ht2["name"] as string;
			newData.descIDS = ht2["description"] as string;
			newData.ord = (int)ht2["ord"];
			newData.level = (int)ht2["level"];
			newData.rareLevel = (int)ht2["rareLevel"];

			newData.attribute0 = (AIHitParams)(int)ht2["attributeId0"];
			newData.value0 = (float)ht2["value0"];

			newData.attribute1 = (AIHitParams)(int)ht2["attributeId1"];
			newData.value1 = (float)ht2["value1"];

			newData.attribute2 = (AIHitParams)(int)ht2["attributeId2"];
			newData.value2 = (float)ht2["value2"];
			
			List<string> idList = new List<string>();
			for (int i = 1; i < 11; i ++)
			{
				string part = ht2["suitePart" + i.ToString()] as string;

				if (!string.IsNullOrEmpty(part))
				{
					idList.Add(part);
				}
				else
				{
					break;
				}
			}
			newData.tdList = idList;

			dataList.dataList.Add(newData);
		}

		dataList.dataList.Sort(new MyComparer());

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Tattoo Suite data imported OK. {0} records.", dataList.dataList.Count));
	}

	private class MyComparer : IComparer<TattooSuiteData>
	{
		public int Compare(TattooSuiteData x, TattooSuiteData y)
		{
			int xx = x.level * 100 + x.ord;

			int yy = y.level * 100 + y.ord;

			if (xx > yy)
			{
				return 1;
			}
			else if (xx == yy)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}
}
