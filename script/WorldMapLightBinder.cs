using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WorldMapLightBinder : MonoBehaviour {

	public Transform _light;
	
	// Update is called once per frame
	void Update () {
		if(_light != null) {
			Shader.SetGlobalVector("_MainLight_Worldmap", _light.position);
		}
	}
}
