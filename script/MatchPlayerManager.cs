using UnityEngine;
using System.Collections.Generic;


public class MatchPlayerProfile
{
	public string _externalIP;
	public int _externalPort;
	//public bool _host;
	public bool _isDropped;
	public PlayerInfo _playerInfo;
}

public class MatchPlayerManager : Photon.MonoBehaviour 
{
	public NotifyEventHandler _eventNumPlayersChanged;
	
	public List<MatchPlayerProfile> _playersProfile = new List<MatchPlayerProfile>();
	
	public int NumPlayers
	{
		get
		{
			return _playersProfile.Count;
		}
	}
	
	// Only Used on Client
	private int _playerIndex = 0;
	
	#region Singleton
	private static MatchPlayerManager _instance = null;
	public static MatchPlayerManager Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = FindObjectOfType(typeof(MatchPlayerManager)) as MatchPlayerManager;
			}
			
			return _instance;
		}
	}
	
	void OnDestroy()
	{
		if(_instance == this)
		{
			_instance = null;
		}
	}	
	#endregion
	
	// Use this for initialization
	void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}
	
	private new PhotonView photonView;
		
	void Start () 
	{
		photonView.viewID = (int)FC_PHOTON_STATIC_SCENE_ID.MATCH_PLAYER_MGR;
	}
	
	void OnCreatedRoom()
	{	
		//MatchPlayerProfile profile = new MatchPlayerProfile();
		////profile._host = true;
		//PlayerInfoManager.Instance.LocalPlayerInfo.UpdateTableInfo();
		//profile._playerInfo = PlayerInfoManager.Instance.LocalPlayerInfo.CopyFCPlayerInfo();
		
		//_playersProfile.Add(profile);
	}
	
	// Called on Host
	void OnPhotonPlayerConnected(PhotonPlayer player) 
	{
		if (PhotonNetwork.isMasterClient)
		{
			if (_eventNumPlayersChanged != null)
			{
				_eventNumPlayersChanged(_playersProfile.Count);
			}
		}
	}
	
	
	//master switched, in battle, drop to single play
	void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		//if the player is host, to single play
		if (GameManager.Instance.GameState == EnumGameState.InBattle)
		{	
			//dropped to single play in battle	
			Debug.Log("[Photon] master left, drop to single play in battle!");
			PhotonNetwork.LeaveRoom();
			DropToSinglePlay();
		}	
	}	
	
	
	
	
//maybe host drop, maybe client
	void OnPhotonPlayerDisconnected(PhotonPlayer player) 
	{
		if (_eventNumPlayersChanged != null)
		{
			_eventNumPlayersChanged(_playersProfile.Count);
		}
		
		
		if (GameManager.Instance.GameState == EnumGameState.InBattle)
		{
			//the dropped player is host or not
//			bool isHost = false;
//			foreach (MatchPlayerProfile profile in _playersProfile)
//			{
//				if (player.name == profile._playerInfo._id)
//				{
//					isHost = profile._host;
//					break;
//				}
//			}
			
			
			//if host drop, do nothing, I already leave room
			//if client drop and I am the host
			if ((PhotonNetwork.isMasterClient) /*&& (!isHost)*/)
			{
				Debug.Log("[Photon] a player drop " + player.name);
		        PhotonNetwork.RemoveRPCs(player);
		        PhotonNetwork.DestroyPlayerObjects(player);			
				
				//find player index and put it into to-be-remove list
				//List<MatchPlayerProfile> tobeRemovePlayersProfile = new List<MatchPlayerProfile>();
					
				
				//find this player index in host
				int playerIndex = 0;
				foreach (MatchPlayerProfile profile in _playersProfile)
				{
					if (player.name == profile._playerInfo.Nickname)
					{
						//tobeRemovePlayersProfile.Add(profile);
						break;
					}
					playerIndex++;
				}
		
				//broad to all clients that drop a player
				BroadcastDropPlayer(playerIndex);
				
				//drop it on server
				DropClientAt(playerIndex);
				
				//in battle, we should not remove the profile, many errors may occur
				//in town, match making page, remove it.
//				if (GameManager.Instance.CurrentStep == GameManager.FC_GAME_STEPS.GAME_TOWN)
//				{
//					foreach (MatchPlayerProfile profile in tobeRemovePlayersProfile)
//					{
//						_playersProfile.Remove(profile);
//					}		
//				}				

			}

		}
	}
	
	// Called on Player connect to server.
	void OnJoinedRoom()
	{
		if (!PhotonNetwork.isMasterClient)
		{
			Debug.Log("[RPC] [send] CommitProfileToHost " + PhotonNetwork.player.name);
			//PlayerInfoManager.Instance.LocalPlayerInfo.UpdateTableInfo();
			
			//photonView.RPC("CommitProfileToHost", PhotonTargets.MasterClient, 
			//    PhotonNetwork.player.name,
			//    PlayerInfoManager.Instance.LocalPlayerInfo._id, 
			//    PlayerInfoManager.Instance.LocalPlayerInfo.Nickname,
			//    PlayerInfoManager.Instance.LocalPlayerInfo._class,
			//    PlayerInfoManager.Instance.LocalPlayerInfo.GetAllEquipmentIds(),
			//    PlayerInfoManager.Instance.LocalPlayerInfo.GetAllSkillState(),
			//    PlayerInfoManager.Instance.LocalPlayerInfo._level,
			//    PlayerInfoManager.Instance.LocalPlayerInfo.GetEquipmentAttribute());
		}
		
	}	

	
	[RPC]// Called on Host, receive from clients
	void CommitProfileToHost(string playerName,
		string userId,
		string userName, 
		int classType,
		string allIds,
		string allSkillLvIds,
		int level,
		string attributes)
	{
		Debug.Log("[RPC] [receive] message from " + playerName);
		
		MatchPlayerProfile profile = new MatchPlayerProfile();

		//profile._host = false;

		//profile._playerInfo = new OLD_PlayerInfo();
		//profile._playerInfo.RoleID = classType;
		//profile._playerInfo._id = userId;
		//profile._playerInfo.Nickname = userName;
		//profile._playerInfo.SetAllEquipmentIds(allIds);
		//profile._playerInfo.SetAllSkillState(allSkillLvIds);
		//profile._playerInfo._level = level;
		//profile._playerInfo.SetEquipmentAttribute(attributes);
		_playersProfile.Add(profile);
		
		if(PhotonNetwork.room.maxPlayers == _playersProfile.Count)
		{
			StartMutiPlayerGame();
		}
	}		
	

	
	//broad from host to clients
    public void BroadcastPlayersProfile()
	{
		Debug.Log("[RPC] [send] Broadcast Players Profile ");
		
		int playerIndex = 0;
				
		foreach (MatchPlayerProfile profile in _playersProfile)
		{	
			photonView.RPC("ReceiveProfilesFromHost", PhotonTargets.OthersBuffered, playerIndex,
//				profile._externalIP,
//				profile._externalPort, 
//				profile._host,
				//profile._playerInfo._id,
				//profile._playerInfo.Nickname,
				//profile._playerInfo._class,
				//profile._playerInfo.GetAllEquipmentIds(),
				//profile._playerInfo.GetAllSkillState(),
				//profile._playerInfo._level,
				//profile._playerInfo.GetEquipmentAttribute(),
				MultiplayerDataManager.Instance.PvPTimestamp
			);
			
			playerIndex++;
		}
	}
	
	[RPC]// Called from the host
	void ReceiveProfilesFromHost(int playerIndex, // bool host, 
		string userId, 
		string userName, 
		int classType,
		string allIds,
		string allSkillLvIds,
		int level,
		string attributes,
		string pvpTimestamp)
	{
		Debug.Log("[RPC] [receive] Players Profiles from Host");
		
		MatchPlayerProfile profile = new MatchPlayerProfile();

//		profile._host = host;

		//profile._playerInfo = new OLD_PlayerInfo();
		//profile._playerInfo._class = classType;
		//profile._playerInfo._id = userId;
		//profile._playerInfo.Nickname = userName;
		//profile._playerInfo.SetAllEquipmentIds(allIds);	
		//profile._playerInfo.SetAllSkillState(allSkillLvIds);
		//profile._playerInfo._level = level;
		//profile._playerInfo.SetEquipmentAttribute(attributes);
		
		_playersProfile.Add(profile);
		
		//todo: sometimes player index will be wrong.
		if (PhotonNetwork.player.name == userId)
		{
			Debug.Log("my index is: " + playerIndex);
			_playerIndex = playerIndex;
		}
		MultiplayerDataManager.Instance.PvPTimestamp = pvpTimestamp;
	}
	
	//broad from host to all clients, drop a specific player
    public void BroadcastDropPlayer(int playerIndex)
	{
		Debug.Log("[RPC] [send] Broadcast drop player at " + _playerIndex);
		photonView.RPC("ReceiveDropPlayer", PhotonTargets.OthersBuffered, playerIndex);
	}	
	
	[RPC]// Called on Host, receive from clients
	void ReceiveDropPlayer(int playerIndex)
	{
		Debug.Log("[RPC] [receive] drop player at " + playerIndex);
		DropClientAt(playerIndex);				
	}		
	
	void StartMutiPlayerGame()
	{
		//close room when enter game
		Room room = PhotonNetwork.room;
		if (room != null)
		{
			room.open = false;
			room.visible = false;
		}		
		
		MultiplayerDataManager.Instance.PvPTimestamp = NetworkManager.Instance.serverTime.ToString();
		
		//broad profile info to all others
		MatchPlayerManager.Instance.BroadcastPlayersProfile();		

		//todo: 
	}
	
	public int GetPlayerIndex()
	{
		if (Debug.isDebugBuild)
		{
			if (_playerIndex > _playersProfile.Count)
			{
				Debug.LogError("player exceed the profile count!");				
			}
		}
		
		return _playerIndex;
	}
	
	
	public short GetPlayerNetworkIndex()
	{
		return (short)(_playerIndex + FCConst.k_network_id_hero_start);
	}	
	
	
	public int GetPlayerCount()
	{
		return _playersProfile.Count;
	}
	
	public MatchPlayerProfile GetMatchPlayerProfile(int playerIndex)
	{
		if (playerIndex < _playersProfile.Count)
		{
			return _playersProfile[playerIndex];
		}
		
		Debug.LogError("The match player index is not exist!");
		
		return null;
	}
	
	public void Clean()
	{
		_playersProfile.Clear();
		
		_playerIndex = 0;
	}
	

	
	//drop and go to single play, only happens on clients when host drops
	private void DropToSinglePlay()
	{
		
		//search for other players and disable them
		int myPlayerID = GetPlayerNetworkIndex();
		for (int i=0; i<FCConst.MAX_PLAYERS; i++)
		{
			OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(i + FCConst.k_network_id_hero_start);
			if (object_ID != null)
			{
				if (object_ID.NetworkId != myPlayerID)
				{
					LevelManager.Singleton.DeactiveHero(i);
				}
			}
		}
	
		//rebuild triggers
		LevelManager.Singleton.RefreshTriggersViaEnemies();
		
		//remove sync objects
		bool needShowErrorMsg = true;
		if(GameManager.Instance.IsPVPMode
		   && PvPBattleSummary.Instance != null
		   && PvPBattleSummary.Instance.IsFinish
			)
		{
			needShowErrorMsg = false;
		}
		if(needShowErrorMsg)
		{
			UIMessageBoxManager.Instance.ShowMessageBox("You have disconnected from host and will continue with single palyer mode!", 
				"MessageBox", MB_TYPE.MB_OK, OnClickGoToTownCallback);
		}
	}
	
	//a player drop and remain multi-play
	private void DropClientAt(int playerIndex)
	{
		GetMatchPlayerProfile(playerIndex)._isDropped = true;
		int playerNetworkIndex = playerIndex + FCConst.k_network_id_hero_start;
		//search for the player and disable it
		for (int i=0; i<FCConst.MAX_PLAYERS; i++)
		{
			OBJECT_ID object_ID = ObjectManager.Instance.GetObjectByNetworkID(i + FCConst.k_network_id_hero_start);
			if (object_ID != null)
			{
				if (object_ID.NetworkId == playerNetworkIndex)
				{
					LevelManager.Singleton.DeactiveHero(i);
					
					//remove sync objects
					break;
				}
			}
		}	

		bool needShowErrorMsg = true;
		if(GameManager.Instance.IsPVPMode
		   && PvPBattleSummary.Instance != null
		   && PvPBattleSummary.Instance.IsFinish
			)
		{
			needShowErrorMsg = false;
		}
		//show drop player message box in battle
		if (GameManager.Instance.GameState == EnumGameState.InBattle && needShowErrorMsg)
		{
			MatchPlayerProfile profile = GetMatchPlayerProfile(playerIndex);
			string text = string.Format("{0} has left from our matchmaking", profile._playerInfo.DisplayNickname);
			MessageController.Instance.AddMessage(2.0f, text);
//			UIMessageBoxManager.Instance.ShowMessageBox(text, "MessageBox", MB_TYPE.MB_OK, OnClickGoToTownCallback);		
		}
	}

	void Update()
	{

	}
	
	void OnClickGoToTownCallback(ID_BUTTON buttonID)
    {
        if (buttonID == ID_BUTTON.ID_OK && GameManager.Instance.GameState == EnumGameState.InBattle)
        {
			ActionController ac = ObjectManager.Instance.GetMyActionController();
			if(ac != null)
			{
				ac.AIUse.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
			}
			GameManager.Instance.GamePaused = false;
	        LevelManager.Singleton.ExitLevel();
        }
    }
}


