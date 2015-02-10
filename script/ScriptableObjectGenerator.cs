using UnityEditor;
using UnityEngine;
using System.Collections;

public class ScriptableObjectGenerator {

	public static T CreateScriptableObject<T>() where T : ScriptableObject
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if(path.Length == 0)
		{
			Debug.LogError("didn't select a folder from \'Project View\'");
			return null;
		}
		
		if(System.IO.File.Exists(path))
		{
			path = System.IO.Path.GetDirectoryName(path);
		}
		
		Debug.Log("Scriptable Object path: " + path);
		path = AssetDatabase.GenerateUniqueAssetPath(path + "/new" + typeof(T).Name + ".asset");
		T newObject = ScriptableObject.CreateInstance<T>();
		AssetDatabase.CreateAsset(newObject, path);
		return newObject;
	}
	
	public static bool IsPathAvailable()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		return path.Length != 0;
	}
	
	[MenuItem("Tools/Create Data Asset/Skeleton Anim Config")]
	public static void CreateAnimList() {
		CreateScriptableObject<SkeletonLib>();
	}

	[MenuItem("Tools/Create Data Asset/Character Table")]
	public static void CreateCharacterTable() {
		CreateScriptableObject<CharacterTable>();
	}

	[MenuItem("Tools/Create Data Asset/Level Config")]
	public static void CreateLevelConfig() {
		CreateScriptableObject<LevelConfig>();
	}

	[MenuItem("Tools/Create Data Asset/Bundles Config")]
	public static void CreateBundlesConfig() {
		CreateScriptableObject<BundlesConfig>();
	}
}
