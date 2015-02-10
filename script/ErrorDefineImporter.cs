using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using InJoy.Utils;
using System.IO;


public class ErrorDefineImporter : AssetPostprocessor
{

	public static string fileName = "Assets/DataTables/error.json";
	public static string sheetName = "#errorDefine";
	public static string outFileName = "Assets/GlobalManagers/Data/Error/ErrorDefineList.asset";
			
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, 
	                                           string[] movedAssets, string[] movedFromPath)
	{
		if( CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets) )
		{
			Read();
		}
	}
	
	private static bool CheckResModified(string[] files)
	{
		bool fileModified = false;
		foreach(string file in files)
		{
			if(file.Contains(fileName))
			{
				fileModified = true;	
				break;
			}
		}
		return fileModified;
	}
	
	public static void Read()
	{
        bool isNew = false;
        object old = AssetDatabase.LoadAssetAtPath(outFileName, typeof(ErrorDefineList));
        ErrorDefineList errorList;
        if (null == old)
        {
            isNew = true;
            errorList = ScriptableObject.CreateInstance(typeof(ErrorDefineList)) as ErrorDefineList;
        }
        else
        {
            errorList = old as ErrorDefineList;
        }

        errorList.errors.Clear();
        string jsonStr = File.ReadAllText(fileName);
        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;
        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable row = obj as Hashtable;
            int errorCode = (int)row["errorCode"];
            errorList.errors.Add(new ErrorDefine(errorCode, (string)row["ids"]));
        }
        if (isNew)
        {
            AssetDatabase.CreateAsset(errorList, outFileName);
        }
        else
        {
            EditorUtility.SetDirty(errorList);
        }
        Debug.Log("error Data List import complete!(" + outFileName + ")");
	}
}

