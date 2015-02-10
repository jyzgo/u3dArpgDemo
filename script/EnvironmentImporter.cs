using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

public class EnvironmentImporter : AssetPostprocessor
{
	private const string k_envPath = "Assets/Levels/";
	private const string k_envFbxPath = "Raw";
	private const string k_collisionPrefix = "CLS_";
	private const string k_staticObjPath = "Assets/Levels/StaticObjects/";

	void OnPreprocessModel()
	{
		// for level models.
		if (assetPath.StartsWith(k_envPath))
		{
			// leagel path should be: Assets/Levels/XXX/Raw/XXX.fbx
			string[] pathPitches = assetPath.Split('/');
			if (pathPitches.Length != 5 || pathPitches[3] != k_envFbxPath)
			{
				//Debug.LogWarning("fbx exception:\"" + assetPath + "\" put in wrong location.");
				return;
			}

			ModelImporter mImporter = assetImporter as ModelImporter;
			mImporter.importMaterials = false;
			mImporter.globalScale = 1.0f;
			mImporter.optimizeMesh = true;
			mImporter.isReadable = true;

			mImporter.generateSecondaryUV = true;
			mImporter.swapUVChannels = false;

			if (assetPath.Contains("_anim") || assetPath.StartsWith(k_staticObjPath) && !assetPath.Contains("_static"))   //static objects need legacy animations
			{
				mImporter.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
				mImporter.animationType = ModelImporterAnimationType.Legacy;
			}
			else
			{
				mImporter.generateAnimations = ModelImporterGenerateAnimations.None;
				mImporter.animationType = ModelImporterAnimationType.None;
			}

			mImporter.normalImportMode = ModelImporterTangentSpaceMode.Import;
			mImporter.tangentImportMode = ModelImporterTangentSpaceMode.None;
		}
	}

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
	{
		foreach (string assetName in importedAssets)
		{
			if (assetName.StartsWith(k_envPath) && assetName.ToLower().EndsWith(".fbx"))
			{
				string[] pathPitches = assetName.Split('/');

				// expected material.
				string[] filePitches = pathPitches[4].Split('_');
				Material newMat = null;
				if (filePitches.Length >= 3)
				{
					string newMatPath = pathPitches[0] + "/" + pathPitches[1] + "/" + pathPitches[2] + "/" + pathPitches[3] + "/Materials/" + filePitches[2] + ".mat";
					newMat = AssetDatabase.LoadAssetAtPath(newMatPath, typeof(Material)) as Material;
					if (newMat == null)
					{
						Debug.LogWarning("fbx exception:\"" + newMatPath + "\" does not exist. A new one will be created");
						newMat = new Material(Shader.Find("Diffuse"));
						string newMatDir = System.IO.Path.GetDirectoryName(newMatPath);
						if (!System.IO.Directory.Exists(newMatDir))
						{
							System.IO.Directory.CreateDirectory(newMatDir);
						}
						AssetDatabase.CreateAsset(newMat, newMatPath);
						newMat = AssetDatabase.LoadAssetAtPath(newMatPath, typeof(Material)) as Material;
					}
				}
				if (newMat == null)
				{
					Debug.LogWarning("fbx exception:\"" + assetName + "\" can not get material from name.");
				}

				//create prefabs
				string ext = assetName.Substring(assetName.LastIndexOf('.'));
				string prefabPath = assetName.Replace(k_envFbxPath, "Prefabs").Replace(ext, ".prefab");

				if (!File.Exists(prefabPath))
				{
					GameObject go = AssetDatabase.LoadAssetAtPath(assetName, typeof(GameObject)) as GameObject;

					go = GameObject.Instantiate(go) as GameObject;

					go.transform.localScale = Vector3.one;

					// set material and add mesh colliders.
					Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
					foreach (Renderer r in renderers)
					{
						GameObject subObj = r.gameObject;
						if (subObj.name.Contains(k_collisionPrefix))
						{
							GameObject.DestroyImmediate(r);
							subObj.AddComponent<MeshCollider>();
							subObj.layer = FCConst.LAYER_WALL;
						}
						else
						{
							if (newMat != null)
							{
								r.sharedMaterial = newMat;
							}
						}
					}

					if (!(assetName.Contains("_anim") ||
						(assetName.StartsWith(k_staticObjPath) && !assetName.Contains("_static")
						))) //skip models with animations, they are non-static and animations should be kept
					{
						// remove animation components.
						Animation[] anims = go.GetComponentsInChildren<Animation>();
						foreach (Animation a in anims)
						{
							GameObject.DestroyImmediate(a);
						}

						// static flag settings.
						Transform[] transforms = go.GetComponentsInChildren<Transform>();
						foreach (Transform t in transforms)
						{
							if (t.name.Contains(k_collisionPrefix))
							{
								GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.NavigationStatic);
							}
							else
							{
								GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.LightmapStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
							}
						}
					}

					PrefabUtility.CreatePrefab(prefabPath, go);

					GameObject.DestroyImmediate(go);
				}
			}
		}
	}
}
