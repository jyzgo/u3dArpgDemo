using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class NickNameDataImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/nickname.json";
    public static string outputFileName = "Assets/GlobalManagers/Data/NickNameData/NickNameDataList.asset";

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
        NickNameDataList dataList;

        bool newFile = false;

        UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(EotDataList));

        if (oldFile == null)
        {
            newFile = true;
            dataList = ScriptableObject.CreateInstance(typeof(NickNameDataList)) as NickNameDataList;
        }
        else
        {
            dataList = oldFile as NickNameDataList;
        }

        dataList.dataList.Clear();

        string jsonStr = File.ReadAllText(fileName);

        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.Values)
        {
            Hashtable ht2 = obj as Hashtable;

            NickNameData newData = new NickNameData();

            newData.id = (int)ht2["id"];
            newData.prefixCh = ht2["prefixCh"] as string;
            newData.prefixEn = ht2["prefixEn"] as string;
            newData.suffixCh = ht2["suffixCh"] as string;
            newData.suffixEn = ht2["suffixEn"] as string;

            dataList.dataList.Add(newData);

        }

        dataList.dataList.Sort(Utils.CompareNickNameFromMinToMax);

        if (newFile)
        {
            AssetDatabase.CreateAsset(dataList, outputFileName);
        }
        else
        {
            EditorUtility.SetDirty(dataList);
        }
        Debug.Log(string.Format("Eot data imported OK. {0} records.", dataList.dataList.Count));
    }
}
