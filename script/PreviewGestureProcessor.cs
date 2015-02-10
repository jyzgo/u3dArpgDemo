using UnityEngine;
using System.Collections;

public class PreviewGestureProcessor : MonoBehaviour{
	

	public Transform _targetTransform = null;
	public Transform TargetTransform
	{
		set{_targetTransform = value;}	
	}

	void OnDrag(Vector2 delta)
	{
		if(_targetTransform != null)
		{
			Vector3 angle = _targetTransform.localEulerAngles;
			angle.y -=  delta.x ; 
			_targetTransform.localEulerAngles = angle;
		}
	}
}
