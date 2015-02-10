using UnityEngine;
using System.Collections;
using UnityEditor;


/// <summary>
/// Localization import postprocessor.
/// </summary>
public class LocalizationImportPostprocessor : AssetPostprocessor
{

	void OnPreprocessTexture()
	{
		TextureImporter texImporter = (TextureImporter)assetImporter;
		
		string pathPrefix = System.IO.Path.Combine(MultiLanguageAssetPostProcessor.MULTILANGUAGE_ASSETS_FOLDER, "Fonts");
		
		if (texImporter.assetPath.StartsWith(pathPrefix))
		{
			texImporter.grayscaleToAlpha = false;
			texImporter.wrapMode = TextureWrapMode.Clamp;
		}
	}

}
