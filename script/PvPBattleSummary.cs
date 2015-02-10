using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;
using InJoy.Utils;
using InJoy.RuntimeDataProtection;

[System.Serializable]
public class PvPPlayerSummaryData
{
	public string _playerID;
	public int _playerIndex;
	public int _mmr;
	public int _TotalLP;
	public int _winnerLP;
	public int _killsLP;
	public int _firstBloodLP;
	public int _tripleKillsLP;
	public string _result;
}

public class PvPBattleSummary : MonoBehaviour
{
	public bool IsFinish {get; set;}

	private float _startTime;
	private float _consumeTime;
	private float _endTime;
	
	public float TimeConsumed
	{
		get
		{
			if (!IsFinish)
			{
				return (Time.time-_startTime);
			}
			else
			{
				return _consumeTime;
			}
		}
	}
	
	public float TimeSurplus
	{
		get
		{
			if (!IsFinish)
			{
				return (_endTime - Time.time);
			}
			else
			{
				return _endTime - _consumeTime;
			}
		}
	}
	
	private bool _needOpenSummaryForData;
	private bool _needOpenSummaryForTime;

	public bool NeedOpenSummaryForData
	{
		get
		{
			return this._needOpenSummaryForData;
		}
		set
		{
			_needOpenSummaryForData = value;
			if(value)
			{
				NeedOpenPvPBattleSummary();
			}
		}
	}

	public bool NeedOpenSummaryForTime
	{
		get
		{
			return this._needOpenSummaryForTime;
		}
		set
		{
			_needOpenSummaryForTime = value;
			if(value)
			{
				NeedOpenPvPBattleSummary();
			}
		}
	}
	
	public delegate void OnCancelBattle();
	
	//private OnCancelBattle _cancelCallback;
	
	private List<PvPPlayerSummaryData> _playerResult = new List<PvPPlayerSummaryData>();

	public List<PvPPlayerSummaryData> PlayerResult
	{
		get
		{
			return this._playerResult;
		}
	}

	#region Singleton
	private static PvPBattleSummary _instance = null;
	public static PvPBattleSummary Instance
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
			Debug.LogError("PvPBattleSummary: Another GameLauncher has already been created previously. " + gameObject.name + " is goning to be destroyed.");
			
			Destroy(this);
			
			return;
		}
		
		_instance = this;
	}
	#endregion
	
	// Use this for initialization
	void Start () {
		
		if(!GameManager.Instance.IsPVPMode)
		{
			this.enabled = false;
		}
	}
	
	public void BeginBattle()
	{
		_startTime = Time.time;
		_endTime = _startTime + 100;
		IsFinish = false;
		NeedOpenSummaryForData = false;
		NeedOpenSummaryForTime = false;
	}
	
	public void FinishBattle()
	{
		_consumeTime = Time.time - _startTime;
		_consumeTime = (int)_consumeTime;
		IsFinish = true;
		ConnectionManager.Instance.RegisterHandler(sendPvPBattleSummary, true);
	}
	
	public void CancelBattle(OnCancelBattle callback)
	{
		IsFinish = true;
		int myIndex = MatchPlayerManager.Instance.GetPlayerIndex();
		MatchPlayerManager.Instance.GetMatchPlayerProfile(myIndex)._isDropped = true;
		//_cancelCallback = callback;
		ConnectionManager.Instance.RegisterHandler(sendPvPBattleSummary, true);
	}
	
	private void sendPvPBattleSummary()
	{
		Hashtable payload = new Hashtable();
		payload.Add("messageType", "pvp");
		payload.Add("op_type", "pvp_end");
		Hashtable data = new Hashtable();

		for(int i = 0; i < MatchPlayerManager.Instance.GetPlayerCount(); i ++)
		{
			int[] playerDetials = new int[7];
			playerDetials[0] = MultiplayerDataManager.Instance.PvPStatisticsList[i].Kills;
			playerDetials[1] = MultiplayerDataManager.Instance.PvPStatisticsList[i].Deaths;
			playerDetials[2] = MultiplayerDataManager.Instance.PvPStatisticsList[i].IsTripleKill ? 1 : 0;
			playerDetials[3] = MultiplayerDataManager.Instance.PvPStatisticsList[i].FirstBlood ? 1 : 0;
			
			OBJECT_ID objectID = ObjectManager.Instance.GetObjectByNetworkID(i + FCConst.k_network_id_hero_start);
			if (objectID != null)
			{
				ActionController ac = objectID.fcObj as ActionController;
				MultiplayerDataManager.Instance.PvPStatisticsList[i].HealthPoint = Mathf.FloorToInt( ac.HitPointPercents * 100 );
			}
			playerDetials[4] = MultiplayerDataManager.Instance.PvPStatisticsList[i].HealthPoint;
			playerDetials[5] = MultiplayerDataManager.Instance.PvPStatisticsList[i].PositionOfKills; // position of current kills
			playerDetials[6] = MatchPlayerManager.Instance.GetMatchPlayerProfile(i)._isDropped ? 1 : 0; // dropped match
			//data.Add(MatchPlayerManager.Instance.GetMatchPlayerProfile(i)._playerInfo._id, playerDetials);
		}
		
		

		string str = FCJson.jsonEncode(data);
		str = Utils.DesEncrypt(str);
		payload.Add("data", str);
        Utils.CustomGameServerMessage(null, OnSendBattleSummary);
	}

    public void OnSendBattleSummary(FaustComm.NetResponse response)
    {
    }
	
	private void NeedOpenPvPBattleSummary()
	{
		if(NeedOpenSummaryForData && NeedOpenSummaryForTime)
		{
			UIManager.Instance.CloseUI("PvPHUDUI");
			UIManager.Instance.OpenUI("PvPBattleSummaryUI");
		}
	}
	
	public void GotoEndBattle()
	{
//		PhotonManager.Instance.Disconnect();
		UIManager.Instance.CloseUI("HomeUI");
		UIManager.Instance.OpenUI("BattleSummeryTimerUI");
		FinishBattle();
	}
	
	public void ReadyToReviveMySelf()
	{
		Invoke("ReviveMySelf", 3.0f);
	}
	
	public void ReviveMySelf()
	{
		UIManager.Instance.OpenUI("HomeUI");
		CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.REVIVE,
			ObjectManager.Instance.GetMyActionController().ObjectID,
			ObjectManager.Instance.GetMyActionController().ThisTransform.localPosition,
			null,FC_PARAM_TYPE.NONE,
			null,FC_PARAM_TYPE.NONE,
			null,FC_PARAM_TYPE.NONE);
		
		CommandManager.Instance.Send(FCCommand.CMD.REVIVE,
			null,FC_PARAM_TYPE.NONE,
			null,FC_PARAM_TYPE.NONE,
			null,FC_PARAM_TYPE.NONE ,
			ObjectManager.Instance.GetMyActionController().ObjectID,
			FCCommand.STATE.RIGHTNOW , true);
		
		//ObjectManager.Instance.GetMyActionController().GoToRevive();
	}
}
