using UnityEngine;
using System.Collections;
using InJoy.FCComm;

public enum PHOTON_ROOM_PROPERTYS
{
	NAME = 0,
	//LEVEL,
	
	//match filter int type
	C0, // int type sql filter 
	C1, // int type sql filter
	//match filter string type
	C5, // string type sql filter 
	C6, // string type sql filter
};


public enum PHOTON_SERVER_LIST
{
	Asia = 0,
	Europe,
	US,
	Beijing,
	PhotonTest,
	
	Count
};

public class PhotonManager : MonoBehaviour {
	
//	PHOTON_SERVER_LIST _currentServer = PHOTON_SERVER_LIST.Asia;
	
	private bool _creatingGame = false;
	
	public bool _enableDebug = false;
	
	private int _currMatchStep = 0;
	
	public delegate void _OnPhotonConnectedComplete(bool success);
	public _OnPhotonConnectedComplete OnPhotonConnectedComplete;	
	
	#region Singleton
	private static PhotonManager _instance = null;
	public static PhotonManager Instance
	{
		get
		{
			if (_instance == null || !_instance)
			{
				_instance = FindObjectOfType(typeof(PhotonManager)) as PhotonManager;
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
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
		
	}
	
	public void OnGUI() {
		
		
		if (!_enableDebug)
			return;
		
		GUI.color = Color.white;
		
		GUILayout.Label("");
		GUILayout.Label("");
		GUILayout.Label("");	
		
		if (PhotonNetwork.connectionState != ConnectionState.Connected)
			GUI.color = Color.red;	
		GUILayout.Label("[Photon] Photon status: " + PhotonNetwork.connectionState.ToString());	
		GUI.color = Color.white;
	
		//GUILayout.Label("[Photon] Photon server: " + _currentServer.ToString());	
		
		if (PhotonNetwork.insideLobby)
			GUILayout.Label("[Photon] In lobby :" + PhotonNetwork.currentLobbyName  + ", rooms count:" + PhotonNetwork.countOfRooms.ToString());	
		else if (PhotonNetwork.room != null)
			GUILayout.Label("[Photon] In room : " + PhotonNetwork.room.name);
		else
		{
			GUI.color = Color.red;
			GUILayout.Label("[Photon] not in lobby or room");
			GUI.color = Color.white;
		}	
		
		GUILayout.Label("[Photon] Ping : " + PhotonNetwork.GetPing().ToString());
		
//		GUILayout.BeginVertical("server");
		if (GUILayout.Button("Photon disconnect!", GUILayout.Width(200.0f), GUILayout.Height(50.0f)))
		{
			PhotonNetwork.Disconnect();
		}
		
		if (GUILayout.Button("Asia server", GUILayout.Width(200.0f), GUILayout.Height(50.0f)))
		{
//			_currentServer = PHOTON_SERVER_LIST.Asia;
			ConnectToPhoton();
		}

		if (GUILayout.Button("Join Lobby 00", GUILayout.Width(200.0f), GUILayout.Height(50.0f)))
		{			
			PhotonNetwork.OpJoinLobby("Test Lobby 00", LobbyType.Default);
		}
		
		if (GUILayout.Button("create game", GUILayout.Width(200.0f), GUILayout.Height(50.0f)))
		{
			CreateGame();
		}		
		
		if (GUILayout.Button("Join Random Game", GUILayout.Width(200.0f), GUILayout.Height(50.0f)))
		{
			PhotonNetwork.JoinRandomRoom("Test Lobby 00", LobbyType.SqlLobby, "C0 < 100 AND C0 > 5");
		}
		
//		GUILayout.EndVertical();
	
		
	}
	
	
	//create game
	public void StartCreateGame() {
		
		if (!PhotonNetwork.connected)
		{
			Debug.LogWarning("Photon - connect to photon first!");	
			return;
		}
		
		if (!PhotonNetwork.insideLobby)
		{
			Debug.LogWarning("Photon - go back to lobby first!");	
			return;
		}
		
		if (_creatingGame)
		{
			Debug.LogWarning("Photon - I am creating my game!");	
			return;
		}
		else
			_creatingGame = true; //begin to create
		

		//already connect
		//create game	
		CreateGame();
		
	}
		

	public void ConnectToPhoton() {
		if (PhotonNetwork.connected)
			Debug.LogError("cannot switch server when connect");

        Debug.Log(PhotonNetwork.playerName);		
		
		PhotonNetwork.PhotonServerSettings.ServerAddress = 
			PhotonNetwork.PhotonServerSettings._serverAddressList[MultiplayerDataManager.Instance.PhotonServerSelectIndex];
			
		PhotonNetwork.networkingPeer.CrcEnabled = true;
		PhotonNetwork.SwitchToProtocol(PhotonNetwork.PhotonServerSettings.ConnectionProtocol);
		//PhotonNetwork.PhotonServerSettings.ServerPort = 4530;

		string gameVersion = string.Empty;
#if DEVELOPMENT_BUILD
		gameVersion = FCConst.PHOTON_CONNECT_VERSION_STAGE;
#else
		gameVersion = FCConst.PHOTON_CONNECT_VERSION_LIVE;
#endif
		PhotonNetwork.Connect(PhotonNetwork.PhotonServerSettings.ServerAddress, 
			PhotonNetwork.PhotonServerSettings.ServerPort,
			PhotonNetwork.PhotonServerSettings.AppID,
			gameVersion);
		

	//	PhotonNetwork.ConnectUsingSettings("0.0.1");	
	}	
	
	
	//create room with default filter
	public void CreateGame() {
		
		//build a ramdom room name
		//int seed = UnityEngine.Random.Range(0, 100000);
		//string roomName = PhotonNetwork.playerName + seed.ToString();		
		string roomName = NetworkManager.Instance.AccountName;
			
		//build property table
		Hashtable customRoomProperties = new Hashtable();
		customRoomProperties[PHOTON_ROOM_PROPERTYS.NAME.ToString()] = roomName;
		customRoomProperties[PHOTON_ROOM_PROPERTYS.C0.ToString()] = MultiplayerDataManager.Instance.GroupLv;  // player level
		customRoomProperties[PHOTON_ROOM_PROPERTYS.C1.ToString()] = 0;  // 
		customRoomProperties[PHOTON_ROOM_PROPERTYS.C5.ToString()] = WorldMapController.LevelName;  // 
		customRoomProperties[PHOTON_ROOM_PROPERTYS.C6.ToString()] = "";  // 
		
		//build property list
		string[] propsToListInLobby	= new string[customRoomProperties.Count];
		propsToListInLobby[(int)PHOTON_ROOM_PROPERTYS.NAME] = PHOTON_ROOM_PROPERTYS.NAME.ToString();
		propsToListInLobby[(int)PHOTON_ROOM_PROPERTYS.C0] = PHOTON_ROOM_PROPERTYS.C0.ToString();
		propsToListInLobby[(int)PHOTON_ROOM_PROPERTYS.C1] = PHOTON_ROOM_PROPERTYS.C1.ToString();
		propsToListInLobby[(int)PHOTON_ROOM_PROPERTYS.C5] = PHOTON_ROOM_PROPERTYS.C5.ToString();
		propsToListInLobby[(int)PHOTON_ROOM_PROPERTYS.C6] = PHOTON_ROOM_PROPERTYS.C6.ToString();
		
		string lobbyName = GameManager.Instance.CurrGameMode.ToString();
		
		PhotonNetwork.CreateRoom(roomName,
			true,
			true,
			FCConst.MAX_PLAYERS,
			customRoomProperties,
			propsToListInLobby,
			lobbyName,
			LobbyType.SqlLobby
			);
		
		Debug.Log("Photon - Create a new game ! roomName = " + roomName + " , lobbyName = " + lobbyName +
			" , C0 = " + customRoomProperties[PHOTON_ROOM_PROPERTYS.C0.ToString()] +
			" , C1 = " + customRoomProperties[PHOTON_ROOM_PROPERTYS.C1.ToString()] +
			" , C5 = " + customRoomProperties[PHOTON_ROOM_PROPERTYS.C5.ToString()] +
			" , C6 = " + customRoomProperties[PHOTON_ROOM_PROPERTYS.C6.ToString()]
			);
	}
	
	//join a room
	public void JoinGame(string hostName)
	{
		PhotonNetwork.JoinRoom(hostName);
	}
	
	
	public void JoinRandomRoom(string lobbyName, LobbyType lobbyType, string sqlLobbyFilter)
	{
		PhotonNetwork.JoinRandomRoom(lobbyName , lobbyType , sqlLobbyFilter);
	}
	
	public void Disconnect()
	{
		if(GameManager.Instance.GameState == EnumGameState.InBattle)
			GameManager.Instance.GameState = EnumGameState.BattleQuit;
		//MatchPlayerManager.Instance.Clean();
		PhotonNetwork.Disconnect();
	}
	#region Callbacks
	
	void OnConnectedToPhoton()
	{
		Debug.Log("Photon - connect to photon success");
		
		if (OnPhotonConnectedComplete != null)
			OnPhotonConnectedComplete(true);
		
	}
	
	void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.LogError(string.Format("Photon - connect to photon failed! {0}", cause));

		if (OnPhotonConnectedComplete != null)
			OnPhotonConnectedComplete(false);
	}
	
	void OnConnectionFail(DisconnectCause cause)
	{ 
		Debug.LogError(string.Format("Photon - disconnect to photon! {0}", cause));
	}
		
	void OnDisconnectedFromPhoton()
	{
		Debug.LogError("Photon - disconnect to photon!");
		Disconnect();
		BroadcastMessage("OnHandleDisconnectedFromPhoton" ,SendMessageOptions.DontRequireReceiver);
	}
		
	void OnConnectedToMaster()
	{
		Debug.Log("Photon - connected to master server");
	}
	
	void OnJoinedLobby()
	{
		Debug.Log("Photon - enter lobby");		
	}
	
	void OnLeftLobby()
	{
		Debug.Log("Photon - left lobby");		
	}	
	
	
	void OnJoinedRoom()
	{
		Debug.Log("Photon - join room");		
	}

	
	void OnCreatedRoom()
	{
		_creatingGame = false;	
		Debug.Log("Photon - create room success !");
	}	
	
	void OnLeftRoom()
	{
		Debug.Log("Photon - left room");		
	}	
	
	void OnPhotonCreateRoomFailed()
	{
		_creatingGame = false;	
		Debug.LogError("Photon - create room failed!");		
	}	
	
	void OnPhotonJoinRoomFailed()
	{
		//Logger.Log("Photon - join room failed!");		
	}	
		
	void OnPhotonRandomJoinFailed()
	{
		Debug.Log("Photon - join find match player failed!");
		_currMatchStep++;
		if(_currMatchStep <  MultiplayerDataManager.Instance.PvPMatchRulerList.Count)
		{
			DoMatchMaking(_currMatchStep);
		}else{
			Debug.Log("Photon - All matchmaking failed! Start to create a new game !");
			StartCreateGame();
		}
		
	}
	#endregion
	
	public void StartMatchMaking()
	{
		_currMatchStep = 0;
		DoMatchMaking(_currMatchStep);
	}
	
	void DoMatchMaking(int matchFilterIndex)
	{
		string lobbyName = GameManager.Instance.CurrGameMode.ToString();
		string matchFilter = MultiplayerDataManager.Instance.PvPMatchRulerList[matchFilterIndex] + 
							 string.Format( " AND C5 = \"{0}\" AND C6 = \"{1}\"", WorldMapController.LevelName , "");

		Debug.Log("Photon - [matchmaking filter] = " + matchFilter);	
		PhotonManager.Instance.JoinRandomRoom( lobbyName , LobbyType.SqlLobby , matchFilter );
		//PhotonManager.Instance.JoinRandomRoom( "" , LobbyType.SqlLobby , matchFilter );
	}
	
}
