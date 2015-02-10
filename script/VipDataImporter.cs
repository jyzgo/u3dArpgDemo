using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class VIPDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/vip.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Store/vip_privileges.asset";

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

		VipPrivilegeList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(VipPrivilegeList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(VipPrivilegeList)) as VipPrivilegeList;
		}
		else
		{
			dataList = oldFile as VipPrivilegeList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			VipPrivilegeData newData = new VipPrivilegeData();

			newData.vipLevel = (int)ht2["vip"];
			newData.maxScExchangeCount = (int)ht2["exchange"];
			newData.maxVitExchangeCount = (int)ht2["vitality"];

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
		Debug.Log(string.Format("Vip privilege data imported OK. {0} records.", dataList.dataList.Count));
	}
}
