#define UILABEL_PREFAB
using UnityEditor;
using UnityEngine;
using System.Collections;

public class IDontGiveDamn : ScriptableWizard {
	
	[MenuItem("Tools/Character/UI shit %#u")]
	static void CreateWeapon() {
		ScriptableWizard.DisplayWizard("I don't give damn", typeof(IDontGiveDamn), "Run");
	}
	
	[System.Serializable]
	public class SpritePair
	{
		public UIAtlas oldAtlas;
		public string oldSpriteName;
		public UIAtlas newAtlas;
		public string newSpriteName;
	};
	
	public Object []processedObject;
	public SpritePair []replacedSprite;
	
#if UISPRITE_PREFAB
	void OnWizardCreate() {
		foreach(Object obj in processedObject) {
			string path = AssetDatabase.GetAssetPath(obj);
			if(System.IO.Directory.Exists(path)) {
				string []pathes = System.IO.Directory.GetFiles(path);
				foreach(string p in pathes) {
					ProcessPrefab(p);
				}
			}
			else {
				ProcessPrefab(path);
			}
		}
	}
#endif

#if UILABEL_PREFAB
	void OnWizardCreate() {
		foreach(Object obj in processedObject) {
			string path = AssetDatabase.GetAssetPath(obj);
			if(System.IO.Directory.Exists(path)) {
				string []pathes = System.IO.Directory.GetFiles(path);
				foreach(string p in pathes) {
					ProcessPrefab(p);
				}
			}
			else {
				ProcessPrefab(path);
			}
		}
	}
#endif

#if UISPRITE_OBJECT
	void OnWizardCreate() {
		foreach(Object obj in processedObject) {
			ProcessObject(obj as GameObject);
		}
	}
#endif
	
	void ProcessPrefab(string path)
	{
		if(path.EndsWith(".prefab") && System.IO.File.Exists(path)) {
			Debug.Log("processing " + path + "...");
			GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
			bool modified = ScanLabel(prefab.transform);
			if(modified) {
				Debug.Log ("saving prefab " + prefab.name + "...");
				EditorUtility.SetDirty(prefab);
			}
		}
	}
	
	void ProcessObject(GameObject go)
	{
		Debug.Log("start scanning " + go.name + "...");
		ScanSprite(go.transform);
	}
	
	bool ScanLabel(Transform t)
	{
		bool ret = false;
		UILabel label = t.gameObject.GetComponent<UILabel>();
		if(label != null) {
			Debug.Log("checking label " + label.gameObject.name +  "...");
			ret = CheckLabel(label);
		}
		foreach(Transform ct in t) {
			ret = ScanLabel(ct) || ret;
		}
		return ret;
	}
	
	bool CheckLabel(UILabel label)
	{
		if(label.shrinkToFit) {
			label.maxLineCount = 0;
			label.lineHeight = Mathf.RoundToInt(label.gameObject.transform.localScale.y);
			return true;
		}
		return false;
	}
	
	bool ScanSprite(Transform t)
	{
		bool ret = false;
		UISprite sprite = t.gameObject.GetComponent<UISprite>();
		if(sprite != null) {
			Debug.Log("checking sprite " + sprite.gameObject.name +  "...");
			ret = CheckSprite(sprite);
		}
		foreach(Transform ct in t) {
			ret = ScanSprite(ct) || ret;
		}
		return ret;
	}
	
	bool CheckSprite(UISprite sprite)
	{
		foreach(SpritePair sp in replacedSprite) {
			if(sp.oldAtlas == sprite.atlas && sp.oldSpriteName == sprite.spriteName) {
				sprite.atlas = sp.newAtlas;
				sprite.spriteName = sp.newSpriteName;
				Debug.Log("replace " + sprite.gameObject.name + "\'s atlas to " + sprite.atlas.name + ", sprite to " + sprite.spriteName);
				return true;
			}
		}
		return false;
	}
}
