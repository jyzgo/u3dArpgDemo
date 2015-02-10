using UnityEditor;
using UnityEngine;
using System.Collections;

public class LootImporter : AssetPostprocessor
{

    static string _pathRoot = "Assets/Loot/";

    void OnPreprocessModel()
    {
        if (assetPath.StartsWith(_pathRoot))
        {
            ModelImporter mimporter = assetImporter as ModelImporter;
            mimporter.isReadable = false;
            mimporter.importMaterials = false;
            mimporter.animationType = ModelImporterAnimationType.None;
            mimporter.optimizeMesh = true;
            mimporter.generateSecondaryUV = false;
            mimporter.globalScale = 1.0f;
        }
    }
}
