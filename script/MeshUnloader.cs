using UnityEngine;
using System.Collections;

public class MeshUnloader : MonoBehaviour {
	
	[HideInInspector]
	public Mesh []_meshes;
	
	void OnDisable() {
		if(_meshes != null) {
			foreach(Mesh m in _meshes) {
				Resources.UnloadAsset(m);
			}
		}
	}
}
