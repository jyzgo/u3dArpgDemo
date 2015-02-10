using UnityEngine;
using System.Collections;

public class MaterialManager : MonoBehaviour {

	#region singleton defines	
	static MaterialManager _inst;
	static public MaterialManager Instance {
		get {
			return _inst;
		}
	}
	
	void Awake() {
		if(_inst != null)
		{
			Debug.LogError(_inst.GetType().ToString() + ": detected singleton instance has existed. About to destroy this one " + gameObject.name);
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
	#endregion
	
	public Material _deadMat;

}
