using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PathLoft : MonoBehaviour {
	
	public Material _material;
	public Material _unlockMaterial;
	public float _width;
	public float _texWidth;
	public Transform []_paths = new Transform[0];
	bool _unlockEffectActived;
	Mesh _mesh = null;
	void Awake() {
		InitMesh();
	}
	
	void InitMesh()
	{
		if(_mesh == null) {
			MeshFilter mf = gameObject.GetComponent<MeshFilter>();
			if(mf == null) {
				mf = gameObject.AddComponent<MeshFilter>();
			}
			_mesh = new Mesh();
			_mesh.name = "Path Mesh";
			mf.sharedMesh = _mesh;
		
			MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
			if(mr == null) {
				mr = gameObject.AddComponent<MeshRenderer>();
			}
			mr.sharedMaterial = _material;
		}
	}
	
	public void UpdatePath() {
		InitMesh();
		_mesh.Clear();
		if(_paths.Length > 1) {
			System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();
			System.Collections.Generic.List<Vector2> uvs = new System.Collections.Generic.List<Vector2>();
			System.Collections.Generic.List<int> indices = new System.Collections.Generic.List<int>();
			
			Vector2 uv = Vector2.zero;
			
			for(int i = 0;i < _paths.Length - 1;++i) {
				Vector3 start = _paths[i].localPosition;
				Vector3 end = _paths[i + 1].localPosition;
				
				Vector3 dir = end - start;
				dir.y = 0.0f;
				Vector3 w = Vector3.Cross(dir.normalized, Vector3.up).normalized * (0.5f * _width);
				int startIdx = vertices.Count - 2;
				// add ajust.
				indices.Add(startIdx);
				indices.Add(startIdx + 2);
				indices.Add(startIdx + 1);
				indices.Add(startIdx + 1);
				indices.Add(startIdx + 2);
				indices.Add(startIdx + 3);
				// add line
				startIdx = vertices.Count;
				indices.Add(startIdx);
				indices.Add(startIdx + 2);
				indices.Add(startIdx + 1);
				indices.Add(startIdx + 1);
				indices.Add(startIdx + 2);
				indices.Add(startIdx + 3);
				vertices.Add(start + (dir.normalized * _width * 0.5f) + w);
				vertices.Add(start + (dir.normalized * _width * 0.5f) - w);
				vertices.Add(end - (dir.normalized * _width * 0.5f) + w);
				vertices.Add(end - (dir.normalized * _width * 0.5f) - w);
				uvs.Add(uv + Vector2.up);
				uvs.Add(uv);
				uv.x += (dir.magnitude - _width) / _texWidth;
				uvs.Add(uv + Vector2.up);
				uvs.Add(uv);
				uv.x += _width / _texWidth;
			}
			indices.RemoveRange(0, 6);
			_mesh.vertices = vertices.ToArray();
			_mesh.triangles = indices.ToArray();
			_mesh.uv = uvs.ToArray();
			MeshRenderer mr = GetComponent<MeshRenderer>();
			mr.sharedMaterial = (_unlockEffectActived ? _unlockMaterial : _material);
		}
	}
	
	[ContextMenu("add node")]
	void AddPnt() {
		System.Collections.Generic.List<Transform> list = new System.Collections.Generic.List<Transform>();
		list.AddRange(_paths);
		
		GameObject newNode = new GameObject("path node " + list.Count);
		newNode.transform.parent = gameObject.transform;
		newNode.transform.localPosition = Vector3.zero;
		newNode.transform.localRotation = Quaternion.identity;
		newNode.transform.localScale = Vector3.one;
		
		list.Add(newNode.transform);
		_paths = list.ToArray();
	}
	
	[ContextMenu("rebuild node")]
	void Rebuild() {
		UpdatePath();
	}
	[ContextMenu("link to UI")]
	void LinkToUI() {
		if(transform.parent != null) {
			WorldMapChessPieceHandler handler = transform.parent.GetComponentInChildren<WorldMapChessPieceHandler>();
			if(handler != null) {
				handler._pathLoft = this;
			}
		}
	}
	[ContextMenu("clear nodes")]
	void ClearNodes() {
		for(int i = _paths.Length - 1;i >= 0; --i) {
			GameObject.DestroyImmediate(_paths[i].gameObject);
		}
		_paths = new Transform[0];
		UpdatePath();
	}
	public bool UnlockEffect {
		set {_unlockEffectActived = value;}
	}
}
