using UnityEngine;
using System.Collections;
 
 
/// <summary>
/// Overhead view controller.
/// 
/// Control camera of world map.
/// </summary>
public class OverheadViewController : IgnoreTimeScale 
{
	public Transform cameraTransform;
	public Camera _camera_iPhone;
 	public Camera _camera_iPad;
	
	int rightFingerId = -1;
	
	Vector2 rightFingerLastPoint;
	bool _shouldMove;
	
	public Vector3 _minConstraint;
	public Vector3 _maxConstraint;
	public Vector3 _cameraPosition;
	
	public float _dragMoveSpeed = 1.0f;
	Vector2 mMomentum = Vector2.zero;
 	public float momentumAmount = 1f;
	public float _strength = 9f;
	public float _manualHeight = 640.0f;
	public float _manualWidth = 960.0f;
	
	public float _cacheTime = 3.0f;
	public float _threshold = 10.0f;
 
	void OnMousePressed(int fingerId, Vector2 pos)
	{
		if (rightFingerId == -1)
		{
			rightFingerLastPoint = pos;
			rightFingerId = fingerId;
			_shouldMove = false;
			
			mMomentum = Vector2.zero;
		}
	} 
 
	void OnMouseReleased(int fingerId, Vector2 pos)
	{
		if (fingerId == rightFingerId)
		{
			rightFingerId = -1;
			
			rightFingerLastPoint = pos;
			
			_shouldMove = true;
		}			
	} 
 
	void OnMouseMoved(int fingerId, Vector2 pos)
	{
		if (fingerId == rightFingerId)
		{
			Vector2 delta = pos - rightFingerLastPoint;
			
			Vector3 offset = new Vector3(delta.x, 0.0f, delta.y);
				
			//offset.Normalize(); 
			offset *= _dragMoveSpeed;
			offset.z *= (_manualHeight/Screen.height);
			offset.x *= (_manualWidth/Screen.width);
			
			Vector3 position = cameraTransform.localPosition - (Vector3)offset;
			
			if (ContrainToBounds(ref position))
			{				
				cameraTransform.localPosition = position;
					
				delta = new Vector2(offset.x,offset.z);
				mMomentum = Vector2.Lerp(mMomentum, mMomentum - delta * (0.01f * momentumAmount), 0.67f);
			}

			rightFingerLastPoint = pos;
		}
	}
 
	void OnTochBegan(Touch touch)
	{
		if (touch.fingerId == -1)
		{
			rightFingerId = touch.fingerId;
			_shouldMove = false;
			
			mMomentum = Vector2.zero;
		}
	}
	
	void OnTouchEnded(Touch touch)
	{
		Vector2 velocity = touch.deltaPosition / touch.deltaTime;
		
		if ((touch.fingerId==rightFingerId) && (velocity.magnitude>_threshold))
		{
			rightFingerId = -1;
			
			mMomentum = velocity;
			
			_shouldMove = true;
		}		
	}
	
	void OnTouchMoved(Touch touch)
	{
		if (touch.fingerId == rightFingerId)
		{
			Vector3 offset = new Vector3(touch.deltaPosition.x, 0.0f, touch.deltaPosition.y);
				
			offset.Normalize(); 
			offset *= _dragMoveSpeed;
			
			Vector3 position = cameraTransform.localPosition - (Vector3)offset;
			
			if (ContrainToBounds(ref position))
			{				
				cameraTransform.localPosition = position;
			}
		}
	}
 
 
	void Start ()
	{
		string deviceModel = GameSettings.Instance.LODSettings.GetDeviceModel();
		bool wideFov = deviceModel.StartsWith("iPad");
		_camera_iPhone.gameObject.SetActive(!wideFov);
		_camera_iPad.gameObject.SetActive(wideFov);
		_cameraPosition = cameraTransform.localPosition;
	}
 
 
 
	void Update ()
	{
		if (Application.isEditor)
		{
			if (Input.GetMouseButtonDown(0))
				OnMousePressed(0, Input.mousePosition);
			else if (Input.GetMouseButtonUp(0))
				OnMouseReleased(0, Input.mousePosition);
			else if (rightFingerId != -1)
				OnMouseMoved(0, Input.mousePosition);
		}
		else
		{
			int count = Input.touchCount;
 
			for (int i = 0;  i < count;  i++) 
			{	
				Touch touch = Input.GetTouch (i);
 
				if (touch.phase == TouchPhase.Began)
					OnMousePressed(touch.fingerId, touch.position);
				else if (touch.phase == TouchPhase.Moved)
					OnMouseMoved(touch.fingerId, touch.position);
				else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
					OnMouseReleased(touch.fingerId, touch.position);
			}
		}
 
		
		Move();
	}
 
 
 
	void Move()
	{		
		float delta = UpdateRealTimeDelta();
		
		Vector3 offset = (Vector3)NGUIMath.SpringDampen(ref mMomentum, _strength, delta);
		
		if (_shouldMove && (mMomentum.magnitude>0.01f))
		{			
			Vector3 position = cameraTransform.localPosition + new Vector3(offset.x, 0.0f, offset.y);
			
			if (ContrainToBounds(ref position))
			{
				cameraTransform.localPosition = position;
			}

			_cameraPosition = cameraTransform.localPosition;
			
			return;
		}
		
		//NGUIMath.SpringDampen(ref mMomentum, _strength, delta);
	}
	
	void CachedMoving()
	{
		float delta = UpdateRealTimeDelta();
		
		if (_shouldMove && (mMomentum.magnitude>0.01f))
		{			
			Vector2 offset = mMomentum * delta;
			Vector3 position = cameraTransform.localPosition + new Vector3(offset.x, 0.0f, offset.y);
			
			if (ContrainToBounds(ref position))
			{
				cameraTransform.localPosition = position;
				
				mMomentum = Vector2.Lerp(mMomentum, mMomentum - offset * (0.01f * momentumAmount), 0.67f);
			}

			_cameraPosition = cameraTransform.localPosition;
			
			return;
		}
	}
	
	bool ContrainToBounds(ref Vector3 pos)
	{
		pos.x = Mathf.Max(pos.x, _minConstraint.x);
		pos.z = Mathf.Max(pos.z, _minConstraint.z);
		
		pos.x = Mathf.Min(pos.x, _maxConstraint.x);
		pos.z = Mathf.Min(pos.z, _maxConstraint.z);
				
		return true;
	}
}
