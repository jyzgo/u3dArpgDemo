using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using InJoy.Utils;
using InJoy.RuntimeDataProtection;
using FaustComm;


public class PlayerInfo : ServerMessage
{
	private static PlayerInfo _instance;

	public static PlayerInfo Instance
	{
		get { return _instance; }
	}

	public static void SetCurrentPlayer(PlayerInfo instance)
	{
        if (null != _instance)
        {
            _instance.RemoveNetworkListeners();
        }
		_instance = instance;
        _instance.AddNetworkListeners();

		GameManager.Instance.isFirstEnterTown = true;
	}

	//properties stored locally
    public bool music = true;

    public bool sound = true;

    public bool haveSpawned = false; //for spawn other players in town

    public bool haveDonwloadedData = false;

    public float itemFindPossibility = 1;

	private int _uid;
	public int UID 
    { 
        get 
        { 
            return _uid; 
        } 
    }

    private string _nickname;
    public string Nickname 
    { 
        get
        { 
            return _nickname;
        }
    }

    public string DisplayNickname
    {
        get
        {
            if (_nickname.Length < 20)
            {
                return _nickname;
            }
            else
            {
                return "visitor";
            }
        }
    }


    private EnumRole _role = EnumRole.NotAvailable;
    public EnumRole Role
    {
        set { _role = value; }
        get { return _role; }
    }

    public int RoleID
    {
        get { return (int)_role; }
    }

    public float ReduseEnergy
    {
        get { return _props[PlayerPropKey.ReduceEnergy]; }
    }

    /// <summary>
    ///     player all props.you can get value by the method Prop(PropKey).
    /// </summary>
    private Dictionary<PlayerPropKey, float> _props = new Dictionary<PlayerPropKey, float>();

	private PlayerInventory _combatInventory = new PlayerInventory();
	public PlayerInventory CombatInventory
	{
		get 
        { 
            return _combatInventory; 
        }
	}

    public List<EquipmentIdx> equipIds = new List<EquipmentIdx>();

    public string GuildName
    {
        get { return string.Empty; }
    }

	public List<PlayerInfo> loadedPlayerInfoList = new List<PlayerInfo>();	// friends and allies

	private bool _saveFlag = false;

	public bool SaveFlag
	{
		get { return _saveFlag; }
		set { _saveFlag = value; }
	}

    public override void Parse(BinaryReader reader)
    {
        _uid = reader.ReadInt32();

        _nickname = NetResponse.ReadString(reader);

        _role = (EnumRole)reader.ReadByte();

        byte propNum = reader.ReadByte();
        for (int key = 0; key < propNum; ++key)
        {
            int value = reader.ReadInt32();
            float floatvalue = value / (FCConst.PropFactor((PlayerPropKey)key) * 1.0f);
            _props.Add((PlayerPropKey)key, floatvalue);
        }

        byte propBattleNum = reader.ReadByte();
        for(int key = 32; key < 32 + propBattleNum; ++key)
        {
            int value = reader.ReadInt32();
            float floatValue = value / (FCConst.PropFactor((PlayerPropKey)key) * 1.0f);
            _props.Add((PlayerPropKey)key, floatValue);
        }

        _equippedInventory = new PlayerInventory();
        _equippedInventory.Parse(reader);
    }

    public void ApplyPlayerPropChanges(PlayerProp[] props)
    {
        foreach (PlayerProp prop in props)
        {
            float floatvalue = prop.Value / (FCConst.PropFactor((PlayerPropKey)prop.Key) * 1.0f);
            _props[prop.Key] = floatvalue;

            if (prop.Key == PlayerPropKey.HardCurrency && onHardCurrencyChanged != null)
            {
                onHardCurrencyChanged(HardCurrency);
            }
            else if (prop.Key == PlayerPropKey.SoftCurrency && onSoftCurrencyChanged != null)
            {
                onSoftCurrencyChanged(SoftCurrency);
            }
            else if (prop.Key == PlayerPropKey.Vitality && onVitalityChanged != null)
            {
                onVitalityChanged();
            }
        }
    }

    #region PlayerProp

    public int CurrentXp
    {
        get
        {
			return (int)_props[PlayerPropKey.Exp];
        }
    }

    public float _currentXpPrecent = 0;
    public int CurrentLevel
    {
        get
        {
            return (int)_props[PlayerPropKey.Level];
        }

        set
        {
            _props[PlayerPropKey.Level] = value;
        }
    }

   

    #region Soft currency and hard currency

    public delegate void OnCurrencyChanged(int currency);
    public OnCurrencyChanged onHardCurrencyChanged;
    public OnCurrencyChanged onSoftCurrencyChanged;

    public delegate void OnVitalityChanged();
    public OnVitalityChanged onVitalityChanged;

    public int HardCurrency
    {
        set
        {
            _props[PlayerPropKey.HardCurrency] = value;

            if (onHardCurrencyChanged != null)
            {
                onHardCurrencyChanged(value);
            }
        }
        get
        {
            return (int)_props[PlayerPropKey.HardCurrency];
        }
    }

    public int SoftCurrency
    {
        set
        {
            _props[PlayerPropKey.SoftCurrency] = value;

            if (onSoftCurrencyChanged != null)
            {
                onSoftCurrencyChanged(value);
            }
        }
        get
        {
            return (int)_props[PlayerPropKey.SoftCurrency];
        }
    }

    public void ReduceHardCurrency(int hardCurrency)
    {

        if ((HardCurrency - hardCurrency) < 0)
        {
            Debug.LogError("ReduceHardCurrency1:" + hardCurrency);
            return;
        }

        if (GameManager.Instance.GameState == EnumGameState.InBattle)
        {
            BattleSummary.Instance.HcCost += hardCurrency;
        }

        HardCurrency -= hardCurrency;
    }

    public void AddHardCurrency(int hardCurrency)
    {
        HardCurrency += hardCurrency;
    }


    public void ReduceSoftCurrency(int softCurrency)
    {
        if ((SoftCurrency - softCurrency) < 0)
        {
            Debug.LogError("ReduceSoftCurrency1:" + softCurrency);
            return;
        }

        SoftCurrency -= softCurrency;
    }

    public void AddSoftCurrency(int softCurrency)
    {
        SoftCurrency += softCurrency;
    }
    #endregion //currency


	public int Vitality
	{
		get
		{
            return (int)_props[PlayerPropKey.Vitality]; 
		}
		set
		{
			_props[PlayerPropKey.Vitality] = value;

			if (onVitalityChanged != null)
			{
				onVitalityChanged();
			}
		}
	}

	public int VitalityBuyCount //within a day
	{
		get
		{
			//check refresh time
			int refreshHour = (int)DataManager.Instance.CurGlobalConfig.getConfig("vitBuyRefreshHour");

			DateTime now = NetworkManager.Instance.serverTime;

			DateTime refreshTime = now.Date.AddHours(refreshHour);

			if (refreshTime > now) //in future
			{
				refreshTime = refreshTime.AddDays(-1);
			}

			DateTime lastVitBuyTime = TimeUtils.k_epoch_time.AddSeconds((int)_props[PlayerPropKey.VitalityBuyTime]);

			if (lastVitBuyTime < refreshTime)
			{
				_props[PlayerPropKey.VitalityBuyCount] = 0;
			}

			return (int)_props[PlayerPropKey.VitalityBuyCount]; 
		}
		set
		{
			_props[PlayerPropKey.VitalityBuyCount] = value;
		}
	}

    public delegate void HPchangeDelegate();
    public HPchangeDelegate OnHpChanged;
    public int HP
    {
        get
        {
            return (int)_props[PlayerPropKey.HP];
        }

        set
        {
            _props[PlayerPropKey.HP] = value;
            if (null != OnHpChanged) 
                OnHpChanged();
        }
    }

    public int MP
    {
        get
        {
            return (int)_props[PlayerPropKey.MP];
        }

        set
        {
            _props[PlayerPropKey.MP] = value;
        }
    }


    public float Attack
    {
        get
        {
            return _props[PlayerPropKey.Attack];
        }

        set
        {
            _props[PlayerPropKey.Attack] = (int)value;
        }
    }

    public float Defense
    {
        get
        {
            return _props[PlayerPropKey.Defense];
        }

        set
        {
            _props[PlayerPropKey.Defense] = value;
        }
    }

    public float Critical
    {
        get
        {
            return _props[PlayerPropKey.Critical];
        }

        set
        {
            _props[PlayerPropKey.Critical] = value;
        }
    }

    public float CriticalRes
    {
        get
        {
            return _props[PlayerPropKey.CriticalRes];
        }

        set
        {
            _props[PlayerPropKey.CriticalRes] = value;
        }
    }

    public float CritDamage
    {
        get
        {
            return _props[PlayerPropKey.CritDamage];
        }

        set
        {
            _props[PlayerPropKey.CritDamage] = value;
        }
    }

    public float CritDamageRes
    {
        get
        {
            return _props[PlayerPropKey.CritDamageRes];
        }

        set
        {
            _props[PlayerPropKey.CritDamageRes] = value;
        }
    }

    public float Damage
    {
        get
        {
            return _props[PlayerPropKey.Damage];
        }

        set
        {
            _props[PlayerPropKey.Damage] = value;
        }
    }

    public float Resistance
    {
        get
        {
            return _props[PlayerPropKey.Resistance];
        }

        set
        {
            _props[PlayerPropKey.Resistance] = value;
        }
    }


    public float FireDmg
    {
        get
        {
            return _props[PlayerPropKey.FireDmg];
        }

        set
        {
            _props[PlayerPropKey.FireDmg] = value;
        }
    }

    public float FireRes
    {
        get
        {
            return _props[PlayerPropKey.FireRes];
        }

        set
        {
            _props[PlayerPropKey.FireRes] = value;
        }
    }

    public float LightningDmg
    {
        get
        {
            return _props[PlayerPropKey.LightningDmg];
        }

        set
        {
            _props[PlayerPropKey.LightningDmg] = value;
        }
    }

    public float LightningRes
    {
        get
        {
            return _props[PlayerPropKey.LightningRes];
        }

        set
        {
            _props[PlayerPropKey.LightningRes] = value;
        }
    }


    public float IceDmg
    {
        get
        {
            return _props[PlayerPropKey.IceDmg];
        }

        set
        {
            _props[PlayerPropKey.IceDmg] = value;
        }
    }

    public float IceRes
    {
        get
        {
            return _props[PlayerPropKey.IceRes];
        }

        set
        {
            _props[PlayerPropKey.IceRes] = value;
        }
    }


    public float PosisonDmg
    {
        get
        {
            return _props[PlayerPropKey.PosisonDmg];
        }

        set
        {
            _props[PlayerPropKey.PosisonDmg] = value;
        }
    }

    public float PosisonRes
    {
        get
        {
            return _props[PlayerPropKey.PosisonRes];
        }

        set
        {
            _props[PlayerPropKey.PosisonRes] = value;
        }
    }

    public int FightScore
    {
        get
        {
            float sum = 0;
            foreach (PlayerPropKey key in _props.Keys)
            {
                PlayerPropData playerPropData = DataManager.Instance.PlayerPropDataList.GetPlayerPropDataByProp(key);
                if (null != playerPropData)
                {
                    float value = _props[key];
                    if (key == PlayerPropKey.Critical || key == PlayerPropKey.CritDamage)
                        value /= 1000;
                    sum += value * playerPropData.fs;
                }
            }
            return (int)sum;
        }
    }


    public int InventoryCount
    {
        get
        {
            return (int)_props[PlayerPropKey.InventoryCount];
        }

        set
        {
            _props[PlayerPropKey.InventoryCount] = value;
        }
    }


    public int VitalityMax
    {
        get
        {
            return (int)_props[PlayerPropKey.VitalityMax];
        }

        set
        {
            _props[PlayerPropKey.VitalityMax] = value;
        }
    }

    public int VitalityTime
    {
        get
        {
            return (int)_props[PlayerPropKey.VitalityTime];
        }

        set
        {
            _props[PlayerPropKey.VitalityTime] = value;
        }
    }


    #endregion

    public int changeNickNameCount;     //player can change his nickname when this number > 0
    public int freeReviveCount;        //player can revive without paying anything when this number > 0

    public double lastMonthCardTime = 0;
    public bool haveClaimMonthCardToday = false;


    //daily bonus related. The data below will not be written back to server.
    public int activeDays = 0; //how many days in this month this account has logged in
    public int dailyBonusClaimTime;
    public int vipLevel
    {
		get
		{
			return (int)_props[PlayerPropKey.Vip];
		}
    }
    public int vipTime;        //time when the vip level was upgraded, in seconds since 1970.01.01
    public int totalIAPTimes = 0;
    public int firstIAP = 0;

    //end daily bonus

    public bool first_tutorial_start_PH = true;
    public bool have_chance_change_nickname = true;

    public int upgradeWeaponTimes;
    public int playMultiplayerTimes;
    public int upgradeSkillTimes;
    public int maxCombo;
    public int honour;

    public int giftState;
    public TimeSpan giftRemainingTime = new TimeSpan(10000);

    public int difficultyLevel;    //max difficulty level this role has opened. 0~2

    public Dictionary<string, int> gachaFreeCount = new Dictionary<string, int>();

    private Dictionary<string, EnumLevelState> _levelsState = new Dictionary<string, EnumLevelState>();

    public Dictionary<string, EnumLevelState> levelsState//level state for normal
    {
        set
        {
            _levelsState = value;
            CalculateUnlockLevel();
        }
        get{return _levelsState;}
    }

    private LevelData _newUnlockLevelData;
    public LevelData NewUnlockLevelData
    {
        get { return _newUnlockLevelData; }
    }

    public Dictionary<string, int> skillsState = new Dictionary<string, int>(); //this will used for local player and multiplayer

	private Dictionary<EnumTutorial, EnumTutorialState> _tutorialState = new Dictionary<EnumTutorial, EnumTutorialState>();
	public Dictionary<EnumTutorial, EnumTutorialState> TutorialState
	{
		set
		{
			_tutorialState = value;
			_hasTutorialDataChange = false;
		}
	}

    public List<string> requestGuilds = new List<string>();
    public List<string> activeSkillList = new List<string>();

    private List<int> _activeQuestIDList = new List<int>();   //list of active quest ids, updated from server

    public List<int> ActiveIDQuestList
    {
        get { return _activeQuestIDList; }
    }

    private List<QuestProgress> _questProgressList = new List<QuestProgress>();

    public List<QuestProgress> QuestProgressList 
    { 
        get 
        { 
            return _questProgressList; 
        } 
    }

	private PlayerInventory _playerInventory;

	public PlayerInventory PlayerInventory
	{
		set
		{
			_playerInventory = value;
		}
		get
		{
			return _playerInventory;
		}
	}

	public PlayerTattoos playerTattoos = new PlayerTattoos();

	public List<string> playerMasteredTattoos;		//the recipes mastered, identified by tattoo id

    public delegate void DelegateLevelUp(int level);
    public delegate void DelegateXpPercent(float percent);
    public DelegateLevelUp OnLevelUp;
    public DelegateXpPercent OnXpPerCentChange;

    private PlayerInventory _equippedInventory = new PlayerInventory();  //this just for other player and multiplayer
	public PlayerInventory EquippedInventory
	{
		get { return _equippedInventory; }
		set { _equippedInventory = value; }
	}

    public Hashtable token = new Hashtable();

    public Hashtable towerRewardHistoryCheckpoint = new Hashtable();

    public bool IsInventoryFull()
    {
        return _playerInventory.itemList.Count >= this.InventoryCount;
    }

    public int HasNewItems()
    {
        int total = 0;
        List<ItemInventory> inventory = PlayerInfo.Instance.PlayerInventory.itemList;

        foreach (ItemInventory inv in inventory)
        {
            if (inv.IsNew)
            {
                total += 1;
            }
        }
        return total;
    }


    public int GetGachaFreeCount(string gachaId)
    {
        if (gachaFreeCount.ContainsKey(gachaId))
        {
            return gachaFreeCount[gachaId];
        }
        else
        {
            return 0;
        }
    }



    public void ChangeGachaFreeCount(string gachaId, int count)
    {
        if (gachaFreeCount.ContainsKey(gachaId))
        {
            gachaFreeCount[gachaId] += count;
        }
        else
        {
            gachaFreeCount[gachaId] = count;
        }
    }

	private bool _hasTutorialDataChange;
	public bool HasTutorialDataChange
	{
		get { return _hasTutorialDataChange; }
	}

	public void ChangeTutorialState(EnumTutorial tutorialId, EnumTutorialState state)
    {
        if (_tutorialState.ContainsKey(tutorialId)) //0=close  , 1=active  ,2=finish
        {
            _tutorialState[tutorialId] = state;
        }
        else
        {
            _tutorialState.Add(tutorialId, state);
        }
		_hasTutorialDataChange = true;
    }

	public EnumTutorialState GetTutorialState(EnumTutorial tutorialId)
    {
		return EnumTutorialState.Finished;

		EnumTutorialState tState = EnumTutorialState.Inactive;
        if (_tutorialState.ContainsKey(tutorialId))
        {
            tState = _tutorialState[tutorialId];
        }

        if (tutorialId == EnumTutorial.Battle_Skill1)
        {
            if (null != DataManager.Instance.GetSkillByKey(FC_KEY_BIND.ATTACK_2))
            {
                if (GetLevelState(LevelManager.Singleton.CurrentLevelData.levelName) >= EnumLevelState.NEW_UNLOCK)
                {
					tState = EnumTutorialState.Finished;
					ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
                }
            }
        }

		if (tutorialId == EnumTutorial.Battle_Skill2)
        {
            if (null != DataManager.Instance.GetSkillByKey(FC_KEY_BIND.ATTACK_3))
            {
                if (GetLevelState(LevelManager.Singleton.CurrentLevelData.levelName) >= EnumLevelState.NEW_UNLOCK)
                {
					tState = EnumTutorialState.Finished;
					ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
                }
            }
        }

		if (tutorialId == EnumTutorial.Town_Skill)
        {
            if (activeSkillList.Count >= 3)
            {
				tState = EnumTutorialState.Finished;
				ChangeTutorialState(tutorialId, EnumTutorialState.Finished);
            }
        }



		if (tState != EnumTutorialState.Finished)
        {
            TutorialTown tutorialTown = TutorialTownManager.Instance.GetTutorialTown(tutorialId);
            if (tutorialTown != null)
            {
                if (tutorialTown.only_once)
                {
                    int tmpState = 0;
                    if (tmpState == 2)
                    {
						return EnumTutorialState.Finished;
                    }
                }
            }
            else
            {
                TutorialLevel tutorialLevel = TutorialManager.Instance.GetTutorialLevel(tutorialId);
                if (tutorialLevel != null)
                {
                    if (tutorialLevel.only_once)
                    {
                        int tmpState = 0;
                        if (tmpState == 2)
                        {
							return EnumTutorialState.Finished;
                        }
                    }
                }
            }
        }
        return tState;
    }

	public void SaveTutorialProgress()
	{
		if (_hasTutorialDataChange)
		{
			NetworkManager.Instance.SendCommand(new SaveTutorialStateRequest(_tutorialState), OnSaveTutorialProgress);
		}
	}

	private void OnSaveTutorialProgress(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			_hasTutorialDataChange = false;
			Debug.Log("Succeess saving tutorial states.");
		}
		else
		{
			Debug.LogError("Error saving tutorial states: " + msg.errorCode);
		}
	}

    public void ChangeLevelState(string levelName, int difficultyLevel, EnumLevelState state)
    {
		Dictionary<string, EnumLevelState> currentLevelState = this.GetLevelStateDict(difficultyLevel);

        if (currentLevelState.ContainsKey(levelName))
        {
            if ((int)state > (int)currentLevelState[levelName])
            {
                currentLevelState[levelName] = state;
				CalculateUnlockLevel();
            }
        }
        else
        {
            currentLevelState.Add(levelName, state);
        }
    }

	public Dictionary<string, EnumLevelState> GetLevelStateDict(int difficultyLevel)
	{
		return levelsState;
	}

    void CalculateUnlockLevel()
    {
        List<LevelData> list = new List<LevelData>(LevelManager.Singleton.LevelsConfig.levels);
        LevelData firstLevelData = null;
        bool hasFound = false;
        foreach (LevelData ld in list)
        {
            if(ld.preLevelID == 0 && ld.available)firstLevelData = ld;
            if (!ld.available) continue;
            EnumLevelState prelvState = PlayerInfo.Instance.GetLevelState(ld.preLevelID);
            EnumLevelState lvState = PlayerInfo.Instance.GetLevelState(ld.levelName);
            if (lvState == EnumLevelState.NEW_UNLOCK //new unlock
            ||
            (lvState == EnumLevelState.LOCKED &&
            prelvState != EnumLevelState.LOCKED &&
            prelvState != EnumLevelState.NEW_UNLOCK
            )
            ||
            (
             lvState == EnumLevelState.LOCKED && ld.preLevelID == 0
            )
            )
            {
                PlayerInfo.Instance.ChangeLevelState(ld.levelName, difficultyLevel, EnumLevelState.NEW_UNLOCK);
                _newUnlockLevelData = ld;
                hasFound = true;
            }
        }
        if (!hasFound)
        {
            _newUnlockLevelData = null;
        }
    }

    public EnumLevelState GetLevelState(string levelName, int difficultyLevel)
    {
		Dictionary<string, EnumLevelState> dict = this.GetLevelStateDict(difficultyLevel);

        if (dict.ContainsKey(levelName))
        {
            return (EnumLevelState)dict[levelName];
        }
        if (null != NewUnlockLevelData && levelName == NewUnlockLevelData.levelName)
            return EnumLevelState.NEW_UNLOCK;
        return EnumLevelState.LOCKED;
    }

    public EnumLevelState GetLevelState(string levelName)
    {
        return GetLevelState(levelName, difficultyLevel);
    }

    public EnumLevelState GetLevelState(int levelId)
    { 
        LevelData lvData = LevelManager.Singleton.LevelsConfig.GetLevelDataByID(levelId);
        if (null != lvData)
            return GetLevelState(lvData.levelName);
        else
            return EnumLevelState.LOCKED;
    }

    private bool _missionUpdate = true;
    private int _lastMission = 0;

    public void SaveLastMissonToLocal(int missionId)
    {
        int player = (int)PlayerInfo.Instance.Role;

        _lastMission = missionId;
        _missionUpdate = true;
        PlayerPrefs.SetInt("LASTLEVEL" + player, missionId);
    }
    public int GetLastMission()
    {
        if (_missionUpdate)
        {
            int player = (int)PlayerInfo.Instance.Role;

            _lastMission = PlayerPrefs.GetInt("LASTLEVEL" + player, 0);
            _missionUpdate = false;
        }

        return _lastMission;
    }

    public int GetSkillLevel(string skillName)
    {
        if (skillsState.ContainsKey(skillName))
        {
            return skillsState[skillName];
        }
        return 0;
    }

    public void ChangeSkillLevel(string skillName, int level)
    {
        skillsState[skillName] = level;
    }

    public ItemInventory GetCurrentEqupedItem(ItemSubType part)
    {
		foreach (ItemInventory item in _equippedInventory.itemList)
        {
            if (item.ItemData.subType == part)
            {
                return item;
            }
        }
        return null;
    }


    public void GetSelfEquipmentIds(List<EquipmentIdx> ids)
    {
        ids.Clear();
		foreach (ItemInventory item in _equippedInventory.itemList)
        {
            EquipmentIdx equipmentIdx = new EquipmentIdx();
            equipmentIdx._id = item.ItemID;
            equipmentIdx._evolutionLevel = 0;
            ids.Add(equipmentIdx);
        }
    }

    public void GetSelfEquipmentInstance(List<GameObject> equipmentInstances)
    {
        equipmentInstances.Clear();
		foreach (ItemInventory item in _equippedInventory.itemList)
        {
            ItemData itemData = DataManager.Instance.GetItemData(item.ItemID);

            if (itemData.type == ItemType.ornament)
            {
                continue;
            }

            if (ItemInventory.IsEquipment(itemData)
                || itemData.type == ItemType.vanity)
            {

                string path = itemData.instance;
                if (path != null && path.Length > 1)
                {
                    GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(path) as GameObject;
                    GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                    FCEquipmentsBase equipBase = instance.GetComponent<FCEquipmentsBase>();
                    equipBase._evolutionLevel = 0;
                    equipmentInstances.Add(instance);
                }
                else
                {
                    Debug.LogError("GetSelfEquipmentInstance:[" + itemData.id + "] path:[" + path + "]");
                }

            }
        }
    }

    public static void GetOtherEquipmentInstanceWithIds(List<GameObject> equipmentInstances, List<EquipmentIdx> ids)
    {
        equipmentInstances.Clear();
        for (int i = 0; i < ids.Count; i++)
        {
            ItemData itemData = DataManager.Instance.GetItemData(ids[i]._id);
            if (itemData.type == ItemType.ornament)
            {
                continue;
            }
            if (ItemInventory.IsEquipment(itemData)
                || itemData.type == ItemType.vanity)
            {
                string path = itemData.instance;
                if (path != null && path.Length > 1)
                {
                    GameObject prefab = InJoy.AssetBundles.AssetBundles.Load(path) as GameObject;
                    GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                    FCEquipmentsBase equipBase = instance.GetComponent<FCEquipmentsBase>();
                    equipBase._evolutionLevel = ids[i]._evolutionLevel;
                    equipmentInstances.Add(instance);
                }
                else
                {
                    Debug.LogError(string.Format("Wrong equipment path. ID = {0}", itemData.id));
                }
            }
        }
    }

    public ItemInventory FindItemInventoryById(Int64 guid)
    {
        ItemInventory item = _playerInventory.GetItem(guid);
        if (null == item) item = _equippedInventory.GetItem(guid);
        return item;
    }


    public void CheatAddItmes(List<string> ids)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            AddItemRandom(ids[i]);
        }
    }


    public void AddItemRandom(string itemId)
    {
        ItemData itemData = DataManager.Instance.GetItemData(itemId);
        AddItemToInvenory(itemData, 1);
    }

    public void AddItemToInvenory(ItemData itemData, int count)
    {
        PlayerInventory.AddItemInventory(itemData, count);
    }

    public static int calculateLevel(int xp)
    {
        List<PlayerLevelData> _list = DataManager.Instance.CurPlayerLevelDataList._dataList;
        int level = 0;
        foreach (PlayerLevelData data in _list)
        {
            if (xp < data._exp)
            {
                break;
            }
            ++level;
            xp -= data._exp;
        }
        return level;
    }


    //if xp <=0  , we will Recalculate level from level = 1;
    public void AddXP(int xp)
    {
        List<PlayerLevelData> _dataList = DataManager.Instance.CurPlayerLevelDataList._dataList;

        int levelCap = _dataList.Count - 1;

        if (CurrentLevel >= levelCap)
        {
            CurrentLevel = levelCap;
            return;
        }

        int requiredXp = 0;

        _props[PlayerPropKey.Exp] += xp;

        while (CurrentXp > 0)
        {
            requiredXp = _dataList[CurrentLevel]._exp;
			if (CurrentXp >= requiredXp)
            {
				_props[PlayerPropKey.Exp] -= requiredXp;

				if (CurrentXp == 0)
                {
                    XpPercentChange(0.0f);
                }

                CurrentLevel++;
                if (CurrentLevel > levelCap)
                {
                    CurrentLevel = levelCap;
                    XpPercentChange(1.0f);
					_props[PlayerPropKey.Exp] = 0;
                }

                if (OnLevelUp != null)
                {
                    OnLevelUp(CurrentLevel);
                    QuestManager.instance.UpdateQuests(QuestTargetType.level_up, "-1", CurrentLevel);
                }
            }
            else
            {
				float tmpXp = CurrentXp * 1.0f;
                XpPercentChange(tmpXp / requiredXp);
                break;
            }
        }
    }


    private void XpPercentChange(float percent)
    {
        _currentXpPrecent = percent;


        if (OnXpPerCentChange != null)
        {
            OnXpPerCentChange(_currentXpPrecent);
        }

    }


    public bool isEquipped(ItemInventory itemInventory)
    {
		return _equippedInventory.itemList.Contains(itemInventory);
    }

    #region save


    public float _skillEffect1 = 0;  //just use in 010. we used _skillState after 010.
    public float _skillEffect2 = 0;

    public bool IsEquipedItem(string itemID)
    {
        foreach (ItemInventory item in _equippedInventory.itemList)
        {
            if (item.ItemID == itemID)
            {
                return true;
            }
        }
        return false;
    }

    #endregion
	
	public void LoadGachaTokenJson(string json)
	{
		Hashtable ht = (Hashtable)FCJson.jsonDecode(json);
		LoadGachaTokenJson(ht);
	}
	
	public void LoadGachaTokenJson(Hashtable ht)
	{
		token.Clear();
		foreach(object key in ht.Keys)
		{
			token[key] = (int)(double)ht[key];
		}
	}
	

	
	public void reduceGachaToken(string id, int amount)
	{
		if(amount < 0)
		{
			return;
		}
		if(!token.ContainsKey(id))
		{
			return;
		}
		amount = (int)token[id] - amount;
		if(amount < 0)
		{
			amount = 0;
		}
		token[id] = amount;
	}
	
	public void addGachaToken(string id, int amount)
	{
		if(amount < 0)
		{
			return;
		}
		if(token.ContainsKey(id))
		{
			amount += (int)token[id];
		}
		token[id] = amount;
	}
	
	public int getGachaTokenAmount(string id)
	{
		if(!token.ContainsKey(id))
		{
			return 0;
		}
		else
		{
			return (int)token[id];
		}
	}	
		
	public bool CheckSkillState(ArrayList skillList)
	{
		foreach(ArrayList al in skillList)
		{
			if(al.Count == 2)
			{
				string key = al[0].ToString();
				if(skillsState.ContainsKey(key))
				{
					if(skillsState[key].ToString() != al[1].ToString())
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		return true;
	}

	public void LoadTowerRewardHistoryCheckpoint(string json)
	{
		Hashtable temp = FCJson.jsonDecode(json) as Hashtable;
		foreach(string key in temp.Keys)
		{
			towerRewardHistoryCheckpoint.Add(key, (temp[key] as Hashtable)["rhcp"]);
		}
	}

	public void SendQueryPlayersRequest()
	{
		//clear _loadedPlayerInfo
		loadedPlayerInfoList = null;

		Debug.Log("SearchPlayers...");
		NetworkManager.Instance.SendCommand(new GetTownPlayersRequest(5), OnGetTownPlayers);

		//CreateFakePlayers();
	}

	private void OnGetTownPlayers(FaustComm.NetResponse msg)
	{
		if (msg.Succeeded)
		{
			loadedPlayerInfoList = (msg as GetTownPlayersResponse).playerInfoList;
		}
		else
		{
			Debug.LogError("Get town players error.");
		}
	}

	private void CreateFakePlayers()
	{
		loadedPlayerInfoList = new List<PlayerInfo>();

		for (int i = 0; i < 6; i++)
		{
			PlayerInfo pi = new PlayerInfo();

			pi.Role = EnumRole.Warrior;

			loadedPlayerInfoList.Add(pi);
		}
	}

	//Select active skills into field activeSkills
	public void UpdatePlayerSkills()
	{
		activeSkillList.Clear();
		Dictionary<string, int> _skillState = skillsState;

		string roleName = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].battleLabel;

		foreach (KeyValuePair<string, int> skill in _skillState)
		{
			SkillData sd = DataManager.Instance.GetSkillData(roleName, skill.Key, false);

			if (!sd.isPassive)
			{
				activeSkillList.Add(skill.Key);
			}
		}
	}

	public void UpgradeSkill(SkillData sd)
	{
		if (skillsState.ContainsKey(sd.skillID))
		{
			skillsState[sd.skillID] = sd.level;
		}
		else
		{
			skillsState.Add(sd.skillID, sd.level);

			this.UpdatePlayerSkills();
		}       
	}

    #region netWorkListeners
    void AddNetworkListeners()
    {
        UpdateInforResponseData.EquipMoveObserver += OnEquipMoveHandler;
        UpdateInforResponseData.ItemUpdateObserver += OnItemUpdateHandler;
        UpdateInforResponseData.PlayerPropUpdateObserver += OnPlayerPropUpdateHandler;
        UpdateInforResponseData.ItemNewAddObserver += OnNewItemsAdded;
        UpdateInforResponseData.ItemsCountUpdateObserver += OnItemsCountUpdateHandler;
		UpdateInforResponseData.PlayerTattoosUpdateObserver += OnPlayerTattooUpdateHandler;
		UpdateInforResponseData.PlayerQuestUpdateObserver += OnPlayerQuestUpdateHandler;
	}

    void RemoveNetworkListeners()
    {
        UpdateInforResponseData.EquipMoveObserver -= OnEquipMoveHandler;
        UpdateInforResponseData.ItemUpdateObserver -= OnItemUpdateHandler;
        UpdateInforResponseData.PlayerPropUpdateObserver -= OnPlayerPropUpdateHandler;
        UpdateInforResponseData.ItemNewAddObserver -= OnNewItemsAdded;
        UpdateInforResponseData.ItemsCountUpdateObserver -= OnItemsCountUpdateHandler;
        UpdateInforResponseData.PlayerTattoosUpdateObserver -= OnPlayerTattooUpdateHandler;
        UpdateInforResponseData.PlayerQuestUpdateObserver -= OnPlayerQuestUpdateHandler;
    }

    void OnEquipMoveHandler(ItemMoveVo[] itemMoveOps)
    {
        foreach (ItemMoveVo moveVo in itemMoveOps)
        {
            ItemInventory item = null;
            if (moveVo.MoveType == ItemMoveType.EquipOn)
            {
                item = PlayerInventory.GetItem(moveVo.ItemGUID);
                PlayerInventory.RemoveItem(moveVo.ItemGUID, 1);
                EquippedInventory.AddItemInventory(item);
            }
            else if (moveVo.MoveType == ItemMoveType.EquipOff)
            {
                item = EquippedInventory.GetItem(moveVo.ItemGUID);
                EquippedInventory.RemoveItem(item.Item_GUID, 1);
                PlayerInventory.AddItemInventory(item);
            }
        }
    }

    void OnNewItemsAdded(ItemInventory[] items)
    {
        foreach (ItemInventory item in items)
        {
            PlayerInventory.AddItemInventory(item);
            if (EnumGameState.InTown == GameManager.Instance.GameState)
            {
                UIRewardHint.Instance.EnqueueDisplayItem(item.ItemID, item.Count);
            }
            Debug.Log(string.Format("Parser New item GUID = {0}", item.Item_GUID));
        }
    }

    void OnItemUpdateHandler(ItemAttributeUpdateVoList[] itemsUpdateItemAttribute)
    {
        foreach (ItemAttributeUpdateVoList listVo in itemsUpdateItemAttribute)
        {
            ItemInventory ii = FindItemInventoryById(listVo.ItemGuid);
            foreach (ItemAttributeUpdateVo attributeVo in listVo.VoList)
            {
                ii.SetEquipExtendAttributeValueByKey(attributeVo.Key, attributeVo.Value);
            }
        }
    }

    void OnPlayerPropUpdateHandler(PlayerProp[] props)
    {
        ApplyPlayerPropChanges(props);
    }

    void OnItemsCountUpdateHandler(List<ItemCountVo> itemCountOps)
    {
        foreach (ItemCountVo itemCountVo in itemCountOps)
        {
            ItemInventory item = PlayerInventory.GetItem(itemCountVo.ItemGuid);
            if (null != item
                &&
                EnumGameState.InTown == GameManager.Instance.GameState
                &&
                itemCountVo.ItemCount - item.Count > 0
                )
            {
                UIRewardHint.Instance.EnqueueDisplayItem(item.ItemID,
                    itemCountVo.ItemCount - item.Count);
            }
        }
        PlayerInventory.ApplyItemCountChanges(itemCountOps);
    }

	void OnPlayerTattooUpdateHandler(List<TattooEquipVo> ops)
	{
		foreach (TattooEquipVo op in ops)
		{
			ItemInventory ii = _playerInventory.GetItem(op.ItemGuid);

			if (ii == null)
			{
				ii = playerTattoos.GetItemByGUID(op.ItemGuid);
			}

			if (op.op == 1) //1: mount
			{
				//check if the current item is equipped on another slot
				if (playerTattoos.tattooDict.ContainsValue(ii))
				{
					foreach (KeyValuePair<EnumTattooPart, ItemInventory> kvp in playerTattoos.tattooDict)
					{
						if (kvp.Value == ii)
						{
							playerTattoos.tattooDict.Remove(kvp.Key);
							break;
						}
					}
				}

				if (playerTattoos.tattooDict.ContainsKey(op.part))
				{
					playerTattoos.tattooDict[op.part] = ii;
				}
				else
				{
					playerTattoos.tattooDict.Add(op.part, ii);
				}
			}
			else  //2: remove
			{
				playerTattoos.tattooDict.Remove(op.part);
			}
		}
	}

	void OnPlayerQuestUpdateHandler(List<int> ops)
	{
		QuestManager.instance.ApplyActiveQuestIdList(ops);
	}
    #endregion

	public bool CanBearTattoo(TattooData newTD, TattooData oldTD)
	{
		int sum = this.BearingPoint;

		int consumed = this.ConsumedBearingPoint;

		if (oldTD == null && consumed + newTD.bearingPoint <= sum ||
			oldTD != null && consumed - oldTD.bearingPoint + newTD.bearingPoint <= sum)
		{
			return true;
		}

		return false;
	}

	public int BearingPoint
	{
		get
		{
			PlayerLevelData pld = DataManager.Instance.CurPlayerLevelDataList.GetPlayerLevelDataByLevel(PlayerInfo.Instance.CurrentLevel);

			return pld.bearPoint;
		}
	}

	public int ConsumedBearingPoint
	{
		get
		{
			int consumed = 0;

			foreach (ItemInventory ii in playerTattoos.tattooDict.Values)
			{
				TattooData tdTmp = DataManager.Instance.GetTattooData(ii.ItemID);

				consumed += tdTmp.bearingPoint;
			}

			return consumed;
		}
	}
}
