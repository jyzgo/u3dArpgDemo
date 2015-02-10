#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;

public class DynamicAssetLoader : ScriptableObject {
	
	public string []_assets;
	
	public void LoadAllAssets(Transform parent)
	{
		foreach(string asset in _assets) {
			GameObject go = InJoy.AssetBundles.AssetBundles.Load(asset, typeof(GameObject)) as GameObject;
			if(go != null) {
				GameObject inst = GameObject.Instantiate(go) as GameObject;
				Assertion.Check(inst != null);
				Transform t = inst.transform;
				t.parent = parent;
				t.localScale = Vector3.one;
				t.localRotation = Quaternion.identity;
				t.localPosition = Vector3.zero;
			}
		}
	}

#if UNITY_EDITOR
	[MenuItem("Tools/DynAsset/Create DynamicAssetLoader")]
	public static void CreateInstance() {
		DynamicAssetLoader dal = ScriptableObject.CreateInstance<DynamicAssetLoader>();
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if(path.Length == 0)
		{
			path = "Assets/";
		}
		path = AssetDatabase.GenerateUniqueAssetPath(path+"/newDynamicAssetLoader.asset");
		AssetDatabase.CreateAsset(dal,path);
	}
#endif
}
