using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class DialogDataImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/dialogs.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Tattoo/dialogs.asset";

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

		DialogDataList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(DialogDataList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(DialogDataList)) as DialogDataList;
		}
		else
		{
			dataList = oldFile as DialogDataList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;

			DialogData dd = new DialogData();

			dd.id = ht2["id"] as string;
			dd.pos = (EnumDialogPos)(int)ht2["pos"];
			dd.speakerIDS = ht2["speaker"] as string;
			dd.contentIDS = ht2["content"] as string;

			dataList.dataList.Add(dd);
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Dialogs imported OK. {0} records.", dataList.dataList.Count));
	}
}
