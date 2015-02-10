using UnityEditor;
using UnityEngine;
using System.Collections;

public class RawDataImporter : AssetPostprocessor {

	static string _rootPath = "Assets/Raw/";	
	
	void OnPreprocessModel()
	{
		if(assetPath.StartsWith(_rootPath)) {
			ModelImporter mi = assetImporter as ModelImporter;
			mi.importMaterials = false;
		}
	}
}
