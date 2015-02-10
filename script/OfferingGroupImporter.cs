using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;


public class OfferingGroupImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/offering_display_group.json";
    public static string outputFileName = "Assets/GlobalManagers/Data/Offering/OfferingGroupList.asset";


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
        OfferingGroupList offeringGroupList;

        UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(OfferingGroupList));

        if (oldFile == null)
        {
            offeringGroupList = ScriptableObject.CreateInstance(typeof(OfferingGroupList)) as OfferingGroupList;
        }
        else
        {
            offeringGroupList = oldFile as OfferingGroupList;
        }

        offeringGroupList.dataList.Clear();

        string jsonStr = File.ReadAllText(fileName);

        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable ht2 = obj as Hashtable;

            OfferingGroup offeringGroup = new OfferingGroup();
            offeringGroup.groupId = ht2["groupId"] as string;
            for (int i = 0; i < 20; i++)
            {
                int index = i + 1;
                SingleGroup group = new SingleGroup();
                group.item = ht2["item" + index.ToString()] as string;
                group.count = (int)ht2["count" + index.ToString()];

                if ("" != group.item && group.count > 0)
                {
                    offeringGroup.groupList.Add(group);
                }
            }

            offeringGroupList.dataList.Add(offeringGroup);
        }


        if (!File.Exists(outputFileName))	//new file
        {
            AssetDatabase.CreateAsset(offeringGroupList, outputFileName);
        }
        else
        {
            EditorUtility.SetDirty(offeringGroupList);
        }
    }
}

