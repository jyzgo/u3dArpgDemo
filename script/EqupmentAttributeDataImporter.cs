using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InJoy.Utils;
using System.IO;
using System.Collections;

public class EqupmentAttributeDataImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/attribute_item.json";
    public static string outFileName = "Assets/GlobalManagers/Data/EquipmentUpgrade/EquipmentFSDataList.asset";

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                                       string[] movedAssets, string[] movedFromPath)
    {
        if (CheckResModified(importedAssets) || CheckResModified(deletedAssets) || CheckResModified(movedAssets))
        {
            Read();
        }
    }

    private static void Read()
    {
        bool isNew = false;
        object old = AssetDatabase.LoadAssetAtPath(outFileName, typeof(EquipmentFSDataList));
        EquipmentFSDataList fsDataList;
        if (null == old)
        {
            isNew = true;
            fsDataList = ScriptableObject.CreateInstance(typeof(EquipmentFSDataList)) as EquipmentFSDataList;
        }
        else
        {
            fsDataList = old as EquipmentFSDataList;
        }

        fsDataList.dataList.Clear();
        string jsonStr = File.ReadAllText(fileName);
        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;
        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable row = obj as Hashtable;
            EquipmentFSData fsData = new EquipmentFSData();
            fsData.prop = (AIHitParams)row["prop"];
            fsData.ids = (string)row["ids"];
            fsData.fs = (int)row["fs"];
            fsData.sc = (int)row["sc"];

            fsDataList.dataList.Add(fsData);
        }
        if (isNew)
        {
            AssetDatabase.CreateAsset(fsDataList, outFileName);
        }
        else
        {
            EditorUtility.SetDirty(fsDataList);
        }
        Debug.Log("fs Data List import complete!(" + outFileName + ")");
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
}
