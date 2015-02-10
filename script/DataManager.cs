using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class DataManager : MonoBehaviour
{
	private static DataManager _instance;
	public static DataManager Instance
	{
		get { return _instance; }
	}

	public string bundleConfigPath;

	public ItemDataList[] _itemDataList;
	private ItemDataManager _itemDataManager = new ItemDataManager();
	public ItemDataManager ItemDataManager
	{
		get { return _itemDataManager; }
	}

	public GlobalConfig _globalConfig;
	public GlobalConfig CurGlobalConfig
	{
		get
		{
			return _globalConfig;
		}
	}

	public PlayerLevelDataList _playerLevelDataList;

	private PlayerLevelDataList _curPlayerLevelDataList;
	public PlayerLevelDataList CurPlayerLevelDataList
	{
		get
		{
			if (_curPlayerLevelDataList == null)
			{
				_curPlayerLevelDataList = _playerLevelDataList;
			}
			return _curPlayerLevelDataList;
		}
	}

	public InitInformation _initInformation;

	/// <summary>
	/// Please use CurAcDataList to instead.
	/// </summary>
	public AcDataList _acDataList;

	private AcDataList _curAcDataList;

	public AcDataList CurAcDataList
	{
		get
		{
			if (_curAcDataList == null)
			{
				_curAcDataList = _acDataList;
			}
			return this._curAcDataList;
		}
	}

	public SkillDataList _skillDataList;



	public FusionDataList _fusionDataList;

	public EvolutionDataList _evolutionDataList;
	private EvolutionDataList _curEvolutionDataList;
	public EvolutionDataList CurEvolutionDataList
	{
		get
		{
			if (_curEvolutionDataList == null)
			{
				_curEvolutionDataList = _evolutionDataList;
			}
			return this._curEvolutionDataList;
		}
	}

    public FC_StoreDataList storeDataList;

	public SocketDataList _socketDataList;
	public UpgradeConfigData _upgradeConfigData;

    public EquipmentFSDataList EquipmentFSDataList;
    public PlayerPropDataList PlayerPropDataList;


	public HeroEquipmentWeight _mageWeight;
	public HeroEquipmentWeight _warriorWeight;
	public HeroEquipmentWeight _monkWeight;

	/// <summary>
	/// Please use CurSkillUpgradeDataList to instead.
	/// </summary>
	public SkillUpgradeDataList _skillUpgradeDataList;

	private SkillUpgradeDataList _curSkillUpgradeDataList;

	public SkillUpgradeDataList CurSkillUpgradeDataList
	{
		get
		{
			if (_curSkillUpgradeDataList == null)
			{
				_curSkillUpgradeDataList = _skillUpgradeDataList;
			}
			return this._curSkillUpgradeDataList;
		}
	}

	private OfferingDataList _offeringDataList;
	public OfferingDataList offeringDataList;

	private OfferingGroupList _offeringGroupList;
	public OfferingGroupList offeringGroupList;

	public TattooDataList tattooDataList;

	public TattooSuiteDataList tattooSuiteDataList;

	public TattooUnlockDataList tattooUnlockDataList;

	public TattooExchangeDataList tattooExchangeDataList;

	public TattooAttributeList tattooAttributeList;	//hit params list

    public EotDataList eotDataList;

    public ErrorDefineList errorDataList;

    public NickNameDataList nickNameDataList;

	public VipPrivilegeList vipPrivilegeList;

	public VitPriceList vitPriceList;

	public DialogDataList dialogDataList;

	public OfferingGroup GetOfferingGroup(string groupId)
	{
		foreach (OfferingGroup offeringGroup in OfferingGroupList.dataList)
		{
			if (offeringGroup.groupId == groupId)
			{
				return offeringGroup;
			}
		}
		return null;
	}

	public List<OfferingData> GetOfferingDataByPlayerLevel(int level)
	{
		return OfferingDataList.dataList.FindAll(delegate(OfferingData off) { return (off.levelMin <= level) && (off.levelMax >= level); });
	}


	public OfferingData GetOfferingData(int playerLevel, OfferingLevel level)
	{
		List<OfferingData> list = GetOfferingDataByPlayerLevel(playerLevel);

		if (null != list && list.Count > 0)
		{
			return list.Find(delegate(OfferingData off) { return (off.level == level); });
		}

		return null;
	}

	public OfferingGroupList OfferingGroupList
	{
		get
		{
			if (_offeringGroupList == null)
			{
				_offeringGroupList = offeringGroupList;
			}
			return _offeringGroupList;
		}
	}

	public OfferingDataList OfferingDataList
	{
		get
		{
			if (_offeringDataList == null)
			{
				_offeringDataList = offeringDataList;
			}
			return _offeringDataList;
		}
	}


	public PLevelData GetCurrentClassLevelData(int level)
	{
		return GetCurrentClassLevelData(level, PlayerInfo.Instance.Role);
	}


    public PLevelData GetCurrentClassLevelData(int level, EnumRole role)
	{
		PLevelData data = new PLevelData();

		if (level < 0 || level >= CurPlayerLevelDataList._dataList.Count)
		{
			Debug.LogError("level is out range: [" + level + "]");
			level = 1;
		}


		PlayerLevelData playerLevelData = CurPlayerLevelDataList._dataList[level];

		data._level = playerLevelData._level;
		data._xp = playerLevelData._exp;
		data._reviveHc = playerLevelData._reviveHc;

		if (role == EnumRole.Mage)
		{
			data._hp = playerLevelData._mage_hp;
			data._attack = playerLevelData._mage_attack;
			data._defense = playerLevelData._mage_defense;
			data._crit_rate = playerLevelData._mage_crit;
			data._crit_damage = playerLevelData._mage_crit_damage;
		}
		else if (role == EnumRole.Warrior)
		{
			data._hp = playerLevelData._warrior_hp;
			data._attack = playerLevelData._warrior_attack;
			data._defense = playerLevelData._warrior_defense;
			data._crit_rate = playerLevelData._warrior_crit;
			data._crit_damage = playerLevelData._warrior_crit_damage;
		}

		return data;
	}

	public bool FusionDataExit(int level, int itemLevel, ItemType type)
	{
		string key = type.ToString() + "_" + itemLevel.ToString() + "_" + level.ToString();
		return _fusionDataList.FusionDataMapping.ContainsKey(key);
	}

	public FusionData GetFusionData(int level, int itemLevel, ItemType type)
	{
		string key = type.ToString() + "_" + itemLevel.ToString() + "_" + level.ToString();
		if (_fusionDataList.FusionDataMapping.ContainsKey(key))
		{
			return _fusionDataList.FusionDataMapping[key];
		}
		else
		{
			Debug.LogError("GetFusionData:[" + key + "] could not find!");
			return null;
		}
	}

	public string GetAttriubteDisplay(AIHitParams type, float value, Color typeColor, Color valueColor)
	{
		string valueStr;
        if (IsPercentFormat(type))
		{
			value = value * 100;
            value = Mathf.Round(value * 10) / 10.0f;
            valueStr = String.Format("{0:F1}", value) + "%";
		}
		else
		{
            value = Mathf.Round(value * 10) / 10.0f;
			valueStr = value.ToString();
		}

        string typeColorString = NGUITools.EncodeColor(typeColor);
        string valueColorString = NGUITools.EncodeColor(valueColor);

        EquipmentFSData fsData = EquipmentFSDataList.GetFSDataByAttribute(type);
        string localizationString = Localization.Localize(fsData.ids);
        if (localizationString.IndexOf("{0}") != -1)
        {
            return string.Format(localizationString, "[" + valueColorString + "]" + valueStr + "[-]");
        }
        else
        {
            return "[" + typeColorString + "]" + localizationString +
                " : [" + valueColorString + "]" + valueStr + "[-]";
        }
	}

    public string GetAttriubteDisplay(AIHitParams type, float value, Color typeColor)
    {
        return GetAttriubteDisplay(type, value, typeColor, new Color(190f / 255f, 170f / 255f, 130f / 255f));
    }

    public string GetAttriubteDisplay(AIHitParams type, float value)
    {
        return GetAttriubteDisplay(type, value,  new Color(255f / 255f, 190f / 255f, 0f / 255f));
    }

	public string GetExtraAttributeValueDisplay(AIHitParams type, float value, Color color)
	{
		string valueStr;
        if (IsPercentFormat(type))
		{
            value = value * 100;
            value = Mathf.Round(value * 10) / 10.0f;
            valueStr = String.Format("{0:F1}", value) + "%";
		}
		else
		{
            value = Mathf.Round(value * 10) / 10.0f;
			valueStr = value.ToString();
		}

        string colorString = NGUITools.EncodeColor(color);
        if (null != EquipmentFSDataList.GetFSDataByAttribute(type))
			return " [" + colorString + "] + " + valueStr + "[-]";
		return "IDS_NULL";
	}

    public string GetExtraAttributeValueDisplay(AIHitParams type, float value)
    {
        return GetExtraAttributeValueDisplay(type, value, new Color(190 / 255f, 170f / 255f, 130f / 255f));
    }

    public bool IsPercentFormat(AIHitParams type)
    { 
        switch (type)
        {
            case AIHitParams.HpPercent:
            case AIHitParams.MpPercent:
            case AIHitParams.RunSpeedPercent:
            case AIHitParams.AllElementResistPercent:
            case AIHitParams.AllElementDamagePercent:
            case AIHitParams.CriticalChance:
            case AIHitParams.CriticalChancePercent:
            case AIHitParams.CriticalChanceResist:
            case AIHitParams.CriticalChanceResistPercent:
            case AIHitParams.CriticalDamage:
            case AIHitParams.CriticalDamagePercent:
            case AIHitParams.CriticalDamageResist:
            case AIHitParams.CriticalDamageResistPercent:
            case AIHitParams.AttackPercent:
            case AIHitParams.DefencePercent:
            case AIHitParams.FireDamagePercent:
            case AIHitParams.FireResistPercent:
            case AIHitParams.IceDamagePercent:
            case AIHitParams.IceResistPercent:
            case AIHitParams.LightningDamagePercent:
            case AIHitParams.LightningResistPercent:
            case AIHitParams.PoisonDamagePercent:
            case AIHitParams.PoisonResistPercent:
            case AIHitParams.ItemFind:
                return true;
            default:
                return false;
        }
    }

    public bool IsPercentFormat(PlayerPropKey key)
    { 
        switch (key)
        {
            case PlayerPropKey.Critical:
            case PlayerPropKey.CritDamage:
            case PlayerPropKey.ItemFind:
                return true;
            default:
                return false;
        }
    }

	public float GetAttriubteValue(AIHitParams type)
	{
		foreach (PropertyScore propertyScore in _upgradeConfigData._attributeDataList._dataList)
		{
			if (propertyScore._type == type)
			{
				return propertyScore._value;
			}
		}
		return 1.0f;
	}


	public PropertyWeightDataList GetWeightList(ItemData itemData, int role)
	{
		HeroEquipmentWeight heroWeight = null;

		if (role == 0)
		{
			heroWeight = _mageWeight;
		}
		else if (role == 1)
		{
			heroWeight = _warriorWeight;
		}
		else if (role == 2)
		{
			heroWeight = _monkWeight;
		}

		if (heroWeight != null)
		{
			if (itemData.subType == ItemSubType.weapon)
			{
				return heroWeight._weaponWeight;
			}
			else if (itemData.subType == ItemSubType.helmet)
			{
				return heroWeight._helmWeight;
			}
			else if (itemData.subType == ItemSubType.shoulder)
			{
				return heroWeight._shoulderWeight;
			}
			else if (itemData.subType == ItemSubType.belt)
			{
				return heroWeight._beltWeight;
			}
			else if (itemData.subType == ItemSubType.armpiece)
			{
				return heroWeight._armpieceWeight;
			}
			else if (itemData.subType == ItemSubType.leggings)
			{
				return heroWeight._leggingsWeight;
			}
			else if (itemData.subType == ItemSubType.necklace)
			{
				return heroWeight._necklaceWeight;
			}
			else if (itemData.subType == ItemSubType.ring)
			{
				return heroWeight._ringWeight;
			}
		}

		return null;
	}





	public SkillUpgradeData GetSkillUpgradeData(string enemyId, string skillName)
	{
		foreach (SkillUpgradeData skillUpgradeData in CurSkillUpgradeDataList.dataList)
		{
			if (skillUpgradeData._enemyId == enemyId)
			{
				if (skillUpgradeData._skillId == skillName)
				{
					return skillUpgradeData;
				}
			}
		}
		Debug.LogError("GetSkillUpgradeData:[" + skillName + "] ,can't find");
		return null;
	}


	public static string GetSkillName(int roleId)
	{
		string skillName = "";
		if (roleId == 0)
		{
			skillName = "MageTraining";
		}
		else if (roleId == 1)
		{
			skillName = "Fortified";
		}
		else
		{
			skillName = "ChiBreak";
		}
		return skillName;
	}

	public List<SkillData> GetCurrentUsedSkill()
	{
		List<SkillData> usedSkills = new List<SkillData>();

		string roleName = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].battleLabel;

		for (int i = 0; i < PlayerInfo.Instance.activeSkillList.Count; i++)
		{
			string skillName = PlayerInfo.Instance.activeSkillList[i];

			if (!string.IsNullOrEmpty(skillName))
			{
				SkillData skillData = GetSkillData(roleName, skillName, true);
				usedSkills.Add(skillData);
			}
		}

		return usedSkills;
	}

	public SkillData GetSkillByKey(FC_KEY_BIND key)
	{
		int pos = (int)key - 2;

		List<string> usedSkills = PlayerInfo.Instance.activeSkillList;
		List<SkillData> usedSkillsData = GetCurrentUsedSkill();

		if (pos >= usedSkills.Count || usedSkills[pos] == "")
		{
			return null;
		}

		foreach (SkillData sd in usedSkillsData)
		{
			if (usedSkills[pos] == sd.skillID)
			{
				return sd;
			}
		}

		return null;
	}

	public List<SkillData> GetAllSkill()
	{
		List<SkillData> usedSkills = new List<SkillData>();

		string roleName = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].battleLabel;

		foreach (SkillData skillData in _skillDataList._dataList)
		{
			if (skillData.enemyID == roleName)
			{
				if (IsSkill(skillData.skillID))
				{
					usedSkills.Add(skillData);
				}
			}
		}
		return usedSkills;
	}


    public List<SkillData> GetAllPassiveSkill()
    {
        List<SkillData> usedSkills = new List<SkillData>();

        string roleName = GameSettings.Instance.roleSettings[PlayerInfo.Instance.RoleID].battleLabel;

        foreach (SkillData skillData in _skillDataList._dataList)
        {
            if (skillData.enemyID == roleName && skillData.isPassive)
            {
                if (IsSkill(skillData.skillID))
                {
                    usedSkills.Add(skillData);
                }
            }
        }
        return usedSkills;
    }


	private bool IsSkill(string skillName)
	{
		if (skillName != "Slashx4"
				&& skillName != "MagicBall")
		{
			return true;
		}
		return false;
	}


	public SkillData GetSkillData(int roleId, string skillName, int level)
	{
        string enemyId = GameSettings.Instance.roleSettings[roleId].battleLabel;
		return GetSkillData(enemyId, skillName, level);
	}


    public SkillData GetSkillData(string enemyId, string skillName, int level)
    {
        foreach (SkillData skillData in _skillDataList._dataList)
        {
            if (skillData.enemyID == enemyId)
            {
                if (skillData.skillID == skillName)
                {
                    skillData.InitSkillUpgradeData();
                    skillData.level = level;

                    return skillData;
                }
            }
        }
        Debug.LogError("GetSkillData:[" + skillName + "] ,can't find");
        return null;
    }


	public SkillData GetSkillData(string enemyId, string skillName, bool getLevel)
	{
		foreach (SkillData skillData in _skillDataList._dataList)
		{
			if (skillData.enemyID == enemyId)
			{
				if (skillData.skillID == skillName)
				{
					skillData.InitSkillUpgradeData();
					if (getLevel)
					{
						skillData.level = PlayerInfo.Instance.GetSkillLevel(skillName);

					}
					return skillData;
				}
			}
		}
		Debug.LogError("GetSkillData:[" + skillName + "] ,can't find");
		return null;
	}

	void Awake()
	{
		_instance = this;
		_itemDataManager.SetData(_itemDataList);

		CurGlobalConfig.initMap();

		// encrypt ac data.
		foreach (AcData ad in _acDataList._dataList)
		{
			ad.Encrypt();
		}
	}

	public TattooSuiteData GetTattooSuiteData(string suiteID)
	{
		return tattooSuiteDataList.dataList.Find(delegate(TattooSuiteData tsd) { return tsd.suiteID == suiteID; });
	}

	public TattooData GetTattooData(string tattooID)
	{
		return tattooDataList.dataList.Find(delegate(TattooData td) { return td.tattooID == tattooID; });
	}

	public TattooData GetTattooDataByRecipeID(string recipeID)
	{
		return tattooDataList.dataList.Find(delegate(TattooData td) { return td.recipeID == recipeID; });
	}

	//get IDS in at most three lines
	public string GetItemAttributesDisplay(ItemData itemData)
	{
		string result = DataManager.Instance.GetAttriubteDisplay(itemData.attrId0, itemData.attrValue0);

		if (itemData.attrId1 != AIHitParams.None)
		{
			result += "\n" + DataManager.Instance.GetAttriubteDisplay(itemData.attrId1, itemData.attrValue1);
		}

		if (itemData.attrId2 != AIHitParams.None)
		{
			result += "\n" + DataManager.Instance.GetAttriubteDisplay(itemData.attrId2, itemData.attrValue2);
		}
		return result;
	}

	void OnDestroy()
	{
		_instance = this;
	}

	public ItemData GetItemData(string itemId)
	{
		return _itemDataManager.GetItemData(itemId);
	}

	public List<EnumTattooPart> GetTattooSuiteApplicablePositions(TattooSuiteData tsd)
	{
		List<EnumTattooPart> list = new List<EnumTattooPart>();

		foreach (string tattooID in tsd.tdList)
		{
			TattooData td = this.GetTattooData(tattooID);
			foreach (EnumTattooPart part in td.applicableParts)
			{
				if (!list.Contains(part))
				{
					list.Add(part);
				}
			}
		}

		return list;
	}

	public int GetTattooSuiteBearPoints(TattooSuiteData tsd)
	{
		int sum = 0;

		foreach (string tattooID in tsd.tdList)
		{
			TattooData td = this.GetTattooData(tattooID);

			sum += td.bearingPoint;
		}
		return sum;
	}

    public EotData GetEotDataByEotID(string eotID)
    {
        foreach(EotData eot in eotDataList.dataList)
        {
            if (eot.eotID == eotID)
            {
                return eot;
            }
        }

        return null;
    }

	public DialogData GetDialogData(string id)
	{
		return dialogDataList.dataList.Find(delegate(DialogData d) { return d.id == id; });
	}
}