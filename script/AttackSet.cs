using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class AttackConditions
{
	public enum ATTACK_JUMP_CONDITIONS
	{
		BIND_KEY_REALEASE,
		HIT_TARGET,
		ON_PARRY,
		TO_END,
		TO_SKILL_END,
		COMMON_CONDITION,
		CHANCE_GO_NEXT
	}
	
	
	
	public enum CONDITION_VALUE
	{	
		NONE,
		DASH_ATTACK,
		DASH_END1,
		DASH_END2,
		DODGE_END1,
		DODGE_END2,
		SEVEN_END1,
		SEVEN_END2,
		SEVEN_END3
	}
	
	public ATTACK_JUMP_CONDITIONS _attCon;
	public CONDITION_VALUE _value = CONDITION_VALUE.NONE;
	public int _conVal;
	public int _jumpIdx;
}
[System.Serializable]
public class FCAttackConfig
{
	public string _attackModuleName;
	protected  AttackBase _attackModule = null;
	public AttackBase AttackModule
	{
		get
		{
			return _attackModule;
		}
		set
		{
			_attackModule = value;
		}
	}
	public int EnergyCost
	{
		get
		{
			return _attackModule._energyCost;
		}
	}
	public AttackConditions[] _attackConditions = null;
}

[System.Serializable]
public class FCSkill
{
	public string _skillName;
	public int _comboHitMax = 0;
	public int _priority = 1;
	public bool _isInLoop = false;
	//means when change to idle or run ,and then change to attack again, we can cache the attack combo count
	public bool _canCacheCombo = false;
	public float _coolDownTimeMax;
	public FC_COMBO_KIND _comboKind;
	public FCAttackConfig[] _attackConfigs;
	public bool _isNormalSkill = false;
	public string[] _defyModule;

	private SkillData _skillData;
	
	//total super armor = _superArmorValue + _superArmorValuePer*defenseValue
	public float _superArmorValuePer = 0;
	public int _superArmorValue = 0;
	public float _superArmorDamageAbsorb = 0;
	
	public float _superArmorLife = -1;
	public bool _needDataFromDatabase = true;
	
	protected int _superArmorTotal = 0;
	
	public int SuperArmorTotal
	{
		get
		{
			return _superArmorTotal;
		}
	}
	
	public void InitSkillData(SkillData skillData, AIAgent owner)
	{
		_skillData = skillData;
		_superArmorTotal = (int)(_superArmorValuePer*owner.ACOwner.Data.TotalHp)+_superArmorValue;
		if(_skillData != null)
		{
			_coolDownTimeMax = _skillData.CurrentLevelData.coolDownTime;
		}
		for(int i = 0 ;i<_attackConfigs.Length ; i++ )
		{
			_attackConfigs[i].AttackModule.InitSkillData(_skillData, owner);
		}
	}
}

[System.Serializable]
public class FCSkillConfig
{
	public string _skillName = "";
	public int _attackWeight = 100;
	
	public FC_KEY_BIND _keyBind = FC_KEY_BIND.NONE;
	
	public float _distanceMinSqrt;
	public float _distanceMaxSqrt;
	
	public bool _isDefaultSkill = false;
	public bool _isRageSkill = false;
	
	//should not use withdefy and withFinalHitDefy to detect wheather a 
	// skill should defy when final attack is finish
	public bool _withDefy = false;
	
	
	protected bool _finalAttackIsHitTarget = false;
	
	protected bool _withSpecCombo = false;
	
	protected float _publicFloatValue = 0;
	
	protected object _publicObjectValue;
	
	public object PublicObjectValue
	{
		get
		{
			return _publicObjectValue;
		}
		set
		{
			_publicObjectValue = value;
		}
	}
	
	public float PublicFloatValue
	{
		get
		{
			return _publicFloatValue;
		}
		set
		{
			_publicFloatValue = value;
		}
	}
	
	protected int _publicIntValue = 0;
	
	public int PublicIntValue
	{
		get
		{
			return _publicIntValue;
		}
		set
		{
			_publicIntValue = value;
		}
	}
	
	public bool WithSpecCombo
	{
		get
		{
			return _withSpecCombo;
		}
		set
		{
			_withSpecCombo = value;
		}
	}
	
	public bool FinalAttackIsHitTarget
	{
		get
		{
			return _finalAttackIsHitTarget;
		}
		set
		{
			_finalAttackIsHitTarget = value;
		}
	}
	public bool WithDefy
	{
		get
		{
			bool ret = false;
			if(!_withFinalHitDefy || _finalAttackIsHitTarget)
			{
				ret = _withDefy;
			}
			return	ret;
		}
	}
	
	//if false, means always defy when attack final
	//if true, means should defy when final attack is hit player
	public bool _withFinalHitDefy = false;
	
	public FCConst.NET_POSITION_SYNC_LEVEL _positionSync = FCConst.NET_POSITION_SYNC_LEVEL.LEVEL_0;
	
	protected FCSkill _skill = null;
	
	protected float _coolDownTime = 0;
	
	
	public int SuperArmorTotal
	{
		get
		{
			return _skill.SuperArmorTotal;
		}
	}
	
	public float SuperArmorLife
	{
		get
		{
			return _skill._superArmorLife;
		}
	}
	public float SuperArmorDamageAbsorb
	{
		get
		{
			return _skill._superArmorDamageAbsorb;
		}
	}
	
	public int EnergyCost
	{
		get
		{
			return _skill._attackConfigs[0].EnergyCost;
		}
	}
	public float CoolDownTime
	{
		get
		{
			return _coolDownTime;
		}
		set
		{
			_coolDownTime = value;
		}
	}
	
	public float CoolDownTimeMax
	{
		get
		{
			return _skill._coolDownTimeMax;
		}
		set
		{
			_skill._coolDownTimeMax = value;
		}
	}
	
	public bool SkillIsCD
	{
		get
		{
			return _coolDownTime < 0.1f;
		}
	}
	
	public float GetSkillActiveDistanceMin()
	{
		return _skill._attackConfigs[0].AttackModule._distanceMin;
	}
	
	public FCSkill SkillModule
	{
		get
		{
			return _skill;
		}
		set
		{
			_skill = value;
		}
	}
	
	public int ComboHitMax
	{
		get
		{
			return _skill._comboHitMax;
		}
	}
	
	public bool IsInLoop
	{
		get
		{
			return _skill._isInLoop;
		}
	}
	
	protected int _comboHitValue = 0;
	
	public int ComboHitValue
	{
		get
		{
			return _comboHitValue;
		}
		set
		{
			_comboHitValue = value;
		}
	}
	
	public int NextComboHitValue
	{
		get
		{
			int v = _comboHitValue+1;
			if(v>=ComboHitMax)
			{
				v = 0;
			}
			return v;
		}
	}
	
	public int KeyBind
	{
		get
		{
			return (int)_keyBind;
		}
		set
		{
			_keyBind = (FC_KEY_BIND)value;
		}
	}
}

[System.Serializable]
public class FCPassiveSkillConfig
{
	public string _skillName = "";
	
	protected FCPassiveSkill _skill = null;
	
	protected float _coolDownTime = 0;
	
	public float CoolDownTime
	{
		get
		{
			return _coolDownTime;
		}
		set
		{
			_coolDownTime = value;
		}
	}
	
	public float CoolDownTimeMax
	{
		get
		{
			return _skill.coolDownTimeMax;
		}
		set
		{
			_skill.coolDownTimeMax = value;
		}
	}
	
	public bool SkillIsCD
	{
		get
		{
			return _coolDownTime < 0.1f;
		}
	}
	
	public FCPassiveSkill SkillModule
	{
		get
		{
			return _skill;
		}
		set
		{
			_skill = value;
		}
	}
}


[System.Serializable]
public class FCPassiveSkillAttribute
{
    public AIHitParams skillType = AIHitParams.Hp;
    public float attributeValue = 0.0f;

}

[System.Serializable]
public class FCPassiveSkill
{
    public string skillName = "";
    public List<FCPassiveSkillAttribute> skillAttributes;
    public float coolDownTimeMax;
    public bool beActive = false;
    public bool needDataFromDatabase = true;

    public PassiveSkillBase passiveSkillBase = null;

    private ActionController _owner = null;
    private SkillData _skillData;

    public ActionController ACOwner
    {
        get
        {
            return _owner;
        }
    }

    public void InitSkillData(SkillData skillData, ActionController owner, PassiveSkillBase[] skillBaseHandles)
    {
        _owner = owner;
        _skillData = skillData;
        if (_skillData != null && _skillData.CurrentLevelData != null)
        {
            SkillGrade skillGrade = _skillData.CurrentLevelData;

            if (null != skillGrade)
            {
                coolDownTimeMax = _skillData.CurrentLevelData.coolDownTime;

                FCPassiveSkillAttribute skillAttribute1 = skillAttributes.Find(delegate(FCPassiveSkillAttribute att1) { return att1.skillType == (AIHitParams)skillGrade.passiveType1; });

                if (null != skillAttribute1)
                {
                    skillAttribute1.attributeValue = skillGrade.attribute1;
                }
                else
                {
                    skillAttribute1 = new FCPassiveSkillAttribute();
                    skillAttribute1.skillType = (AIHitParams)skillGrade.passiveType1;
                    skillAttribute1.attributeValue = skillGrade.attribute1;
                    skillAttributes.Add(skillAttribute1);
                }

                FCPassiveSkillAttribute skillAttribute2 = skillAttributes.Find(delegate(FCPassiveSkillAttribute att2) { return att2.skillType == (AIHitParams)skillGrade.passiveType2; });

                if (null != skillAttribute2)
                {
                    skillAttribute2.attributeValue = skillGrade.attribute2;
                    skillAttribute2.attributeValue = 0;
                }
                else
                {
                    skillAttribute2 = new FCPassiveSkillAttribute();
                    skillAttribute2.skillType = (AIHitParams)skillGrade.passiveType2;
                    skillAttribute2.attributeValue = skillGrade.attribute2;
                    skillAttributes.Add(skillAttribute2);
                }
            }

            if (skillData.level == 0)
            {
                beActive = false;
            }
        }
        else if (_skillData != null)
        {
            beActive = false;
        }

        if (skillName != "" && null != skillBaseHandles)
        {
            foreach (PassiveSkillBase psb in skillBaseHandles)
            {
                if (psb._skillName == skillName)
                {
                    passiveSkillBase = psb;
                    break;
                }
            }
        }

        if (passiveSkillBase != null)
        {
            passiveSkillBase.Init(this);
        }
    }
}

/*[System.Serializable]
public class FCPassiveSkillHitPoint
{
	public float _hpScalePercent;
	public float _hpValue;
}

[System.Serializable]
public class FCPassiveSkillDamageAbsorb : FCPassiveSkill
{
	public float _absorbPercent;
	public float _absorbValue;
}

[System.Serializable]
public class FCPassiveSkillDamageScale : FCPassiveSkill
{
	public float _damagePercent;
	public float _damageValue;
}

[System.Serializable]
public class FCPassiveSkillCriticle : FCPassiveSkill
{
	public float _criticleDamagePercent;
}

[System.Serializable]
public class FCPassiveSkillMoveSpeed : FCPassiveSkill
{
	public float _moveSpeedPercent;
}

[System.Serializable]
public class FCPassiveSkillEnergyGain : FCPassiveSkill
{
	public float _energyGainValue;
}*/

[System.Serializable]
public class FCSkillMap
{
	public FCSkillConfig[] _skillConfigs;
}

[System.Serializable]
public class FCPassiveSkillMap
{
	public FCPassiveSkillConfig[] _skillConfigs;
}
