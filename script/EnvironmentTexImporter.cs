using UnityEditor;
using UnityEngine;
using System.Collections;

public class EnvironmentTexImporter :  AssetPostprocessor{
	
	static string _envPath = "Assets/Levels/";
	static string _envFbxPath = "Raw";

	void OnPreprocessTexture() {
		
		if(assetPath.StartsWith(_envPath))
		{
			string []pathPitches = assetPath.Split('/');
			// trying to match path 'Assets/Levels/XXX/Raw/XXXXX'
			if(pathPitches.Length >= 5 && pathPitches[3] == _envFbxPath)
			{
				TextureImporter tImporter = assetImporter as TextureImporter;
				
				if(assetPath.Contains("_uncompressed.")) {
					tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				}
				else {
					tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
				}
				tImporter.maxTextureSize = 1024;
				tImporter.isReadable = false;
				tImporter.mipmapEnabled = false;
				tImporter.filterMode = FilterMode.Bilinear;
				tImporter.textureType = TextureImporterType.Advanced;
			}
		}
	}
}
