using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class TattooUnlockDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/tattoo_unlock.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Tattoo/TattooUnlockDataList.asset";

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

	[MenuItem("Tools/Import Data Table/Tattoo Unlock Data")]
	public static void Read()
	{
		bool newFile = false;

		TattooUnlockDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(TattooUnlockDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(TattooUnlockDataList)) as TattooUnlockDataList;
		}
		else
		{
			dataList = oldFile as TattooUnlockDataList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			TattooUnlockData newData = new TattooUnlockData();

			newData.part = (EnumTattooPart)(int)ht2["tattooPart"];
			newData.playerLevel = (int)ht2["unlockLevel"];

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

		dataList.dataList.Sort(new MyComparer());

		Debug.Log(string.Format("Tattoo unlock data imported OK. {0} records.", dataList.dataList.Count));
	}

	private class MyComparer : IComparer<TattooUnlockData>
	{
		public int Compare(TattooUnlockData x, TattooUnlockData y)
		{
			int xx = x.playerLevel;

			int yy = y.playerLevel;

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
