using UnityEditor;
using UnityEngine;
using System.Collections;

public class CharacterImporter : AssetPostprocessor {
	
	public static string _characterRoot = "Assets/Characters/";
	public static string []_ignorePath = new string[]{ // WARNING: this array should not be too long
		"Assets/Characters/Animations/"
	};
	
	public static string _heroRoot = "Assets/Characters/Heros/";
	public static string _commonRoot = "Assets/Characters/Common/";
	
	void OnPreprocessModel()
	{
		if(assetPath.StartsWith(_characterRoot)) {
			// filter ignored pathes
			foreach(string i in _ignorePath) {
				if(assetPath.StartsWith(i)) {
					return;
				}
			}
			// process 'common' folder
			if(assetPath.StartsWith(_commonRoot)) {
				ModelImporter mi = assetImporter as ModelImporter;
				mi.animationType = ModelImporterAnimationType.Legacy;
				mi.generateSecondaryUV = false;
				mi.importMaterials = false;
				mi.isReadable = false;
				mi.optimizeMesh = true;
				mi.globalScale = 1.0f;
				return;
			}
			// find folder name to determine core fbx file to process.
			string subfolder = assetPath.Substring(_characterRoot.Length);
			string []groups = subfolder.Split('/'); 
			// group[0] is the animation name
			// group[1] is the folder name, group[2] is fbx file name
			// group[2] should be group[1] appended string '.fbx'
			// the format should be Assets/Characters/[class]/XXX/XXX.fbx
			// other path is illegal
			
			// process atlas pieces model.
			// the fbx path name should be Assets/Characters/[class]/XXX/XXX_atlas.fbx
			if(groups[1] + "_atlas.fbx" == groups[2]) {
				ModelImporter mi = assetImporter as ModelImporter;
				// basic settings.
				mi.globalScale = 1.0f;
				mi.meshCompression = ModelImporterMeshCompression.Off;
				mi.addCollider = false;
				mi.isReadable = false;
				mi.optimizeMesh = true;
				mi.generateSecondaryUV = false;
				mi.normalImportMode = ModelImporterTangentSpaceMode.None;
				mi.tangentImportMode = ModelImporterTangentSpaceMode.None;
				mi.importMaterials = false;
				// rig settings.
				mi.animationType = ModelImporterAnimationType.None;
				// animation settings.
				mi.generateAnimations = ModelImporterGenerateAnimations.None;
				// create a material for this model if not existed.
				string matPath = assetPath.Replace(".fbx", ".mat");
				if(!System.IO.File.Exists(matPath)) {
					Material mat = new Material(Shader.Find("InJoy/editor/suit atlas"));
					AssetDatabase.CreateAsset(mat, matPath);
				}
			}
			// process fbx file with same folder name only.
			else if(groups[1] + ".fbx" == groups[2]
				|| groups[1] + "_high.fbx" == groups[2]
				|| groups[1] + "_low.fbx" == groups[2]) {
				ModelImporter mi = assetImporter as ModelImporter;
				// basic settings.
				mi.globalScale = 1.0f;
				mi.isReadable = false;
				mi.optimizeMesh = true;
				mi.generateSecondaryUV = true; // generate 2nd UV to enable VFP process
				mi.addCollider = false;
				mi.swapUVChannels = false;
				mi.normalImportMode = ModelImporterTangentSpaceMode.Import;
				mi.tangentImportMode = ModelImporterTangentSpaceMode.Calculate;
				mi.importMaterials = false;
				// rig settings.
				mi.animationType = ModelImporterAnimationType.Generic;
				// animation settings.
				mi.generateAnimations = ModelImporterGenerateAnimations.None;
			}
			else {
				ModelImporter mi = assetImporter as ModelImporter;
				// basic settings.
				mi.globalScale = 1.0f;
				mi.isReadable = false;
				mi.optimizeMesh = true;
				mi.generateSecondaryUV = false; // generate 2nd UV to enable VFP process
				mi.addCollider = false;
				mi.swapUVChannels = false;
				mi.normalImportMode = ModelImporterTangentSpaceMode.Import;
				mi.tangentImportMode = ModelImporterTangentSpaceMode.Calculate;
				mi.importMaterials = false;
				// rig settings.
				mi.animationType = ModelImporterAnimationType.Legacy;
				// animation settings.
			}
		}
	}
	
	void OnPostprocessModel(GameObject go) {
		if(assetPath.StartsWith(_characterRoot)) {
			// filter ignored pathes
			foreach(string i in _ignorePath) {
				if(assetPath.StartsWith(i)) {
					return;
				}
			}
			// filter common path.
			if(assetPath.StartsWith(_commonRoot)) {
				return;
			}
			// find folder name to determine core fbx file to process.
			string subfolder = assetPath.Substring(_characterRoot.Length);
			string []groups = subfolder.Split('/'); 
			// group[0] is the animation name
			// group[1] is the folder name, group[2] is fbx file name
			// group[2] should be group[1] appended string '.fbx'
			// the format should be Assets/Characters/[class]/XXX/XXX.fbx
			// other path is illegal
			if(groups[1] + ".fbx" == groups[2]) {
				// force set bones weight count to '2'
				SkinnedMeshRenderer []renderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
				foreach(SkinnedMeshRenderer r in renderers) {
					r.quality = SkinQuality.Bone2;
				}
				// check unused nodes.
				Transform []transforms = go.GetComponentsInChildren<Transform>();
				foreach(Transform t in transforms) {
					if(t.gameObject.name.EndsWith("Nub")) {
						Debug.LogError(t.gameObject.name + " should not existed in " + assetPath);
						GameObject.DestroyImmediate(t.gameObject);
					}
				}
			}
			if(groups[1] + "_atlas.fbx" == groups[2]) {
				// add material for atlas meshes.
				Material mat = AssetDatabase.LoadAssetAtPath(
					assetPath.Replace(".fbx", ".mat"), typeof(Material)) as Material;
				if(mat == null) {
					Debug.LogError(assetPath + ": can not find material for this atlas mesh");
					return;
				}
				// update material.
				Renderer []renderers = go.GetComponentsInChildren<Renderer>();
				foreach(Renderer r in renderers) {
					r.sharedMaterial = mat;
				}
				// bind camera
				Transform camera = go.transform.Find("camera");
				Transform up = go.transform.Find("camera/up");
				if(camera == null || up == null) {
					Debug.LogError(assetPath + ": can not find camera node!");
					return;
				}
				Camera cam = camera.gameObject.AddComponent<Camera>();
				cam.clearFlags = CameraClearFlags.Nothing;
				cam.cullingMask = 1 << LayerMask.NameToLayer("SCRIPT");
				cam.isOrthoGraphic = true;
				cam.orthographicSize = 0.5f;
				cam.nearClipPlane = 0.0f;
				cam.farClipPlane = 1.0f;
				cam.renderingPath = RenderingPath.Forward;
				cam.enabled = false; // disable at the beginning.
				// rotate camera to look at the pieces.
				camera.LookAt(go.transform, (up.position - camera.position).normalized);
				
				// set layer
				Transform []allnodes = go.GetComponentsInChildren<Transform>();
				foreach(Transform t in allnodes) {
					t.gameObject.layer = LayerMask.NameToLayer("SCRIPT");
				}
				
				// add material builder.
				go.AddComponent<ModifiableMaterialBuilder>();
			}
		}
	}
	
	void OnPreprocessTexture() {
		TextureImporter timporter = assetImporter as TextureImporter;
		if(assetPath.StartsWith(_characterRoot)) {
			if(!assetPath.StartsWith(_commonRoot)) {
				timporter.mipmapEnabled = false;
				timporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
			}
		}
	}
}
