using UnityEngine;
using System.Collections;



public class ShadowRenderer : MonoBehaviour {

	public static Matrix4x4 UpdateShadowMatrix(Vector3 planeNormal, float D, Vector3 light, float pointFlag) {
		
		Matrix4x4 shadowMatrix = Matrix4x4.identity;
		float dot = Vector3.Dot(planeNormal, light);

		shadowMatrix[0, 0] = dot + D * pointFlag - planeNormal.x * light.x;
		shadowMatrix[0, 1] = -planeNormal.y * light.x;
		shadowMatrix[0, 2] = -planeNormal.z * light.x;
		shadowMatrix[0, 3] = -D * light.x;
		
		shadowMatrix[1, 0] = -planeNormal.x * light.y;
		shadowMatrix[1, 1] = dot + D * pointFlag - planeNormal.y * light.y;
		shadowMatrix[1, 2] = -planeNormal.z * light.y;
		shadowMatrix[1, 3] = -D * light.y;
		
		shadowMatrix[2, 0] = -planeNormal.x * light.z;
		shadowMatrix[2, 1] = -planeNormal.y * light.z;
		shadowMatrix[2, 2] = dot + D * pointFlag - planeNormal.z * light.z;
		shadowMatrix[2, 3] = -D * light.z;
		
		shadowMatrix[3, 0] = -planeNormal.x * pointFlag;
		shadowMatrix[3, 1] = -planeNormal.y * pointFlag;
		shadowMatrix[3, 2] = -planeNormal.z * pointFlag;
		shadowMatrix[3, 3] = dot;
		
		return shadowMatrix;
	}
	
	public Camera _shadowCamera;
	public Material _clearMat;
	
	System.Collections.Generic.List<Transform> _pointLights = new System.Collections.Generic.List<Transform>();
	System.Collections.Generic.List<AvatarController> _shadowInstances = new System.Collections.Generic.List<AvatarController>();
	Transform _directionalLight;
	
	public void SetDirectionalLight(Transform newLight) {
		_directionalLight = newLight;
	}
	
	public void AddPointLight(Transform newLight) {
		if(!_pointLights.Contains(newLight)) {
			_pointLights.Add(newLight);
		}
	}
	
	public void RemovePointLight(Transform lightToRemove) {
		_pointLights.Remove(lightToRemove);
	}
	
	public void AddShadowInstance(AvatarController inst) {
		_shadowInstances.Add(inst);
	}
	
	public void RemoveShadowInstance(AvatarController inst) {
		_shadowInstances.Remove(inst);
	}
	
	public void DrawShadow() {
		Vector4 range = Vector4.one;
		if(_directionalLight != null) {
			range.w = 1000000.0f;
			foreach(AvatarController a in _shadowInstances) {
				a.UpdateShadowMatrix(-_directionalLight.forward, 0.0f, range);
			}
			_shadowCamera.Render();
		}
		foreach(Transform l in _pointLights) {
			foreach(AvatarController a in _shadowInstances) {
				Vector3 pos = l.position;
				pos.y = a.myTransform.position.y + 4.0f;
				range = pos;
				range.w = 270.0f;
				a.UpdateShadowMatrix(pos, 1.0f, range);
			}
			_shadowCamera.Render();
		}
	}
	
	public void DoClear() {
		RenderBuffer colorBuffer = Graphics.activeColorBuffer;
		RenderBuffer depthBuffer = Graphics.activeDepthBuffer;
		Graphics.Blit(null, _shadowCamera.targetTexture, _clearMat);
		Graphics.SetRenderTarget(colorBuffer, depthBuffer);
	}
}
