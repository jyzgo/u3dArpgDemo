using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class TattooAttributeImporter : AssetPostprocessor
{
	public static string fileName = "Assets/DataTables/attribute_tattoo.json";
	public static string outFileName = "Assets/GlobalManagers/Data/Tattoo/TattooAttributeList.asset";

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

		TattooAttributeList dataList = null;

		UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(TattooAttributeList));
		if (oldFile == null)
		{
			newFile = true;
			dataList = ScriptableObject.CreateInstance(typeof(TattooAttributeList)) as TattooAttributeList;
		}
		else
		{
			dataList = oldFile as TattooAttributeList;
		}

		dataList.dataList.Clear();

		string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
		{
			Hashtable ht2 = obj as Hashtable;


			AIHitParams param = (AIHitParams)Enum.Parse(typeof(AIHitParams), ht2["attribute"] as string);

			List<EnumTattooPart> parts = new List<EnumTattooPart>();

			dataList.dataList.Add(param);
		}

		if (newFile)
		{
			AssetDatabase.CreateAsset(dataList, outFileName);
		}
		else
		{
			EditorUtility.SetDirty(dataList);
		}
		Debug.Log(string.Format("Tattoo attributes imported OK. {0} records.", dataList.dataList.Count));
	}
}
