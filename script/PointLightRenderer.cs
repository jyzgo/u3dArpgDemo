using UnityEngine;
using System.Collections;

public class PointLightRenderer : MonoBehaviour {
	
	public Material _material;
	public Color _lightColor;
	public Renderer []_renderers;
	public float _radiusFactor = 1.0f;
	public bool _generateShadow = true;
	
	Material _matInst;
	Transform _myTransform;
	// Use this for initialization
	void Awake () {
		_myTransform = gameObject.transform;
	}
	
	void Start () {
		if(!GameSettings.Instance.IsDeferredShadingActived()) {
			enabled = false;
			return;
		}
		_matInst = Utils.CloneMaterial(_material);
		_matInst.SetColor("_LightColor", _lightColor);
		foreach(Renderer r in _renderers) {
			r.sharedMaterial = _matInst;
		}
		
		_matInst.SetFloat("_LightRange", _myTransform.localScale.x);
	}
	
	void OnEnable() {
		if(_generateShadow) {
			CameraController.Instance.AddPointLight(_myTransform);
		}
	}
	
	void OnDisable() {
		if(CameraController.Instance != null && _generateShadow) {
			CameraController.Instance.RemovePointLight(_myTransform);
		}
	}
	
	// Update is called once per frame
	void OnWillRenderObject() {
		_matInst.SetFloat("_LightRange", _myTransform.localScale.x * _radiusFactor);
		Matrix4x4 inverseProjMtx = DeferredShadingRenderer.InverseProjectMatrix;
		Matrix4x4 cameraMtx = DeferredShadingRenderer.CameraMatrix;
		_matInst.SetMatrix("_ProjectToCamera", inverseProjMtx);
		Vector3 p = cameraMtx.MultiplyPoint(gameObject.transform.position);
		_matInst.SetVector("_SourcePos", new Vector4(p.x, p.y, p.z, 1.0f));		
	}
}
