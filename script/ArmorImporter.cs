using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class ArmorImporter : AssetPostprocessor
{
	private const string k_armor_path = "Assets/Armor/";
	private const string k_armor_ignore_path = "Assets/Armor/Common/";

	void OnPreprocessModel()
	{
		if (assetPath.StartsWith(k_armor_path) && !assetPath.StartsWith(k_armor_ignore_path))
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
			mImporter.swapUVChannels = false;
			mImporter.animationType = ModelImporterAnimationType.None;

			mImporter.isReadable = false;
		}
	}

	void OnPreprocessTexture()
	{
		if (assetPath.StartsWith(k_armor_path) && !assetPath.StartsWith(k_armor_ignore_path))
		{
			string[] pieces = assetPath.Split('/');
			if (pieces.Length < 5)
			{
				Debug.LogError(assetPath + "must be placed in wrong position.");
			}
			TextureImporter timporter = assetImporter as TextureImporter;
			if (pieces[pieces.Length - 1].StartsWith("UI_"))
			{
				// ui icon texture.
				timporter.isReadable = false;
				timporter.mipmapEnabled = false;
				timporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
				timporter.alphaIsTransparency = true;
			}
			else
			{
				// model textures.
				timporter.isReadable = false;
				timporter.mipmapEnabled = false;
				timporter.maxTextureSize = 1024;
				if (pieces[pieces.Length - 1].Contains("_normal") || pieces[pieces.Length - 1].Contains("_mid"))
				{
					timporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
				}
				else
				{
					timporter.textureFormat = TextureImporterFormat.Automatic16bit;
				}
			}
		}
	}

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
	{
		foreach (string asset in importedAssets)
		{
			if (asset.StartsWith(k_armor_path) && !asset.StartsWith(k_armor_ignore_path) && asset.EndsWith(".mat"))
			{
				Material matObj = AssetDatabase.LoadAssetAtPath(asset, typeof(Material)) as Material;
				matObj.mainTexture = null;
				EditorUtility.SetDirty(matObj);
			}
		}
	}

	[MenuItem("Tools/Equipment/Create a set of prefabs for an armor texture")]
	static void GeneratePrefabsByTexture()
	{
		Texture2D tex = Selection.activeObject as Texture2D;

		string texFileName = AssetDatabase.GetAssetPath(tex); Debug.Log("ArmorImporter.GeneratePrefabsByTexture\t\tTexture file: " + texFileName);

		if (!texFileName.StartsWith(k_armor_path) || tex == null)
		{
			EditorUtility.DisplayDialog("Warning", "Please choose a texture in the armor folder.\n\nFor example:\n\n\tArmor/Mage_001/Suit_A/xxx.tga", "OK");
			return;
		}

		//search for any prefabs under the folder
		string[] files = Directory.GetFiles(Path.GetDirectoryName(texFileName));

		if (files.Length > 1)
		{
			if (EditorUtility.DisplayDialog("Warning", "All other files in the folder will be removed!\n\nAre you sure?", "Yes", "No"))
			{
				//remove other files
				foreach (string filename in files)
				{
					string actualFileName = filename.Replace('\\', '/');

					if (actualFileName.EndsWith(".prefab") || actualFileName.EndsWith(".prefab.meta"))
					{
						File.Delete(filename);
						Debug.LogWarning("ArmorImporter.GeneratePrefabsByTexture\t\tDeleted: " + actualFileName);
					}
				}
			}
			else
			{
				return;
			}
		}

		//create a new material
		string matPath = Path.ChangeExtension(texFileName, ".mat");

		Material newMat = new Material(Shader.Find("Diffuse"));

		newMat.mainTexture = tex;

		AssetDatabase.CreateAsset(newMat, matPath);

		Debug.Log("ArmorImporter.GeneratePrefabsByTexture\t\tMaterial created for texture at " + matPath);

		//create prefabs
		string parentDir = Path.GetDirectoryName(texFileName);

		string subFolder = parentDir.Substring(parentDir.LastIndexOf('/'));

		parentDir = parentDir.Substring(0, parentDir.LastIndexOf('/'));

		files = Directory.GetFiles(parentDir, "*.fbx");

		foreach (string filename in files)
		{
			GameObject go = AssetDatabase.LoadMainAssetAtPath(filename) as GameObject;

			go = GameObject.Instantiate(go) as GameObject;

			go.hideFlags = HideFlags.DontSave;

			go.GetComponentsInChildren<MeshRenderer>()[0].sharedMaterial = newMat;  //suppose each fbx has only one mesh renderer

			string actualFileName = filename.Replace('\\', '/');

			string prefabName = actualFileName.Substring(0, actualFileName.LastIndexOf('/')) + subFolder + '/' + Path.GetFileNameWithoutExtension(actualFileName) + ".prefab";

			// add mesh unloader
			if (filename.Contains("_high."))
			{
				System.Collections.Generic.List<Mesh> meshes = new System.Collections.Generic.List<Mesh>();
				MeshUnloader mu = go.AddComponent<MeshUnloader>();
				SkinnedMeshRenderer[] srenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach (SkinnedMeshRenderer smr in srenderers)
				{
					meshes.Add(smr.sharedMesh);
				}
				MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
				foreach (MeshFilter mf in filters)
				{
					meshes.Add(mf.sharedMesh);
				}
				mu._meshes = meshes.ToArray();
			}

			PrefabUtility.CreatePrefab(prefabName, go);

			GameObject.DestroyImmediate(go);
		}

		AssetDatabase.Refresh();

		Debug.Log("ArmorImporter.GeneratePrefabsByTexture\t\tPrefabs for armor suit created at " + parentDir + '/' + subFolder);
	}

}
