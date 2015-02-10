using UnityEngine;
using System.Collections;

public class ConeLightRenderer : MonoBehaviour {

	public Material _material;
	public Renderer []_renderers;
	
	Material _matInst;
	Transform _myTransform;
	// Use this for initialization
	void Start () {
		if(!GameSettings.Instance.IsDeferredShadingActived()) {
			enabled = false;
			return;
		}
		_matInst = Utils.CloneMaterial(_material);
		foreach(Renderer r in _renderers) {
			r.sharedMaterial = _matInst;
		}
		_myTransform = gameObject.transform;
		_matInst.SetFloat("_LightRange", _myTransform.localScale.x);
	}
	
	// Update is called once per frame
	void OnWillRenderObject() {
		Matrix4x4 inverseProjMtx = DeferredShadingRenderer.InverseProjectMatrix;
		Matrix4x4 cameraMtx = DeferredShadingRenderer.CameraMatrix;
		cameraMtx = cameraMtx * _myTransform.localToWorldMatrix;
		_matInst.SetMatrix("_ProjectToCamera", inverseProjMtx);
		Vector3 p = cameraMtx.MultiplyPoint(Vector3.up);
		_matInst.SetVector("_SourcePos", new Vector4(p.x, p.y, p.z, 1.0f));
		Vector3 d = cameraMtx.MultiplyVector(Vector3.down).normalized;
		_matInst.SetVector("_SourceDir", new Vector4(d.x, d.y, d.z, 1.0f));
		Vector3 s = _myTransform.localScale;
		s.z = 0.0f;
		float length = s.magnitude;
		_matInst.SetVector("_LightRange", new Vector4(s.y / length, length, 0.0f, 0.0f));
		
	}
}
