using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class SkillDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/skill.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Skill/SkillDataList.asset";

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

	[MenuItem("Tools/Import Data Table/Skill Data")]
	public static void Read()
	{
		bool newFile = false;

		SkillDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(SkillDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(SkillDataList)) as SkillDataList;
		}
		else
		{
			dataList = oldFile as SkillDataList;
		}

		dataList._dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			SkillData newData = new SkillData();

			newData.enemyID = ht2["enemyId"] as string;
			newData.skillID = ht2["skill"] as string;
			newData.roleID = (int)ht2["role"];
			newData.ord = (int)ht2["ord"];
			newData.nameIDS = ht2["nameIDS"] as string;
			newData.descIDS = ht2["description"] as string;
			newData.iconPath = ht2["iconPath"] as string;

			newData.isPassive = (int)ht2["isPassive"] == 1;

			dataList._dataList.Add(newData);
		}

		dataList._dataList.Sort(new SDComparer());

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Skill data imported OK. {0} records.", dataList._dataList.Count));
	}

	private class SDComparer : IComparer<SkillData>
	{
		public int Compare(SkillData x, SkillData y)
		{
			int xx = x.roleID * 10 + x.ord;

			int yy = y.roleID * 10 + y.ord;

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
