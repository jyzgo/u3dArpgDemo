using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class OfferingDataImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/offering.json";
    public static string outputFileName = "Assets/GlobalManagers/Data/Offering/OfferingDataList.asset";

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
        OfferingDataList offeringDataList;

        UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(OfferingDataList));

        if (oldFile == null)
        {
            offeringDataList = ScriptableObject.CreateInstance(typeof(OfferingDataList)) as OfferingDataList;
        }
        else
        {
            offeringDataList = oldFile as OfferingDataList;
        }

        offeringDataList.dataList.Clear();

        string jsonStr = File.ReadAllText(fileName);

        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable ht2 = obj as Hashtable;

            OfferingData offeringData = new OfferingData();
            offeringData.id = ht2["id"] as string;
            offeringData.level = (OfferingLevel)((int)ht2["level"]);
            offeringData.nameIds = ht2["nameIds"] as string;
            offeringData.desIds = ht2["desIds"] as string;
            offeringData.isVisible = (int)ht2["isVisible"];
            offeringData.levelMin = (int)ht2["levelMin"];
            offeringData.levelMax = (int)ht2["levelMax"];
            offeringData.costHC = (int)ht2["costHC"];
            for (int i = 0; i < 5; i++ )
            {
                offeringData.costItemList.Add(ht2["costItem" + (i + 1).ToString()] as string);
            }
            offeringData.displayGroup = ht2["displayGroup"] as string;
            offeringData.hitMoney = (int)ht2["hitMoney"];

            offeringDataList.dataList.Add(offeringData);
        }

        if (!File.Exists(outputFileName))	//new file
        {
            AssetDatabase.CreateAsset(offeringDataList, outputFileName);
        }
        else
        {
            EditorUtility.SetDirty(offeringDataList);
        }
    }




}
