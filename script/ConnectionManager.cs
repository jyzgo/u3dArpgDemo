using UnityEngine;
using System.Collections;
using InJoy.FCComm;
using System.Collections.Generic;

public class ConnectionManager : MonoBehaviour
{
	private class APITemplate
	{
		public sourceAPIDelegate sourceAPI = null;
		public bool isBlock = true;
	}
	private Queue<APITemplate> _sourceAPIQueue = new Queue<APITemplate>();

	public delegate void sourceAPIDelegate();

	private sourceAPIDelegate _sourceAPI = null;	//the source API in the top


	bool _isBlock = false; //this is a block connect?

	enum MsgBoxFlag
	{
		None = 0,
		Lv1,
		Lv2,
	}
	//    MsgBoxFlag _msgBoxFlag = MsgBoxFlag.None;

	GameObject _msgBoxLv1 = null;
	GameObject _msgBoxLv2 = null;

	string _msgBoxLv1Text = null;
	string _msgBoxLv2Text = null;
	string _msgBoxLv2Text_noblock = null;

	#region Singleton
	private static ConnectionManager _instance = null;
	public static ConnectionManager Instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("Connect Manager: Another ConnectManager has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			Destroy(this);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(this);
	}

	void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
	#endregion

	// Use this for initialization
	void Start()
	{
		_msgBoxLv1Text = Localization.instance.Get("IDS_MESSAGE_SERVER_RECONNECT_WAIT");
		_msgBoxLv2Text = Localization.instance.Get("IDS_BUTTON_SERVER_RECONNECT_RETRY");
		_msgBoxLv2Text_noblock = Localization.instance.Get("IDS_BUTTON_SERVER_RECONNECT_QUIT");
	}

	// Update is called once per frame
	void Update()
	{
		//clear all msg boxes if no API is waiting
		if (_sourceAPI == null)
		{
			//close msg box lv 1
			if (_msgBoxLv1 != null)
			{
				UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);
				_msgBoxLv1 = null;
			}
		}

		//if queue has APIs and current API is null, pop up one API
		if ((_sourceAPIQueue.Count > 0) && (_sourceAPI == null))
		{
			APITemplate api = _sourceAPIQueue.Dequeue();
			DoHandler(api.sourceAPI, api.isBlock);
		}

	}

	void Reset()
	{
		Debug.Log("[Connection manager] Reset!");

		//		_msgBoxFlag = MsgBoxFlag.None;
		_sourceAPI = null;
		_isBlock = false;

		//close msg box lv 1
		if (_msgBoxLv1 != null)
		{
			UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);
			_msgBoxLv1 = null;
		}

		//close msg box lv 2
		if (_msgBoxLv2 != null)
		{
			UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv2);
			_msgBoxLv2 = null;
		}

	}

	public void RegisterHandler(sourceAPIDelegate sourceAPI, bool isBlock)
	{
		APITemplate api = new APITemplate();
		api.sourceAPI = sourceAPI;
		api.isBlock = isBlock;
		_sourceAPIQueue.Enqueue(api);
	}

	private void DoHandler(sourceAPIDelegate sourceAPI, bool isBlock)
	{
		//return if I am in offline play mode
		//if (NetworkManager.Instance.isOfflinePlay)
		//	return;

		//already a handler exist? skip this request
		if (_sourceAPI != null)
		{
			Debug.LogError("[Connection Manager] Already have an API running " + _sourceAPI.ToString());
			return;
		}

		Reset();

		_sourceAPI = sourceAPI;
		_isBlock = isBlock;


		//start to run the handler
		Debug.Log("[Connection Manager] Running API: " + _sourceAPI.ToString());
		_sourceAPI();

		//set timeout handler
		StartCoroutine(JudgeSourceAPITimeOut());

		//set msg box level 1 handler
		StartCoroutine(BlockMsgBoxLevel1());
	}

	//this should from the callback of source API, info me that success or not
	public void SendACK(sourceAPIDelegate sourceAPI, bool success, string errorCode = "", string errorType = "", string description = "")
	{
		Debug.Log(string.Format("[Connection Manager] ACK received for API {0}, Success = {1}", sourceAPI.ToString(), success.ToString()));

		//check the source API is null?
		if (_sourceAPI == null)
		{
			Debug.LogWarning("[Connection Manager] ACK received but no API exists.");
			return;
		}

		//check the ACK api is not my current source API?
		if (!_sourceAPI.Equals(sourceAPI))
		{
			Debug.LogWarning("[Connection Manager] ACK received but is not current API: " + sourceAPI.ToString());
			return;
		}

		//stop JudgeSourceAPITimeOut coroutine
		StopCoroutine("JudgeSourceAPITimeOut");

		//stop BlockMsgBoxLevel1 coroutine
		StopCoroutine("BlockMsgBoxLevel1");

		//close msg box lv 1
		if (_msgBoxLv1 != null)
		{
			UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);
			_msgBoxLv1 = null;
		}

		//get a success, reset all.
		if (success)
		{
			Reset();
		}
		else
		{
			ProcessSourceAPITimeOutOrFail();
		}
	}



	//coroutine for source api timeout
	IEnumerator JudgeSourceAPITimeOut()
	{
		//wait for reconnect timeout locally
		yield return new WaitForSeconds(FCConst.k_reconnect_timeout);

		if (_sourceAPI != null)
		{
			//if still processing, the source API is time out
			ProcessSourceAPITimeOutOrFail();
		}
	}

	//process source api time out or fail, an entrance of re-connect
	void ProcessSourceAPITimeOutOrFail()
	{
		Debug.LogWarning("[Connection Manager] Processing time out or fail...");

		//close msg box lv 1 if exist
		if (_msgBoxLv1 != null)
		{
			UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);
			_msgBoxLv1 = null;
		}

		//already block by lv2 box, discard this
		if (_msgBoxLv2 != null)
		{
			return;
		}

		if (_isBlock)
		{
			//popup the lv2 msg box
			_msgBoxLv2 = UIMessageBoxManager.Instance.ShowMessageBox(_msgBoxLv2Text,
				null,
				MB_TYPE.MB_OK,
				onReconnectPressCallback);
		}
		else
		{
			//if this is non-block connect, popup a msg and reset

			Reset();

			//open new box
			UIMessageBoxManager.Instance.ShowMessageBox(_msgBoxLv2Text_noblock,
				null,
				MB_TYPE.MB_OK,
				OnNonBlockPressCallback);
		}


	}

	//coroutine for Level 1 block msg box 
	IEnumerator BlockMsgBoxLevel1()
	{
		yield return new WaitForSeconds(FCConst.BLOCKING_TIMEOUT_1);

		//close old box?
		if (_msgBoxLv1 == null)
			UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);

		//open new box
		//do not show lv1 box if lv2 is showing
		if (_msgBoxLv2 == null)
		{
			_msgBoxLv1 = UIMessageBoxManager.Instance.ShowMessageBox(_msgBoxLv1Text,
				"",
				MB_TYPE.MB_WAITING_DELAY,
				null);
		}

	}

	//check the status of connection, have some branches
	void CheckConnectionStatus()
	{
		Debug.LogWarning("[Connection Manager] Checking network status...");

		if (!NetworkManager.Instance.IsAuthenticated())
		{
			//msg lv1 time out
			StartCoroutine(BlockMsgBoxLevel1());

			//re-auth time out 
			StartCoroutine(JudgeReAuthTimeOut());
		}
		else
		{
			//do some flag and reset

			//re call the source API
			_sourceAPI();

			//set timeout handler
			StartCoroutine(JudgeSourceAPITimeOut());

			//set msg box level 1 handler
			StartCoroutine(BlockMsgBoxLevel1());
		}
	}


	void onAuthenticateReconnectCallback(bool success)
	{
		Debug.Log("[Connection Manager] onAuthenticateReconnectCallback " + success.ToString());

		//discard this if block by lv2 box
		if (_msgBoxLv2 != null)
		{
			return;
		}

		//stop re-auth time out 
		StopCoroutine("JudgeReAuthTimeOut");

		//stop BlockMsgBoxLevel1 coroutine
		StopCoroutine("BlockMsgBoxLevel1");

		//close msg box lv 1 if exist
		if (_msgBoxLv1 != null)
		{
			UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);
			_msgBoxLv1 = null;
		}

		if (success)
		{
			CheckConnectionStatus();
		}
		else
		{
			//popup the lv2 msg box
			_msgBoxLv2 = UIMessageBoxManager.Instance.ShowMessageBox(_msgBoxLv2Text,
				"",
				MB_TYPE.MB_OK,
				onReconnectPressCallback);
		}
	}



	//press reconnect button of lv2 message box
	void onReconnectPressCallback(ID_BUTTON buttonID)
	{
		Debug.Log("[Connection Manager] Reconect button pressed.");

		UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv2);
		_msgBoxLv2 = null;

		if (_sourceAPI != null)
			CheckConnectionStatus();
	}

	//press reconnect button of lv2 message box and no-block mode
	void OnNonBlockPressCallback(ID_BUTTON buttonID)
	{
		Debug.Log("[Connection Manager] Reconnect button pressed in non-blocking mode");
	}

	//coroutine for re-auth timeout
	IEnumerator JudgeReAuthTimeOut()
	{
		//wait for reconnect timeout locally
		yield return new WaitForSeconds(FCConst.k_reconnect_timeout);

		if (_sourceAPI != null)
		{
			//close msg box lv 1 if exist
			if (_msgBoxLv1 != null)
			{
				UIMessageBoxManager.Instance.CloseMessageBox(_msgBoxLv1);
				_msgBoxLv1 = null;
			}

			//popup the lv2 msg box
			_msgBoxLv2 = UIMessageBoxManager.Instance.ShowMessageBox(_msgBoxLv2Text,
				"",
				MB_TYPE.MB_OK,
				onReconnectPressCallback);
		}
	}
}
