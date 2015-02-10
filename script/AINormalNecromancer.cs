using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum SummonPregress
{
    None = 0,
    Step1,
    Step2,
    Step3,
    Step4,
    Step5

}

public class AINormalNecromancer :AiNormalBoss
{
    public FC_CHARACTER_EFFECT invincibleEffect;
    public FC_CHARACTER_EFFECT invincibleEffectEgg;

    private bool _isFight = false;
    private bool _isSummonFinish1 = false;
    private bool _isSummonFinish2 = false;
    private bool _isSummonFinish3 = false;
    private bool _isSummonFinish4 = false;
    private bool _isSummonFinish5 = false;

    private SummonPregress currentSummon;

    private BornPoint point1;
    private BornPoint point2;
    private BornPoint point3;
    private BornPoint point4;
    private BornPoint point5;

    public override void ActiveAI()
    {
        base.ActiveAI();

        currentSummon = SummonPregress.None;
    }


    public override void BornTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            CameraController.Instance.AddCharacterForShadow(ACOwner._avatarController);
        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {

        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            if (_needHideWeaponWhenRun)
            {
                _owner.SwitchWeaponTo(EnumEquipSlot.weapon_hang, _defaultWeaponType);
            }
            _runOnPath = false;
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);

            // to find a nearest player as target
            _owner.ACBeginToSearch(true);


        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            if (GameManager.Instance.GameState == EnumGameState.InBattleCinematic)
            {
                SetNextState(STATE.DUMMY);
            }
            else if (GameManager.Instance.GameState == EnumGameState.InBattle)
            {
                if (!_isSummonFinish1)
                {
                    AddInvincible();
                    SummonMonster();
                }
            }
        }
    }

    private void AddInvincible()
    {
        _owner._avatarController.InvincibleColor(true);
        _owner.ACSetCheatFlag(FC_ACTION_SWITCH_FLAG.IN_GOD, true);
        CharacterEffectManager.Instance.PlayEffect(invincibleEffect, _owner._avatarController, -1);
        CharacterEffectManager.Instance.PlayEffect(invincibleEffectEgg, _owner._avatarController, -1);
    }

    private void Removeinvincible()
    {
        _owner._avatarController.InvincibleColor(false);
        _owner.ACSetCheatFlag(FC_ACTION_SWITCH_FLAG.IN_GOD, false);
        CharacterEffectManager.Instance.SetEffectConsiderForce(invincibleEffect, _owner._avatarController, false);
        CharacterEffectManager.Instance.StopEffect(invincibleEffect, _owner._avatarController, -1);
        CharacterEffectManager.Instance.SetEffectConsiderForce(invincibleEffectEgg, _owner._avatarController, false);
        CharacterEffectManager.Instance.StopEffect(invincibleEffectEgg, _owner._avatarController, -1);
    }

    public override void IdleTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            if (!_isFight)
            {
                _wanderTimeThisTime = -1;
                _timeForIdleThisTime = Random.Range(_timeForIdleMin, _timeForIdleMax);
                //_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
                _haverWanderActionThisTime = _haveWanderAction;
                AIAgent.STATE aas = AIAgent.STATE.IDLE;
                if (_state._preState != null)
                {
                    aas = _state._preState.CurrentStateID;
                }
                AIAgent.STATE ret = AIAgent.STATE.NONE;
                if (_state._preState.CurrentStateID == AIAgent.STATE.BORN)
                {
                    ret = AIAgent.STATE.ATTACK;
                }
                else if (_skillCount > 0 && aas == AIAgent.STATE.ATTACK && _afterSkill != null && _afterSkill.Count > 0)
                {
                    foreach (StateToGoCondition stgc in _afterSkill)
                    {
                        if (stgc.GetStateToGo(_skillCount, ref ret, ref _timeForIdleThisTime, ref _wanderTimeThisTime))
                        {
                            break;
                        }
                    }
                }
                if (ret == AIAgent.STATE.ATTACK)
                {
                    GoToAttack();
                }
                else if (ret == AIAgent.STATE.IDLE)
                {
                    _haverWanderActionThisTime = false;
                }
            }
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD, null);
            if (!_owner.IsPlayer)
            {
                _owner.ACStop();
            }
        }
        else if (cmd == FCCommand.CMD.STATE_FINISH)
        {
        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            if (_isFight)
            {
                if (_haverWanderActionThisTime)
                {
                    SetNextState(AIAgent.STATE.HESITATE);
                }
                else
                {
                    GoToAttack();
                }
            }
            else
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
        }
    }

    public override void RunTaskChange(FCCommand.CMD cmd)
    {
        base.RunTaskChange(cmd);
    }


    public override void SummonTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            AttackBase ab = _owner.ACGetAttackByName("SummonBiont");
            if (ab != null)
            {
                //single attackbase used for other state ,should not have attack conditions
                ab.AttCons = null;
            }
            _currentAttack = ab;
            ab.Init(this);
            ab.AttackEnter();
            AniEventAniIsOver = _currentAttack.AniIsOver;
            _timeForIdleThisTime = 2.0f;
        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {
            if (_currentAttack != null)
            {
                _currentAttack.AttackUpdate();
            }
        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            ActionSummonBiont();
            SetNextState(AIAgent.STATE.IDLE);
        }
    }

    public override void DummyTaskChange(FCCommand.CMD cmd)
    {
        base.DummyTaskChange(cmd);

        if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            if (!_isSummonFinish1)
            {
                AddInvincible();
                SummonMonster();
            }
        }
    }
    

    protected override void UpdateLevelUpConditions()
    {
        base.UpdateLevelUpConditions();

        CheakSummon();
    }

    private void CheakSummon()
    {
        switch (currentSummon)
        {
            case SummonPregress.Step1:
                {
                    if (null != point1 && point1.IsAllDead && !_isSummonFinish2)
                    {
                        SummonMonster();
                    }
                }
                break;
            case SummonPregress.Step2:
                {
                    if (null != point2 && point2.IsAllDead)
                    {
                        _isFight = true;

                        Removeinvincible();
                        
                        if (_owner.HitPointPercents <= 0.7f && !_isSummonFinish3)
                        {
                            SummonMonster();
                        }
                    }
                }
                break;
            case SummonPregress.Step3:
                {
                    if (null != point3 && point3.IsAllDead && !_isSummonFinish4)
                    {
                        if (_owner.HitPointPercents <= 0.5f)
                        {
                            SummonMonster();
                        }
                    }
                }
                break;
            case SummonPregress.Step4:
                {
                    if (null != point4 && point4.IsAllDead && !_isSummonFinish5)
                    {
                        if (_owner.HitPointPercents <= 0.3f)
                        {
                            SummonMonster();
                        }
                    }
                }
                break;
        }

    }


    private void ActionSummonBiont()
    {
        switch (currentSummon)
        {
            case SummonPregress.Step1:
                {
                    point1 = TriggerManager.Singleton.GetAITriggerByName("trigger_necromancer1");

                    if (null != point1)
                    {
                        point1.Active();
                    }
                }
                break;
            case SummonPregress.Step2:
                {
                    point2 = TriggerManager.Singleton.GetAITriggerByName("trigger_necromancer2");

                    if (null != point2)
                    {
                        point2.Active();
                    }

                }
                break;
            case SummonPregress.Step3:
                {
                    point3 = TriggerManager.Singleton.GetAITriggerByName("trigger_necromancer3");

                    if (null != point3)
                    {
                        point3.Active();
                    }
                }
                break;
            case SummonPregress.Step4:
                {
                    point4 = TriggerManager.Singleton.GetAITriggerByName("trigger_necromancer4");

                    if (null != point4)
                    {
                        point4.Active();
                    }
                }
                break;
            case SummonPregress.Step5:
                {
                    point5 = TriggerManager.Singleton.GetAITriggerByName("trigger_necromancer5");

                    if (null != point5)
                    {
                        point5.Active();
                    }
                }
                break;
        }
    }

    private void SummonMonster()
    {
        switch (currentSummon)
        {
            case SummonPregress.None:
                {
                    SetNextState(AIAgent.STATE.SUMMON, true);
                    _isSummonFinish1 = true;
                    currentSummon = SummonPregress.Step1;
                }
                break;
            case SummonPregress.Step1:
                {
                    SetNextState(AIAgent.STATE.SUMMON, true);
                    _isSummonFinish2 = true;
                    currentSummon = SummonPregress.Step2;
                }
                break;
            case SummonPregress.Step2:
                {
                    SetNextState(AIAgent.STATE.SUMMON, true);
                    _isSummonFinish3 = true;
                    currentSummon = SummonPregress.Step3;
                }
                break;
            case SummonPregress.Step3:
                {
                    SetNextState(AIAgent.STATE.SUMMON, true);
                    _isSummonFinish4 = true;
                    currentSummon = SummonPregress.Step4;
                }
                break;
            case SummonPregress.Step4:
                {
                    SetNextState(AIAgent.STATE.SUMMON, true);
                    _isSummonFinish5 = true;
                    currentSummon = SummonPregress.Step5;
                }
                break;
        }

    }


}
