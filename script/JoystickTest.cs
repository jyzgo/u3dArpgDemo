using UnityEngine;
using System.Collections;

public class JoystickTest : MonoBehaviour 
{
	public Joystick _joystick;
	public Transform _joystickTransform;
	
	Camera _Carema;
	
	// Use this for initialization
	void Start () 
	{
		_Carema = NGUITools.FindInParents<Camera>(gameObject);
	}
	
	void OnPress(bool isDown)
	{
#if ((!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR)
		Vector3 moveDir = Vector3.zero;
		
		Vector3 center = _Carema.WorldToScreenPoint(_joystickTransform.position);
		
		moveDir.x = UICamera.lastTouchPosition.x - center.x;
		moveDir.z = UICamera.lastTouchPosition.y - center.y;
		
		if (isDown)
		{
			_joystick.Move(moveDir);
		}
		else
		{
			_joystick.StopMove(moveDir);
		}
#endif
	}
}
