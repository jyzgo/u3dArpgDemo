using UnityEngine;
using System.Collections;

public class RimLightRenderer : MonoBehaviour {
	
	Camera _coreCamera;
	public Camera _referenceCamera;
	public Shader _replacementShader;
	public Vector4 _rimFactor;
	
	// Use this for initialization
	void Awake () {
		_coreCamera = GetComponent<Camera>();
		if(_replacementShader != null) {
			_coreCamera.SetReplacementShader(_replacementShader, "");
		}
	}
	
	
	RenderBuffer _activedColorBuffer;
	RenderBuffer _activedDepthBuffer;
	void OnPreRender () {
        _coreCamera.fieldOfView = _referenceCamera.fieldOfView;
		_coreCamera.nearClipPlane = _referenceCamera.nearClipPlane;
		_coreCamera.farClipPlane = _referenceCamera.farClipPlane;
		
		Shader.SetGlobalVector("_RimFactor", _rimFactor);
		
		_activedColorBuffer = Graphics.activeColorBuffer;
		_activedDepthBuffer = Graphics.activeDepthBuffer;
		Graphics.SetRenderTarget(DeferredShadingRenderer.FBuffer);
	}
	
	void OnPostRender() {
		Graphics.SetRenderTarget(_activedColorBuffer, _activedDepthBuffer);
	}
}
