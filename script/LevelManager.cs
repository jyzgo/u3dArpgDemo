using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.FCComm;
using InJoy.Utils;

public class LevelManager : Photon.MonoBehaviour
{
	public string[] LevelConfigPaths;        //the path where level configs are stored
	public string PvpLevelConfigPath;
	public string SurvivalModeConfigPath;
	public string BundlesConfigPath;

	private static LevelManager _inst;
	public static LevelManager Singleton
	{
		get { return _inst; }
	}

	public delegate void OnPlayerInitialized(int index);
	public OnPlayerInitialized _onPlayerInitialized;

	public delegate void OnPlayerDestroyed(int index);
	public OnPlayerDestroyed _onPlayerDestroyed;

	public enum VarNeedCount
	{
		ENERGY_CURRENT = 0, //ENERGY_MAX = ENERGY_CURRENT - ENERGY_GAIN_TOTAL - ENERGY_GAIN_BY_POTION - ENERGY_GAIN_BY_OTHER - ENERGY_COST;
		ENERGY_GAIN_TOTAL,
		ENERGY_COST,// ENERGY_COST + SKILL_1_ENG_COST + SKILL_2_ENG_COST ...+ SKILL_10_ENG_COST = 0
		ENERGY_GAIN_BY_POTION,
		ENERGY_GAIN_BY_OTHER,
		ENERGY_MAX,
		SKILL_1_UES_TIMES = 15,
		SKILL_2_UES_TIMES, //if 2~10 times > 0, the cost should != 0
		SKILL_3_UES_TIMES,
		SKILL_4_UES_TIMES,
		SKILL_5_UES_TIMES,
		SKILL_6_UES_TIMES,
		SKILL_7_UES_TIMES,
		SKILL_8_UES_TIMES,
		SKILL_9_UES_TIMES,
		SKILL_10_UES_TIMES,
		SKILL_1_CD_MIN_TIME = 30,
		SKILL_2_CD_MIN_TIME,
		SKILL_3_CD_MIN_TIME,
		SKILL_4_CD_MIN_TIME,
		SKILL_5_CD_MIN_TIME,
		SKILL_6_CD_MIN_TIME,
		SKILL_7_CD_MIN_TIME,
		SKILL_8_CD_MIN_TIME,
		SKILL_9_CD_MIN_TIME,
		SKILL_10_CD_MIN_TIME,
		SKILL_1_ENG_COST,
		SKILL_2_ENG_COST,
		SKILL_3_ENG_COST,
		SKILL_4_ENG_COST,
		SKILL_5_ENG_COST,
		SKILL_6_ENG_COST,
		SKILL_7_ENG_COST,
		SKILL_8_ENG_COST,
		SKILL_9_ENG_COST,
		SKILL_10_ENG_COST,
		HIT_POINT = 50,
		HIT_PLAYER = 55, // HIT_PLAYER = NOT_HIT_PLAYER + ATTACK_TOTAL
		BLOCK_PLAYER,
		NOT_HIT_PLAYER,
		ATTACK_TOTAL, // ATTACK_TOTAL = BLOCK_PLAYER + ATTACK_IGNORE + ATTACK_EFFECT
		ATTACK_IGNORE,
		ATTACK_EFFECT,
		ATTACK_MAX_POINT = 65,
		ATTACK_MAX_POINT1, //ATTACK_MAX_POINT1 <= 3 x ATTACK_MAX_POINT + 3
		MAX = 100
	}
	//total counter
	protected float[] _totalCounter;
	public float[] TotalCounter
	{
		get
		{
			return _totalCounter;
		}
		set
		{
			_totalCounter = value;
		}
	}

	//loading step control
	private int _loadingStep = -1;	//load battle scene steps, >-1 is in loading step

	private AsyncOperation _loadingAsyncOp = null;
	private AsyncOperation _loadingCommandAsyncOp = null;
	private AsyncOperation _loadingTownUIAsyncOp = null;

	private bool _isEnterTown = false;
	//end loading step

	private new PhotonView photonView;

	void Awake()
	{
		if (_inst != null)
		{
			Debug.LogError("LevelManager: detected singleton instance has existed. Destroy this one " + gameObject.name);
			Destroy(this);
			return;
		}

		_activedTriggers = new System.Collections.Generic.List<ActivedTriggerInfo>();
		_inst = this;

		photonView = GetComponent<PhotonView>();
		_totalCounter = new float[(int)VarNeedCount.MAX];
	}

	void OnDestroy()
	{
		if (_inst == this)
		{
			_inst = null;
		}
	}

	//on start up, register delegates
	void Start()
	{
		photonView.viewID = (int)FC_PHOTON_STATIC_SCENE_ID.LEVEL_MGR;
		ClearNetwork();
	}

	// Load levels config
	public void LoadDefaultLevelConfig()
	{
		LoadLevelConfig(PlayerInfo.Instance.difficultyLevel);

		_bundlesConfig = InJoy.AssetBundles.AssetBundles.Load(BundlesConfigPath) as BundlesConfig;
		Assertion.Check(_bundlesConfig != null);
	}

	public void LoadLevelConfig(int difficultyLevel)
	{
		_levelsConfig = LoadLevelConfigData(difficultyLevel);

		Debug.Log("Enemy config loaded. Difficulty level: " + difficultyLevel);
		Assertion.Check(_levelsConfig != null);
	}

	public LevelConfig LoadLevelConfigData(int difficultyLevel)
	{
		LevelConfig levelsConfig = InJoy.AssetBundles.AssetBundles.Load(LevelConfigPaths[difficultyLevel]) as LevelConfig;
		List<LevelData> ldList = new List<LevelData>();
		ldList.AddRange(levelsConfig.levels);
		LevelConfig result = ScriptableObject.CreateInstance<LevelConfig>();
		result.levels = ldList.ToArray();
		return result;
	}

	#region LEVEL UP
	private ActionController _playerSelf;
	void EndGame()
	{
		_playerSelf = null;
	}


	public bool LevelFinishFlag
	{
		get;
		set;
	}

	public void BeginGame()
	{
		PlayerInfo.Instance.OnLevelUp += OnLevelUp;

		PlayerInfo.Instance.CombatInventory.Clear();
		if (GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.SINGLE)
		{
			BattleSummary.Instance.BeginBattle();
		}
		else if (GameManager.Instance.IsPVPMode)
		{
			MultiplayerDataManager.Instance.Init();
			PvPBattleSummary.Instance.BeginBattle();
		}
		LevelFinishFlag = false;

		TutorialManager.Instance.InitHudCompoment();

		if (IsTutorialLevel())
		{
		    //TutorialManager.Instance.EnterTutorialLevel();
		}

		if (GameManager.Instance.IsPVPMode)
		{
			UIManager.Instance.OpenUI("PvPHUDUI");
		}
		else
		{
			UIManager.Instance.CloseUI("PvPHUDUI");
		}
	}

	private string _tutorialLevel;
	public string TutorialLevel
	{
		get
		{
			if (string.IsNullOrEmpty(_tutorialLevel))
			{
				_tutorialLevel = DataManager.Instance.CurGlobalConfig.getConfig("tutorial_level_name") as string;
			}
			return _tutorialLevel;
		}
	}

	public bool IsTutorialLevel()
	{
		return _currentLevel == this.TutorialLevel;
	}

	void OnLevelUp(int level)
	{
		SoundManager.Instance.PlaySoundEffect("warrior_levelup");
		DoLevelUpEffect();
		if (_playerSelf != null)
		{
			_playerSelf.OnLevelUp(level);
		}
	}

	public void DoLevelUpEffect()
	{
		StartCoroutine(OnLevelUpEffect());
	}


	IEnumerator OnLevelUpEffect()
	{
		yield return new WaitForSeconds(1.0f);
		//show level up  "pop up message"
		//show level up  "normal message"

	}
	#endregion


	void Update()
	{
		//update loading
		if (_loadingStep >= 0)
		{
			LoadingManager.Instance.UpdateLoadingIcon();

			if (_isEnterTown)
			{
				UpdateLoadingTown();
			}
			else
			{
				UpdateLoadingBattleLevel();
			}
			return;
		}

		//update battle
		if (ObjectManager.Instance == null)
		{
			return;
		}

		//todo: change to game state
		if (GameManager.Instance.GameState != EnumGameState.InBattle)
		{
			return;
		}

		ActionController ac = ObjectManager.Instance.GetMyActionController();
		if ((ac == null) || (!ac.IsAlived))
		{
			return;
		}

		for (int i = _activedTriggers.Count - 1; i >= 0; --i)
		{
			ActivedTriggerInfo info = _activedTriggers[i];
			if (info._trigger.CallbackPerFrame(info))
			{
				_activedTriggers.RemoveAt(i);
				break;
			}
		}
	}

	private void UpdateLoadingTown()
	{
		switch (_loadingStep)
		{
			case 0:
				if (GameManager.Instance.isFirstEnterTown)
				{
					_loadingStep = 1;
					GetLevelsStateFromServer();

					
				}
				else
				{
					_loadingStep = 100;
				}
				break;

			case 1: //wait for get levels state to finish
				break;

			case 2:
				_loadingStep++;
				NetworkManager.Instance.GetSkills(OnGetSkillsFromServer);
				
				break;

			case 3:  //wait for GetSkills to finish
				break;

			case 4: //get tattoos
				_loadingStep++;
				NetworkManager.Instance.GetTattoos(OnGetTattoosFromServer);

				
				break;

			case 5: //wait for get tattoos to finish
				break;

			case 6: //get quests
				_loadingStep++;
				QuestManager.instance.CheckToActivateQuests(OnGetQuestCallback);
				
				break;

			case 7: //wait for quests
				break;

			case 8:
				//NetworkManager.Instance.SendCommand(new GetTutorialStateRequest(), OnGetTutorialProgressFromServer);
				//_loadingStep++;
			_loadingStep = 100;
				break;

			case 9:	//wait for tutorial states
				break;

			case 100:
				string townSceneName = FCConst.k_level_town;  //todo: load festival town

				_loadingAsyncOp = Application.LoadLevelAsync(townSceneName);

				_loadingStep++;
				break;

			case 101:
				if (null != _loadingAsyncOp && _loadingAsyncOp.isDone)
				{
					PhotonNetwork.isMessageQueueRunning = true; //resume photon msg queue
					_loadingAsyncOp = null;

					//enter town
					_loadingTownUIAsyncOp = Application.LoadLevelAdditiveAsync("townui");

					_loadingStep = 1000;
				}
				break;

			case 1000:  //set in OnLevelStatesReceived();
				if (_loadingTownUIAsyncOp.isDone)
				{
					_isEnterTown = false;
					_loadingStep = -1;
					_loadingTownUIAsyncOp = null;

					LevelManager.Singleton.EnterTown();
				}
				break;
		} //end switch
	}

	private void OnGetQuestCallback()
	{
		_loadingStep = 8;
	}

	private void OnGetTutorialProgressFromServer(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			PlayerInfo.Instance.TutorialState = (msg as GetTutorialStateResponse).tutorialState;
			_loadingStep = 100;
		}
		else
		{
			Debug.LogError("Error getting tutorial states: " + msg.errorCode);
			_loadingStep = 8;	//retry
		}
	}

	private void UpdateLoadingBattleLevel()
	{
		switch (_loadingStep)
		{
			case 0:	//load enemy and loot config from server. Set step to 2 in callback

				break;

			case 2:
				LoadLevelByName(true);

				_loadingStep = 5;

				break;

			case 5: //wait scene loading Ok step
				if (null != _loadingAsyncOp && _loadingAsyncOp.isDone)
				{
					PhotonNetwork.isMessageQueueRunning = true; //resume photon msg queue
					_loadingAsyncOp = null;

					//enter battle
					_loadingStep = 10;
				}
				break;

			case 10:
				Debug.Log("[Loading] load level res -- end");

				//finish load combat level, tell host load level ready
				if (PhotonNetwork.room != null)
				{
					LevelManager.Singleton.SendFinishLoadLevelResToAll();
				}

				_loadingStep = 20;
				break;

			case 20:
				//if all are in loading, load command scene
				if (IsLevelResReady())
				{
					Debug.Log("[Loading] load command -- begin");
					_loadingCommandAsyncOp = Application.LoadLevelAdditiveAsync("command");
					_loadingStep = 30;
				}
				break;

			case 30: //wait command scene loading Ok step
				if (_loadingCommandAsyncOp.isDone)
				{
					_loadingStep = 40;
					_loadingCommandAsyncOp = null;
					Debug.Log("[Loading] load command -- end");
				}
				break;

			case 40: //init level, this step takes much time
				Debug.Log("[Loading] init level -- begin");
				StartCoroutine(LevelManager.Singleton.InitLevel());
				_loadingStep = 50;
				break;

			case 50: //wait init level coroutine finish
				if (_levelInitFinished)
				{
					Debug.Log("[Loading] init level -- end");

					PrepareLoot();
					_loadingStep = 60;
				}
				break;

			case 60: //start enemies and heros.
				if (IsAllPlayersLevelInitReady())
				{
					//spawn path enemy and heros
					LevelManager.Singleton.SpawnObjectsAtLevelStart();
					_loadingStep = 70;
				}
				break;

			case 70: //end step, enter game
				if (PhotonNetwork.isMasterClient)
				{
					LevelManager.Singleton.SendStartLevelToAll();
				}

				LevelManager.Singleton.BeginGame();

				_loadingStep = -1;
				break;
		} //end switch
	}

	public void StartEnterBattle()
	{
		_isEnterTown = false;
		_loadingStep = 0;
		PhotonNetwork.isMessageQueueRunning = false;
	}

	public void StartEnterTown()
	{
		_isEnterTown = true;
		_loadingStep = 0;
		PhotonNetwork.isMessageQueueRunning = false;
	}

	//all level res load ready?
	private bool IsLevelResReady()
	{
		// for server, 'ready' means that every client has stood by.
		if (PhotonNetwork.isMasterClient)
		{
			return LevelManager.Singleton.IsAllLevelResLoaded();
		}
		else
		{
			return true;
		}
	}

	//all level init ready?
	private bool IsAllPlayersLevelInitReady()
	{
		// for server, 'ready' means that every client has stood by.
		if (PhotonNetwork.isMasterClient)
		{
			return LevelManager.Singleton.IsAllLevelInited();
		}
		else
		{
			return true;
		}
	}

	//flag for all level res ready
	private bool[] _levelResLoaded = new bool[FCConst.MAX_PLAYERS];

	//flag for init level ready
	private bool[] _levelInited = new bool[FCConst.MAX_PLAYERS];

	//true if player cheats on anything: potion, etc
	public bool IsCheat { get; set; }

	private bool _levelInitFinished;	//true if all level init steps have been finished.

	private LevelData _levelData;

	public LevelData CurrentLevelData
	{
		get
		{
			return _levelData;
		}
	}


	private string _currentLevel = null;
	public string CurrentLevel
	{
		get
		{
			return _currentLevel;
		}

		set
		{
			_currentLevel = value;
		}

	}

	private bool _newDifficultyLevelOpened;
	public bool NewDifficultyLevelOpened
	{
		get { return _newDifficultyLevelOpened; }
		set { _newDifficultyLevelOpened = value; }
	}

	//keep this, we may need in future-- pwt
	private int _currentDifficultyLevel = -1;
	
	public int CurrentDifficultyLevel { get { return _currentDifficultyLevel; } }

	private Transform _bulletRoot;
	public Transform BulletRoot
	{
		get { return _bulletRoot; }
	}

	private Transform _globalEffectRoot;
	public Transform GlobalEffectRoot
	{
		get { return _globalEffectRoot; }
	}

	private Transform _characterEffectRoot;
	public Transform CharacterEffectRoot
	{
		get { return _characterEffectRoot; }
	}

	private Transform _charactersRoot;
	public Transform CharactersRoot
	{
		get { return _charactersRoot; }
	}
	private LevelConfig _levelsConfig;
	public LevelConfig LevelsConfig
	{
		get
		{
			return _levelsConfig;
		}
	}

	private BundlesConfig _bundlesConfig;
	public BundlesConfig BundleConfig
	{
		get
		{
			return _bundlesConfig;
		}
	}

	private EnemyInstance[] _enemyInfos;
	private HeroInstance[] _heroInfos;

	//remember the starting index in _enemyInfos of each enemy group of the current level. Will be recreated for each new level
	private int[] _enemyStartingIndices;

	private GameObject _pathManager;
	public GameObject PathManager
	{
		get { return _pathManager; }
		set { _pathManager = value; }
	}

	private System.Collections.Generic.List<ActivedTriggerInfo> _activedTriggers;


	public void OnSaveOK()
	{
		LevelManager.Singleton.ExitLevel();
	}

	private void MessageCallBack(ID_BUTTON buttonID)
	{
		if (WorldMapController.Instance != null)
		{
			WorldMapController.Instance.DestroyWorldMap();
		}
		UIManager.Instance.CloseUI("TownHome");
	}


	public void LoadLevelWithRandomConfig(string levelName, int difficultyLevel = 0)
	{
		LoadLevelConfig(difficultyLevel);   //always reload levelconfig

		_currentDifficultyLevel = difficultyLevel;

		LoadLevelByName(levelName, true);
	}

	bool CheckInventory()
	{
		string title = Localization.instance.Get("IDS_INVENTORY_FULL_TITLE");
		string message = Localization.instance.Get("IDS_INVENTORY_FULL_TIP");

		{
			if (PlayerInfo.Instance.PlayerInventory.itemList.Count >= GameSettings.Instance.MaxBagSize)
			{
				UIMessageBoxManager.Instance.ShowMessageBox(message, title,
					"IDS_INVENTORY_FULL_TO_BATTLE_BUTTON_2", string.Empty, MB_TYPE.MB_OK, MessageCallBack);
				return false;
			}
			else if (PlayerInfo.Instance.IsInventoryFull())
			{
				UIMessageBoxManager.Instance.ShowMessageBox(message, title, "IDS_INVENTORY_FULL_TO_BATTLE_BUTTON_2", "IDS_INVENTORY_FULL_TO_BATTLE_BUTTON_1",
					MB_TYPE.MB_OKCANCEL, MessageCallBack);
				return false;
			}
		}
		return true;
	}

	private void LoadLevelByName(string levelName, bool isServer)
	{
		_levelData = _levelsConfig.GetLevelData(levelName);

		Assertion.Check(_levelData != null);

		if (PlayerInfo.Instance.Vitality < _levelData.vitalityCost)
		{
			UIMessageBoxManager.Instance.ShowErrorMessageBox(5004, "FCEnterLevel");

			return;
		}

		_isServer = isServer;

		ConnectionManager.Instance.RegisterHandler(EnterBattle, true);

		StartEnterBattle();
	}

	//inquery server for enemy config and loot config
	private void EnterBattle()
	{
		NetworkManager.Instance.EnterBattle(_levelData.id, OnEnterBattle);
	}

	private Dictionary<string, List<EnemyInstanceInfo>> _triggerEnemyMapping;

	private Dictionary<StaticObjectType, List<StaticObjectInfo>> _staticObjMapping;

	public Dictionary<StaticObjectType, List<StaticObjectInfo>> staticObjMapping
	{
		get { return _staticObjMapping; }
	}

	private void OnEnterBattle(FaustComm.NetResponse response)
	{
		if (response.Succeeded)
		{
			ConnectionManager.Instance.SendACK(EnterBattle, true);

			BeginBattleResponse msg = response as BeginBattleResponse;

			_triggerEnemyMapping = msg.triggerEnemyMapping;

			_staticObjMapping = msg.staticObjMapping;

			_loadingStep = 2;
		}
		else
		{
			//display error and exit to UI enter level
			ConnectionManager.Instance.SendACK(EnterBattle, true);

			UIMessageBoxManager.Instance.ShowErrorMessageBox(response.errorCode, "WorldMapHome");

			_loadingStep = -1;	//reset

            if (null != CharacterSelector.Instance)
            {
                CharacterSelector.Instance.StepAt(EnumLoadingStep.step9);
            }

			Debug.LogError(string.Format("[LevelManager] Error entering battle. Error = {0}", response.errorMsg));
		}
	}

	private void PrepareLoot()
	{
		//enemy
		foreach (EnemyInstance instance in _enemyInfos)
		{
			instance._actionController.CalculateLoot(instance.spawnInfo.lootTable);
		}

		//static object
		foreach (List<StaticObjectInfo> list in _staticObjMapping.Values)
		{
			foreach (StaticObjectInfo info in list)
			{
				LootManager.Instance.PrepareLootPrefabs(info.lootTable);
			}
		}
	}

	private bool _isServer;

	//should only for battle level
	private void LoadLevelByName(bool isServer)
	{
		_currentLevel = _levelData.levelName;

		_newDifficultyLevelOpened = false;

		//start loading screen
		LevelManager.Singleton.StartLoadingScreenForBattle();

		GameManager.Instance.GameState = EnumGameState.InBattle;
		UIButtonMessage._allowMulti = true;

		//display loading page
		LoadingManager.Instance.SetLoadingScreenActive(true);

		_loadingAsyncOp = Application.LoadLevelAsync(_levelData.sceneName);
	}

	//begin enter town
	public void BeginEnterTown()
	{
		StartLoadingScreenForTown();

		StartEnterTown();
	}


	//enter town, invoked when town is loaded
	public void EnterTown()
	{
        TownHUD.Instance.UpdateMailIndicators();
		//UIMonthCard.CheckMonthCard();

		//close loading scrren
		LoadingManager.Instance.SetLoadingScreenActive(false);

		//load other players in town 
		if(TutorialTownManager.Instance.IfAllTutorialHaveFinish())
		{
			PlayerInfo.Instance.SendQueryPlayersRequest();
		}

		CurrentLevel = FCConst.k_level_town;
		GameManager.Instance.GameState = EnumGameState.InTown;

		if (Application.platform != RuntimePlatform.WindowsEditor
			&& Application.platform != RuntimePlatform.OSXEditor)
		{
			//todo: PluginPushNotifications.Init();
		}

		//clear battle counter
		ClearBattleCounter();
	}

	public void ExitLevel()
	{
		GameManager.Instance.GameState = EnumGameState.BattleQuit;

		StopAllCoroutines();  //stop playing any unfinished dead enemy animation

		// TODO: Close Multiplayer Game Connection
		if (PhotonNetwork.room != null)
		{
			PhotonNetwork.LeaveRoom();
			PhotonManager.Instance.Disconnect();
		}

		//rest effect manager
		CharacterEffectManager.Instance.DestroyPool();
		GlobalEffectManager.Instance.DestroyPool();

		//reset level manager
		ClearLevel();

		//reset match play manager
		MatchPlayerManager.Instance.Clean();

		//pause the photon msg queue
		PhotonNetwork.isMessageQueueRunning = false;

		if (IsTutorialLevel())
		{
			UIProfileHandler._isFirstChangeName = true;
		}

		BeginEnterTown();

		//check chat room?
		ChatManager.Instance.ReconnectChatRoom();

		_levelData = null;
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus)
		{
			bool isFinish = false;
			if (GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.SINGLE)
			{
				isFinish = BattleSummary.Instance == null || BattleSummary.Instance.IsFinish;
			}
			else if (GameManager.Instance.IsPVPMode)
			{
				isFinish = PvPBattleSummary.Instance == null || PvPBattleSummary.Instance.IsFinish;
			}

			if (isFinish || _loadingStep >= 0
				|| UIManager.Instance.IsUIOpened("BattleReviveUI")
				|| UIManager.Instance.IsUIOpened("BattleSummeryTimerUI")
				)
			{
				return;
			}

			if (GameManager.Instance.GameState == EnumGameState.InBattle)
			{
				if (!IsTutorialLevel() && !TutorialManager.Instance.InTutorialFlag)
				{
					UIManager.Instance.OpenUI("UIGamePaused");
				}
			}
		}
	}

	//loading screen To town
	void StartLoadingScreenForTown()
	{
		//set loading BG
		string texName = _levelsConfig.GetLevelLoadingBGTexture(FCConst.k_level_town);
		LoadingManager.Instance.SetLoadingBG(texName);

		LevelData levelData = _levelsConfig.GetLevelData(FCConst.k_level_town);
		LoadingManager.Instance._name.text = Localization.instance.Get(levelData.levelNameIDS);

		int ran = UnityEngine.Random.Range(0, FCConst.LevelLoadingTips.Length);
		LoadingManager.Instance._tip.text = Localization.instance.Get(FCConst.LevelLoadingTips[ran]);

		//display loading page
		LoadingManager.Instance.SetLoadingScreenActive(true);
	}


	//set params for loading screen
	//this method will execute after level/command scene start.
	public void StartLoadingScreenForBattle()
	{
		//set loading BG
		string texName = _levelsConfig.GetLevelLoadingBGTexture(CurrentLevel);
		LoadingManager.Instance.SetLoadingBG(texName);

		LevelData levelData = _levelsConfig.GetLevelData(CurrentLevel);
		LoadingManager.Instance._name.text = Localization.instance.Get(levelData.levelNameIDS);

		int ran = UnityEngine.Random.Range(0, FCConst.LevelLoadingTips.Length);
		LoadingManager.Instance._tip.text = Localization.instance.Get(FCConst.LevelLoadingTips[ran]);

		//set player names and initial status
		int playerCount = MatchPlayerManager.Instance.GetPlayerCount();
		for (int i = 0; i < playerCount; i++)
		{
			MatchPlayerProfile profile = MatchPlayerManager.Instance.GetMatchPlayerProfile(i);
			LoadingManager.Instance.SetPlayerName(i, profile._playerInfo.Nickname);
			LoadingManager.Instance.SetPlayerStatus(i, FC_MULTIPLAY_STATUS.LOADING);
		}
	}

	//init level steps
	public void InitLevel_Pre()
	{
		_levelInitFinished = false;

		PhotonNetwork.isMessageQueueRunning = false;

		_isLoadingEnemy = true;
		_bulletRoot = Utils.NewGameObjectWithParent("Bullet_Root");

		GameObject go;

		go = GameObject.Find("/Global_Effect_Root");

		if (go == null)
		{
			_globalEffectRoot = Utils.NewGameObjectWithParent("Global_Effect_Root");
		}
		else
		{
			_globalEffectRoot = go.transform;
		}

		go = GameObject.Find("/Character_Effect_Root");
		if (go == null)
		{
			_characterEffectRoot = Utils.NewGameObjectWithParent("Character_Effect_Root");
		}
		else
		{
			_characterEffectRoot = go.transform;
		}
	}

	public void InitLevel_Post()
	{
		PhotonNetwork.isMessageQueueRunning = true;

		if (PhotonNetwork.room != null)
		{
			SendFinishInitLevelToAll();
		}

		_levelInitFinished = true;

		this.IsCheat = false;
	}

	public void InitLevel_Camera()
	{
		LevelData ld = _levelsConfig.GetLevelData(CurrentLevel);
		CameraController.Instance.SetCurrentCamera(ld.cameraMode);
	}

	public void InitLevel_Triggers()
	{
		//set the index range for each trigger
		TriggerManager.Singleton.InitTriggerIndexRange();
	}

	public void InitLevel_StaticObjects()
	{
		//find the static object root
		GameObject staticObjRoot = GameObject.Find("/level_root/static_objects");

		if (staticObjRoot)
		{
			StaticObjectSpawner[] spawners = staticObjRoot.GetComponentsInChildren<StaticObjectSpawner>();

			foreach (StaticObjectSpawner spawner in spawners)
			{
				spawner.InstantiateNormalPrefab();

				Destroy(spawner.gameObject);
			}
		}
	}
	//enable the navMesh obstacle component, it must be enabled later to take effect, otherwise sometimes they can be passed through
	public void InitLevel_StaticObjects2()
	{
		//find the static object root
		GameObject staticObjRoot = GameObject.Find("/level_root/static_objects");

		if (staticObjRoot)
		{
			NavMeshObstacle[] navMeshObstacles = staticObjRoot.GetComponentsInChildren<NavMeshObstacle>();
			foreach (NavMeshObstacle obstacle in navMeshObstacles)
			{
				obstacle.enabled = true;
			}
		}
	}
	public void InitLevel_Players()
	{

		//init hero infos, the count is current connected users
		if (PhotonNetwork.room == null)
		{
			//single mode	
			_heroInfos = new HeroInstance[1];
			_heroInfos[0] = new HeroInstance();
			_heroInfos[0]._index = 0;

			//my class type
			EnumRole role = EnumRole.Warrior;//todo_pwt: PlayerInfo.CurrentPlayer.Role;

			Debug.Log("role = " + role.ToString());
			if (role == EnumRole.Mage)
				_heroInfos[0]._instLabel = "mage0";
			else if (role == EnumRole.Warrior)
				_heroInfos[0]._instLabel = "warrior0";
		}
		else
		{
			//multi mode	
			int playerCount = MatchPlayerManager.Instance.GetPlayerCount();
			_heroInfos = new HeroInstance[playerCount];
			for (int i = 0; i < _heroInfos.Length; ++i)
			{
				_heroInfos[i] = new HeroInstance();
				_heroInfos[i]._index = i;

				//my class type
				MatchPlayerProfile thisPlayerProfile = MatchPlayerManager.Instance.GetMatchPlayerProfile(i);
				if (thisPlayerProfile != null)
				{
					int classType = thisPlayerProfile._playerInfo.RoleID;
					if (classType == 0)
						_heroInfos[i]._instLabel = "mage0";
					else if (classType == 1)
						_heroInfos[i]._instLabel = "warrior0";
					else
						_heroInfos[i]._instLabel = "monk0";
					if (GameManager.Instance.IsPVPMode)
					{
						_heroInfos[i]._instLabel += "_pvp";
					}
				}
				else
					Debug.LogError("try to active player without profile!");




			}
		}

	}

	private bool _isLoadingEnemy = false;


	//init level
	public IEnumerator InitLevel()
	{
		InitLevel_Pre();
		yield return null;

		InitLevel_Camera();
		yield return null;

		StartCoroutine(InitLevel_Enemies());
		yield return null;

		while (_isLoadingEnemy)
			yield return null;

#if PVP_ENABLED
		if (GameManager.Instance.IsPVPMode)
		{
		}
		else
		{
#endif
			InitLevel_Triggers();
			yield return null;
#if PVP_ENABLED
		}
#endif
		InitLevel_StaticObjects();
		yield return null;

		InitLevel_StaticObjects2();
		yield return null;

		InitLevel_Players();
		yield return null;

		InitLevel_Post();
		yield return null;
	}

	private IEnumerator InitLevel_Enemies()
	{
		_charactersRoot = Utils.NewGameObjectWithParent("Characters");

		// get PathManager.
		_pathManager = GameObject.Find("PathManager");

		// init enemys.
		ClearEnemy();

		List<EnemyInstanceInfo> enemyInfoList = new List<EnemyInstanceInfo>();

		//recreate the index array for new level
		_enemyStartingIndices = new int[_triggerEnemyMapping.Count];
		yield return null;

		int triggerIndex = 0;
		int enemyIndex = 0;
		foreach (KeyValuePair<string, List<EnemyInstanceInfo>> kvp in _triggerEnemyMapping)
		{
			_enemyStartingIndices[triggerIndex] = enemyInfoList.Count;

			foreach (EnemyInstanceInfo info in kvp.Value)
			{
				enemyInfoList.Add(info);
			}

			triggerIndex++;
			yield return null;
		}

		_enemyInfos = new EnemyInstance[enemyInfoList.Count];
		enemyIndex = 0;

		if (CharacterFactory.Singleton == null)
			Debug.LogError("CharacterFactory.Singleton");

		if (TriggerManager.Singleton == null)
			Debug.LogError("TriggerManager.Singleton.SpawnPoint");

		CharacterFactory.Singleton.BornPoint = TriggerManager.Singleton.SpawnPoint;

		foreach (EnemyInstanceInfo eii in enemyInfoList)
		{
			_enemyInfos[enemyIndex] = new EnemyInstance();
			_enemyInfos[enemyIndex]._index = enemyIndex;
			_enemyInfos[enemyIndex].InitCharacter(eii.enemyLabel, "");
			_enemyInfos[enemyIndex].spawnInfo = enemyInfoList[enemyIndex].Clone();

			_enemyInfos[enemyIndex]._actionController.ThisTransform.parent = _charactersRoot;
			_enemyInfos[enemyIndex]._actionController.Data.patrolPath = eii.path;

			// deactive it initially.
			DeactiveEnemy(enemyIndex);
			++enemyIndex;

			yield return null;
		}

		// init UI indicator in advance.
		if (UIManager.Instance._indicatorManager != null)
		{
			UIManager.Instance._indicatorManager.InitIndicatorPool(5);
		}

		_isLoadingEnemy = false;
	}



	//active path enemies and heros
	//I must ensure that this operation be excuted after all clients are ready.
	public void SpawnObjectsAtLevelStart()
	{
		//active path enemies
		int index = 0;
		foreach (EnemyInstance info in _enemyInfos)
		{
			//have path
			if (!string.IsNullOrEmpty(info._actionController.Data.patrolPath))
			{
				ActivateEnemyAtSpot(index, null);
			}

			index++;
		}


		//spawn heros
		if (_heroInfos != null)
		{
			Vector3[] bornPnts = new Vector3[]{TriggerManager.Singleton._player1BornPoint, TriggerManager.Singleton._player2BornPoint, 
											TriggerManager.Singleton._player3BornPoint, TriggerManager.Singleton._player4BornPoint};

			float[] rotationYs = new float[] 
            {
                TriggerManager.Singleton._player1RotationY,
                TriggerManager.Singleton._player2RotationY,
                TriggerManager.Singleton._player3RotationY,
                TriggerManager.Singleton._player4RotationY
            };
			for (int i = 0; i < _heroInfos.Length; ++i)
			{
				ActiveHero(i, bornPnts[i], rotationYs[i]);
			}
		}
	}


	public void GetIndexRangeByTrigger(string triggerName, out int startIndex, out int endIndex)
	{
		//search for the trigger name
		int index = 0;
		foreach (KeyValuePair<string, List<EnemyInstanceInfo>> kvp in _triggerEnemyMapping)
		{
			if (kvp.Key == triggerName)
			{
				break;
			}
			index++;
		}

		if (index >= _triggerEnemyMapping.Count)
		{
			Debug.LogError("There are more triggers than needed. Unassigned triggers ignored.");

			startIndex = endIndex = -1;

			return;
		}

		startIndex = _enemyStartingIndices[index];
		endIndex = index == _triggerEnemyMapping.Count - 1 ? _enemyInfos.Length : _enemyStartingIndices[index + 1];
	}

	public BornPoint.SpawnPointType GetEnemySpawnPointTypeByIndex(int index)
	{
		return _enemyInfos[index].spawnInfo.spawnAt;
	}

	bool CreateHero(int index)
	{
		// create heros.
		Transform t = null;
		GameObject root = GameObject.Find("Characters");
		if (root != null)
		{
			t = root.transform;
		}
		if (_heroInfos != null && _heroInfos.Length > index && index >= 0 && _heroInfos[index] != null)
		{

			_heroInfos[index].InitCharacter(_heroInfos[index]._instLabel, "");
			_heroInfos[index]._actionController.ThisTransform.parent = t;
			return true;
		}
		return false;
	}

	public void ClearLevel()
	{
		EndGame();

		if (UIManager.Instance._indicatorManager != null)
		{
			UIManager.Instance._indicatorManager.Cleanup();
		}
		StopCoroutine("PlayEnemyDeadAnim");
		ClearEnemy();
		ClearHeros();
		_pathManager = null;
		_bulletRoot = null;

		//clear some network flags
		ClearNetwork();

		_triggerEnemyMapping = null;
		_staticObjMapping = null;
		_activedTriggers.Clear();
	}

	void ClearEnemy()
	{
		if (_enemyInfos != null)
		{
			foreach (EnemyInstance ei in _enemyInfos)
			{
				//if(ei != null)
				{
					CameraController.Instance.RemoveCharacterForShadow(ei._avatarController);
					ei.Unload();
				}
			}
			_enemyInfos = null;
		}
	}

	void ClearHeros()
	{
		if (_heroInfos != null)
		{
			foreach (HeroInstance hi in _heroInfos)
			{
				CameraController.Instance.RemoveCharacterForShadow(hi._avatarController);
				hi.Unload();
			}
			_heroInfos = null;
		}
	}

	/// <summary>
	/// Get the delay time for a given enemy index in enemyInfo[].
	/// </summary>
	/// <param name="enemyIndex"></param>
	/// <returns></returns>
	public float GetDelayTimeByEnemyIndex(int enemyIndex)
	{
		return _enemyInfos[enemyIndex].spawnInfo.delayTime;
	}


	public void ActivateEnemyAtSpot(int index, EnemySpot spot)
	{
		if (PhotonNetwork.room == null)
		{
			//single player
			if (index >= 0 && index < _enemyInfos.Length)
			{
				_enemyInfos[index].Active(spot);
				// UI update.
				if (UIManager.Instance._indicatorManager != null)
				{
					UIManager.Instance._indicatorManager.ActiveEnemyIndicator(_enemyInfos[index]._actionController.ThisTransform);
				}
			}
		}
		else if (PhotonNetwork.isMasterClient)
		{
			//server
			SendActiveEnemyToAll(index, spot.transform.position);
		}
	}


	public void DeactiveEnemy(int index)
	{
		if (index >= 0 && index < _enemyInfos.Length)
		{
			_enemyInfos[index].Deactive();
		}
	}

	IEnumerator PlayEnemyDeadAnim(CharacterInstance ci)
	{
		ci._avatarController.PlayDyingEffect();
		GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.DEAD_EFFECT, ci._actionController.ThisTransform.position);
		yield return new WaitForSeconds(2.0f);
		ci.Deactive();
	}

	//dead will invoke deactive, otherwise not
	public void DeadEnemy(int index, bool playAnim)
	{
		if (_enemyInfos.Length > 0)
		{
			if (index >= 0 && index < _enemyInfos.Length)
			{
				if (playAnim)
				{
					StartCoroutine(PlayEnemyDeadAnim(_enemyInfos[index]));
				}
				else
				{
					_enemyInfos[index].Deactive();
				}
				_enemyInfos[index].isDead = true;
				CameraController.Instance.RemoveCharacterForShadow(_enemyInfos[index]._avatarController);
				// UI update.
				/*if(UIManager.Instance._indicatorManager != null) {
					UIManager.Instance._indicatorManager.DeactiveEnemyIndicator(_enemyInfos[index]._actionController.ThisTransform);
				}*/
				foreach (ActivedTriggerInfo info in _activedTriggers)
				{
					info.OnEnemyDie(index);
				}
			}
		}
	}


	//some operation should be done after my hero init and before my hero active
	public void DoSomethingWhenMyHeroActive(ActionController myHeroAc)
	{
		myHeroAc.IsPlayerSelf = true;

		_playerSelf = myHeroAc;

		//register HUD Actor 
		UIManager.Instance.gameObject.BroadcastMessage("RegisterEvents", myHeroAc, SendMessageOptions.RequireReceiver);

		//set camera target and defreeze it
		CameraController.Instance.SetTarget(myHeroAc.ThisTransform);
		CameraController.Instance.Defreeze();

		//close loading scrren
		LoadingManager.Instance.SetLoadingScreenActive(false);
	}

	//some operation should be done after my hero active
	public void DoSomethingAfterMyHeroActive()
	{
		//calculate loot for enemies
		if (_enemyInfos == null)
		{
			Debug.LogError("_enemyInfos is null in loot!");
			return;
		}


		foreach (EnemyInstance eii in _enemyInfos)
		{
			if (eii == null)
			{
				Debug.LogError("eii is null in loot!");
				continue;
			}

			if (eii._actionController == null)
			{
				Debug.LogError("eii._actionController is null in loot!");
				continue;
			}

			if (eii._actionController.Data == null)
			{
				Debug.LogError("eii._actionController.Data is null in loot!");
				continue;
			}

			if (eii._actionController.Data.id == null)
			{
				Debug.LogError("eii._actionController.Data._id is null in loot!");
				continue;
			}
		}
	}

	public void ActiveHero(int index, Vector3 pos, float rotationY)
	{
		if (PhotonNetwork.room == null)
		{
			//single player
			if (index >= 0 && index < _heroInfos.Length)
			{
				if (_heroInfos[index]._inst == null)
				{
					if (!CreateHero(index))
					{
						Debug.LogError("Can not create hero " + index);
						return;
					}
				}


				DoSomethingWhenMyHeroActive(_heroInfos[index]._actionController);

				_heroInfos[index].Active(pos, rotationY);

				DoSomethingAfterMyHeroActive();


				//add this player to all enemies' thread list
				ActionControllerManager.Instance.InitThreat(_heroInfos[index]._actionController);

				//do some init
				if (_onPlayerInitialized != null)
				{
					_onPlayerInitialized(index);
				}

			}
		}
		else if (PhotonNetwork.isMasterClient)
		{
			//server
			SendActivePlayerToAll(index, pos, rotationY);
		}
	}

	public void DeactiveHero(int index)
	{
		//[TODO]
		//clear threat by this player, move target to others
		ActionControllerManager.Instance.ClearThreat(_heroInfos[index]._actionController);

		//deactive this hero
		if (index >= 0 && index < _heroInfos.Length)
		{
			_heroInfos[index].Deactive();

			if (_onPlayerDestroyed != null)
			{
				_onPlayerDestroyed(index);
			}
		}
	}

	public void AddActivedTrigger(ActivedTriggerInfo info)
	{
		_activedTriggers.Add(info);
	}


	//reconstruct all trigger status via enemies 
	//each trigger have 4 status
	//0. actived and closed -- nothing to do
	//1. actived and has entered, not closed -- build trigger info and put into activedTriggers
	//2. actived and has not entered, not closed -- active box collider
	//3. not actived -- nothing to do
	public void RefreshTriggersViaEnemies()
	{

		_activedTriggers.Clear();

		List<BornPoint> livingTriggers = new List<BornPoint>();

		TriggerManager tm = TriggerManager.Singleton;
		int triggerCount = tm.transform.childCount;

		int[] enemyInTriggerAlive = new int[triggerCount];	//collect how many enemy alive	
		int[] enemyInTriggerActive = new int[triggerCount];	//collect how many enmey active		

		//for each trigger, get how many enemies alive & active
		for (int i = 0; i < triggerCount; i++)
		{
			enemyInTriggerAlive[i] = 0;
			enemyInTriggerActive[i] = 0;

			BornPoint trigger = tm.transform.GetChild(i).GetComponent<BornPoint>();

			//close BP shield
			trigger.Shielding = false;

			int enemyIdx = 0;
			foreach (EnemyInstance enemy in _enemyInfos)
			{
				if (enemyIdx >= trigger.StartEnemyIndex && enemyIdx < trigger.EndEnemyIndex)
				{
					//this enemy is in the trigger

					//save enemy active info
					if (enemy.isActive)
					{
						enemyInTriggerActive[i]++;

						//put actived trigger and its child into sort list
						foreach (BornPoint bp in trigger._nextTriggers)
						{
							if (!livingTriggers.Contains(bp))
								livingTriggers.Add(bp);
						}

						if (!livingTriggers.Contains(trigger))
							livingTriggers.Add(trigger);
					}

					//save enemy alive info
					if (!enemy.isDead)
						enemyInTriggerAlive[i]++;

				}

				enemyIdx++;
			}
		}

		//trace the livingTriggers
		foreach (BornPoint trigger in livingTriggers)
		{
			//get this trigger index
			int index = -1;
			for (int i = 0; i < triggerCount; i++)
			{
				if (tm.transform.GetChild(i) == trigger.transform)
				{
					index = i;
					break;
				}
			}
			Assertion.Check(index >= 0);
			Debug.Log("parsing trigger index = " + index);

			//check alive and active enemies
			if (enemyInTriggerActive[index] > 0)
			{
				//this born point has been triggered
				if (enemyInTriggerAlive[index] > 0)
				{
					//status 1

					//build trigger info and put into activedTriggers
					ActivedTriggerInfo info = new ActivedTriggerInfo();
					info._trigger = trigger;
					info._alived = enemyInTriggerAlive[index];
					AddActivedTrigger(info);

				}
				else
				{
					//status 2, nothing to do 
				}
			}
			else
			{
				//status 3
				//active it
				trigger.Active();
			}
		}
	}

	#region cheat spawn enemy
#if DEVELOPMENT_BUILD || UNITY_EDITOR
	public void CheatSpawnOneEnemy(string label, int level, int armor)
	{
		//string label = "skeleton_normal";

		EnemyInstance instance = new EnemyInstance();

		if (_heroInfos != null && _heroInfos.Length > 0)
		{
			instance.InitCharacter(label, "");

			GameObject heroGO = _heroInfos[0]._actionController.gameObject;

			instance._actionController.transform.parent = heroGO.transform.parent;

			Vector3 pos = heroGO.transform.localPosition + Vector3.forward * 3;

			instance.Active(pos, 0);
		}

		//level and armor will take effect only when level > 0
		if (level > 0)
		{
			AcData data = instance._actionController.Data;
			data.Level = level;
			data.physicalDefense = armor;
		}
	}
#endif
	#endregion  //cheat

	#region network methods

	void ClearNetwork()
	{
		for (int i = 0; i < FCConst.MAX_PLAYERS; i++)
		{
			_levelResLoaded[i] = false;
			_levelInited[i] = false;

		}
	}

	//test all players level res are ready
	public bool IsAllLevelResLoaded()
	{
		int playerLevelLoadedCount = MatchPlayerManager.Instance.GetPlayerCount();

		for (int i = 0; i < playerLevelLoadedCount; i++)
		{
			if (_levelResLoaded[i] == false)
				return false;
		}

		return true;
	}

	//test all players level init are ready
	public bool IsAllLevelInited()
	{
		int playerLevelInitedCount = MatchPlayerManager.Instance.GetPlayerCount();

		for (int i = 0; i < playerLevelInitedCount; i++)
		{
			if (_levelInited[i] == false)
				return false;
		}

		return true;
	}

	//send load level to all, use this to sync load level
	public void SendLoadLevelToAll(string levelName, string enemyConfig, int difficultyLevel)
	{
		Debug.Log("[RPC] [send] load level to all");
		photonView.RPC("ReceiveLoadLevelToAll", PhotonTargets.AllBuffered, levelName, enemyConfig, difficultyLevel);
	}

	[RPC]
	void ReceiveLoadLevelToAll(string levelName, PhotonMessageInfo msgInfo)
	{
	}


	//send finish load level res to all
	public void SendFinishLoadLevelResToAll()
	{
		Debug.Log("[RPC] [send] finish load level res to the server");

		int playerIndex = MatchPlayerManager.Instance.GetPlayerIndex();

		photonView.RPC("ReceiveFinishLoadLevelResToAll", PhotonTargets.AllBuffered, playerIndex);
	}

	[RPC]
	void ReceiveFinishLoadLevelResToAll(int playerIndex, PhotonMessageInfo msgInfo)
	{

		Debug.Log("[RPC] [receive] finish load level res message, from player:" + playerIndex);

		if (playerIndex >= FCConst.MAX_PLAYERS)
			Debug.LogError("player exceed max");

		_levelResLoaded[playerIndex] = true;
	}


	//send finish init level to all
	public void SendFinishInitLevelToAll()
	{
		Debug.Log("[RPC] [send] finish init level to the server");

		int playerIndex = MatchPlayerManager.Instance.GetPlayerIndex();

		photonView.RPC("ReceiveFinishInitLevelToAll", PhotonTargets.AllBuffered, playerIndex);
	}

	[RPC]
	void ReceiveFinishInitLevelToAll(int playerIndex, PhotonMessageInfo msgInfo)
	{

		Debug.Log("[RPC] [receive] finish init level message, from player:" + playerIndex);

		if (playerIndex >= FCConst.MAX_PLAYERS)
			Debug.LogError("player exceed max");

		_levelInited[playerIndex] = true;

		//set player status to ready
		LoadingManager.Instance.SetPlayerStatus(playerIndex, FC_MULTIPLAY_STATUS.READY);
	}


	//send spawn player to all.
	public void SendActivePlayerToAll(int playerIndex, Vector3 spwanPosition, float rotationY)
	{
		Debug.Log("[RPC] [send] active player to all");
		photonView.RPC("ReceiveActivePlayer", PhotonTargets.AllBuffered, playerIndex, spwanPosition, rotationY);
	}

	[RPC]
	IEnumerator ReceiveActivePlayer(int playerIndex, Vector3 spwanPosition, float rotationY, PhotonMessageInfo msgInfo)
	{

		Debug.Log("[RPC] [receive] active player message, index: " + playerIndex + " pos: " + spwanPosition + " from player:" + msgInfo.sender);

		//prevent from active too many players in one frame
		PhotonNetwork.isMessageQueueRunning = false;
		yield return new WaitForSeconds(0.1f);

		int index = playerIndex;
		if (index >= 0 && index < _heroInfos.Length)
		{
			if (_heroInfos[index]._inst == null)
			{
				if (!CreateHero(index))
				{
					Debug.LogError("Can not create hero " + index);
				}
			}

			//if I am the player, I can control it.
			if (index == MatchPlayerManager.Instance.GetPlayerIndex())
			{
				//this is my hero
				DoSomethingWhenMyHeroActive(_heroInfos[index]._actionController);

				_heroInfos[index].Active(spwanPosition, rotationY);

				DoSomethingAfterMyHeroActive();
			}
			else
			{
				//not my hero
				//optimize, close character controller and low some rigidbody
				_heroInfos[index].Active(spwanPosition, rotationY);

				//				CharacterController _characterController = _heroInfos[index]._actionController.gameObject.GetComponent<CharacterController>();
				//				if (_characterController != null)
				//					_characterController.enabled = false;

				Rigidbody _rigidbody = _heroInfos[index]._actionController.gameObject.rigidbody;
				if (_rigidbody != null)
				{
					_rigidbody.isKinematic = true;
					_rigidbody.interpolation = RigidbodyInterpolation.None;
				}

			}

			//add this player to all enemies' thread list
			ActionControllerManager.Instance.InitThreat(_heroInfos[index]._actionController);

			//do some init
			if (_onPlayerInitialized != null)
			{
				_onPlayerInitialized(playerIndex);
			}
		}

		yield return new WaitForSeconds(0.1f);
		PhotonNetwork.isMessageQueueRunning = true;
	}





	//send spawn enemy to all.
	public void SendActiveEnemyToAll(int enemyIndex, Vector3 spwanPosition)
	{
		Debug.Log("[RPC] [send] active enemy to all");

		photonView.RPC("ReceiveActiveEnemy", PhotonTargets.AllBuffered, enemyIndex, spwanPosition);
	}

	[RPC]
	IEnumerator ReceiveActiveEnemy(int enemyIndex, Vector3 spwanPosition, PhotonMessageInfo msgInfo)
	{

		Debug.Log("[RPC] [receive] active enemy message, index: " + enemyIndex + " pos: " + spwanPosition + " from player:" + msgInfo.sender);

		//prevent from active too many players in one frame
		PhotonNetwork.isMessageQueueRunning = false;
		yield return new WaitForSeconds(0.1f);

		int index = enemyIndex;
		if (index >= 0 && index < _enemyInfos.Length)
		{
			_enemyInfos[index].Active(spwanPosition);
		}

		yield return new WaitForSeconds(0.1f);
		PhotonNetwork.isMessageQueueRunning = true;
	}

	//server will send start level RPC to inform all clients
	public void SendStartLevelToAll()
	{
		Debug.Log("[RPC] [send] start level to all");
		photonView.RPC("ReceiveStartLevel", PhotonTargets.AllBuffered);
	}

	[RPC]
	void ReceiveStartLevel(PhotonMessageInfo msgInfo)
	{

		Debug.Log("[RPC] [receive] start level from player:" + msgInfo.sender);
		CommandManager.Instance.CollectSyncCommands();
	}
	#endregion

	// Check download state by level name.
	public bool IsLevelDownloaded(string levelName)
	{
#if FORCE_ASSET_BUNDLES_IN_EDITOR || (!UNITY_EDITOR&&UNITY_IPHONE)
		if (levelName == "")
		{
			return true;
		}
		
		LevelData ld = LevelsConfig.GetLevelData(levelName);
		
		if (ld.worldName == "")
		{
			return true;
		}
		
		WorldBundlesData wbd = BundleConfig.GetBundlesByWorld(ld.worldName);
			
		FCDownloadManager.IndexCheckResult result = FCDownloadManager.Instance.CheckIndexState(wbd._bundles);
		if (result != FCDownloadManager.IndexCheckResult.ICR_AllFinished)
		{
			return false;
		}
#endif

		return true;
	}

	public void CheckDownloadByGroup(WorldBundlesData world)
	{
#if FORCE_ASSET_BUNDLES_IN_EDITOR || (!UNITY_EDITOR&&UNITY_IPHONE)
        bool allDownloaded = true;
        
        if (world != null)
		{	
			foreach (string file in world._bundles)
			{
				if (!FCDownloadManager.Instance.IsIndexDownloaded(file))
				{
					if (!FCDownloadManager.Instance.IsIndexDownloading(file))
					{
						FCDownloadManager.Instance.AddDownloadIndex(file, FCDownloadManager.DownloadType.DT_Background);
					}
					allDownloaded = false;
				}
			}
			
					
			if (!allDownloaded)
			{
				FCDownloadManagerView.Instance.RegisterDownloadEvent(world);
				FCDownloadManagerView.Instance.ShowDownloadProgress(true);
			}
		}
#endif
	}

	public bool CheckDownloadByLevel(string levelName)
	{
		LevelData levelData = LevelsConfig.GetLevelData(levelName);

		bool allDownloaded = CheckDownloadByWorld(levelData.worldName);

		return allDownloaded;
	}

	public bool CheckDownloadByWorld(string worldName)
	{
		bool allDownloaded = true;

#if FORCE_ASSET_BUNDLES_IN_EDITOR || (!UNITY_EDITOR&&UNITY_IPHONE)
		if ((worldName!=null) && (worldName!=""))
		{			
			WorldBundlesData wbd = BundleConfig.GetBundlesByWorld(worldName);
			
			foreach (string file in wbd._bundles)
			{
				if (!FCDownloadManager.Instance.IsIndexDownloaded(file))
				{
					if (!FCDownloadManager.Instance.IsIndexDownloading(file))
					{
						FCDownloadManager.Instance.AddDownloadIndex(file, FCDownloadManager.DownloadType.DT_Background);
					}
					
					FCDownloadManagerView.Instance.RegisterDownloadEvent(wbd);
					
					allDownloaded = false;
				}
			}
		}
#endif

		return allDownloaded;
	}

	public bool CheckDownloadAndEnterLevel(string levelName, int difficultyLevel)
	{
		bool allDownloaded = CheckDownloadByLevel(levelName);

		// All bundles are in local device.
		if (allDownloaded)
		{
			WorldMapController.LevelName = levelName;

			WorldMapController.DifficultyLevel = difficultyLevel;
		}
		else
		{
			FCDownloadManagerView.Instance.ShowDownloadProgress(true);
		}
		return allDownloaded;
	}

	public void StopActorMoving()
	{
		List<FCObject> clients = InputManager.Instance.Clients;

		foreach (FCObject client in clients)
		{
			CommandManager.Instance.Send(FCCommand.CMD.INPUT_KEY_RELEASE,
					FCConst.FC_KEY_DIRECTION, FC_PARAM_TYPE.INT,
					client.ObjectID, FCCommand.STATE.RIGHTNOW, true);
		}
	}

	//destroy the objects of level and characters
	public void DestroyTriggerManager()
	{
		TriggerManager triggerMgr = TriggerManager.Singleton;
		if (triggerMgr != null)
		{
			GameObject.Destroy(triggerMgr);
		}
	}

	private void SendPvPBeginBattle()
	{
		Utils.CustomGameServerMessage(null, OnSendPvPBeginBattle);
	}


	private void OnSendPvPBeginBattle(FaustComm.NetResponse response)
	{
	}

	#region Battle Counter Statics
	public void ClearBattleCounter()
	{
		_totalCounter = null;
	}

	//return a json string of battle counter info
	public string GetBattleCounterInfo()
	{
		if (_totalCounter == null)
			return "";

		Hashtable ht = new Hashtable();
		ht.Add("Code", JudgeBattleCheated());
		foreach (VarNeedCount ii in System.Enum.GetValues(typeof(VarNeedCount)))
		{
			if (ii != VarNeedCount.MAX)
				ht.Add(ii.ToString(), _totalCounter[(int)ii]);
		}
		string info = FCJson.jsonEncode(ht);

		return info;
	}



	public int JudgeBattleCheated()
	{
		if (_totalCounter == null)
			return 0;

		int cheatCode = 0;



		if (TotalCounter[(int)VarNeedCount.ENERGY_MAX] !=
			(TotalCounter[(int)VarNeedCount.ENERGY_CURRENT] -
			TotalCounter[(int)VarNeedCount.ENERGY_GAIN_TOTAL] -
			TotalCounter[(int)VarNeedCount.ENERGY_GAIN_BY_POTION] -
			TotalCounter[(int)VarNeedCount.ENERGY_GAIN_BY_OTHER] -
			TotalCounter[(int)VarNeedCount.ENERGY_COST]
			))
		{
			cheatCode += (int)FC_CHEAT_FLAG.Energe;
		}

		if (TotalCounter[(int)VarNeedCount.HIT_PLAYER]
			!= (TotalCounter[(int)VarNeedCount.NOT_HIT_PLAYER] + TotalCounter[(int)VarNeedCount.ATTACK_TOTAL])
			)
		{
			cheatCode += (int)FC_CHEAT_FLAG.Hit;
		}

		if (TotalCounter[(int)VarNeedCount.ATTACK_TOTAL]
			!= (TotalCounter[(int)VarNeedCount.BLOCK_PLAYER] + TotalCounter[(int)VarNeedCount.ATTACK_IGNORE] + TotalCounter[(int)VarNeedCount.ATTACK_EFFECT])
			)
		{
			cheatCode += (int)FC_CHEAT_FLAG.Attack;
		}

		//disable CD cheat in 110
		//CD cheat
		/*
		for (int i=(int)VarNeedCount.SKILL_1_CD_MIN_TIME;
			i<=(int)VarNeedCount.SKILL_10_CD_MIN_TIME;
			i++)
		{
			if (TotalCounter[i] > 0.01f)
			{
				cheatCode += (int)FC_CHEAT_FLAG.SkillCD;
				break;
			}
		}
		*/

		return cheatCode;
	}


	#endregion

    
    public LevelData LastScoreLevelData
    {
        get
        {
            if (null != NewUnlockLevelData)
            {
            return LevelsConfig.GetLevelDataByID(NewUnlockLevelData.preLevelID);
            }
            else
            {
                return LevelsConfig.MaxLevelData;
            }
        }
    }

    public LevelData NewUnlockLevelData
    {
        get
        {
            return PlayerInfo.Instance.NewUnlockLevelData;
        }
    }

	private void GetLevelsStateFromServer()
	{
		NetworkManager.Instance.GetLevelsState(OnGetServerLevelsState);
	}

	private void OnGetServerLevelsState(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			LevelStatesResponse myMsg = (LevelStatesResponse)msg;
			Dictionary<string, EnumLevelState> dict = new Dictionary<string, EnumLevelState>();

			//convert id-state mapping to levelName-state mapping
			foreach (KeyValuePair<int, int> kvp in myMsg.levelStates)
			{
				dict.Add(_levelsConfig.GetLevelDataByID(kvp.Key).levelName, (EnumLevelState)kvp.Value);
			}
			PlayerInfo.Instance.levelsState = dict;

			_loadingStep = 2;
		}
		else
		{
			Assertion.Check(false, "Failed to get level states.");
		}
	}

	private void OnGetSkillsFromServer(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			GetSkillResponse myMsg = (GetSkillResponse)msg;

			Dictionary<string, int> dict = new Dictionary<string, int>();

			foreach (SkillData skillData in myMsg.skillDataList)
			{
				dict.Add(skillData.skillID, skillData.level);
			}


			if(NetworkManager.isOfflinePlay)
			{
				dict.Clear();
				dict.Add("Dash",10);
				dict.Add("Hexagrare",10);
				dict.Add("Storm",10);
				dict.Add("Avatar",10);
			}

			PlayerInfo.Instance.skillsState = dict;
			PlayerInfo.Instance.UpdatePlayerSkills();

			_loadingStep = 4;

			GameManager.Instance.isFirstEnterTown = false;
		}
		else
		{
			Assertion.Check(false, "Failed to get user skills.");
		}
	}

	private void OnGetTattoosFromServer(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			PlayerTattooResponse myMsg = (PlayerTattooResponse)msg;

			PlayerInfo.Instance.playerTattoos = myMsg.playerTattoos;

			PlayerInfo.Instance.playerMasteredTattoos = myMsg.recipeList;

			_loadingStep = 6;

			GameManager.Instance.isFirstEnterTown = false;
		}
		else
		{
			Assertion.Check(false, "Failed to get user skills.");
		}
	}
}
