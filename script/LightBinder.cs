using UnityEngine;
using System.Collections;

public class LightBinder : MonoBehaviour {

	public Light _pointLight1;
	public Light _pointLight2;
	public Color _ambient;
	public float _gloss;
	public bool _shadowEnable;
	public Transform _shadowPlane;
	public Transform _shadowLight;
	public Camera _shadowCam;
	public Shader _shadowShader;
	public Color _shadowColor;
	
	void OnEnable() {
		Shader.SetGlobalVector("preview_LightPos", _pointLight1.transform.position);
		Shader.SetGlobalColor("preview_LightColor", _pointLight1.color);
		Shader.SetGlobalFloat("preview_LightGloss", _gloss);
		
		Shader.SetGlobalVector("preview_LightPos2", _pointLight2.transform.position);
		Shader.SetGlobalColor("preview_LightColor2", _pointLight2.color);
		Shader.SetGlobalFloat("preview_LightGloss2", _gloss);
		
		Shader.SetGlobalColor("preview_AmbientColor", _ambient);
		if(_shadowEnable) {
			_shadowCam.SetReplacementShader(_shadowShader, "");
			Matrix4x4 shadowmatrix = ShadowRenderer.UpdateShadowMatrix(_shadowPlane.up, _shadowPlane.position.y, _shadowLight.position, 1.0f);
			Shader.SetGlobalMatrix("preview_shadowmat", shadowmatrix);
			Shader.SetGlobalColor("preview_shadowcolor", _shadowColor);
		}
	}
	
#if UNITY_EDITOR	
	void Update() {
		Shader.SetGlobalVector("preview_LightPos", _pointLight1.transform.position);
		Shader.SetGlobalColor("preview_LightColor", _pointLight1.color);
		Shader.SetGlobalFloat("preview_LightGloss", _gloss);
		
		Shader.SetGlobalVector("preview_LightPos2", _pointLight2.transform.position);
		Shader.SetGlobalColor("preview_LightColor2", _pointLight2.color);
		Shader.SetGlobalFloat("preview_LightGloss2", _gloss);
		
		Shader.SetGlobalColor("preview_AmbientColor", _ambient);
		
		if(_shadowEnable) {
			Matrix4x4 shadowmatrix = ShadowRenderer.UpdateShadowMatrix(_shadowPlane.up, _shadowPlane.position.y, _shadowLight.position, 1.0f);
			Shader.SetGlobalMatrix("preview_shadowmat", shadowmatrix);
			Shader.SetGlobalColor("preview_shadowcolor", _shadowColor);
		}
	}
#endif
}
