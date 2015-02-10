using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;


public class LevelConfigImporter : AssetPostprocessor
{
	private const string fileName = "Assets/DataTables/level_config.json";

	private const string outputFileName = "Assets/Data/Level/Config/LevelConfigNormal.asset";

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


	[MenuItem("Tools/Import Data Table/Level Config")]
	public static void Read()
	{
		bool newFile = false;

		LevelConfig newList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(LevelConfig));
		if (oldFile == null)
		{
			newFile = true;
			newList = ScriptableObject.CreateInstance(typeof(LevelConfig)) as LevelConfig;
		}
		else
		{
			newList = oldFile as LevelConfig;
		}

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

		newList.levels = new LevelData[ht.Count];

		int index = 0;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable data = obj as Hashtable;

			LevelData newData = new LevelData();
			newData.id = (int)data["id"];
			newData.levelName = data["level"] as string;
			newData.sceneName = data["sceneName"] as string;
			newData.levelNameIDS = data["levelNameIDS"] as string;

			newData.levelDescIDS = data["levelDescIDS"] as string;
            newData.isBoss = (int)data["isBoss"] == 1;
			newData.available = (int)data["available"] == 1;

			newData.levelLoadingBGTexture = data["bgTextureN"] as string;
			newData.unlockPlayerLevel = (int)data["unlockPlayerLevel"];
			newData.suggestedPlayerLevel = (int)data["suggestedPlayerLevel"];
			newData.preLevelID = (int)data["preLevelName"];
			newData.worldName = data["worldName"] as string;
			newData.vitalityCost = (int)data["vitalityCost"];

			newData.lootList = data["lootList"] as string;

			newData.cameraMode = (CameraController.CameraMode)(int)data["cameraMode"];
			newData.minTime = (int)data["timeOfMinScore"];
			newData.maxTime = (int)data["timeOfMaxScore"];

			newList.levels[index++] = newData;
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(newList, outputFileName);
		}
		else
		{
			EditorUtility.SetDirty(newList);
		}
		Debug.Log(string.Format("Player level config data successfully imported. {0} records.", newList.levels.Length));
	}
}
