using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using InJoy.Utils;

public class CharacterTableImporter : AssetPostprocessor
{
    public static string fileName = "Assets/DataTables/character_table.json";
    public static string outputFileName = "Assets/GlobalManagers/Data/Characters/CharacterTable.asset";


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
        CharacterTable characterTable; ;

        bool newFile = false;

        UnityEngine.Object oldFile = AssetDatabase.LoadAssetAtPath(outputFileName, typeof(CharacterTable));
        if (oldFile == null)
        {
            newFile = true;
            characterTable = ScriptableObject.CreateInstance(typeof(CharacterTable)) as CharacterTable;
        }
        else
        {
            characterTable = oldFile as CharacterTable;
        }

        characterTable.characterTableList.Clear();

        string jsonStr = File.ReadAllText(fileName);

        JsonHashtable ht = FCJson.jsonDecode(jsonStr) as JsonHashtable;

        foreach (System.Object obj in ht.ValueList)
        {
            Hashtable data = obj as Hashtable;

            CharacterInformation info = new CharacterInformation();

            info.label = data["labelID"] as string;
            info.aiAgentPath = data["aiAgentPath"] as string;
            info.modelPath = data["modelPath"] as string;

            characterTable.characterTableList.Add(info);
        }

        if (newFile)
        {
            AssetDatabase.CreateAsset(characterTable, outputFileName);
        }
        else
        {
            EditorUtility.SetDirty(characterTable);
        }
        Debug.Log(string.Format("Character data successfully imported. {0} records.", characterTable.characterTableList.Count));
    }
}
