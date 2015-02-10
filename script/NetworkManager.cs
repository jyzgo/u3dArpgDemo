using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary; 
using System.Text.RegularExpressions;

using FaustComm;

public class NetworkManager : MonoBehaviour
{
	private static NetworkManager _instance;

	public static NetworkManager Instance
	{
		get { return _instance;}
	}

	public static bool isOfflinePlay = false;
	private EnumRole _selectedRole = EnumRole.NotAvailable;
    
	public EnumRole SelectedRole
	{
		get { return _selectedRole; }
        
		set { _selectedRole = value; }
	}

    #region Client definition
	private FCClient _client;
    #endregion

	private short _serverID;      //the game server that player chooses
	public short ServerID { get { return _serverID; } set { _serverID = value; } }

	private string _account;	//device-related unique id
	public string Account { get { return _account; } set { _account = value; } }

	private string _password = "111";

	public string Password { get { return _password; } set { _password = value; } }

	private bool _isAccountBind = false;

	public bool IsAccountBind { get { return _isAccountBind; } set { _isAccountBind = value; } }

	//seconds from 1970.01.01
	public DateTime serverTime
	{
		get
		{
			return isOfflinePlay?DateTime.Now:_client.ServerTime;
		}
	}

	private float _lastSyncTime;
	public string AccountName;

	public string NickName
	{
		get;
		set;
	}

	//todo: get connnection status.
	public bool IsAuthenticated()
	{
		return false;
	}

	public string UdidDisplay
	{
		get
		{
            if (_account == null || _account.IndexOf("#") == 0)
            {
                return Localization.instance.Get("IDS_MESSAGE_LOGIN_UNBOUND");
            }
            else
            {
                if (_account.Length > 20)
                    return _account.Substring(0, 17) + "...";
                else
                    return _account;    
            }
		} 
	}

	void Awake()
	{
		_instance = this;

		this.transform.parent = LevelManager.Singleton.transform.parent;
	}
	
	// Use this for initialization
	void Start()
	{

		if (!isOfflinePlay)
		{
			_client = new FCClient(FCDownloadManager.URL_ZONE_SERVER, this, Assembly.GetAssembly(typeof(NetworkManager)));

			_client.fatalServerErrorProcessor = FatalServerErrorCallback;

			Login();
		}
		else
		{
			LoadCharacterSelectionScene();
		}
	}

	private void FatalServerErrorCallback(NetResponse msg)
	{
		UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), null, MB_TYPE.MB_OK, OnFatalError);
	}

	private void OnFatalError(ID_BUTTON buttonID)
	{
		Application.LoadLevel("PreBoot");	//restart game
	}

	void LateUpdate()
	{
		if (!isOfflinePlay)
		{
			if (null != _client)
			{
				_client.Update();
			}
		}
	}

	void OnDestroy()
	{
		_client = null;
	}

	public void RegisterNewUser(string account, string password, string os, ServerCallbackDelegate callback)
	{
		ResetSession(0, 0);

		Debug.Log("Trying to register new user... account = " + account);

		RegisterRequest cmd = new RegisterRequest();
		cmd.account = account;
		cmd.password = password;
		cmd.os = os;

		SendCommand(cmd, callback);

	}

	private void GetLoginInfoFromCache()
	{
		if (!PlayerPrefs.HasKey(PrefsKey.Account))
		{
			_account = NetworkUtils.GetUDID();
			PlayerPrefs.SetString(PrefsKey.Account, _account);
			Debug.Log("UDID = " + _account);
		}
		else
		{
			_account = PlayerPrefs.GetString(PrefsKey.Account);
		}

		if (!PlayerPrefs.HasKey(PrefsKey.Password))
		{
			_password = NetworkUtils.GetUDPassword(_account);
			PlayerPrefs.SetString(PrefsKey.Password, _password);
		}
		else
		{
			_password = PlayerPrefs.GetString(PrefsKey.Password);
		}

		if (!PlayerPrefs.HasKey(PrefsKey.ServerID))
		{
			_serverID = NetworkUtils.GetUDServerID();
			PlayerPrefs.SetInt(PrefsKey.ServerID, _serverID);
		}
		else
		{
			_serverID = (short)PlayerPrefs.GetInt(PrefsKey.ServerID);
		}
	}

	private void Login()
	{
		GetLoginInfoFromCache();

		_isAccountBind = _account.Contains("#") ? false : true;

		OnLogin(_account, _password, OnLoginCallback);
	}

	private void OnLoginCallback(NetResponse msg)
	{
		if (msg.errorCode == 0)
		{
			LoginResponse myMsg = (LoginResponse)msg;

			_client.accountID = myMsg.accountId;

			_client.sessionID = myMsg.session;

			Debug.Log(string.Format("Login succeeded. AccountId = {0}  Session id = {1}", myMsg.accountId, myMsg.session));

			LoadCharacterSelectionScene();
		}
		else
		{
			Debug.Log("Login failed. Error code = " + msg.errorCode);

			switch (msg.errorCode)
			{
				case 1001:
					_account = NetworkUtils.GetUDID();
					PlayerPrefs.SetString(PrefsKey.Account, _account);
					_password = NetworkUtils.GetUDPassword(_account);
					PlayerPrefs.SetString(PrefsKey.Password, _password);

					NetworkManager.Instance.RegisterNewUser(_account, _password, string.Empty, OnRegister);
					break;
				case 1005:

					Debug.LogError(" Account of illegal ");

					break;
			}

            
		}
	}

	private void LoadCharacterSelectionScene()
	{
		StopAllCoroutines();
		NetworkManager.Instance.SelectedRole = EnumRole.NotSelected;
		Application.LoadLevelAsync("character_selection");
	}

	private void OnRegister(NetResponse msg)
	{
		Debug.Log("called back.");

		if (msg.errorCode == 0)
		{
			RegisterResponse myMsg = (RegisterResponse)msg;
			_client.accountID = myMsg.accountId;
			_client.sessionID = myMsg.session;
			Debug.Log("Registered: " + ((RegisterResponse)msg).accountId);

			LoadCharacterSelectionScene();
		}
	}

	public void OnLogin(string account, string password, ServerCallbackDelegate callback)
	{
		_client.accountID = 0;
		_client.sessionID = 0;
		LoginRequest req = new LoginRequest();
		req.account = account;
		req.password = password;

		SendCommand(req, callback);
	}

	public void OnBindAccount(string account, string password, ServerCallbackDelegate callback)
	{
		BindRequest bind = new BindRequest();
		bind.account = _account;
		bind.password = _password;
		bind.newAccount = account;
		bind.newPassword = password;

		SendCommand(bind, callback);
	}

	public void ResetSession(int accountID, int sessionID)
	{
		_client.accountID = accountID;

		_client.sessionID = sessionID;
	}

	public void GetCharacters(FaustComm.ServerCallbackDelegate callback)
	{
		GetCharactersRequest request = new GetCharactersRequest();
		request.serverId = _serverID;

		SendCommand(request, callback);
	}

	public void CreateNewCharacterRequest(string nickName, byte roleID, ServerCallbackDelegate OnCreateNewCharacter)
	{
		CreatePlayerRequest request = new CreatePlayerRequest();
		request.job = roleID;
		request.serverId = _serverID;
		request.nickName = nickName;
		SendCommand(request, OnCreateNewCharacter);
	}

	public void EnterGameRequest(int UID, ServerCallbackDelegate callback)
	{
		EnterGameRequest request = new EnterGameRequest();
		request.ServerId = _serverID;
		request.UID = UID;
		SendCommand(request, callback);
	}

    #region inventory
    public void InventoryOperation(long itemGUID, InventoryMenuItemOperationType operationType, ServerCallbackDelegate callback)
	{
		ItemOperationRequest request = new ItemOperationRequest();
		request.ItemGuid = itemGUID;
		request.OprationType = operationType;
		SendCommand(request, callback);
	}

    public void IncrementInventory(ServerCallbackDelegate callback)
    {
        IncrementInventoryRequest request = new IncrementInventoryRequest();
        SendCommand(request, callback);
    }
    #endregion

    #region fusion
	public void FusionEquipment(Int64 itemGUID, ServerCallbackDelegate callback)
	{
		FusionRequest request = new FusionRequest();
		request.IsUseHC = 0;
		request.ItemGUID = itemGUID;
		SendCommand(request, callback);
	}
    #endregion

    #region mail
    public void MailsList(int startMailId, ServerCallbackDelegate callback)
    {
        MailListRequest request = new MailListRequest();
        request.StartMailId = startMailId;
        SendCommand(request, callback);
    }

    public void MailDetail(int mailId, ServerCallbackDelegate callback)
    {
        MailDetailRequest request = new MailDetailRequest();
        request.MailId = mailId;
        SendCommand(request, callback);
    }

    public void MailOperate(int mailId, MailOperation operation, ServerCallbackDelegate callback)
    {
        MailOperationRequest request = new MailOperationRequest();
        request.MailId = mailId;
        request.Operation = operation;
        SendCommand(request, callback);
    }
    #endregion

    #region store

    public void StoreGetList(ServerCallbackDelegate callback)
    {
        StoreGetListRequest request = new StoreGetListRequest();
        SendCommand(request, callback);
    }

    public void StoreBuy(int goodsId, ServerCallbackDelegate callback)
    {
        StoreBuyRequest request = new StoreBuyRequest();
        request.GoodsId = goodsId;
        SendCommand(request, callback);
    }

    public void StoreSCExchange(int type, ServerCallbackDelegate callback)
    {
        StoreSCExchangeRequest request = new StoreSCExchangeRequest();
        request.Type = type;
        SendCommand(request, callback);
    }

    public void StoreQueryGoodsHCPrice(List<InventoryHCWorth> WantBuyItems, ServerCallbackDelegate callback)
    {
        StoreHCPriceQueryRequest request = new StoreHCPriceQueryRequest();
        request.WantBuyItems = WantBuyItems;
        SendCommand(request, callback);
    }

    public void StoreBuyInventoryGoods(List<InventoryHCWorth> inventoryHCBuys, ServerCallbackDelegate callback)
    {
        InventoryItemsListBuyRequest request = new InventoryItemsListBuyRequest();
        request.Items = inventoryHCBuys;
        SendCommand(request, callback);
    }

    #endregion

    public void EnterBattle(int levelID, ServerCallbackDelegate callback)
	{
		BeginBattleRequest request = new BeginBattleRequest();

		request.levelID = levelID;

		SendCommand(request, callback);
	}

    #region offering

	public void OnSendOfferingRequset(byte type, string itemId, byte useHc, ServerCallbackDelegate callback)
	{
		OfferingRequest request = new OfferingRequest();

		request.type = type;
		request.itemId = itemId;
		request.useHc = useHc;

		SendCommand(request, callback);
	}

    #endregion

	//retrieve states of all levels
	public void GetLevelsState(ServerCallbackDelegate callback)
	{
		SendCommand(new LevelStatesRequest(), callback);
	}

	public void GetSkills(ServerCallbackDelegate callback)
	{
		SendCommand(new GetSkillRequest(), callback);
	}

	public void GetTattoos(ServerCallbackDelegate callback)
	{
		SendCommand(new PlayerTattooRequest(), callback);
	}

	public void BattleEnd(BattleEndRequest request, ServerCallbackDelegate callback)
	{
		SendCommand(request, callback);
	}

	public void UseItemInBattle(int levelID, string itemID, byte count, byte useHC, ServerCallbackDelegate callback)
	{
		BattleUseItemRequest request = new BattleUseItemRequest();

		request.levelID = levelID;

		request.itemID = itemID;

		request.count = count;

		request.useHC = useHC;

		SendCommand(request, callback);
	}

	public void ReviveInBattle(int levelID, string itemID, byte count, byte useHC, ServerCallbackDelegate callback)
	{
		BattleUseReviveRequest request = new BattleUseReviveRequest();

		request.levelID = levelID;

		request.itemID = itemID;

		request.count = count;

		request.useHC = useHC;

		SendCommand(request, callback);
	}

	public void SendCommand(NetRequest request, ServerCallbackDelegate callback)
	{
		if(!isOfflinePlay){
			Debug.Log("[SEND MSG]"+request.GetType().Name+"=>");
			_client.SendCommand(request, delegate(NetResponse msg) {
				callback(msg);
				Debug.Log("[RECIVE MSG]"+request.GetType().Name+"<="+msg.ToString());
			});
			return;
		}

		Regex regex = new Regex("Request");
		string cmdname = regex.Split(request.GetType().Name)[0];
		FileInfo info = new FileInfo(Application.streamingAssetsPath+"/command/"+cmdname+".txt");
		if(!info.Exists){
			Debug.Log("[NOT RUN MSG]"+cmdname+"");
			return;
		}

		NetResponse response = Activator.CreateInstance(
			Assembly.GetAssembly(request.GetType())
			.GetType(cmdname+"Response")
			) as FaustComm.NetResponse;
		FileStream fs = info.OpenRead();
		fs.Position = 0;
		BinaryReader br = new BinaryReader(fs);
		response.Decode(br);
		callback(response);
		fs.Close();
		fs.Dispose();
		Debug.Log("[RUN MSG]"+cmdname+"");
	}



}