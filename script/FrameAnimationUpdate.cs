using UnityEngine;
using System.Collections;

public class FrameAnimationUpdate : MonoBehaviour {
	
	public float step;
	public VolumeTexturePtr _texture;
	public Material _mat;
	
	private int index = 0;
	private float counter = 0.0f;
	
	public void Reset() {
		index = 0;
		counter = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
		if(_mat != null) {
			counter -= Time.deltaTime;
			if(counter <= 0.0f) {
				_mat.mainTexture = _texture.GetTexture(index);
				++index;
				counter = step;
			}
		}
	}
}
