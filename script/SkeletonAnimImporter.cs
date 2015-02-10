using UnityEditor;
using UnityEngine;
using System.Collections;

public class SkeletonAnimImporter : AssetPostprocessor {

	static string _rootDirectory = "Assets/Characters/Animations/";
	static string _BackupDirectory = "Assets/Editor/Animation/Backup/";
	
	// constant vars.
	static string animStart = "    - name: ";
	static string curveStart = "      curves:";
	static string []curveDataStart = {"      -", "       "};
	
	static bool _isProcessEnabled = false;

	public static bool IsPathAvailable()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		return path.Length != 0 && path.StartsWith(_rootDirectory) && path.EndsWith(".fbx");
	}

    [MenuItem("Tools/Character/Split Animation")]
	public static void SplitAnimation() {
		_isProcessEnabled = true;
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		AssetDatabase.Refresh();
		_isProcessEnabled = false;
	}

    [MenuItem("Tools/Character/Restore Animation")]
	public static void RestoreAnimation() {
		AssetDatabase.SaveAssets();
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		{
			string f = path.Replace(_rootDirectory, _BackupDirectory).Replace(".fbx", ".asset");
			string fbxfilePath = f.Replace(_BackupDirectory, _rootDirectory).Replace(".asset", ".fbx.meta");
			if(System.IO.File.Exists(fbxfilePath)) {
				using( System.IO.StreamReader reader = new System.IO.StreamReader(fbxfilePath)) {
					string line = reader.ReadLine();
					AnimBackup ab = AssetDatabase.LoadAssetAtPath(f, typeof(AnimBackup)) as AnimBackup;
					string name = null;
					while(line != null) {
						if(name == null) { // not enter animation config yet.
							if(line.StartsWith(animStart)) {
								// find an animtion.
								name = line.Substring(animStart.Length);
							}
							sb.Append(line + '\n');
						}
						else {
							string content = line + '\n';
							if(line.StartsWith(curveStart)) {
								string c = ab.GetAnimationCurveData(name);
								if(c != null) {
									content = c;
								}
								name = null;
							}
							sb.Append(content);
						}
						line = reader.ReadLine();
					}
				}
				using(System.IO.StreamWriter writer = new System.IO.StreamWriter(fbxfilePath)) {
					writer.Write(sb.ToString());
				}
				sb.Remove(0,sb.Length);
			}
		}
		AssetDatabase.Refresh();
	}
	
	void OnPreprocessModel()
	{
		if(!_isProcessEnabled) {
			return;
		}
		// for skeleton animations.
		if(assetPath.StartsWith(_rootDirectory))
		{			
			ModelImporter mImporter = assetImporter as ModelImporter;
			// basic settings.
			mImporter.importMaterials = false;
			mImporter.globalScale = 1.0f;		
			mImporter.addCollider = false;
			mImporter.generateSecondaryUV = false;
			mImporter.normalImportMode = ModelImporterTangentSpaceMode.None;
			mImporter.tangentImportMode = ModelImporterTangentSpaceMode.None;
			// rig settings.
			mImporter.animationType = ModelImporterAnimationType.Generic;
			// animation settings.
			System.Collections.Generic.List<string> anims = new System.Collections.Generic.List<string>();
			using(System.IO.StreamReader reader = new System.IO.StreamReader(assetPath))
			{
				string line;
				bool readAfter = false;
				while((line = reader.ReadLine()) != null) {
					if(readAfter)
					{
						if(!line.Contains("_ign"))
						{
							anims.Add(line);
						}
					}
					if(line == ";PawaRangerv1.0")
					{
						reader.ReadLine();
						reader.ReadLine();
						readAfter = true;
					}
				}
			}
			
			// found animation clip
			if(anims.Count > 0) {
				ModelImporterClipAnimation []newClipAnims = new ModelImporterClipAnimation[anims.Count];
				
				int count = 0;
				
				foreach(string a in anims)
				{
					string []info = a.Split(' ', '-'); // format is ';AnimationName start-end'
					string s = info[0].Substring(1);
					newClipAnims[count] = new ModelImporterClipAnimation();
					newClipAnims[count].name = s; // animation name.
					newClipAnims[count].firstFrame = int.Parse(info[1]); // start time
					newClipAnims[count].lastFrame = int.Parse(info[2]); // end time
					newClipAnims[count].loopPose = (s.Contains("_loop")); // loop flag
					// rotation lock
					newClipAnims[count].lockRootRotation = true;
					newClipAnims[count].keepOriginalOrientation = true;
					newClipAnims[count].rotationOffset = 0.0f;
					// height lock
					newClipAnims[count].lockRootHeightY = true;
					newClipAnims[count].keepOriginalPositionY = true;
					newClipAnims[count].heightOffset = 0.0f;
					// position lock
					newClipAnims[count].lockRootPositionXZ = false;
					newClipAnims[count].cycleOffset = 0.0f;
					++count;
				}
				mImporter.clipAnimations = newClipAnims;
			}
		}
	}

    [MenuItem("Tools/Character/Backup Animation")]
	public static void BackupAnimation() {
		
		AssetDatabase.SaveAssets();
		string []files = System.IO.Directory.GetFiles(_rootDirectory);
		foreach(string f in files) {
			// look for meta file of animation file to get curver information.
			if(f.EndsWith(".fbx.meta")) {
				System.Collections.Generic.List<AnimBackup.AnimBackupData> curveData = 
												new System.Collections.Generic.List<AnimBackup.AnimBackupData>();
				// read every lines to find animation.
				using( System.IO.StreamReader reader = new System.IO.StreamReader(f)) {
					string line = reader.ReadLine();
					AnimBackup.AnimBackupData data = null;
					bool reachCurve = false;
					while(line != null) {
						if(data == null) {
							// find animation beginning.
							if(line.StartsWith(animStart)) {
								data = new AnimBackup.AnimBackupData();
								data._name = line.Substring(animStart.Length);
							}
						}
						else {
							if(reachCurve) {
								bool written = false;
								foreach(string v in curveDataStart) {
									if(line.StartsWith(v)) {
										// write data.
										data._curve += line + '\n';
										written = true;
										break;
									}
								}
								// reach end of curve data, break.
								if(!written) {
									curveData.Add(data);
									reachCurve = false;
									data = null;
								}
							}
							else {
								// found curve data start.
								if(line.StartsWith(curveStart)) {
									reachCurve = true;
									data._curve = line + '\n';
								}
							}
						}
						line = reader.ReadLine();
					}
				}
				// save backup.
				AnimBackup ab = ScriptableObject.CreateInstance<AnimBackup>();
				ab._data = curveData.ToArray();
				AssetDatabase.CreateAsset(ab, _BackupDirectory + f.Replace(_rootDirectory, "").Replace("fbx.meta", "asset"));
			}
		}
		AssetDatabase.Refresh();
	}
	
}