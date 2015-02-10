using UnityEngine;
using System.Collections;

public class IconRenderer : MonoBehaviour {
	public Camera _camera;
	public Shader _shader;
	
	public void Render(RenderTexture rt) {
		_camera.targetTexture = rt;
		_camera.RenderWithShader(_shader, "");
		_camera.targetTexture = null;
	}
}
