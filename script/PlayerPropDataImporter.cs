using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using InJoy.Utils;
using System.Collections;

public class PlayerPropDataImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/attribute_player.json";
    public static string outFileName = "Assets/GlobalManagers/Data/PlayerLevelData/PlayerPropDataList.asset";

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
        object old = AssetDatabase.LoadAssetAtPath(outFileName, typeof(PlayerPropDataList));
        PlayerPropDataList fsDataList;
        if (null == old)
        {
            isNew = true;
            fsDataList = ScriptableObject.CreateInstance(typeof(PlayerPropDataList)) as PlayerPropDataList;
        }
        else
        {
            fsDataList = old as PlayerPropDataList;
        }

        fsDataList.dataList.Clear();
        string jsonStr = File.ReadAllText(fileName);
        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;
        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable row = obj as Hashtable;
            PlayerPropData fsData = new PlayerPropData();
            fsData.propKey = (PlayerPropKey)row["prop"];
            fsData.ids = (string)row["ids"];
            fsData.fs = (int)row["fs"];

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
        Debug.Log("player fs Data List import complete!(" + outFileName + ")");
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
