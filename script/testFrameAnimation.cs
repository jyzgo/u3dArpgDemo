using UnityEngine;
using System.Collections;

public class testFrameAnimation : MonoBehaviour {

	public Texture2D []_array;
	public Material _mat;
	int index = 0;
	public float _frameTime;
	float _timer = 0.0f;
	// Update is called once per frame
	void Update () {
		_timer -= Time.deltaTime;
		if(_timer <= 0.0f) {
			index = (index + 1) % _array.Length;
			_mat.mainTexture = _array[index];
			_timer = _frameTime;
		}
	}
}
