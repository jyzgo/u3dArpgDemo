using UnityEngine;
using System.Collections;

public class MainLightBinder : MonoBehaviour {
	
	public Light _mainLight;

	void OnEnable() {
		Transform t = _mainLight.transform;
		Shader.SetGlobalVector("_MainLightDir", t.forward);
	}
	
	// Update is called once per frame
	void Update () {
		Transform t = _mainLight.transform;
		Shader.SetGlobalVector("_MainLightDir", t.forward);
		Shader.SetGlobalColor("_MainLightColor", _mainLight.color);
	}
}
