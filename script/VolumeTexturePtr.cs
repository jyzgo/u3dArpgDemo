using UnityEngine;
using System.Collections;

public class VolumeTexturePtr : MonoBehaviour {
	
	public Texture2D []_sources;
	public TextureWrapMode _wrapMode;
	
	Texture3D _texturePtr = null;
	
	VolumeTexturePtr _instance = null;
	VolumeTexturePtr _prefab = null;
	
	public Texture3D TexturePtr {
		get {
			if(_instance == null) {
				// get or create root node of 3d textures.
				GameObject go = GameObject.Find("Volume Textures");
				if(go == null) {
					go = new GameObject("Volume Textures");
				}
				// create instance of this texture.
				GameObject inst = GameObject.Instantiate(gameObject) as GameObject;
				inst.transform.parent = go.transform;
				_instance = inst.GetComponent<VolumeTexturePtr>();
				_instance._prefab = this;
				_instance.CreateTexture();
			}
			return _instance._texturePtr;
		}
	}
	
	public Texture2D GetTexture(int index) {
		return _sources[index%_sources.Length];
	}
	
	void Awake() {
		foreach(Texture2D t in _sources) {
			Resources.UnloadAsset(t);
		}
	}
	
	void CreateTexture() {
		_texturePtr = new Texture3D(_sources[0].width, _sources[0].height, _sources.Length, TextureFormat.ARGB32, false);
		int colorCount = 0;		
		foreach(Texture2D t in _sources) {
			colorCount += t.width * t.height;
		}
		Color []colors = new Color[colorCount];
		colorCount = 0;
		foreach(Texture2D t in _sources) {
			Color []cs = t.GetPixels();
			foreach(Color c in cs) {
				colors[colorCount] = c;
				++colorCount;
			}
			Resources.UnloadAsset(t);
		}
		_texturePtr.SetPixels(colors);
		_texturePtr.wrapMode = _wrapMode;
		_texturePtr.Apply();
	}
	
	void OnDestroy() {
		_texturePtr = null;
		if(_prefab != null && _prefab._instance == this) {
			_prefab._instance = null;
		}
	}
}
