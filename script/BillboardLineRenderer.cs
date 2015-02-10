using UnityEngine;
using System.Collections;

public class BillboardLineRenderer : MonoBehaviour {
	
	// billboard function
	Transform _myTransform;
	// uv patching function
	public Transform _referenceTransform;
	public Renderer []_renderers;
	public ParticleSystem []_particles;
	public float _scaleFactor = 1.0f;
	// TODO: bending function
	
	void Awake () {
		_myTransform = GetComponent<Transform>();
		foreach(Renderer r in _renderers) {
			r.sharedMaterial = Utils.CloneMaterial(r.sharedMaterial);
		}
	}
	
	public void SetColor(Color color) {
		foreach(Renderer r in _renderers) {
			r.sharedMaterial.SetColor("_MainColor", color);
		}
		
		foreach(ParticleSystem p in _particles) {
			p.startColor = color;
		}
	}
	
	void Update() {
		// billboard.
		Camera cam = CameraController.Instance.MainCamera;
		Vector3 forward = -cam.transform.forward;
		forward = _myTransform.parent.worldToLocalMatrix.MultiplyVector(forward);
		Quaternion rot = Quaternion.FromToRotation(Vector3.up, (new Vector3(forward.x, forward.y, 0.0f)).normalized);
		_myTransform.localRotation = rot;
		// uv stretch.
		float stretch = _referenceTransform.localScale.z;
		foreach(Renderer r in _renderers) {
			Vector2 uvscale = r.sharedMaterial.mainTextureScale;
			uvscale.x = stretch * _scaleFactor;
			r.sharedMaterial.mainTextureScale = uvscale;
		}
	}
}
