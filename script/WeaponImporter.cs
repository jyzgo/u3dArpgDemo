using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class WeaponImporter : AssetPostprocessor
{
	private const string k_armor_path = "Assets/Weapon/";
	private static string _ignorePath = "Assets/Weapon/Common/";

	void OnPreprocessModel()
	{
		if (assetPath.StartsWith(k_armor_path) && !assetPath.StartsWith(_ignorePath))
		{
			//property settings
			ModelImporter mImporter = assetImporter as ModelImporter;

			mImporter.importMaterials = false;

			mImporter.globalScale = 1.0f;

			mImporter.generateSecondaryUV = false;

			mImporter.generateAnimations = ModelImporterGenerateAnimations.None;

			mImporter.normalImportMode = ModelImporterTangentSpaceMode.Import;

			mImporter.tangentImportMode = ModelImporterTangentSpaceMode.Calculate;

			mImporter.meshCompression = ModelImporterMeshCompression.Off;

			mImporter.optimizeMesh = true;

			mImporter.animationType = ModelImporterAnimationType.None;

			mImporter.isReadable = false;
		}
	}

	void OnPostprocessModel(GameObject go)
	{
		if (assetPath.StartsWith(k_armor_path) && !assetPath.StartsWith(_ignorePath))
		{
			string matPath = assetPath.ToLower().Replace(".fbx", ".mat");
			matPath = matPath.Replace("_left", "");
			matPath = matPath.Replace("_right", "");
			Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
			if (mat == null)
			{
				Shader shader = Shader.Find("InJoy/character/common");
				if (assetPath.Contains("_high"))
				{
					shader = Shader.Find("InJoy/character/preview");
				}
				mat = new Material(shader);
				AssetDatabase.CreateAsset(mat, matPath);
				mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
				string texPath = matPath.ToLower().Replace(".mat", ".tga");
				Texture2D tex = AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D)) as Texture2D;
				mat.mainTexture = tex;
			}
			Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
			{
				r.sharedMaterial = mat;
			}
		}
	}

	void OnPreprocessTexture()
	{
		if (assetPath.StartsWith(k_armor_path) && !assetPath.StartsWith(_ignorePath))
		{
			TextureImporter timporter = assetImporter as TextureImporter;
			timporter.mipmapEnabled = false;
			if (assetPath.Contains("/UI_"))
			{
				timporter.textureFormat = TextureImporterFormat.AutomaticCompressed;

				timporter.alphaIsTransparency = true;
			}
			else
			{
				timporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
			}
		}
	}
}
