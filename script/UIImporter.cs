using UnityEditor;
using UnityEngine;
using System.Collections;

public class UIImporters : AssetPostprocessor
{
	static string _UIRootPath = "Assets/UI/";

	static string _UIBundlePath = "Assets/UI/bundle/";

	void OnPreprocessTexture()
	{
		if (assetPath.StartsWith(_UIRootPath) && !assetPath.StartsWith("Assets/UI/Textures/Loading"))
		{
			TextureImporter tImporter = assetImporter as TextureImporter;

			tImporter.npotScale = TextureImporterNPOTScale.None;

			tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			if (assetPath.Contains("_compressed") || assetPath.StartsWith(_UIBundlePath))
			{
				tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
			}
			tImporter.maxTextureSize = 1024;

			if (assetPath.Contains("Assets/UI/bundle/"))
			{
				tImporter.alphaIsTransparency = true;
			}
			else
			{
				tImporter.alphaIsTransparency = false;
			}
			tImporter.mipmapEnabled = false;
			tImporter.filterMode = FilterMode.Bilinear;
			tImporter.textureType = TextureImporterType.Advanced;
		}
	}
}