using UnityEditor;
using UnityEngine;
using System.Collections;

public class WorldmapImporter : AssetPostprocessor
{

    static string WorldmapPath = "Assets/Levels/Worldmap";

    void OnPreprocessTexture()
    {
        if (assetPath.StartsWith(WorldmapPath))
        {
            TextureImporter tImporter = assetImporter as TextureImporter;
            tImporter.npotScale = TextureImporterNPOTScale.None;
            
            tImporter.maxTextureSize = 1024;
            tImporter.textureFormat = TextureImporterFormat.ARGB16;
            tImporter.mipmapEnabled = false;
            tImporter.filterMode = FilterMode.Point;
            tImporter.textureType = TextureImporterType.GUI;
        }
    }
}