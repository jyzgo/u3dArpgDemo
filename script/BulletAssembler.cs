using UnityEngine;
using System.Collections;

public class BulletAssembler : MonoBehaviour {
	
	private static BulletAssembler _inst;
	public static BulletAssembler Singleton {
		get {return _inst;}
	}
	
	void Awake() {
		if(_inst != null) {
			Debug.LogError("Duplicated BulletAssembler.");
			Destroy(this);
			return;
		}
		_inst = this;
	}
	
	void OnDestroy() {
		if(_inst == this) {
			_inst = null;
		}
	}
	
	public string _bulletRootPath;
	
	public GameObject AssembleBullet(string label) {
		string path = _bulletRootPath + label + "/" + label + ".prefab";
		GameObject go = InJoy.AssetBundles.AssetBundles.Load(path) as GameObject;
		go = GameObject.Instantiate(go) as GameObject;
		return go;
	}
}
