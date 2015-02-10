using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EotAgent : MonoBehaviour, FCAgent
{

    protected List<Eot> _eotList = new List<Eot>();
    protected float _timeCounter = 0;
    protected AIAgent _owner = null;

    protected int _eotFlag = 0;

    public void Init(FCObject eb)
    {
        _owner = eb as AIAgent;
        _eotList.Clear();
        _eotFlag = 0;
    }

    public int AddEot(Eot[] eots)
    {
        int dotDamageTotal = 0;

        if (eots != null)
        {
            foreach (Eot eot in eots)
            {
                if (eot != null && eot.eotType != Eot.EOT_TYPE.NONE)
                {
                    if (GameManager.Instance.IsPVPMode)
                    {
                        AddEot(eot.eotType, eot.lastTime, eot.eotPercent, eot.eotValue);
                    }
                    else
                    {
                        dotDamageTotal += AddEot(eot);
                    }
                }
            }
        }

        return dotDamageTotal;
    }

    public void AddEot(Eot.EOT_TYPE eet, float lastTime, float damageP, float damageV)
    {
        Eot tp = new Eot();
        tp.eotPercent = damageP;
        tp.eotValue = damageV;
        tp.lastTime = lastTime;
        tp.eotType = eet;
        tp.OwnerDamage = 0;
        AddEot(tp);

        if (_owner.ACOwner.IsPlayerSelf && GameManager.Instance.IsPVPMode)
        {
            CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ACTION_EOT, _owner.ACOwner.ObjectID,
                _owner.ACOwner.ThisTransform.localPosition,
                new Vector3((float)eet, 0, 0), FC_PARAM_TYPE.VECTOR3,
                new Vector3(lastTime, damageP, damageV), FC_PARAM_TYPE.VECTOR3,
                null, FC_PARAM_TYPE.NONE);
        }
    }

    private int AddEot(Eot eot)
    {
        bool ret = false;
        Eot tp = null;
        int dotDamage = 0;

        foreach (Eot tmp in _eotList)
        {
            if (!tmp.IsActive)
            {
                tp = tmp;
            }
            else if (tmp.eotType == eot.eotType)
            {
                if (Mathf.Abs(eot.eotPercent) >= Mathf.Abs(tmp.eotPercent)
                    || eot.lastTime >= tmp.JumpCount)
                {
                    Eot.Copy(tmp, eot);
                    ret = true;
                    dotDamage = RefreshEffect(eot, Eot.EOT_EVENT.START);
                    break;
                }
            }
        }
        if (!ret)
        {
            if (tp == null)
            {
                tp = new Eot();
                _eotList.Add(tp);
            }
            Eot.Copy(tp, eot);
            tp.IsActive = true;

            dotDamage = RefreshEffect(eot, Eot.EOT_EVENT.START);
        }
        return dotDamage;
    }


    public void UpdateEot()
    {
        bool ret = false;
        _timeCounter += Time.deltaTime;

        if (_timeCounter > FCConst.EOT_REFRESH_TIME)
        {
            _timeCounter -= FCConst.EOT_REFRESH_TIME;

            foreach (Eot tmp in _eotList)
            {
                if (tmp.IsActive)
                {
#if xingtianbo
                    switch (Eot.GetTimeType(tmp))
                    {
                        //will last for all combat
                        case Eot.EOT_TYPE.COMBAT:
                            break;
                        case Eot.EOT_TYPE.TEMP:
                            tmp.JumpCount--;
                            tmp._lastTime -= 1;
                            RefreshEffect(tmp, Eot.EOT_EVENT.REFRESH);
                            break;
                    }
                    if (tmp.JumpCount <= 0)
                    {
                        tmp.IsActive = false;
                        RefreshEffect(tmp, Eot.EOT_EVENT.END);
                    }
                    else
                    {
                        ret = true;
                    }
#endif
                    tmp.JumpCount--;
                    tmp.lastTime -= 1;
                    RefreshEffect(tmp, Eot.EOT_EVENT.REFRESH);


                    if (tmp.JumpCount <= 0)
                    {
                        tmp.IsActive = false;
                        RefreshEffect(tmp, Eot.EOT_EVENT.END);
                    }
                    else
                    {
                        ret = true;
                    }

                }
            }
        }
        else
        {
            ret = true;
        }
        if (!ret)
        {
            _eotList.Clear();
            _eotFlag = 0;
        }

    }


    private int RefreshEffect(Eot eot, Eot.EOT_EVENT et)
    {
        //debuffer only effect on start and end
        int dotValue = 0;

        Utils.SetFlag((int)eot.eotType, ref _eotFlag);
        PauseEffect(eot, false);

        if (et == Eot.EOT_EVENT.START)
        {
            switch (eot.eotType)
            {
                case Eot.EOT_TYPE.EOT_PHYSICAL_ATTACK:
                    {
                        _owner.ACOwner.EotPhysicalAttack = eot.eotPercent;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_PHYSICAL_DEFENSE:
                    {
                        _owner.ACOwner.EotPhysicalDefense = eot.eotPercent;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_ELEMENTAL_ATTACK:
                    {
                        _owner.ACOwner.EotElementalAttack = eot.eotPercent;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_ELEMENTAL_RESISTANCE:
                    {
                        _owner.ACOwner.EotElementalResistance = eot.eotPercent;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_SPEED:
                    {
                        _owner.ACOwner.BufferSpeedPercent = eot.eotPercent;
                        _owner.ACOwner.BufferAniMoveSpeedPercent = eot.eotPercent;
                        _owner.ACOwner.BufferAniPlaySpeedPercent = eot.eotPercent;

                        if (eot.eotPercent < 1)
                        {
                            _owner.ACOwner._avatarController.IceHurtColor(eot.lastTime);
                        }
                    }
                    break;
            }
        }
        else if (et == Eot.EOT_EVENT.END)
        {
            Utils.ClearFlag((int)eot.eotType, ref _eotFlag);
            PauseEffect(eot, true);

            switch (eot.eotType)
            {
                case Eot.EOT_TYPE.EOT_PHYSICAL_ATTACK:
                    {
                        _owner.ACOwner.EotPhysicalAttack = 1;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_PHYSICAL_DEFENSE:
                    {
                        _owner.ACOwner.EotPhysicalDefense = 1;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_ELEMENTAL_RESISTANCE:
                    {
                        _owner.ACOwner.EotElementalResistance = 1;
                    }
                    break;
                case Eot.EOT_TYPE.EOT_SPEED:
                    {
                        _owner.ACOwner.BufferSpeedPercent =1;
                        _owner.ACOwner.BufferAniMoveSpeedPercent =1;
                        _owner.ACOwner.BufferAniPlaySpeedPercent = 1;
                    }
                    break;
            }
        }
        else if (et == Eot.EOT_EVENT.REFRESH)
        {
#if xingtianbo
            Eot.EOT_TYPE bigType = (Eot.EOT_TYPE)(((int)eot._eotType / 1000) + 10000);
            Eot.EOT_TYPE midType = (Eot.EOT_TYPE)((((int)eot._eotType % 1000) / 100) + 10000);
            Eot.EOT_TYPE smallType = (Eot.EOT_TYPE)((((int)eot._eotType % 100) / 10) + 10000);
            int detailType = (int)eot._eotType % 10;
            if (bigType == Eot.EOT_TYPE.DOT)
            {
                int reduceHp = 0;
                if (detailType == 0)
                {
                    reduceHp = (int)(_owner.ACOwner.Data.TotalHp * eot._damagePercent + eot._damageValue);
                }
                else
                {
                    reduceHp = (int)(eot.OwnerDamage * eot._damagePercent + eot._damageValue);
                }
                if (_owner.ACOwner.HitPoint - reduceHp <= 0)
                {
                    reduceHp = _owner.ACOwner.HitPoint - 1;
                }

                if (_owner.ACOwner.IsClientPlayer)
                {
                    reduceHp = 0;
                }

                if (reduceHp > 0)
                {
                    if (CheatManager._showAttackInfo)
                    {
                        string logger = _owner.ACOwner.name;
                        logger += " dot effect do " + reduceHp + " damage";
                        Debug.Log(logger);
                    }
                    _owner.ACOwner.ACReduceHP(reduceHp, true, eot.IsFrom2p, false, false, true);
                }
            }
            else if (bigType == Eot.EOT_TYPE.BUFF)
            {
                if (midType == Eot.EOT_TYPE.BUFF_ATT)
                {
                    if (smallType == Eot.EOT_TYPE.BUFF_HP)
                    {
                        int increaseHp = 0;
                        if (detailType == 0)
                        {
                            increaseHp = (int)(_owner.ACOwner.Data.TotalHp * eot._damagePercent + eot._damageValue);
                        }
                        else
                        {
                            increaseHp = (int)(eot._damageValue);
                        }
                        if (increaseHp > 0)
                        {
                            _owner.ACOwner.ACIncreaseHP(increaseHp);
                            //CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_HP_ONCE, _owner.ACOwner._avatarController, -1);
                        }
                    }
                    else if (smallType == Eot.EOT_TYPE.BUFF_ENG)
                    {
                        int increaseEng = 0;
                        if (detailType == 0)
                        {
                            increaseEng = (int)(_owner.ACOwner.Data.TotalEnergy * eot._damagePercent + eot._damageValue);
                        }
                        else
                        {
                            increaseEng = (int)(eot._damageValue);
                        }
                        if (increaseEng > 0)
                        {
                            _owner.ACOwner.CostEnergy(increaseEng, 2);
                            //CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_HP_ONCE, _owner.ACOwner._avatarController, -1);
                        }
                    }
                }
            }
#endif

#if xingtianbo
            int detailType = (int)eot.eotType % 10;
#endif

            switch (eot.eotType)
            {
                case Eot.EOT_TYPE.EOT_FIRE:
                case Eot.EOT_TYPE.EOT_ICE:
                case Eot.EOT_TYPE.EOT_THUNDER:
                case Eot.EOT_TYPE.EOT_POISON:
                case Eot.EOT_TYPE.EOT_PHYSICAL:
                    {
                        int reduceHp = 0;
#if xingtianbo
                        //if (detailType == 0)
                        //{
                        //    reduceHp = (int)(_owner.ACOwner.Data.TotalHp * eot.damagePercent + eot.damageValue);
                        //}
                        //else
                        //{
                        //    reduceHp = (int)(eot.OwnerDamage * eot.damagePercent + eot.damageValue);
                        //}
#endif
                        reduceHp = (int)(eot.OwnerDamage * eot.eotPercent);

                        if (reduceHp <= 0)
                        {
                            reduceHp = 1;
                        }

                        if (_owner.ACOwner.HitPoint - reduceHp <= 0)
                        {
                            reduceHp = _owner.ACOwner.HitPoint - 1;
                        }

                        if (_owner.ACOwner.IsClientPlayer)
                        {
                            reduceHp = 0;
                        }

                        if (reduceHp > 0)
                        {
                            if (CheatManager.showAttackInfo)
                            {
                                string logger = _owner.ACOwner.name;
                                logger += " dot effect do " + reduceHp + " damage";
                                Debug.Log(logger);
                            }
                            _owner.ACOwner.ACReduceHP(reduceHp, true, eot.IsFrom2p, false, false, true);
                        }
                    }
                    break;
                case Eot.EOT_TYPE.EOT_HP:
                    {
                        int increaseHp = 0;
#if xingtianbo
                        if (detailType == 0)
                        {
                            increaseHp = (int)(_owner.ACOwner.Data.TotalHp * eot.eotValue + eot.damageValue);
                        }
                        else
                        {
                            increaseHp = (int)(eot.damageValue);
                        }
#endif
                        increaseHp = (int)(_owner.ACOwner.Data.TotalHp * eot.eotPercent * (1 + _owner.ACOwner.Data.TotalHPReply));

                        if (increaseHp > 0)
                        {
                            _owner.ACOwner.ACIncreaseHP(increaseHp);
                            //CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_HP_ONCE, _owner.ACOwner._avatarController, -1);
                        }
                    }
                    break;
                case Eot.EOT_TYPE.EOT_MANA:
                    {
                        int increaseEng = 0;
#if xingtianbo
                        if (detailType == 0)
                        {
                            increaseEng = (int)(_owner.ACOwner.Data.TotalEnergy * eot.eotValue + eot.damageValue);
                        }
                        else
                        {
                            increaseEng = (int)(eot.damageValue);
                        }
#endif
                        increaseEng = (int)(_owner.ACOwner.Data.TotalEnergy * eot.eotPercent);

                        if (increaseEng > 0)
                        {
                            _owner.ACOwner.CostEnergy(increaseEng, 2);
                            //CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_HP_ONCE, _owner.ACOwner._avatarController, -1);
                        }
                    }
                    break;

            }

        }

        return dotValue;
    }


    private bool HasEffect(Eot.EOT_TYPE eet)
    {
        return Utils.HasFlag((int)eet, _eotFlag);
    }


    //if ret = true ,means pause,otherside = resume
    public void PauseEffect(Eot.EOT_TYPE eet, bool ret)
    {
        if (HasEffect(eet))
        {
            foreach (Eot tmp in _eotList)
            {
                if (tmp.IsActive && tmp.eotType == eet)
                {
                    PauseEffect(tmp, ret);
                    break;
                }
            }
        }

    }

    //if ret = true ,means pause,otherside = resume
    private void PauseEffect(Eot eot, bool ret)
    {
        switch (eot.eotType)
        {
            case Eot.EOT_TYPE.EOT_SPEED:

                if (eot.eotPercent < 1)
                {
                    if (!ret)
                    {
                        //_owner.ACOwner.BufferSpeedPercent = eot.eotPercent;
                        //_owner.ACOwner.BufferAniMoveSpeedPercent = eot.eotPercent;
                        //_owner.ACOwner.BufferAniPlaySpeedPercent = eot.eotPercent;
                        CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DOT_ICE, _owner.ACOwner._avatarController, -1);

                        //_owner.ACOwner._avatarController.IceHurtColor(eot.lastTime);
                    }
                    else
                    {
                        //_owner.ACOwner.BufferSpeedPercent = 1f;
                        //_owner.ACOwner.BufferAniMoveSpeedPercent = 1f;
                        //_owner.ACOwner.BufferAniPlaySpeedPercent = 1f;

                        CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DOT_ICE, _owner.ACOwner._avatarController, -1);
                    }
                }


                break;

            case Eot.EOT_TYPE.EOT_PHYSICAL:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DOT_PHYSICAL, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DOT_PHYSICAL, _owner.ACOwner._avatarController, -1);
                }

                break;

            case Eot.EOT_TYPE.EOT_ICE:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DOT_ICE, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DOT_ICE, _owner.ACOwner._avatarController, -1);
                }

                break;

            case Eot.EOT_TYPE.EOT_FIRE:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DOT_FIRE, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DOT_FIRE, _owner.ACOwner._avatarController, -1);
                }

                break;



            case Eot.EOT_TYPE.EOT_THUNDER:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DOT_THUNDER, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DOT_THUNDER, _owner.ACOwner._avatarController, -1);
                }

                break;


            case Eot.EOT_TYPE.EOT_POISON:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DOT_POISON, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DOT_POISON, _owner.ACOwner._avatarController, -1);
                }

                break;
            case Eot.EOT_TYPE.EOT_HP:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_HP_LOOP, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.RESTORE_HP_LOOP, _owner.ACOwner._avatarController, -1);
                }
                break;
            case Eot.EOT_TYPE.EOT_MANA:
                if (!ret)
                {
                    CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_MP_LOOP, _owner.ACOwner._avatarController, -1);
                }
                else
                {
                    CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.RESTORE_MP_LOOP, _owner.ACOwner._avatarController, -1);
                }
                break;
        }


    }

    public void ClearEot(Eot.EOT_TYPE eotType)
    {
        //_eotList.Clear();
        foreach (Eot tmp in _eotList)
        {
            if (tmp.IsActive && tmp.eotType == eotType)
            {
#if xingtianbo
                switch (Eot.GetTimeType(tmp))
                {
                    //will last for all combat
                    case Eot.EOT_TYPE.COMBAT:
                        break;
                    case Eot.EOT_TYPE.TEMP:
                        tmp.IsActive = false;
                        RefreshEffect(tmp, Eot.EOT_EVENT.END);
                        break;
                }
#endif

                tmp.IsActive = false;
                RefreshEffect(tmp, Eot.EOT_EVENT.END);
            }
        }
    }

    public void ClearEot()
    {
        //_eotList.Clear();
        foreach (Eot tmp in _eotList)
        {
            if (tmp.IsActive)
            {
#if xingtianbo
                switch (Eot.GetTimeType(tmp))
                {
                    //will last for all combat
                    case Eot.EOT_TYPE.COMBAT:
                        break;
                    case Eot.EOT_TYPE.TEMP:
                        tmp.IsActive = false;
                        RefreshEffect(tmp, Eot.EOT_EVENT.END);
                        break;
                }
#endif

                tmp.IsActive = false;
                RefreshEffect(tmp, Eot.EOT_EVENT.END);
            }
        }
    }

}
