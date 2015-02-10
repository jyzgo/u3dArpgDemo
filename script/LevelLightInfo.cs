using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LevelLightInfo : MonoBehaviour {
	
	public Color _spotLightColor = Color.gray;
	public float _characterLighting = 0;
	public float _characterStaticeLighting = 0;
	public Color _reflectionColor = Color.white;
	public FCCamera _completeCamera;
	
	public static FCCamera s_completeCamera = null;
	
	void OnDestroy() {
		s_completeCamera = null;
	}
	
	// Use this for initialization
	void Awake () {
		Shader.SetGlobalFloat("_LightingFactor", GameSettings.Instance.AllowDeferredShading ? _characterLighting : _characterLighting + 0.2f);
		Shader.SetGlobalColor("_spotLightColor", _spotLightColor);
		Shader.SetGlobalColor("_sceneReflectionColor", _reflectionColor);
		
		s_completeCamera = _completeCamera;
	}
	
#if UNITY_EDITOR
	// Update is called once per frame
	void Update () {
		Shader.SetGlobalFloat("_LightingFactor", GameSettings.Instance.AllowDeferredShading ? _characterLighting : _characterLighting + 0.2f);	
		Shader.SetGlobalColor("_spotLightColor", _spotLightColor);
		Shader.SetGlobalColor("_sceneReflectionColor", _reflectionColor);
	}
#endif
}
