using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class EotDataImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/eot.json";
    public static string outputFileName = "Assets/GlobalManagers/Data/EotData/EotDataList.asset";

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
        EotDataList dataList;

        bool newFile = false;

        UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(EotDataList));

        if (oldFile == null)
        {
            newFile = true;
            dataList = ScriptableObject.CreateInstance(typeof(EotDataList)) as EotDataList;
        }
        else
        {
            dataList = oldFile as EotDataList;
        }

        dataList.dataList.Clear();

        string jsonStr = File.ReadAllText(fileName);

		JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.Values)
        {
            Hashtable ht2 = obj as Hashtable;

            EotData newData = new EotData();

            newData.eotID = ht2["eotId"] as string;

            for (int i = 1; i < 4; i++)
            {
                int type = (int)ht2[string.Format("eotType{0}", i)];

                if (type > -1)
                {
                    Eot eot = new Eot();
                    eot.eotType = (Eot.EOT_TYPE)type;
                    eot.eotPercent = (float)ht2[string.Format("att{0}", i)];
                    eot.eotValue = (float)ht2[string.Format("att{0}", i)];
                    eot.lastTime = (float)ht2[string.Format("lastTime{0}", i)];
                    newData.eotList.Add(eot);
                }
            }

            dataList.dataList.Add(newData);
        }

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
