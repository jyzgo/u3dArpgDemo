using UnityEngine;
using System.Collections;

public class LBufferRenderer : MonoBehaviour {

	Camera _coreCamera;	
	public Camera _referenceCamera;
	public Material _mergeLightMat;
	public Material _clearMat;
	
	void Awake() {
		_coreCamera = GetComponent<Camera>();
		_coreCamera.backgroundColor = new Color32(0,0,0,0);
	}
	
	void OnPreRender() {
        _coreCamera.fieldOfView = _referenceCamera.fieldOfView;
		_coreCamera.nearClipPlane = _referenceCamera.nearClipPlane;
		_coreCamera.farClipPlane = _referenceCamera.farClipPlane;
		//Graphics.Blit(null, _clearMat);
	}
	
	void OnPostRender() {
		Graphics.Blit(DeferredShadingRenderer.TBuffer, DeferredShadingRenderer.FBuffer, _mergeLightMat);
	}
}
