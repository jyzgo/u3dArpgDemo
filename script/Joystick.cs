using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Joystick : MonoBehaviour
{
	private Camera _mainCamera;
	private Camera _uiCamera;

	public GameObject uiJoyStick;
	public Vector2 relativeRegion;

	public float _deadZoneForPress = 0.5f;

	private Vector3 _joystickUIPos = Vector3.zero;
	private Vector2 _joystickUISize;
	private int _fingerId = -1;
	private UICamera _UICameraScript;

	private float _activeZoneRaduis = 0;
	bool _needUpdatePos = false;

	// Use this for initialization
	void Start()
	{
		_mainCamera = CameraController.Instance.MainCamera;

		_uiCamera = UIManager.Instance.MainCamera;

		_joystickUISize = uiJoyStick.GetComponent<BoxCollider>().size;

		_UICameraScript = _uiCamera.GetComponent<UICamera>();

		_activeZoneRaduis = _joystickUISize.magnitude / 2;

		InitTutorialEffect();
	}

	// Update is called once per frame
	void Update()
	{
		//has opening box? do not update it
		//if (!UIMessageBoxManager.Instance.hasBoxOpening())
		JoystickUpdate();
	}

	bool IsTouchingUI(Vector3 inPos)
	{
		Ray ray = _uiCamera.ScreenPointToRay(inPos);
		int mask = _uiCamera.cullingMask & (int)_UICameraScript.eventReceiverMask;
		float dist = (_UICameraScript.rangeDistance > 0f) ? _UICameraScript.rangeDistance : _uiCamera.farClipPlane - _uiCamera.nearClipPlane;

		return Physics.Raycast(ray, dist, mask);
	}

	public void JoystickUpdate()
	{
		Vector3 moveDir = Vector3.zero;
		bool isPressed = false;
		Vector3 touchPos = Vector3.zero;

#if UNITY_EDITOR
		if (Input.GetMouseButton(0))
#else
		for (int i=0; i<Input.touchCount; i++)
#endif
		{
#if UNITY_EDITOR
			touchPos = Input.mousePosition;
#else
			Touch touch = Input.GetTouch(i);
			touchPos = touch.position;
#endif


			if ((touchPos.x > Screen.width * relativeRegion.x) || (touchPos.y > Screen.height * relativeRegion.y))
			{
#if UNITY_EDITOR
				if (uiJoyStick.activeSelf)
				{
					MoveOut();
				}
				return;
#else
				continue;
#endif
			}
			if (uiJoyStick.activeSelf)
			{
#if UNITY_EDITOR
#else
				if (touch.fingerId == _fingerId)
#endif
				{
					moveDir.x = touchPos.x - _joystickUIPos.x;
					moveDir.z = touchPos.y - _joystickUIPos.z;
					float len1 = moveDir.magnitude;
					float len = moveDir.magnitude - _activeZoneRaduis;
					if (len > 0)
					{
						Vector3 dir = _joystickUIPos + moveDir.normalized * len;
						dir.y = dir.z;
						dir.z = 0;
						SetJoystickPostion(dir);
					}

#if UNITY_EDITOR
					if (len1 > _deadZoneForPress)
					{
						Move(moveDir);
					}
#else
					if ((touch.phase==TouchPhase.Canceled) || (touch.phase==TouchPhase.Ended))
					{
						//NGUITools.SetActive(_joystickUI, false);
						
						StopMove(moveDir);
					}
					else
					{
						if(len1 > _deadZoneForPress)
						{
							Move(moveDir);	
						}
						
					}
#endif

				}
			}
			else if (!IsTouchingUI(touchPos))
			{
#if UNITY_EDITOR
#else
				if ((touch.phase!=TouchPhase.Canceled) && (touch.phase!=TouchPhase.Ended))
#endif
				{
					uiJoyStick.SetActive(true);
					SetJoystickPostion(touchPos);
					_needUpdatePos = true;
#if UNITY_EDITOR
					_fingerId = 0;
#else
					_fingerId = touch.fingerId;
#endif
				}
			}

			isPressed = true;
#if UNITY_EDITOR
#else
			break;
#endif

		}

		if (!isPressed && uiJoyStick.activeSelf)
		{
			MoveOut();
		}
	}

	void SetJoystickPostion(Vector3 pos)
	{
		_joystickUIPos.x = pos.x;
		_joystickUIPos.z = pos.y;


		_joystickUIPos.x = Mathf.Clamp(_joystickUIPos.x, _joystickUISize.x / 2, Screen.width * relativeRegion.x - _joystickUISize.x / 2);
		_joystickUIPos.z = Mathf.Clamp(_joystickUIPos.z, _joystickUISize.y / 2, Screen.width * relativeRegion.y - _joystickUISize.y / 2);
		/*if (_joystickUIPos.x < _joystickUISize.x/2)
		{
			_joystickUIPos.x = _joystickUISize.x/2;
		}
		if (_joystickUIPos.x >= Screen.width*_relativeRegion.x-_joystickUISize.x/2)
		{
			_joystickUIPos.x = Screen.width*_relativeRegion.x-_joystickUISize.x/2;
		}
		if (_joystickUIPos.z < _joystickUISize.y/2)
		{
			_joystickUIPos.z = _joystickUISize.y/2;
		}*/
	}

	void LateUpdate()
	{
#if ((UNITY_IPHONE || UNITY_ANDROID) || UNITY_EDITOR)
		Vector3 pos = new Vector3(_joystickUIPos.x, _joystickUIPos.z, 0f);
		if (_needUpdatePos)
		{
			uiJoyStick.transform.position = _uiCamera.ScreenToWorldPoint(pos);
			_needUpdatePos = false;
		}
#endif
	}

	public void Move(Vector3 dir)
	{
		if (InputManager.Instance == null)
		{
			return;
		}

		if (dir.sqrMagnitude < Mathf.Epsilon)
		{
			return;
		}

		Vector3 movement = _mainCamera.transform.TransformDirection(dir);

		movement.y = 0f;
		movement.Normalize();

		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
				FCConst.FC_KEY_DIRECTION, FC_PARAM_TYPE.INT,
				movement, FC_PARAM_TYPE.VECTOR3,
				client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	void MoveOut()
	{
		Vector3 moveDir = Vector3.zero;
		Vector3 touchPos = Vector3.zero;

		//bool isStop = false;
#if UNITY_EDITOR
		{
			touchPos = Input.mousePosition;
#else
		for (int i=0; i<Input.touchCount; i++)
		{
			Touch touch = Input.GetTouch(i);
			touchPos = touch.position;
			if (touch.fingerId == _fingerId)
#endif
			{
				moveDir.x = touchPos.x - _joystickUIPos.x;
				moveDir.z = touchPos.y - _joystickUIPos.z;


#if UNITY_EDITOR
#else
				break;
#endif
			}
		}
		StopMove(moveDir);
		uiJoyStick.SetActive(false);
		_fingerId = -1;
	}

	public void StopMove(Vector3 dir)
	{
		if (InputManager.Instance == null)
		{
			return;
		}

		if (_mainCamera == null)
		{
			_mainCamera = CameraController.Instance.MainCamera;
		}

		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_DIRECTION, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}


	public GameObject _tutorialEffect = null;
	public void InitTutorialEffect()
	{
		if (_tutorialEffect != null)
		{
			_tutorialEffect.SetActive(false);
		}
	}

	IEnumerator TryStartTutorial(float time)
	{
		_tutorialEffect.SetActive(true);
		yield return new WaitForSeconds(time);

		while (true)
		{
			EnumTutorialState state = PlayerInfo.Instance.GetTutorialState(EnumTutorial.Battle_Move);
			if (state == EnumTutorialState.Finished)
			{
				break;
			}

			if (uiJoyStick.activeSelf)
			{
				_tutorialEffect.SetActive(false);
			}
			else
			{
				if (!NGUITools.GetActive(_tutorialEffect))
				{
					_tutorialEffect.SetActive(true);
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
		_tutorialEffect.SetActive(false);
	}

	public void BeginTutorial(EnumTutorial tutorialId)
	{
		if (TutorialManager.Instance.TryStartTutorialLevel(tutorialId))
		{
			StartCoroutine(TryStartTutorial(1.0f));
		}
	}

	public void FinishTutorial(EnumTutorial tutorialId)
	{
		TutorialManager.Instance.TryFinishTutorialLevel(tutorialId);
	}

}
