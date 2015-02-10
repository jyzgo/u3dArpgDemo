using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiplayerDataManager : Photon.MonoBehaviour
{
	/// <summary>
	/// don't directly use this
	/// </summary>
	public MultiplayerDataSet _pvpData;

	public MultiplayerDataSet PvPData {
		get {
			return this._pvpData;
		}
	}	
	
	private static MultiplayerDataManager _instance = null;
	public static MultiplayerDataManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(MultiplayerDataManager)) as MultiplayerDataManager;
			}

			return _instance;
		}
	}
	
	private List<PlayerPvPStatisticsData> _pvpStatisticsList = new List<PlayerPvPStatisticsData>();

	public List<PlayerPvPStatisticsData> PvPStatisticsList {
		get {
			return _pvpStatisticsList;
		}
	}
	
	public delegate void OnPlayerKillsChange(int playerIndex, int kills);
	
	public OnPlayerKillsChange _onPlayerKillsChange;
	
	public string PvPTimestamp
	{
		get;set;
	}
	
	private List<string> _pvpMatchRulerList = new List<string>();

	public List<string> PvPMatchRulerList
	{
		get
		{
			return this._pvpMatchRulerList;
		}
	}	
	
	private int _groupLv = 0;
	public int GroupLv
	{
		set { _groupLv = value; }
		get { return _groupLv; }
	}
	
	private int _mmr;
	public int MMR
	{
		set { _mmr = value; }
		get { return _mmr; }
	}
	
	private Color[] _playerColors;
	public Color[] PlayerIndicatorColors
	{
		get{ return _playerColors; }
	}
	
	private int _photonServerSelectIndex = -1;
	private bool _pssiNeedLoaded = true;
	public int PhotonServerSelectIndex
	{
		get
		{
			if(_pssiNeedLoaded)
			{
				_pssiNeedLoaded = false;
				int pssi = PlayerPrefs.GetInt("PSSI", -1);
				_photonServerSelectIndex = pssi;
			}
			return _photonServerSelectIndex;
		}
		set
		{
			if(_photonServerSelectIndex != value)
			{
				PlayerPrefs.SetInt("PSSI", value);
				_photonServerSelectIndex = value;
			}
		}
	}

    private int[] _positionOfKillsCounters;
	
	private new PhotonView photonView;
	void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}
	void Start ()
	{
		photonView.viewID = (int)FC_PHOTON_STATIC_SCENE_ID.MULTIPLAYER_DATA_MGR;
	}
	
	void Update ()
	{
	
	}

	public void Init()
	{
		_pvpStatisticsList.Clear();
		for(int i = 0; i < MatchPlayerManager.Instance.GetPlayerCount(); i ++)
		{
			_pvpStatisticsList.Add(new PlayerPvPStatisticsData());
		}
		
		InitData();
	}

	public void MsgMyselfDead()
	{
		int myIndex = MatchPlayerManager.Instance.GetPlayerIndex();
		photonView.RPC("OnPlayerDead", PhotonTargets.OthersBuffered, myIndex);
		OnPlayerDead(myIndex);
	}

	[RPC]
	public void OnPlayerDead(int playerIndex)
	{
		if(playerIndex >= 0 && playerIndex < _pvpStatisticsList.Count)
		{
			PlayerPvPStatisticsData pd = _pvpStatisticsList[playerIndex];
			pd.Deaths += 1;
			pd.TripleKillCounter = 0;
			// increase opponent kills, this only for 2 players
			int opponentIndex = (playerIndex + 1) % 2;
			pd = _pvpStatisticsList[ opponentIndex ];
			pd.Kills += 1;
			pd.TripleKillCounter += 1;
			_positionOfKillsCounters[pd.Kills] += 1;
			pd.PositionOfKills = _positionOfKillsCounters[pd.Kills];
			if(pd.Kills == 1 && pd.PositionOfKills == 1)
			{
				pd.FirstBlood = true;
			}
			if(_onPlayerKillsChange != null)
			{
				_onPlayerKillsChange(opponentIndex, pd.Kills);
			}
		}
		// check kills
		bool needGotoEnd = false;

		if(needGotoEnd)
		{
			PvPBattleSummary.Instance.GotoEndBattle();
		}
		else if(playerIndex == MatchPlayerManager.Instance.GetPlayerIndex())
		{
			PvPBattleSummary.Instance.ReadyToReviveMySelf();
		}
	}
	
	void InitData()
	{
		Dictionary<PlayerColorTypes, Color> d = new Dictionary<PlayerColorTypes, Color>();
		d.Add(PlayerColorTypes.Blue, PvPData._blueColor);
		d.Add(PlayerColorTypes.Green, PvPData._greenColor);
		d.Add(PlayerColorTypes.Red, PvPData._redColor);
		d.Add(PlayerColorTypes.Yellow, PvPData._yellowColor);
		
		int supportMaxNumber = MatchPlayerManager.Instance.GetPlayerCount();
		List<Color> l = new List<Color>();
		for(int i = 0; i < supportMaxNumber; i ++)
		{
			Color c = d[PvPData._playerColorTypeList[i]];
			l.Add(c);
		}
		_playerColors = l.ToArray();
	}
}
