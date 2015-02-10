using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;
using FaustComm;


public enum EnumLoadingStep
{
    step1 = 1,
    step2,
    step3,
    step4,
    step5,
    step6,
    step7,
    step8,
    step9,
    NotAvalible = 100
}

public class CharacterSelector : MonoBehaviour
{
    public GameObject preveiwRoot;   //stage for current hero
    public Transform preveiwStage;  //for moving

    public GameObject existPlayerContainer;
    public GameObject newPlayerContainer;

    public UIFightScore fightScore;
    public UILabel accountId;
    public UILabel playerNameLabel;
    public UILabel plyaerLevelLabel;
    public UILabel roleNameTitle;
    public UILabel roleIntro;
    public UIImageButton nickNameRandomButton;
    public UILabel nickNameInput;
    public UIButton startGameButton;
    public UIImageButton leftArrowButton;
    public UIImageButton rightArrowButton;

    public UIImageButton accountBindingButton;
    public UIImageButton accountSwitchButton;
    public UILabel selectServerLabel;
    public UIImageButton selectServerButton;

    #region singleton
    public static CharacterSelector Instance
    {
        get { return _instance; }
    }
    private static CharacterSelector _instance;
    #endregion

    private Dictionary<EnumRole, PlayerInfo> _playerInfos = new Dictionary<EnumRole, PlayerInfo>();

    [HideInInspector]
    public bool IsManualSelectServerComplete = false;

    public static bool IsForceToSelectServer = false;

    public bool IsFreshFish
    {
        get
        {
            foreach (PlayerInfo playerInfo in _playerInfos.Values)
            {
                if (playerInfo != null)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public float animation_duration = 1.7f; //duration of animation

    public int nickNameLengthMin = 2;

    public int nickNameLengthMax = 6;

    private EnumLoadingStep _loadingStep = EnumLoadingStep.step1;

    private EnumRole _selectedRoleID = EnumRole.NotSelected;		//current selection

    private int _requestEnterGamePlayerId;

    #region role info mapping

    private Dictionary<EnumRole, string> _defaultRoleLabels = new Dictionary<EnumRole, string>() 
    { 
        {EnumRole.Mage, "mage_selection"},
        {EnumRole.Warrior, "warrior_selection"}
    };

    private Dictionary<EnumRole, string> CharacterJobs = new Dictionary<EnumRole, string>()
    {
        {EnumRole.NotSelected, ""},
        {EnumRole.NotAvailable, ""},
        {EnumRole.Mage, "IDS_NAME_CLASS_MAGE"},
        {EnumRole.Warrior, "IDS_NAME_CLASS_WARRIOR"}
    };

    private Dictionary<EnumRole, string> k_sfx_selected = new Dictionary<EnumRole, string>()
    {
        {EnumRole.Mage, "mage_selection_show_start"},
        {EnumRole.Warrior, "warrior_selection_show"}
    };

    private Dictionary<EnumRole, string> k_sfx_start_game = new Dictionary<EnumRole,string>()
    {
        {EnumRole.Mage, "mage_selection_start"},
        {EnumRole.Warrior, "warrior_selection_start"}
    };

    private Dictionary<EnumRole, string> Role_Introduction_Keys = new Dictionary<EnumRole, string>()
    {
        {EnumRole.NotSelected, ""},
        {EnumRole.NotAvailable, ""},
        {EnumRole.Mage ,"IDS_DESCRIPTION_CLASS_MAGE"},
        {EnumRole.Warrior, "IDS_DESCRIPTION_CLASS_WARRIOR"}
    };

    private Dictionary<EnumRole,GameObject> _rolesMapping = new Dictionary<EnumRole,GameObject>();

    private Dictionary<EnumRole, LoginCharAnimation> _roleAnimators = new Dictionary<EnumRole, LoginCharAnimation>();
    #endregion

    void Awake()
    {
        _instance = this;
        OnRandomName(null);

        //button regist
        UIEventListener.Get(nickNameRandomButton.gameObject).onClick = OnRandomName;
        UIEventListener.Get(startGameButton.gameObject).onClick = OnClickStartGame;
        UIEventListener.Get(selectServerButton.gameObject).onClick = OnSelectServer;
        UIEventListener.Get(leftArrowButton.gameObject).onClick = ShowNextSelection;
        UIEventListener.Get(rightArrowButton.gameObject).onClick = ShowPreviousSelection;
        UIEventListener.Get(accountSwitchButton.gameObject).onClick = OnSwitchAccount;
        UIEventListener.Get(accountBindingButton.gameObject).onClick = OnBindingAccount;
    }

    void Start()
    {
		Camera camera = this.GetComponentInChildren<Camera>();
        camera.cullingMask = 1 << LayerMask.NameToLayer("2DUILOADING");
    }

    void OnRandomName(GameObject button)
    {
        string nickname = DataManager.Instance.nickNameDataList.GetNickName();
        nickNameInput.text = nickname;
    }

    #region butotn operation
    void OnBindingAccount(GameObject button = null)
    {
        UIManager.Instance.CloseUI("CharacterSelection");
        if (!UIManager.Instance.IsUIOpened("UIBindAccount"))
        {
            UIManager.Instance.OpenUI("UIBindAccount");
        }
    }

    void OnSelectServer(GameObject button = null)
    {
        UIManager.Instance.CloseUI("CharacterSelection");
        if (!UIManager.Instance.IsUIOpened("UISelectServer"))
        {
            UIManager.Instance.OpenUI("UISelectServer", null == button);
        }
    }

    void OnSwitchAccount(GameObject button = null)
    {
        UIManager.Instance.CloseUI("CharacterSelection");
        if (!UIManager.Instance.IsUIOpened("UILogin"))
        {
            UIManager.Instance.OpenUI("UILogin");
        }
    }
    #endregion

    /// <summary>
    /// Create default roles for later selection.
    /// </summary>
    void InitRoleModelsWithEquipments()
    {
        GameObject go;

        for (int i = 0; i < FCConst.k_role_count; i++)
        {
            EnumRole role = (EnumRole)i;
            if (!_rolesMapping.ContainsKey(role))
            {
                go = CharacterAssembler.Singleton.AssembleCharacterWithoutAI(_defaultRoleLabels[role]);
                go.transform.parent = preveiwStage.FindChild("Dummy00" + (i + 1).ToString());
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.GetComponent<AvatarController>().Init("", "", i);
                _rolesMapping[role] = go;
                _roleAnimators[role] = go.GetComponent<LoginCharAnimation>();
            }
            UpdateEquipments(_rolesMapping[role], role, true);
        }
    }

    void InitSelectServerButton()
    {
        short serverID = (short)PlayerPrefs.GetInt(PrefsKey.ServerID);
        ServerInfo serverInfo = FCDownloadManager.Instance.GetServerInfoById(serverID);
        if (null == serverInfo)
        {
            serverID = NetworkUtils.GetUDServerID();
            PlayerPrefs.SetInt(PrefsKey.ServerID, serverID);
            serverInfo = FCDownloadManager.Instance.GetServerInfoById(serverID);
        }
        selectServerLabel.text = string.Format(Localization.instance.Get("IDS_MESSAGE_GLOBAL_CURRNETSERVER"), 
		                                       serverInfo!=null?serverInfo.name:"null");
    }

    bool PassStep(EnumLoadingStep step)
    {
        return step <= _loadingStep;
    }

    public void StepAt(EnumLoadingStep step)
    {
        _loadingStep = step;
    }

    void UpdateLoading()
    {
        switch (_loadingStep)
        {
            case EnumLoadingStep.step1://get Characters
                ConnectionManager.Instance.RegisterHandler(GetCharacters, true);
                _loadingStep = EnumLoadingStep.step2;
                break;

            case EnumLoadingStep.step2:  //waiting

                break;

            case EnumLoadingStep.step3:
                IsManualSelectServerComplete = false;
                if (!UIManager.Instance.IsUIOpened("UISelectServer"))
                {
                    OnSelectServer();
                    if (AppLoading.Instance != null)
                        AppLoading.Instance.FinishLoading();
                }
                break;

            case EnumLoadingStep.step4:
                if (UIManager.Instance.IsUIOpened("UISelectServer"))
                {
                    UIManager.Instance.CloseUI("UISelectServer");
                    UIManager.Instance.OpenUI("CharacterSelection");
                }
                accountId.text = NetworkManager.Instance.UdidDisplay;
                InitSelectServerButton();
                RefreshAccount();
                ShowRoleByID(NetworkManager.Instance.SelectedRole);
                _loadingStep = EnumLoadingStep.step5;
                break;
            case EnumLoadingStep.step5:
                //waiting...
                break;
            case EnumLoadingStep.step6: //start game clicked
                _loadingStep = EnumLoadingStep.step7;
                StartGameCheckPlayerFirst();
                break;
            case EnumLoadingStep.step7:
                //waiting for net message.
                break;

            case EnumLoadingStep.step8:
                StartCoroutine(EnterGame());
                _loadingStep = EnumLoadingStep.NotAvalible;
                break;
            case EnumLoadingStep.step9://resume
                _roleAnimators[_selectedRoleID].enabled = true;
                _loadingStep = EnumLoadingStep.step5;
                break;
            default:
                break;
        }
    }

    public void RefreshAccount()
    {
        if (NetworkManager.Instance.IsAccountBind)
        {
            accountSwitchButton.gameObject.SetActive(true);
            accountBindingButton.gameObject.SetActive(false);
        }
        else
        {
            accountSwitchButton.gameObject.SetActive(false);
            accountBindingButton.gameObject.SetActive(true);
        }
        accountId.text = NetworkManager.Instance.UdidDisplay;
    }

    void Update()
    {
        UpdateLoading();
    }

    /// <summary>
    /// according to the roleid index to show or hide arrowbutton.
    /// </summary>
    void EnabledArrowButton()
    {
        rightArrowButton.isEnabled = (int)_selectedRoleID <= 0;
        leftArrowButton.isEnabled = (int)_selectedRoleID >= FCConst.k_role_count - 1;
    }

    /// <summary>
    /// Show currently selected role info in UI
    /// </summary>
    void UpdateRoleInfo()
    {
        PlayerInfo player = null;
        if (_selectedRoleID != EnumRole.NotSelected &&
            _selectedRoleID != EnumRole.NotAvailable &&
            _playerInfos.ContainsKey(_selectedRoleID))
        {
            player = _playerInfos[_selectedRoleID];
        }
        string roleText;
        string levelText;
        string softCurrency;
        string hardCurrency;
        if (null != player)
        {
            roleText = player.Nickname;
            levelText = string.Format("{0} {1}", Localization.instance.Get("IDS_MESSAGE_GLOBAL_LEVEL"), player.CurrentLevel);
            softCurrency = player.SoftCurrency.ToString();
            hardCurrency = player.HardCurrency.ToString();
            newPlayerContainer.SetActive(false);
            existPlayerContainer.gameObject.SetActive(true);
            _roleAnimators[_selectedRoleID].enabled = true;
            fightScore.FS = player.FightScore;
        }
        else
        {
            roleText = Localization.instance.Get(CharacterJobs[_selectedRoleID]);
            levelText = "";
            softCurrency = "0";
            hardCurrency = "0";
            existPlayerContainer.SetActive(false);
            newPlayerContainer.gameObject.SetActive(true);
        }
        playerNameLabel.text = roleText;
        plyaerLevelLabel.text = levelText;
        roleNameTitle.text = Localization.instance.Get(CharacterJobs[_selectedRoleID]);
        roleIntro.text = Localization.instance.Get(Role_Introduction_Keys[_selectedRoleID]);
    }

    void ShowRoleByID(EnumRole roleID)
    {
        GameManager.Instance.GameState = EnumGameState.CharacterSelect;

        //retrieve the roles under this account

        if (roleID == EnumRole.NotSelected || roleID == EnumRole.NotAvailable)  //no roles used yet
        {
            _selectedRoleID = EnumRole.Warrior;	//for new players, default role is 1
            EnabledArrowButton();
        }
        else  //found a role. Choose the last used as the current.
        {
            _selectedRoleID = roleID;
        }
        SwitchCharacterModel(_selectedRoleID);
        UpdateRoleInfo();
        //finish loading at first time? remove app loading obj
        if (AppLoading.Instance != null)
            AppLoading.Instance.FinishLoading();
        BeginBackgroundDownloadBundles();
        SoundManager.Instance.PlaySoundEffect(k_sfx_selected[_selectedRoleID]);
    }

    public void ShowNextSelection(GameObject go = null)
    {
        if (_loadingStep == EnumLoadingStep.step5)
        {
            if (preveiwRoot.activeInHierarchy)
            {
                if (_selectedRoleID > 0)
                {
                    _selectedRoleID--;
                    SwitchCharacterModel(_selectedRoleID);
                    EnabledArrowButton();
                    UpdateRoleInfo();
                    SoundManager.Instance.PlaySoundEffect(k_sfx_selected[_selectedRoleID]);
                }
            }
        }
    }

    public void ShowPreviousSelection(GameObject go = null)
    {
        if (_loadingStep == EnumLoadingStep.step5)
        {
            if (preveiwRoot.activeInHierarchy)
            {
                if ((int)_selectedRoleID < FCConst.k_role_count - 1)
                {
                    _selectedRoleID++;
                    SwitchCharacterModel(_selectedRoleID);
                    EnabledArrowButton();
                    UpdateRoleInfo();
                    SoundManager.Instance.PlaySoundEffect(k_sfx_selected[_selectedRoleID]);
                }
            }
        }
    }

    /// <summary>
    /// if enabled is true, show character and hide shadow.
    /// else show shadow and hide character.
    /// </summary>
    /// <param name="role"></param>
    void SwitchCharacterModel(EnumRole role)
    {
        Transform dummy = preveiwStage.FindChild("Dummy00" + (role + 1));
        GameObject roleModel = _rolesMapping[role];
		_roleAnimators[_selectedRoleID].enabled = true;
		Transform dummy1 = preveiwStage.FindChild("Dummy001");
		Transform dummy2 = preveiwStage.FindChild("Dummy002");
		if( role == EnumRole.Mage )
		{
            _rolesMapping[EnumRole.Mage].transform.parent = dummy2;
            _rolesMapping[EnumRole.Mage].transform.localPosition = Vector3.zero;
            _rolesMapping[EnumRole.Mage].transform.localRotation = Quaternion.identity;
            _roleAnimators[EnumRole.Mage].enabled = true;

            _rolesMapping[EnumRole.Warrior].transform.parent = dummy1;
            _rolesMapping[EnumRole.Warrior].transform.localPosition = Vector3.zero;
            _rolesMapping[EnumRole.Warrior].transform.localRotation = Quaternion.identity;
            _roleAnimators[EnumRole.Warrior].enabled = false;
		}
		else if(role == EnumRole.Warrior)
		{
            _rolesMapping[EnumRole.Mage].transform.parent = dummy1;
            _rolesMapping[EnumRole.Mage].transform.localPosition = Vector3.zero;
            _rolesMapping[EnumRole.Mage].transform.localRotation = Quaternion.identity;
            _roleAnimators[EnumRole.Mage].enabled = false;

            _rolesMapping[EnumRole.Warrior].transform.parent = dummy2;
            _rolesMapping[EnumRole.Warrior].transform.localPosition = Vector3.zero;
            _rolesMapping[EnumRole.Warrior].transform.localRotation = Quaternion.identity;
            _roleAnimators[EnumRole.Warrior].enabled = true;
		}
	}

    /// <summary>
    /// Called by UI button start to enter game.
    /// </summary>
    void OnClickStartGame(GameObject obj)
    {
        if (IsCurrentCharacterLocked())
        {
            UIMessageBoxManager.Instance.ShowMessageBox(Localization.Localize("IDS_MESSAGE_GLOBAL_LOCK"),
                Localization.Localize("IDS_TITLE_GLOBAL_WARNING"), MB_TYPE.MB_OK, null);
            return;
        }

        if (!PassStep(EnumLoadingStep.step6))
        {
            StepAt(EnumLoadingStep.step6);
        }
    }

    /// <summary>
    ///create new player if has no player yet.
    /// </summary>
    void StartGameCheckPlayerFirst()
    {
        if (_playerInfos.ContainsKey(_selectedRoleID) && _playerInfos[_selectedRoleID] != null)
        {
            PlayerInfo player = _playerInfos[_selectedRoleID];
            PlayerInfo.SetCurrentPlayer(player);
            OnCreatePlayerComplete(player.UID);
        }
        else
        {
            if (NetworkManager.isOfflinePlay)
            {
                StepAt(EnumLoadingStep.step8);
            }
            else
            {
                if (ValidateNewplayerName())
                {
                    ConnectionManager.Instance.RegisterHandler(CreateNewPlayer, true);
                }
                else
                {
                    StepAt(EnumLoadingStep.step5);
                }
            }
        }
    }

    void CreateNewPlayer()
    {
        NetworkManager.Instance.CreateNewCharacterRequest(nickNameInput.text, (byte)EnumRole.Warrior, OnCreateNewCharacter);  //force to create warrior
    }

    bool ValidateNewplayerName()
    {
        string text = nickNameInput.text;
		int otherCharsCount = Utils.CountOfNotNumberOrEnglish(text);
        int enCharsCount = text.Length - otherCharsCount;
		bool isAllEnglish = Utils.IsAllNumberOrEnglish(text);
		if (
#if !ENABLE_OTHER_LETTER_FOR_NAME
            !isAllEnglish ||
            nickNameInput.text.Length < nickNameLengthMin * 2 || nickNameInput.text.Length > nickNameLengthMax * 2 ||
#endif
Utils.IsContainIllegalSymbols(text))
		{
            string message = Localization.instance.Get("IDS_MESSAGE_LOGIN_NOTICE_SPECIALCHARACTER");
            message = string.Format(message, nickNameLengthMin * 2, nickNameLengthMax * 2);
			UIMessageBoxManager.Instance.ShowMessageBox(message, "", MB_TYPE.MB_OK, null);
			return false;
		}
		
		if(
#if !ENABLE_OTHER_LETTER_FOR_NAME
			!isAllEnglish ||
#endif
			otherCharsCount * 2 + enCharsCount > nickNameLengthMax * 2 ||
            otherCharsCount * 2 + enCharsCount < nickNameLengthMin * 2)
		{
            string message = Localization.instance.Get("IDS_MESSAGE_LOGIN_NICKNAME_DEMAND");
			message = string.Format(message,
                nickNameLengthMin , nickNameLengthMax,
                nickNameLengthMin * 2, nickNameLengthMax * 2);
			UIMessageBoxManager.Instance.ShowMessageBox(message, "", MB_TYPE.MB_OK, null);
			return false;
		}
		if(Utils.FilterWords(text))
		{
			return true;
		}
        UIMessageBoxManager.Instance.ShowMessageBox(Localization.instance.Get("IDS_MESSAGE_LOGIN_NOTICE_ILLEGAL"), "", MB_TYPE.MB_OK, null);
		return false;
    }

    void OnCreateNewCharacter(NetResponse msg)
    {
        ConnectionManager.Instance.SendACK(CreateNewPlayer, true);
        if (msg.Succeeded)
        {
            CreatePlayerResponse myMsg = (CreatePlayerResponse)msg;
            PlayerInfo.SetCurrentPlayer(myMsg.playerInfo);
			_playerInfos[_selectedRoleID] = myMsg.playerInfo;
            OnCreatePlayerComplete(myMsg.playerInfo.UID);
        }
        else
        {
            StepAt(EnumLoadingStep.step5);
            UIMessageBoxManager.Instance.ShowErrorMessageBox(msg.errorCode, "");
        }
    }

    void OnCreatePlayerComplete(int playerId)
    {
        _requestEnterGamePlayerId = playerId;
        ConnectionManager.Instance.RegisterHandler(RequestToEnterGame, true);
    }

    void RequestToEnterGame()
    {
        NetworkManager.Instance.EnterGameRequest(_requestEnterGamePlayerId, OnEnterGameRequest);
    }

    public void OnEnterGameRequest(FaustComm.NetResponse msg)
    {
        ConnectionManager.Instance.SendACK(RequestToEnterGame, msg.Succeeded);
		if (msg.Succeeded)
		{
			//fill PlayerInfoManager
			EnterGameResponse myMsg = (EnterGameResponse)msg;
			PlayerInfo.Instance.PlayerInventory = myMsg.playerInventory;
            StepAt(EnumLoadingStep.step8);
		}
		else
		{
			//show msg box
            StepAt(EnumLoadingStep.step5);
			Debug.LogError(string.Format("Cannot enter game. Error = {0}  {1}", msg.errorCode, msg.errorMsg));
		}
    }

    bool IsCurrentCharacterLocked()
    {
        if ((EnumRole)_selectedRoleID == EnumRole.Mage && GameManager._mageUnlock == 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Actually enter level "town" or "tutorial" using the selected role
    /// </summary>
    IEnumerator EnterGame()
    {
        _roleAnimators[_selectedRoleID].PlayStartupAnimation();
        SoundManager.Instance.PlaySoundEffect(k_sfx_start_game[_selectedRoleID]);
        QuestManager.instance.ResetSyncTime();
        //wait for the animation to play for some time

        yield return new WaitForSeconds(animation_duration);

		PlayerPrefs.SetInt("LastUsedRole", (int)_selectedRoleID);
		//todo: start tutorial for new character
        GameManager.Instance.StartTime = Time.time;
        LevelManager.Singleton.LoadDefaultLevelConfig();

		//if the role is new, enter tutorial level, otherwise enter Town
		PlayerInfo pi = _playerInfos[_selectedRoleID];

//todo yindao
//        if (pi.CurrentXp == 0 && pi.CurrentLevel == 1)
//        {
//			LevelManager.Singleton.LoadLevelWithRandomConfig(FCConst.k_level_tutorial, 0);
//		}
//        else
//        {
			LevelManager.Singleton.BeginEnterTown();
//        }
    }

    void UpdateEquipments(GameObject model, EnumRole roleID, bool useFancyGear)
    {
        List<GameObject> equipInstances = new List<GameObject>();

        InitInformation initialInfo = DataManager.Instance._initInformation;

        List<EquipmentIdx> equipmentListWithLevel = new List<EquipmentIdx>();
        if (_playerInfos.ContainsKey(roleID) && _playerInfos[roleID] != null)
        {
            PlayerInfo playerInfo = _playerInfos[roleID];
            playerInfo.GetSelfEquipmentIds(equipmentListWithLevel);
        }
        else
        {
            List<string> equipmentList = roleID == EnumRole.Mage ? initialInfo._loginEquipment_mage : initialInfo._loginEquipment_warrior;
            foreach (string name in equipmentList)
            {
                EquipmentIdx eIdx = new EquipmentIdx();
                eIdx._id = name;
                eIdx._evolutionLevel = 0;
                equipmentListWithLevel.Add(eIdx);
            }
        }

        PlayerInfo.GetOtherEquipmentInstanceWithIds(equipInstances, equipmentListWithLevel);

        List<FCEquipmentsBase> equipments = new List<FCEquipmentsBase>();

        // get equipment data array
        foreach (GameObject g in equipInstances)
        {
            FCEquipmentsBase[] es = g.GetComponentsInChildren<FCEquipmentsBase>();
            equipments.AddRange(es);
            // destroy ai game object
            GameObject.Destroy(g);
        }

        // equip models.
        AvatarController avatar = model.GetComponent<AvatarController>();
        avatar.RemoveWeapons();
        foreach (FCEquipmentsBase eeb in equipments)
        {
            EquipmentAssembler.Singleton.Assemble(eeb, avatar);
        }
        model.SetActive(false);
        model.SetActive(true);
    }


    void BeginBackgroundDownloadBundles()
    {
#if !UNITY_EDITOR || FORCE_ASSET_BUNDLES_IN_EDITOR
		// background download bundles
        BundlesConfig bf = InJoy.AssetBundles.AssetBundles.Load(DataManager.Instance.bundleConfigPath, typeof(BundlesConfig)) as BundlesConfig;

        foreach (WorldBundlesData wfd in bf._worlds)
        {
            if (!wfd._downloadInLaunch)
            {
                continue;
            }

            bool isAllDownload = true;
            Debug.Log("start bf(" + wfd._bundles.Length + ")");
            foreach (string index in wfd._bundles)
            {
                if (index != null && index != "")
                {
                    Debug.Log("step1:" + index);
                    if (!FCDownloadManager.Instance.IsIndexDownloaded(index))
                    {
                        Debug.Log("step2:" + index);
                        if (!FCDownloadManager.Instance.IsIndexDownloading(index))
                        {
                            Debug.Log("step3:" + index);
                            FCDownloadManager.Instance.AddDownloadIndex(index, FCDownloadManager.DownloadType.DT_Background);
                        }
                        isAllDownload = false;
                    }
                }
            }
            if (!isAllDownload)
            {
                FCDownloadManagerView.Instance.RegisterDownloadEvent(wfd);
            }
        }
#endif
    }

    void GetCharacters()
    {
        NetworkManager.Instance.GetCharacters(OnGetCharacters);
    }

    void OnGetCharacters(FaustComm.NetResponse msg)
    {

        if (msg.Succeeded)
        {
            ConnectionManager.Instance.SendACK(GetCharacters, true);

            Camera camera = this.GetComponentInChildren<Camera>();
            camera.cullingMask = 1 << LayerMask.NameToLayer("2DUILOADING") |
            1 << LayerMask.NameToLayer("2DUI");

            Debug.Log("Characters retrieved successfully.");
            GetCharactersResponse myMsg = (GetCharactersResponse)msg;
            for (int i = 0; i < FCConst.k_role_count; i++ )
            {
                EnumRole role = (EnumRole)i;
                if (myMsg.playerInfos.ContainsKey(role))
                {
                    _playerInfos[role] = myMsg.playerInfos[role];
                }
                else
                {
                    _playerInfos[role] = null;
                }
            }

            _selectedRoleID = (EnumRole)PlayerPrefs.GetInt("LastUsedRole", 0);
            InitRoleModelsWithEquipments();
            if (IsForceToSelectServer)
            {
                _loadingStep = EnumLoadingStep.step3;
                IsForceToSelectServer = false;
            }
            else
            {
                if (!IsFreshFish || IsManualSelectServerComplete)
                {
                    _loadingStep = EnumLoadingStep.step4;
                }
                else
                {
                    _loadingStep = EnumLoadingStep.step3;
                }
            }
        }
        else
        {
            UIMessageBoxManager.Instance.ShowMessageBox(Utils.GetErrorIDS(msg.errorCode), "", MB_TYPE.MB_OK, null);
        }
    }
}