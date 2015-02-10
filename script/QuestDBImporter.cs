using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;


public class QuestDBImporter : AssetPostprocessor
{
	private const string questFileName = "Assets/DataTables/quest.json";

	private const string outFileNameQuest = "Assets/GlobalManagers/Data/QuestData/QuestDB.asset";

	private const string dailyBonusFileName = "Assets/DataTables/quest_daily_bonus.json";
	private const string outFileNameDailyBonus = "Assets/GlobalManagers/Data/QuestData/DailyBonus.asset";

	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
											   string[] movedAssets, string[] movedFromPath)
	{
		if (CheckResModified(importedAssets, questFileName) || CheckResModified(deletedAssets, questFileName) || CheckResModified(movedAssets, questFileName))
		{
			ReadQuest(questFileName, outFileNameQuest);
		}

		if (CheckResModified(importedAssets, dailyBonusFileName) || CheckResModified(deletedAssets, dailyBonusFileName) || CheckResModified(movedAssets, dailyBonusFileName))
		{
			ReadDailyBonusData(dailyBonusFileName, outFileNameDailyBonus);
		}
	}

	private static bool CheckResModified(string[] files, string fileName)
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


	public static void ReadQuest(string fileName, string outFileName)
	{
		bool newFile = false;

		QuestDataList questDataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(QuestDataList));
		if (oldFile == null)
		{
			newFile = true;
			questDataList = ScriptableObject.CreateInstance(typeof(QuestDataList)) as QuestDataList;
		}
		else
		{
			questDataList = oldFile as QuestDataList;
		}

		questDataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

		foreach (System.Object obj in ht.ValueList)
		{
			Hashtable data = obj as Hashtable;

			QuestData newData = new QuestData();

			newData.quest_id = (int)data["quest_id"];
			newData.quest_name = (string)data["quest_name"];
			newData.giver = (string)data["giver"];
			newData.description = (string)data["description"];
			newData.status = (QuestStatus)(int)data["status"];
			newData.quest_type = (QuestType)(int)data["quest_type"];
			newData.cycle_type = (QuestCycleType)(int)data["cycle_type"];
			newData.start_offset = (int)(data["start_offset"]);
			newData.end_offset = (int)(data["end_offset"]);
			newData.recommended_level_name = (string)data["recommended_level_name"];

			//quest target
			List<QuestTarget> targetList = newData.target_list;
			for (int j = 1; j <= 2; j++)
			{
				int targetType = (int)data["target_type" + j.ToString()];
				if (targetType != -1)
				{
					QuestTarget target = new QuestTarget();
					target.target_type = (QuestTargetType)targetType;

					target.target_id = (string)data["target_id" + j.ToString()];
					target.target_var1 = (string)data["target_var" + j.ToString()];
					target.target_count = (int)data["target_count" + j.ToString()];

					targetList.Add(target);
				}
			}

			newData.reward_exp = (int)(data["reward_exp"]);
			newData.reward_sc = (int)(data["reward_sc"]);
			newData.reward_hc = (int)(data["reward_hc"]);

			//reward items
			List<QuestRewardItem> itemList = newData.reward_item_list;
			for (int j = 1; j <= 4; j++)
			{
				string rewardID = (string)data["reward_item_id" + j.ToString()];
				if (rewardID != "")
				{
					QuestRewardItem item = new QuestRewardItem();
					item.reward_item_id = rewardID;
					item.reward_role = (EnumRole)(int)data["reward_role" + j.ToString()];
					item.reward_item_count = (int)data["reward_item_count" + j.ToString()];

					itemList.Add(item);
				}
			}

			questDataList.dataList.Add(newData);
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(questDataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(questDataList);
		}

		Debug.Log(questDataList.dataList.Count + " quest records imported.");

	}

	public static void ReadDailyBonusData(string fileName, string outFileName)
	{
		bool newFile = false;
		DailyBonusDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(DailyBonusDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(DailyBonusDataList)) as DailyBonusDataList;
		}
		else
		{
			dataList = oldFile as DailyBonusDataList;
		}

		dataList.dataList.Clear();
		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

		foreach (System.Object obj in ht.ValueList)
		{
			Hashtable data = obj as Hashtable;

			DailyBonusData newData = new DailyBonusData();

			newData.day = (int)(data["day"]);
			newData.reward_item_id = (string)data["reward_item_id"];
			newData.amount = (int)(data["amount"]);

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

		Debug.Log(dataList.dataList.Count + " daily bonus records read.");

	}
}