using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class SkillUpgradeDataImporter : AssetPostprocessor
{

	public static string fileName = "Assets/DataTables/skill_upgrade.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Skill/SkillUpgradeDataList.asset";
			
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

	[MenuItem("Tools/Import Data Table/Skill Upgrade Data")]
	public static void Read()
	{
		bool newFile = false;

		SkillUpgradeDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(SkillUpgradeDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(SkillUpgradeDataList)) as SkillUpgradeDataList;
		}
		else
		{
			dataList = oldFile as SkillUpgradeDataList;
		}

		dataList.dataList.Clear();

		string curSkillName = "";

		SkillUpgradeData curSkillUpgradeData = null;

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable data = obj as Hashtable;

			string enemyId = data["enemyId"] as string;

			string skillName = data["skill"] as string;

			curSkillUpgradeData = dataList.dataList.Find(
				delegate(SkillUpgradeData sud)
				{
					return (sud._enemyId == enemyId && sud._skillId == skillName);
				});

			if (curSkillUpgradeData == null)
			{
				curSkillName = skillName;
				curSkillUpgradeData = new SkillUpgradeData();
				curSkillUpgradeData._skillId = curSkillName;
				curSkillUpgradeData._enemyId = enemyId;
				dataList.dataList.Add(curSkillUpgradeData);
				curSkillUpgradeData.upgradeDataList.Clear();
			}

			SkillGrade newData = new SkillGrade();

            newData.level = (int)data["level"];
			newData.cost = (int)data["cost"];
			newData.coolDownTime = (float)(data["coolDownTime"]);
			newData.chargeTime = (float)(data["chargeTime"]);
			newData.skillTime = (float)(data["skillTime"]);
			newData.damageScale = (float)(data["damageScale"]);
			newData.effect = (int)(data["effect"]);

			newData.specialAttackType = (int)(data["specialAttackType"]);
			newData.attackRange = (float)(data["attackRange"]);
			newData.attackNumber = (int)(data["attackNumber"]);
			newData.bulletCount = (int)(data["bulletCount"]);
			newData.dotDamageTime = (float)(data["dotDamageTime"]);
			newData.attackEffectTime = (float)(data["attackEffectTime"]);
			newData.speed = (float)(data["speed"]);
			newData.godTime = (float)(data["godTime"]);

			newData.attribute1 = (float)(data["att1"]);
			newData.attribute2 = (float)(data["att2"]);
			newData.passiveType1 = (AIHitParams)(int)(data["passiveType1"]);
            newData.passiveType2 = (AIHitParams)(int)(data["passiveType2"]);

			newData.costSc = (int)(data["costSc"]);
			newData.costHc = (int)(data["costHc"]);
			newData.unlockLevel = (int)(data["unlockLevel"]);
			newData.preSkillId = data["preSkillId"] as string;
			newData.preSkillLevel = (int)(data["preSkillLevel"]);
			newData.descIDS = data["descIDS"] as string;
            newData.eotId = data["eotId"] as string;

			curSkillUpgradeData.upgradeDataList.Add(newData);
		}

		foreach (SkillUpgradeData sud in dataList.dataList)
		{
			sud.upgradeDataList.Sort(new SUDComparer());
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Skill upgrade data imported OK. {0} records.", dataList.dataList.Count));
	}

	private class SUDComparer : IComparer<SkillGrade>
	{
		public int Compare(SkillGrade x, SkillGrade y)
		{
			if (x.unlockLevel > y.unlockLevel)
			{
				return 1;
			}
			else if (x.unlockLevel == y.unlockLevel)
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
