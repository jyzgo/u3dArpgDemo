using UnityEngine;
using System.Collections;

public class CustomUIRenderer : MonoBehaviour {
	
	public static CustomUIRenderer _inst = null;
	public Material _screenCopyMat;
	
	void Awake() {
		enabled = GameSettings.Instance.IsDeferredShadingActived();
		if(_inst != null) {
			Debug.LogError("UIRenderer Singleton error!");
			return;
		}
		_inst = this;
	}
	
	void OnDestroy() {
		if(_inst == this) {
			_inst = null;
		}
	}
	
	static public void SetEnable(bool isEnable) {
		if(_inst != null) {
			_inst.enabled = (GameSettings.Instance.IsDeferredShadingActived() && isEnable);
		}
	}
	
	void OnPreRender() {
		if(DeferredShadingRenderer.FBuffer != null) {
			Graphics.Blit(DeferredShadingRenderer.FBuffer, _screenCopyMat);
			_screenCopyMat.mainTexture = null;
		}
	}
}
