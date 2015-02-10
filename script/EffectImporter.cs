using UnityEditor;
using UnityEngine;
using System.Collections;

public class EffectImporter : AssetPostprocessor {
	
	static string _pathRoot = "Assets/Bullets/";
	
	void OnPreprocessModel() {
		if(assetPath.StartsWith(_pathRoot)) {
			ModelImporter mimporter = assetImporter as ModelImporter;
			mimporter.isReadable = false;
			mimporter.importMaterials = false;
			mimporter.animationType = ModelImporterAnimationType.Legacy;
			mimporter.optimizeMesh = true;
			mimporter.generateSecondaryUV = false;
			mimporter.globalScale = 1.0f;
		}
	}
	
	void OnPreprocessTexture() {
		if(assetPath.StartsWith(_pathRoot)) {
			TextureImporter timporter = assetImporter as TextureImporter;
			timporter.mipmapEnabled = false;
			timporter.maxTextureSize = 128;
			timporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
		}
	}
}
