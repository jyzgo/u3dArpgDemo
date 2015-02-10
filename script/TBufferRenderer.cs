using UnityEngine;
using System.Collections;

public class TBufferRenderer : MonoBehaviour {
	
	Camera _coreCamera;
	public Camera _referenceCamera;
	public Material _tBufferMat;
	public Shader _replacementShader;
	
	// Use this for initialization
	void Awake () {
		_coreCamera = GetComponent<Camera>();
		_coreCamera.backgroundColor = RenderSettings.fogColor;
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
	}
	
	void OnPostRender() {
		//Graphics.Blit(DeferredShadingRenderer.TBuffer, DeferredShadingRenderer.FBuffer, _tBufferMat);
		//Graphics.Blit(DeferredShadingRenderer.TBuffer, DeferredShadingRenderer.FBuffer);
		//Debug.Log ("TBuffer OnPostRender");
	}
}
