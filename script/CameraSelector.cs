using UnityEngine;
using System.Collections;

public class CameraSelector : MonoBehaviour {
	
	public string _forwardCameraPath;
	public string _deferredCameraPath;
	public Transform _cameraParent;
	
	// Use this for initialization
	void Start () {
		string cameraPath = _forwardCameraPath;

		if(GameSettings.Instance.IsDeferredShadingActived()) {
			cameraPath = _deferredCameraPath;
		}
		GameObject camObj = InJoy.AssetBundles.AssetBundles.Load(cameraPath, typeof(GameObject)) as GameObject;
		camObj = GameObject.Instantiate(camObj) as GameObject;
		camObj.transform.parent = _cameraParent;
		camObj.transform.localPosition = Vector3.zero;
		camObj.transform.localRotation = Quaternion.identity;
		camObj.transform.localScale = Vector3.one;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
