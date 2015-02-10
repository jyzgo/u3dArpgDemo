using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;
using InJoy.Utils;

public class GameManager : MonoBehaviour
{
	public bool isFirstEnterTown { get; set; }

	public static int _cheatFlag = 0;
	public static int _mageUnlock = 0;

	// every attack should have a hitid to tell target wheather a target should be hurt by this attack
	protected static int _hitID = 0;

	static public int GainHitID()
	{
		_hitID++;
		if (_hitID > 0xffffff)
		{
			_hitID = 1;
		}
		return _hitID;
	}
	#region Singleton
	private static GameManager _instance = null;
	public static GameManager Instance
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
			Debug.LogError("Gamemanager: Another GameLauncher has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			Destroy(this);
			return;
		}
		_instance = this;
	}

	void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
	#endregion


	private EnumGameState _gameState = EnumGameState.None;
	public EnumGameState GameState
	{
		get
		{
			return _gameState;
		}
		set
		{
			_gameState = value;
		}
	}

	// game mode , GC means gamecenter extra mode
	public enum FC_GAME_MODE
	{
		SINGLE,
		MULTIPLAY,
		MULTIPLAY_GC,
		PVP,
		PVP_GC,
	}

	private FC_GAME_MODE _currGameMode = FC_GAME_MODE.SINGLE;
	public FC_GAME_MODE CurrGameMode
	{
		get { return _currGameMode; }
		set { _currGameMode = value; }
	}

	//private bool _isMultiplayMode = false;
	public bool IsMultiplayMode
	{
		get { return CurrGameMode == FC_GAME_MODE.MULTIPLAY || CurrGameMode == FC_GAME_MODE.MULTIPLAY_GC; }
	}

	//private bool _isPVPMode = false;
	public bool IsPVPMode
	{
		get
		{
#if PVP_ENABLED
			return CurrGameMode == FC_GAME_MODE.PVP || CurrGameMode == FC_GAME_MODE.PVP_GC;
#else
			return false;		
#endif
		}
	}


	public string CGSID
	{
		get
		{
			return string.Empty;
		}
	}

	bool _pauseStatus = false;
	public bool GamePaused
	{
		set
		{
			_pauseStatus = value;
			Time.timeScale = value ? 0f : GameSettings.Instance.TimeScale;
		}
		get
		{
			return _pauseStatus;
		}
	}

	public float StartTime { set; get; }

	// Use this for initialization
	void Start()
	{
		Application.backgroundLoadingPriority = ThreadPriority.Low;
	}

	bool cheatBoxPopup = false;
	void Update()
	{
		//if cheater, popup a UI to block.
		if ((_cheatFlag == 1) && !cheatBoxPopup)
		{
			string captionTxt = Localization.instance.Get("IDS_CHEATER_CAPTION");

			UIMessageBoxManager.Instance.ShowMessageBox(captionTxt,
				"",
				MB_TYPE.MB_OK,
				onCheaterPressCallback);

			cheatBoxPopup = true;
		}

		//fix a bug when time scale is lower
		//Time.fixedDeltaTime = FC_CONST.TIME_FIXEDSTEP * Time.timeScale;
	}

	//press reconnect button of lv2 message box
	void onCheaterPressCallback(ID_BUTTON buttonID)
	{
		Debug.Log("press cheater button");

		string address = FCDownloadManager.CheaterHtmlAddress + "/cheater_" + LocalizationContainer.CurSystemLang + ".html";
		Application.OpenURL(address);

		Application.Quit();

	}

	private FCCommStatus _commStatus = FCCommStatus.Idle;

	public FCCommStatus CommStatus
	{
		get
		{
			return _commStatus;
		}
		set
		{
			if (_commStatus == FCCommStatus.Busy)
			{
				Debug.LogWarning("Comm status is already busy!");
			}
			_commStatus = value;
		}
	}

	#region Rate App

	public static bool _showRateApp = false;
	public static RateAppPoint _currentRatePoint = RateAppPoint.NONE;
	public enum RateAppPoint
	{
		NONE,
		AT_FUSION,
		AT_EVOLUTION,
		AT_FINIST_QUEST
	}
	public void ShowRateApp(RateAppPoint rap)
	{
		_currentRatePoint = rap;
		int ray = PlayerPrefs.GetInt("RateAppY");
		if (ray == 0 && PlayerPrefs.GetInt("RateApp" + (int)rap) == 0)
		{
			UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_RATE_OUR_GAME"), "", MB_TYPE.MB_OKCANCEL, OnRateClick);
		}
		_showRateApp = false;
	}

	public void OnRateClick(ID_BUTTON buttonID)
	{
		if (buttonID == ID_BUTTON.ID_OK)
		{
			PlayerPrefs.SetInt("RateAppY", 1);
			PlayerPrefs.Save();
			GotoRateApp();
		}
		else
		{
			PlayerPrefs.SetInt(("RateApp" + (int)_currentRatePoint), 1);
		}
		_currentRatePoint = RateAppPoint.NONE;
	}

	public void GotoRateApp()
	{
		Application.OpenURL("");
	}
	#endregion //Rate app
}
