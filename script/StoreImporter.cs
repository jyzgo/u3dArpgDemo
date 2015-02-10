using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;
using UnityEngine;

public class StoreImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/store.json";
    public static string outFileName = "Assets/GlobalManagers/Data/Store/Store.asset";

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

        FC_StoreDataList dataList = null;

        UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outFileName, typeof(FC_StoreDataList));
        if (oldFile == null)
        {
            newFile = true;
            dataList = ScriptableObject.CreateInstance(typeof(FC_StoreDataList)) as FC_StoreDataList;
        }
        else
        {
            dataList = oldFile as FC_StoreDataList;
        }

        dataList.dataList.Clear();
        string jsonStr = File.ReadAllText(fileName);
		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable ht2 = obj as Hashtable;
            FC_StoreData sd = new FC_StoreData();
            sd.id = (int)ht2["id"];
            sd.name = ht2["name"] as string;
            sd.count = (int)ht2["count"];
            sd.displayNameIds = ht2["displayNameIds"] as string;
            sd.order = (int)ht2["order"];
            sd.storeIconName = ht2["storeIconName"] as string;
            dataList.dataList.Add(sd);
        }
        if (newFile)
        {
            AssetDatabase.CreateAsset(dataList, outFileName);
        }
        else
        {
            EditorUtility.SetDirty(dataList);
        }
        Debug.Log(string.Format("Store data imported OK. {0} records.", dataList.dataList.Count));
    }
}
