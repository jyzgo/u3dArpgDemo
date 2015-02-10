using UnityEngine;
using System.Collections;

public class GBufferRenderer : MonoBehaviour {
	
	Camera _coreCamera;	
	public Camera _referenceCamera;
	
	void Awake() {
		_coreCamera = GetComponent<Camera>();
	}
	
	void OnPreRender() {
		_coreCamera.fieldOfView = _referenceCamera.fieldOfView;
		_coreCamera.nearClipPlane = _referenceCamera.nearClipPlane;
		_coreCamera.farClipPlane = _referenceCamera.farClipPlane;
		
		DeferredShadingRenderer.InverseProjectMatrix = _referenceCamera.projectionMatrix.inverse;
		DeferredShadingRenderer.CameraMatrix = _referenceCamera.worldToCameraMatrix;
	}
}
