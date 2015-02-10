using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InJoy.RuntimeDataProtection;
using System;

public class HitParamsData
{
    public AIHitParams key;
    public float value;
}


[System.Serializable]
public class AcData
{
    #region from excel
    public string id;
    public string characterId;
    public string classIds; //type. warriors or mage or monster
    public string nameIds; //name string id
    public int level;

    public int Level
    {
        set { level = value; _levelEncrypted = value; }
        get { return _levelEncrypted; }
    }

    public FC_ELITE_TYPE eliteType;
    public FC_AC_FACTIOH_TYPE faction;
    public bool isPlayer;
    public float speed;
    public float angleSpeed;
    public int hp;
    public int HP
    {
        set { hp = value; _hitPointEncrypted = value; }
        get { return _hitPointEncrypted; }
    }
    public int energy;
    public int vitality;
    public int thresholdMax; //if damage > _thresholdMax , it will change to hurt state.

    public int bufferHp = 0;
    public float physicalAttack;
    public float physicalDefense;
    public float fireAttack;
    public float fireResist;
    public float poisonAttack;
    public float poisonResist;
    public float lightningAttack;
    public float lightningResist;
    public float iceAttack;
    public float iceResist;
    public float critRate;
    public float critDamage;

    private float critDamageResist;
    private float exp_add = 0;       //experience
    private float sc_add = 0;        //gold
    private float item_find = 0;     //Items for
    private float reduce_energy = 0; //reduce_energy
    private float hp_reply = 0;      // HPreply
    private float skillTriggerRate;  //tattoo add skills attack damage increase ratio
    private float skillAttackDamage; //tattoo add skill damage increased

    public List<EquipmentIdx> equipList = new List<EquipmentIdx>();

    private int _hitPointEncrypted;
    private int _levelEncrypted;
    private int _lootXpEncrypted;
    private int _lootScore1Encrypted;
    private int _lootScore2Encrypted;
    private float _lootBonus1Encrypted;
    private float _lootBonus2Encrypted;
    private float _lootBonus3Encrypted;

    public int lootXp;
    public int LootXp
    {
        set
        {
            lootXp = value;
            if (ShouldEncrype())
            {
                _lootXpEncrypted = value;
            }
        }
        get
        {
            if (ShouldEncrype())
            {
                return _lootXpEncrypted;
            }
            else
            {
                return lootXp;
            }
        }
    }

    public int lootVp;
    public List<LootObjData> lootItems = new List<LootObjData>();

    public int lootScore1;
    public int LootScore1
    {
        set
        {
            lootScore1 = value;
            if (ShouldEncrype())
            {
                _lootScore1Encrypted = value;
            }
        }
        get
        {
            if (ShouldEncrype())
            {
                return _lootScore1Encrypted;
            }
            else
            {
                return lootScore1;
            }
        }
    }

    public float lootRate1;
    public int lootScore2;
    public int LootScore2
    {
        set
        {
            lootScore2 = value;
            if (ShouldEncrype())
            {
                _lootScore2Encrypted = value;
            }
        }
        get
        {
            if (ShouldEncrype())
            {
                return _lootScore2Encrypted;
            }
            else
            {
                return lootScore2;
            }
        }
    }

    public float lootRate2;
    public float lootBonus1;
    public float LootBonus1
    {
        set
        {
            lootBonus1 = value;
            if (ShouldEncrype())
            {
                _lootBonus1Encrypted = value;
            }
        }
        get
        {
            if (ShouldEncrype())
            {
                return _lootBonus1Encrypted;
            }
            else
            {
                return lootBonus1;
            }
        }
    }

    public float lootBonus2;
    public float LootBonus2
    {
        set
        {
            lootBonus2 = value;
            if (ShouldEncrype())
            {
                _lootBonus2Encrypted = value;
            }
        }
        get
        {
            if (ShouldEncrype())
            {
                return _lootBonus2Encrypted;
            }
            else
            {
                return lootBonus2;
            }
        }
    }
    public float lootBonus3;
    public float LootBonus3
    {
        set
        {
            lootBonus3 = value;
            if (ShouldEncrype())
            {
                _lootBonus3Encrypted = value;
            }
        }
        get
        {
            if (ShouldEncrype())
            {
                return _lootBonus3Encrypted;
            }
            else
            {
                return lootBonus3;
            }
        }
    }
    #endregion

    #region level
    public string patrolPath = string.Empty;

    public List<HitParamsData> paramList = new List<HitParamsData>();

    public PLevelData pLevelData = new PLevelData();

    public void AddHitParamsData(AIHitParams key, float value)
    {
        if (null == key || key == AIHitParams.None)
        {
            return;
        }

        HitParamsData paramaData = GetHitParamsData(key);

        if (null == paramaData)
        {
            paramaData = new HitParamsData();
            paramaData.key = key;
            paramaData.value = value;
            paramList.Add(paramaData);
        }
        else
        {
            paramaData.value += value;
        }
    }

    public HitParamsData GetHitParamsData(AIHitParams key)
    {
        HitParamsData paramaData = paramList.Find(delegate(HitParamsData data) { return data.key == key; });

        return paramaData;
    }

    public float GetHitParamsValue(AIHitParams key)
    {
        HitParamsData paramaData = GetHitParamsData(key);

        return (null == paramaData) ? 0 : paramaData.value;
    }

    public void DealPlayerLevelData()
    {
        AddHitParamsData(AIHitParams.Hp, pLevelData._hp);
        AddHitParamsData(AIHitParams.Attack, pLevelData._attack);
        AddHitParamsData(AIHitParams.Defence, pLevelData._defense);
        AddHitParamsData(AIHitParams.CriticalDamage, pLevelData._crit_damage);
        AddHitParamsData(AIHitParams.CriticalChance, pLevelData._crit_rate);
    }

    public int TotalHp
    {
        get { return Mathf.RoundToInt(GetHitParamsValue(AIHitParams.Hp) * (1 + GetHitParamsValue(AIHitParams.HpPercent)) * 1000) / 1000; }
    }

    public int TotalDefense
    {
        get { return Mathf.RoundToInt(GetHitParamsValue(AIHitParams.Defence) * (1 + GetHitParamsValue(AIHitParams.DefencePercent)) * 1000) / 1000; }
    }

    public int TotalIceDefense
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.IceResist) + GetHitParamsValue(AIHitParams.AllElementResist)) * (1 + GetHitParamsValue(AIHitParams.IceResistPercent) + GetHitParamsValue(AIHitParams.AllElementResistPercent)) * 1000) / 1000; }
    }

    public int TotalFireDefense
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.FireResist) + GetHitParamsValue(AIHitParams.AllElementResist)) * (1 + GetHitParamsValue(AIHitParams.FireResistPercent) + GetHitParamsValue(AIHitParams.AllElementResistPercent)) * 1000) / 1000; }
    }

    public int TotalLightningDefense
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.LightningResist) + GetHitParamsValue(AIHitParams.AllElementResist)) * (1 + GetHitParamsValue(AIHitParams.LightningResistPercent) + GetHitParamsValue(AIHitParams.AllElementResistPercent)) * 1000) / 1000; }
    }

    public int TotalPoisonDefense
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.PoisonResist) + GetHitParamsValue(AIHitParams.AllElementResist)) * (1 + GetHitParamsValue(AIHitParams.PoisonResistPercent) + GetHitParamsValue(AIHitParams.AllElementResistPercent)) * 1000) / 1000; }
    }

    public int TotalAttack
    {
        get { return Mathf.RoundToInt(GetHitParamsValue(AIHitParams.Attack) * (1 + GetHitParamsValue(AIHitParams.AttackPercent)) * 1000) / 1000; }
    }

    public int BaseAttack
    {
        get { return Mathf.RoundToInt(GetHitParamsValue(AIHitParams.Attack) * (1 + GetHitParamsValue(AIHitParams.AttackPercent)) * 1000) / 1000; }
    }

    public int TotalIceAttack
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.IceDamage) + GetHitParamsValue(AIHitParams.AllElementDamage)) * (1 + GetHitParamsValue(AIHitParams.IceDamagePercent) + GetHitParamsValue(AIHitParams.AllElementDamagePercent)) * 1000) / 1000; }
    }

    public int TotalFireAttack
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.FireDamage) + GetHitParamsValue(AIHitParams.AllElementDamage)) * (1 + GetHitParamsValue(AIHitParams.FireDamagePercent) + GetHitParamsValue(AIHitParams.AllElementDamagePercent)) * 1000) / 1000; }
    }

    public int TotalLightningAttack
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.LightningDamage) + GetHitParamsValue(AIHitParams.AllElementDamage)) * (1 + GetHitParamsValue(AIHitParams.LightningDamagePercent) + GetHitParamsValue(AIHitParams.AllElementDamagePercent)) * 1000) / 1000; }
    }

    public int TotalPoisonAttack
    {
        get { return Mathf.RoundToInt((GetHitParamsValue(AIHitParams.PoisonDamage) + GetHitParamsValue(AIHitParams.AllElementDamage)) * (1 + GetHitParamsValue(AIHitParams.PoisonDamagePercent) + GetHitParamsValue(AIHitParams.AllElementDamagePercent)) * 1000) / 1000; }
    }


    public float TotalCritRate
    {
        get
        {
            return Mathf.Round((GetHitParamsValue(AIHitParams.CriticalChance) * (1 + GetHitParamsValue(AIHitParams.CriticalChancePercent))) * 1000) / 1000.0f;
        }
    }

    public float TotalCritDamage
    {
        get { return Mathf.Round((GetHitParamsValue(AIHitParams.CriticalDamage) * (1 + GetHitParamsValue(AIHitParams.CriticalDamagePercent))) * 1000) / 1000.0f; }
    }

    public float TotalCritDamageResist
    {
        get { return Mathf.Round((GetHitParamsValue(AIHitParams.CriticalDamageResist) * (1 + GetHitParamsValue(AIHitParams.CriticalDamageResistPercent))) * 1000) / 1000.0f; }
    }


    public float TotalExpAddition
    {
        get
        {
            return Mathf.Round(GetHitParamsValue(AIHitParams.ExpAddition) * 1000) / 1000.0f;
        }
    }

    public float TotalScAddition
    {
        get
        {
            return Mathf.Round(GetHitParamsValue(AIHitParams.ScAdditon) * 1000) / 1000.0f;
        }
    }

    public float TotalItemFind
    {
        get
        {
            return Mathf.Round(GetHitParamsValue(AIHitParams.ItemFind) * 1000) / 1000.0f;
        }
    }

    public float TotalReduceEnergy
    {
        get
        {
            return Mathf.Round(GetHitParamsValue(AIHitParams.ReduceEnergy) * 1000) / 1000.0f;
        }
    }

    public float TotalHPReply
    {
        get
        {
            return Mathf.Round(GetHitParamsValue(AIHitParams.HpRestore) * 1000) / 1000.0f;
        }
    }

    public float PassiveSkillGodDownCriticalToHp
    {
        get
        {
            return Mathf.Round(GetHitParamsValue(AIHitParams.PassiveSkillGodDownCriticalToHp) * 1000) / 1000.0f;
        }
    }


    public float TotalSkillTriggerRate
    {
        get { return 0.1f; }
    }

    public float TotalSkillAttackDamage
    {
        get { return Mathf.Round(GetHitParamsValue(AIHitParams.SkillDamageAddition) * 1000) / 1000.0f; }
    }

    public float TotalMoveSpeed
    {
        get { return Mathf.Round(GetHitParamsValue(AIHitParams.RunSpeed) * (1 + GetHitParamsValue(AIHitParams.RunSpeedPercent)) * 1000) / 1000.0f; }
    }

    public int TotalEnergy
    {
        get { return Mathf.RoundToInt(energy * 1000) / 1000; }
    }

    public void ClearHitParamsData()
    {
        if (null != paramList)
        {
            paramList.Clear();
        }
        paramList = new List<HitParamsData>();
    }

    public void InitBaseHitParamsData()
    {
        AddHitParamsData(AIHitParams.RunSpeed, speed);
        AddHitParamsData(AIHitParams.Hp, hp);
        AddHitParamsData(AIHitParams.Attack, physicalAttack);
        AddHitParamsData(AIHitParams.Defence, physicalDefense);
        AddHitParamsData(AIHitParams.FireDamage, fireAttack);
        AddHitParamsData(AIHitParams.FireResist, fireResist);
        AddHitParamsData(AIHitParams.PoisonDamage, poisonAttack);
        AddHitParamsData(AIHitParams.PoisonResist, poisonResist);
        AddHitParamsData(AIHitParams.LightningDamage, lightningAttack);
        AddHitParamsData(AIHitParams.LightningResist, lightningResist);
        AddHitParamsData(AIHitParams.IceDamage, iceAttack);
        AddHitParamsData(AIHitParams.IceResist, iceResist);
        AddHitParamsData(AIHitParams.CriticalChance, critRate);
        AddHitParamsData(AIHitParams.CriticalDamage, critDamage);

    }

    #endregion


    public AcData Copy()
    {
        AcData newData = Utils.Clone(this) as AcData;
        newData.HP = HP;
        newData.Level = Level;
        newData.LootXp = LootXp;
        newData.LootScore1 = LootScore1;
        newData.LootScore2 = LootScore2;
        newData.LootBonus1 = LootBonus1;
        newData.LootBonus2 = LootBonus2;
        newData.LootBonus3 = LootBonus3;

        return newData;
    }

    //Encrype will cost a lot of time. So that we only encrypt some of them
    private bool ShouldEncrype()
    {
        if (eliteType == FC_ELITE_TYPE.Boss)
            return true;
        else
            return false;
    }

    public void Encrypt()
    {
        HP = hp;
        Level = level;
        LootXp = lootXp;
        LootScore1 = lootScore1;
        LootScore2 = lootScore2;
        LootBonus1 = lootBonus1;
        LootBonus2 = lootBonus2;
        LootBonus3 = lootBonus3;
    }
}


public class AcDataList : ScriptableObject
{
    public List<AcData> _dataList = new List<AcData>();


    public AcData Find(string acId)
    {
        foreach (AcData ac in _dataList)
        {
            if (ac.id == acId)
            {
                return ac.Copy();
            }
        }

        Debug.LogError(string.Format("Error! AcData not found! id = \"{0}\"", acId));
        return null;
    }

}
