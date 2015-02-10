using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {
	
	List<FCObject> _clients;
	
	static InputManager _instance = null;
	
	public static InputManager Instance
	{
		get
		{
			return _instance;
		}
	}
	
	public List<FCObject> Clients
	{
		get
		{
			return _clients;
		}
	}
	
	void Awake()
	{
		_instance = this;
		_clients = new List<FCObject>();
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	
	public void AddWatch(FCObject eobj)
	{
		_clients.Add(eobj);
	}
	
	public void RemoveWatch(FCObject eobj)
	{
		_clients.Remove(eobj);
	}
#if UNITY_EDITOR
	bool wasHoldingAny = false;
	void Update()
	{
		if (UICamera.inputHasFocus)
		{
			return;
		}
		Camera cam = CameraController.Instance.MainCamera;
		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;
		forward.y = 0.0f;
		right.y = 0.0f;
		Vector3 move = Vector3.zero;
		bool pressed = false;
		if (Input.GetKey(KeyCode.W)) 
		{
#if FC_AUTHENTIC
            move += forward;
#else
            move = forward;
#endif
			pressed = true;
		}
		if(Input.GetKey(KeyCode.S))
		{
#if FC_AUTHENTIC
			move += -forward;
#else
            move = -forward;
#endif
            pressed = true;
		}
		if(Input.GetKey(KeyCode.A))
		{
#if FC_AUTHENTIC
			move += -right;
#else
            move = -right;
#endif
            pressed = true;
		}
		if(Input.GetKey(KeyCode.D))
		{
#if FC_AUTHENTIC
            move += right;
#else
            move = right;
#endif
			pressed = true;
		}
		if(pressed)
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_DIRECTION,FC_PARAM_TYPE.INT,
					move.normalized,FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
			wasHoldingAny = true;
		}
		else if(wasHoldingAny)
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_DIRECTION,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
			wasHoldingAny = false;
		}
		if (Input.GetKeyDown (KeyCode.J)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_1,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		if (Input.GetKeyUp (KeyCode.J)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_1,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		if (Input.GetKeyDown (KeyCode.K)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_2,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		if (Input.GetKeyUp (KeyCode.K)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_2,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		if (Input.GetKeyDown (KeyCode.L)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_3,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		if (Input.GetKeyUp (KeyCode.L)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_3,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyUp (KeyCode.U)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_4,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyDown (KeyCode.U)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_4,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyUp (KeyCode.I)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_5,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyDown (KeyCode.I)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_5,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyUp (KeyCode.O)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_6,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyDown (KeyCode.O)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_6,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyUp (KeyCode.P)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_7,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyDown (KeyCode.P)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_7,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyUp (KeyCode.M)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_ATTACK_8,FC_PARAM_TYPE.INT,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
		
		if (Input.GetKeyDown (KeyCode.M)) 
		{
			foreach( FCObject client in _clients)
			{
				CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_PRESS,
					FCConst.FC_KEY_ATTACK_8,FC_PARAM_TYPE.INT,
					new Vector3(-1,0,0),FC_PARAM_TYPE.VECTOR3,
					client.ObjectID,FCCommand.STATE.RIGHTNOW,true);
			}
		}
	}
#endif
}
