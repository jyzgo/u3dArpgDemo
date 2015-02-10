using UnityEngine;
using System.Collections;


// singleton, only 1 instance exists in game's life time.
public class CharacterFactory : MonoBehaviour {
	
	static CharacterFactory _inst;
	static public CharacterFactory Singleton {
		get {
			return _inst;
		}
	}
	
	void Awake() {
		if(_inst != null)
		{
			Debug.LogError("CharacterFactory: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}
		
		_inst = this;
	}
	
	void OnDestroy() {
		if(_inst == this)
		{
			_inst = null;
		}
	}
	
	public GameObject AssembleCharacter(string path) {
		GameObject ret = null;
		GameObject modelRes = InJoy.AssetBundles.AssetBundles.Load(path) as GameObject;
		if(modelRes != null)
		{
			ret = GameObject.Instantiate(modelRes, _characterBornPoint, Quaternion.identity) as GameObject;
		}
		return ret;
	}
	
	private Vector3 _characterBornPoint;
	public Vector3 BornPoint {
		set {_characterBornPoint = value;}
	}
}
