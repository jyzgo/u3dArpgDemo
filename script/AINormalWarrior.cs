using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/AI/AINormalWarrior")]
public class AINormalWarrior : AIAgent
{

    //if joy stick is press,we need cache the key info

    protected int _skillCount = 0;
    protected int _hurtCount = 0;

    public string _skillForCounter = "";

    public string[] _skillsForCounter;
    public string[] _skillsForCounterFar;
    public float _distanceForCounterFar = 6;

    public string _skillForAwake = "";
    public string[] _skillsForAwake;
    //percent of hp total
    public float _damageValueToAction = 0;

    protected int _damageAbsorbCurrent = 0;

    public override void ActiveAI()
    {
        base.ActiveAI();
        _damageAbsorbCurrent = 0;
        SetNextState(STATE.BORN);
        if (_isInTown && _owner.ThisObject.name.Contains("warrior"))
        {
            _owner.SwitchWeaponTo(EnumEquipSlot.weapon_hang, _defaultWeaponType);
        }
        if (_owner.IsPlayer)
        {
#if PVP_ENABLED
			bool needShowInPvP = GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.PVP
				&& _owner.IsClientPlayer;
			_owner._avatarController._uiHPController.SetHPBarVisible(needShowInPvP);
			_owner._avatarController._uiHPController.SetEnergyBarVisible(needShowInPvP);
			HaveHpBar = needShowInPvP;
			HaveEnergyBar = needShowInPvP;
#else
            _owner._avatarController._uiHPController.SetHPBarVisible(false);
            _owner._avatarController._uiHPController.SetEnergyBarVisible(false);
#endif
        }
    }

    public override bool HandleInnerCmd(FCCommand.CMD cmd, object param0)
    {
        return HandleInnerCmd(cmd, param0, null, null, null);
    }

    public override bool HandleInnerCmd(FCCommand.CMD cmd, object param0, object param1, object param2, object param3)
    {
        //from client, the network agent will handle this
        if (cmd > FCCommand.CMD.CLIENT)
            return false;


        bool ret = false;
        if (_sAHandleCmd != null)
        {
            ret = _sAHandleCmd(cmd, param0, param1, param2, param3);
        }
        if (!ret)
        {
            switch (cmd)
            {

                //target
                case FCCommand.CMD.TARGET_FINDED:
                    {
                        TargetFinded((ActionController)param1);

                        ret = true;
                        break;
                    }
                case FCCommand.CMD.TARGET_IN_ATTACK_DISTANCE:
                    {
                        _isInAttackDistance = true;
                        ret = true;
                        break;
                    }

                case FCCommand.CMD.TARGET_CHANGE:
                    {
                        _targetAC = param1 as ActionController;
                        ret = HandleInnerCmd(FCCommand.CMD.TARGET_FINDED, null, _targetAC, null, null);
                        break;
                    }

                //move
                case FCCommand.CMD.MOVE_TO_POINT:
                    {
                        Vector3 p = (Vector3)param1;
                        _owner.ACMove(ref p);
                        ret = true;
                        break;
                    }

                //stop

                //Set
                case FCCommand.CMD.SET_TO_DEFAULT_SPEED:
                    {
                        _owner.ACRestoreToDefaultSpeed();
                        ret = true;
                        break;
                    }
                case FCCommand.CMD.SET_HITPOINT_CHANGEBY:
                    {
                        ACOwner.ACReduceHP((int)param1, false, false, false, false);
                        ret = true;
                        break;
                    }

                //Init
                case FCCommand.CMD.INIT_ATTACK_INFO:
                    _attackCountAgent = _behaviourAgents.GetComponent<FCAttackCountAgent>();
                    _attackCountAgent.Init(this);
                    ret = true;
                    break;

                //Attack

                case FCCommand.CMD.ATTACK_WITH_SPEC_CONTS:
                    {
                        int packed = (int)param1;

                        _netSkillInfo._ailevel = packed & 0x000000ff;
                        _netSkillInfo._skillID = (packed & 0x0000ff00) >> 8;
                        _netSkillInfo._comboID = (packed & 0x00ff0000) >> 16;
                        //				int targetID = (int)param2;
                        _owner.ACStop();
                        Vector3 v3 = Vector3.zero;
                        float y = (float)param3;
                        //means for melee len = 10
                        /*float lenSqrt = 100;
                        if(_aiType == FC_AI_TYPE.PLAYER_MAGE)
                        {
                            // for ranger ,len = 20
                            lenSqrt = 400;
                        }
                        if(targetID != -1)
                        {
                            ac = ObjectManager.Instance.GetObjectByNetworkID(targetID).ewObj as ActionController;
                            v3 = ac.ThisTransform.position - _owner.ThisTransform.position;
                            v3.y = 0;
                            //if target is out range ,we need choose new target
                            if(v3.sqrMagnitude >= lenSqrt)
                            {
                                ac = ActionControllerManager.Instance.GetEnemyTargetBySight(_owner.ThisTransform,
                                    10,0,FC_AC_FACTIOH_TYPE.NEUTRAL_1,360,true);
                            }
                        }
                        //hit target or face to the same direction with 1p in other handset
                        if(ac != null && ac.IsAlived)
                        {
                            v3.Normalize();
                        }
                        else
                        {
                            Quaternion rotation = Quaternion.Euler(new Vector3(0, y, 0));
                            v3 = rotation * Vector3.forward;
                        }*/
                        Quaternion rotation = Quaternion.Euler(new Vector3(0, y, 0));
                        v3 = rotation * Vector3.forward;
                        _owner.ACRotateTo(v3, -1, true, true);
                        if (_owner.IsAlived)
                        {
                            SetNextState(AIAgent.STATE.ATTACK, true);
                        }
                        ret = true;
                        break;
                    }

                //direction
                case FCCommand.CMD.DIRECTION_FOLLOW_FORWARD:
                    {
                        _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
                        ret = true;
                        break;
                    }
                case FCCommand.CMD.DIRECTION_UNLOCK:
                    {
                        _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
                        ret = true;
                        break;
                    }
                case FCCommand.CMD.DIRECTION_FACE_TARGET:
                    {
                        if (param1 != null)
                        {
                            Vector3 dir = ((ActionController)param1).ThisTransform.localPosition - _owner.ThisTransform.localPosition;
                            dir.y = 0;
                            dir.Normalize();
                            if (dir != Vector3.zero)
                            {
                                _owner.ACRotateTo(dir, -1, true);
                                ret = true;
                            }
                        }
                        break;
                    }

                //die
                case FCCommand.CMD.DIE_NORMAL:
                    GotoDead();
                    ret = true;
                    break;
                //revive
                case FCCommand.CMD.REVIVE:
                    GoToRevive();
                    ret = true;
                    break;

                case FCCommand.CMD.ACTION_DISMISS_TICKET:
                    _haveActionTicket = false;
                    _monsterTicket = null;
                    break;
                case FCCommand.CMD.ACTION_GAIN_TICKET:
                    _haveActionTicket = true;
                    if (param1 != null)
                    {
                        _monsterTicket = (FCTicket)param1;
                    }
                    break;
                case FCCommand.CMD.ACTION_SHOULD_GOTO_ATTACK:
                    ShouldGotoAttack();
                    break;
            }


        }
        return ret;

    }

    public virtual void ShouldGotoAttack()
    {
    }

    #region logic
    public override bool HandleCommand(ref FCCommand ewd)
    {
        int rootcmd = ((int)ewd._cmd / 100) * 100;
        int subcmd = ((int)ewd._cmd);
        bool canrun = true;
        bool ret = false;

        if (rootcmd == (int)FCCommand.CMD.NET)
        {
            _netCommand = (FCCommand)ewd._param1;
            canrun = false;
        }
        if (canrun)
        {
            if (rootcmd < 0
                ||
                (
                    (rootcmd == (int)FCCommand.CMD.MOVE
                    && HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE))
                    ||
                    (rootcmd == (int)FCCommand.CMD.ROTATE
                    && HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE))
                    ||
                    (!_owner.IsAlived && (rootcmd == (int)FCCommand.CMD.MOVE || rootcmd == (int)FCCommand.CMD.ATTACK))
                )
            )
            {
                canrun = false;
            }
            if (ewd._isHost != _inHostControl)
            {
                if (!_inHostControl)
                {
                    if (rootcmd == (int)FCCommand.CMD.MOVE && _state.CurrentStateID == AIAgent.STATE.RUN)
                    {
                        canrun = false;
                    }
                }
            }
        }

        if (canrun)
        {
            switch ((FCCommand.CMD)rootcmd)
            {
                case FCCommand.CMD.INPUT:
                    {
                        if (ewd._cmd == FCCommand.CMD.INPUT_KEY_PRESS)
                        {
                            int pressKey = (int)ewd._param1;
                            Vector3 dir = (Vector3)ewd._param2;
                            HandleKeyPress(pressKey, dir);
                        }
                        else
                        {
                            int releaseKey = (int)ewd._param1;
                            HandleKeyRelease(releaseKey);
                        }
                        subcmd = -1;
                        ret = true;
                        break;
                    }
                case FCCommand.CMD.MOVE:
                    if (_state != null
                        && _state.CurrentStateID == AIAgent.STATE.IDLE)
                    {
                        ret = SetNextState(AIAgent.STATE.RUN);
                    }
                    else
                    {
                        //if
                    }
                    break;
                case FCCommand.CMD.ACTION:
                    if (_owner.IsAlived && (ewd._cmd == FCCommand.CMD.ACTION_CANCEL))
                    {
                        SetNextState(AIAgent.STATE.IDLE, true);
                        subcmd = -1;
                    }

                    break;
                //hurt
                case FCCommand.CMD.HURT:
                    {
                        bool canHit = true;
                        int hitID = -1;
                        if (_owner._onHit != null)
                        {
                            _owner._onHit((int)LevelManager.VarNeedCount.HIT_PLAYER);
                        }
                        if (ewd._param3 != null)
                        {
                            hitID = (int)ewd._param3;
                            if (hitID == -1)
                            {
                                canHit = false;
                            }
                            else if (hitID != 0)
                            {
                                for (int i = 0; i < _hitIDs.Length; i++)
                                {
                                    if (_hitIDs[i] == hitID)
                                    {
                                        Debug.LogError("The error is in the same Hit ID");
                                        canHit = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (canHit)
                        {
                            if (_owner._onHit != null)
                            {
                                _owner._onHit((int)LevelManager.VarNeedCount.ATTACK_TOTAL);
                            }
                            ret = CountHurtResult((AttackUnit)ewd._param1);

                            if (ret && hitID > 0)
                            {
                                _hitIDs[_hitIDsIdx] = hitID;
                                _hitIDsIdx++;
                                if (_hitIDsIdx >= _hitIDs.Length)
                                {
                                    _hitIDsIdx = 0;
                                }
                            }
                        }
                        else
                        {
                            if (_owner._onHit != null)
                            {
                                _owner._onHit((int)LevelManager.VarNeedCount.NOT_HIT_PLAYER);
                            }
                        }
                        subcmd = -1;
                        break;
                    }

                case FCCommand.CMD.ROTATE:
                    {
                        _owner.ACRotateTo((Vector3)ewd._param1, -1, true);
                        subcmd = -1;
                        ret = true;
                        break;
                    }
                case FCCommand.CMD.STOP:
                    {
                        if (ewd._cmd == FCCommand.CMD.STOP_IS_ARRIVE_POINT)
                        {
                            if (EventIsStopAtPoint != null && EventIsStopAtPoint())
                            {
                                subcmd = -1;
                                ret = true;
                            }
                            else
                            {
                                ACStopAtPoint();
                            }
                        }
                        break;
                    }
            }
            if (subcmd >= 0)
            {
                ret = HandleInnerCmd((FCCommand.CMD)subcmd, null, ewd._param1, ewd._param2, ewd._param3);
            }
        }
        if (ret)
        {
            _inHostControl = ewd._isHost;
        }
        return ret;
    }

    protected float _addDpsCountTime = 0;
    public void Update()
    {
        if (!GameManager.Instance.GamePaused)
        {
            if (_owner.IsPlayerSelf)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (CheatManager.dpsCountEnabled)
                {
                    if (_state.CurrentStateID == AIAgent.STATE.ATTACK)
                    {
                        CheatManager.hitTime += Time.deltaTime;
                    }
                    else
                    {
                        if (_addDpsCountTime > 0)
                        {
                            _addDpsCountTime -= Time.deltaTime;
                            CheatManager.hitTime += Time.deltaTime;
                        }
                    }
                }
#endif
            }
            else
            {
                UpdateNetCommand();
            }

            if (_godTime > 0)
            {
                _godTime -= Time.deltaTime;
                if (_godTime <= 0)
                {
                    ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
                }
            }

            if (_owner.IsAlived && _eotAgent != null)
            {
                _eotAgent.UpdateEot();
            }

            if (!_owner.IsAlived && _state.CurrentStateID != AIAgent.STATE.HURT)
            {
                GotoDead();
            }
            if (_netCommand != null)
            {
                int rootcmd = ((int)_netCommand._cmd / 100) * 100;
                if (rootcmd == (int)FCCommand.CMD.ATTACK && (_state.CurrentStateID == STATE.RUN || _state.CurrentStateID == STATE.IDLE))
                {
                    if (HandleCommand(ref _netCommand))
                    {
                        CommandManager.Instance.SetNextNetCommand((int)_owner.ObjectID);
                    }
                }
            }
            UpdateComboTime();
            _owner.IsCriticalHit = false;

            if (_attackCountAgent != null)
            {
                _attackCountAgent.UpdateSkill();
            }
            if (_keyAgent != null)
            {
                _keyAgent.UpdateKeyAndSkillCDState();
            }
            UpdateLevelUpConditions();
        }

    }

    public override void BornTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
            CameraController.Instance.AddCharacterForShadow(ACOwner._avatarController);

        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {

        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            if (_needHideWeaponWhenRun)
            {
                _owner.SwitchWeaponTo(EnumEquipSlot.weapon_hang, _defaultWeaponType);
            }
            _runOnPath = false;
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            if (!_owner.IsPlayer)
            {
                // to find a nearest player as target
                _owner.ACBeginToSearch(true);
            }
            else
            {
                if (_owner.IsClientPlayer && GameManager.Instance.IsMultiplayMode)
                {
                    SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_NAMEKIAN);
                }

                //SetNextState(AIAgent.STATE.IDLE);
            }

        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            if (GameManager.Instance.GameState == EnumGameState.InBattleCinematic)
            {
                SetNextState(STATE.DUMMY);
            }
            else if (GameManager.Instance.GameState == EnumGameState.InBattle)
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
        }
    }

    public override void IdleTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            //_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
            _timeForIdleThisTime = Random.Range(_timeForIdleMin, _timeForIdleMax);
            if (_owner.IsPlayer)
            {
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
                _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
            }
            if (_owner.IsPlayer && GameManager.Instance.IsPVPMode)
            {
                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
            }
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD, null);
            //if(!_owner.IsPlayer)
            {
                _owner.ACStop();
            }
        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {
            _owner.ACStop();
        }
        else if (cmd == FCCommand.CMD.STATE_FINISH)
        {
        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            if (!_owner.IsPlayer && TestTicketAvailable(false))
            {
                GoToAttack();
            }
        }
    }

    public virtual void ClearCollider()
    {
        _owner.ACSetColliderAsTrigger(true);
        _owner.ACSetRaduis(0);
    }

    public override void DeadTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            if (_owner.IsPlayer && _owner.IsPlayerSelf && GameManager.Instance.IsPVPMode)
            {
                MultiplayerDataManager.Instance.MsgMyselfDead();
            }
            QuestManager.instance.UpdateQuests(QuestTargetType.kill_monster, _owner.Data.id, 1);
            _owner.ACEffectByGravity(0, null, 0, true, false);
            // test nav mesh agent error log.
            //_owner.SelfMoveAgent._navAgent.enabled = false;
            _owner.ACStop();
            ClearCollider();
            //when endless level, dont disable Avoidance
            //fix for:  V110
            if (!_owner.IsPlayer)
            {
                _owner.SelfMoveAgent._navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            }
            //_owner.ACSetAvoidPriority(0);
            if (_owner.IsPlayer)
            {
                ActionControllerManager.Instance.PlayerIsDead(_owner);
            }
            else
            {

            }

            if(_owner.IsPlayerSelf)
            {
                if (null != _owner._onACDead)
                {
                    _owner._onACDead(_owner);
                }
            }

            _needPlayDeadAnimation = true;
            if (_hurtAgent._canBeHitFly)
            {
                if (_hurtAgent.CurrentHitType == AttackHitType.KnockDown || _hurtAgent.CurrentHitType == AttackHitType.HitFly)
                {
                    _needPlayDeadAnimation = false;
                }
                else if (_hurtAgent.NextHitType == AttackHitType.KnockDown || _hurtAgent.NextHitType == AttackHitType.HitFly)
                {
                    _deadWithFly = true;
                }
                //if (_hurtAgent.CurrentHitType == AttackHitType.KnockDown)
                //{
                //    _needPlayDeadAnimation = false;
                //}
                //else if (_hurtAgent.NextHitType == AttackHitType.KnockDown)
                //{
                //    _deadWithFly = true;
                //}
            }
            else
            {
                _deadWithFly = false;
                _needPlayDeadAnimation = true;
            }
            if (_needPlayDeadAnimation
                && _state.PreStateID != AIAgent.STATE.HURT)
            {
                FaceToTarget(_targetAC, true);
            }
            _eotAgent.PauseEffect(Eot.EOT_TYPE.EOT_SPEED, true);
            _eotAgent.ClearEot();
            if (!_deadWithFly && _needPlayDeadAnimation)
            {

                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);

                HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD, null);
            }
            else
            {
                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            }

            if (!_owner.IsPlayer)
            {
                //_owner.ACActiveNavmeshPath(false);
            }
            else
            {
                _owner.ACSetAvoidPriority(FCConst.HILL_WEIGHT);
            }
            if (!_owner.IsPlayer && UIManager.Instance._indicatorManager != null)
            {
                UIManager.Instance._indicatorManager.DeactiveEnemyIndicator(_owner);
            }
            //if I am flying, play a explode effect and go to dead
            if (_deadWithExplode)
            {
                //play blood explde effect
                Vector3 bloodPos = _owner.ACGetTransformByName(EnumEquipSlot.belt).position;
                GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.BLOOD_EXPLODE, bloodPos);
            }
            if (DeferredShadingRenderer.Singleton != null)
            {
                DeferredShadingRenderer.Singleton.StartPhasEffect();
            }
            //HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD,null);
        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            if (_owner.IsPlayer && _owner.IsPlayerSelf)
            {
                //SetNextState(AIAgent.STATE.REVIVE);
                if (GameManager.Instance.CurrGameMode == GameManager.FC_GAME_MODE.SINGLE
                   && !BattleSummary.Instance.IsFinish)
                {
                    UIManager.Instance.CloseUI("HomeUI");
                    UIManager.Instance.OpenUI("BattleReviveUI");
                }
                else if (GameManager.Instance.IsPVPMode
                    && !PvPBattleSummary.Instance.IsFinish)
                {
                    UIManager.Instance.CloseUI("HomeUI");
                }
            }
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            if (_lockByCamera)
            {
                CameraController.Instance.DisableRotate();
            }
            _owner.ACStop();


            /*if(EndlessLevel._isEndlessLevel)
            {
                if(_owner.IsPlayer)
                {
				
                }
                else
                {
                    _owner.SelfMoveAgent._navAgent.enabled = false;
                }
            }*/


            if (!_owner.IsPlayer)
            {
                _owner.ACDead();
            }
        }
    }

    public override void StandTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            _hurtAgent.ClearHurtState();
            _currentAttack = null;
            AttackBase ab = _owner.ACGetAttackByName("Stand");
            if (ab != null)
            {
                //single attackbase used for other state ,should not have attack conditions
                ab.AttCons = null;
                //Debug.Log("StandTaskChange");
                _currentAttack = ab;
                ab.Init(this);
                ab.AttackEnter();
                _owner.CurrentWeaponHitType = ab._hitType;
                AniEventAniIsOver = _currentAttack.AniIsOver;
                _eotAgent.PauseEffect(Eot.EOT_TYPE.EOT_SPEED, true);
            }
            else
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
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
            //_currentAttack = null;
            if (_currentAttack != null)
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            if (_currentAttack != null)
            {
                _currentAttack.AttackQuit();
                _currentAttack = null;
                AniEventAniIsOver = AniIsOver;
                _eotAgent.PauseEffect(Eot.EOT_TYPE.EOT_SPEED, false);
            }

        }
    }

    public override void ReviveTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            AttackBase ab = _owner.ACGetAttackByName("Revive");
            if (ab != null)
            {
                //single attackbase used for other state ,should not have attack conditions
                ab.AttCons = null;
            }
            _currentAttack = ab;
            ab.Init(this);
            ab.AttackEnter();
            AniEventAniIsOver = _currentAttack.AniIsOver;
            if (_owner.IsPlayerSelf)
            {
                _keyAgent.ClearKeyPress(-1);
            }

        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            _hurtAgent.ClearHurtState();
            _owner.ACRecoverAll();

            if (_keyAgent.ActiveKey > FC_KEY_BIND.DIRECTION && _owner.IsPlayer)
            {
                if (!TryToAttack())
                {
                    SetNextState(AIAgent.STATE.IDLE);
                }
            }
            else
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {
            if (_currentAttack != null)
            {
                _currentAttack.AttackUpdate();
            }
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            _prepareToDeadState = false;
            _currentAttack.AttackQuit();
            _currentAttack = null;
            AniEventAniIsOver = AniIsOver;

            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);

            if (_owner.IsPlayer)
            {
                _owner.ACSetAvoidPriority(_bodyMass);
            }
            if (_owner.IsPlayer)
            {
                ActionControllerManager.Instance.PlayerIsRevive(_owner);
            }
        }
    }

    //in run state ,I will set CurrentSubStateID before enter.
    public override void RunTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            if (_state._nextState.CurrentStateID != AIAgent.STATE.RUN)
            {
                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            }
        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            if (_state._preState.CurrentStateID != AIAgent.STATE.RUN)
            {
                _runStateTime = 0;
            }
            if (_needHideWeaponWhenRun)
            {
                _owner.SwitchWeaponTo(EnumEquipSlot.weapon_hang, _defaultWeaponType);
            }
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
            if (_state._curState.CurrentSubStateID == (int)FCStateRun_meleeNormal.STATE.PATROL)
            {
                GotoNextPathPoint();
            }
        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {
            if (!_owner.ACIsMove && _owner.IsClientPlayer && _runStateTime > 0.1f)
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
            if (_owner.IsPlayerSelf
                && _keyAgent.ActiveKey == FC_KEY_BIND.NONE
                && _state._nextState == null)
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
            _runStateTime += Time.deltaTime;
            if (_owner._onRunUpdate != null)
            {
                _owner._onRunUpdate(_runStateTime);
            }
        }
        else if (cmd == FCCommand.CMD.STATE_FINISH)
        {
            STATE state = AIAgent.STATE.MAX;
            if (_brainAgent != null)
            {
                state = _brainAgent.GetNextStateByRun(true,
                    _state._curState.CurrentSubStateID == (int)FCStateRun_meleeNormal.STATE.SEEK);
                SetNextState(state);
            }
            else
            {
                Debug.LogError("Ai should have brain for seek");
            }
        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            STATE state = AIAgent.STATE.MAX;
            if (_brainAgent != null)
            {
                state = _brainAgent.GetNextStateByRun(_isInAttackDistance,
                    _state._curState.CurrentSubStateID == (int)FCStateRun_meleeNormal.STATE.SEEK);
                SetNextState(state);
            }
            else
            {
                Debug.LogError("Ai should have brain for STATE_DONE");
            }
        }
    }


    public override void DummyTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            _timeForIdleThisTime = Random.Range(_timeForIdleMin, _timeForIdleMax);

            if (_owner.IsPlayer)
            {
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
                _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
            }

            if (_owner.IsPlayer && GameManager.Instance.IsPVPMode)
            {
                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
            }

            _owner.SelfMoveAgent.Stop(true);
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD, null);

            _owner.ACStop();

        }
        else if (cmd == FCCommand.CMD.STATE_UPDATE)
        {
            _owner.ACStop();
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            SetNextState(AIAgent.STATE.IDLE);
        }
    }

    protected IEnumerator AttackDelayCounter(float timeC, AttackBase ab)
    {
        _isInAttackEndDelay = true;
        //		float timeCount = timeC;
        bool inState = _isInAttackEndDelay;
        if (_owner.SelfAniSpeedAgent != null)
        {
            _owner.SelfAniSpeedAgent.SlowDownAnimation(ab._aniReduceSpeed, ab._aniReduceSpeedFinal, ab._attackEndDelayTime + 0.01f);
        }
        while (inState)
        {
            timeC -= Time.deltaTime;
            if (timeC <= 0)
            {
                inState = false;
            }
            yield return null;
        }

        if (_isInAttackEndDelay)
        {
            EndCurrentAttack();
        }
    }

    protected void EndCurrentAttack()
    {
        _isInAttackEndDelay = false;
        AttackBase ab = _currentAttack;
        Assertion.Check(ab != null);
        if (!_owner.IsPlayer && ab.IsFinalAttack)
        {
            if (CurrentSkill != null && CurrentSkill.WithDefy)
            {
                SetNextState(AIAgent.STATE.DEFY);
            }
            else
            {
                SetNextState(AIAgent.STATE.IDLE);
            }
        }
        else if
        (
            !_owner.IsClientPlayer
            &&
            (
                (ab != null && ab.ShouldGotoNextHit && ab.AttackCanSwitch
                    && _owner.ACIncreaseAttackCount(_attackCountAgent.CurrentSkillID, _aiLevel, ab) >= 0)
                || (_attackCountAgent.CurrentSkillID != _attackCountAgent.NextSkillID
                    && _attackCountAgent.NextSkillID != -1)
            )
        )
        {
            SetNextState(AIAgent.STATE.ATTACK);
        }
        else if (_owner.IsPlayerSelf
            && _keyAgent.GetNextAttackID() != -1
            && _keyAgent.GetNextAttackID() != _attackCountAgent.CurrentSkillID)
        {
            _attackCountAgent.NextSkillID = _keyAgent.GetNextAttackID();
            SetNextState(AIAgent.STATE.ATTACK);
        }
        else if (_keyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
        {
            _owner.ACRevertToDefalutMoveParams();
            SetNextState(AIAgent.STATE.RUN);
            MoveByDirection(_keyAgent._directionWanted, _owner.Data.TotalMoveSpeed, 9999f);
        }
        else
        {
            SetNextState(AIAgent.STATE.IDLE);
        }
    }
    public override void AttackTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_FINISH)
        {
            AttackBase ab = _currentAttack;
            Assertion.Check(ab != null);
            if (ab._attackEndDelay && ab.AttackCanSwitch && ab.ShouldGotoNextHit)
            {
                if (!_isInAttackEndDelay)
                {
                    StartCoroutine(AttackDelayCounter(ab._attackEndDelayTime, ab));
                }
            }
            else
            {
                EndCurrentAttack();
            }
        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            if (_owner.IsPlayer)
            {
                _owner.SwitchWeaponTo(EnumEquipSlot.MAX, _defaultWeaponType);
            }
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            AttackBase ab = null;
            _isInAttackEndDelay = false;
            if (!_owner.IsPlayer)
            {
                if (_attackCountAgent.CurrentSkillID == -1 && _attackCountAgent.NextSkillID == -1)
                {
                    _attackCountAgent.NextSkillID = 0;
                }
            }
            if (_owner._onAttackEnter != null)
            {
                int sid = _attackCountAgent.NextSkillID;
                if (sid < 0)
                {
                    sid = _attackCountAgent.CurrentSkillID;
                }
                _owner._onAttackEnter(sid);
            }
            if (_owner.IsPlayerSelf && _state._preState.CurrentStateID == AIAgent.STATE.HURT)
            {
                //when after hurt, we should true player face to enemy
                _keyAgent._directionWanted = _owner.ThisTransform.forward;
            }
            if (_owner.IsClientPlayer)
            {
                _attackCountAgent.CurrentSkillID = _attackCountAgent.NextSkillID = _netSkillInfo._skillID;
                ab = _owner.ACGetAttack(_attackCountAgent.CurrentSkillID, _netSkillInfo._ailevel, _netSkillInfo._comboID);

            }
            else
            {
                if ((_attackCountAgent.NextSkillID != _attackCountAgent.CurrentSkillID && _attackCountAgent.NextSkillID != -1)
                || _attackCountAgent.CurrentSkillID == -1 && _attackCountAgent.NextSkillID != -1)
                {
                    _attackCountAgent.CurrentSkillID = _attackCountAgent.NextSkillID;
                    //_owner.ACClearAttackCount(_attackCountAgent.CurrentSkillID,_aiLevel);
                    ab = _owner.ACGetAttack(_attackCountAgent.CurrentSkillID, _aiLevel, -1);
                }
                else
                {
                    int retID = _owner.ACGetAttackComboCount(_attackCountAgent.CurrentSkillID, _aiLevel);
                    if (retID <= 0)
                    {
                        retID = 0;
                    }
                    ab = _owner.ACGetAttack(_attackCountAgent.CurrentSkillID, _aiLevel, retID);
                }
            }
            //this is for net need to sync
            int attackID = _attackCountAgent.CurrentSkillID;
            int comboID = _owner.ACGetAttackComboCount(attackID, _aiLevel);
            //SendAttackCommandToOthers(ab, attackID, comboID);
            //send attack command to others.
            /*if (_owner.IsPlayerSelf && ab._needSyncToOthers)
            {
                //packed with aiLevel & attackID & comboID;
                Assertion.Check((_aiLevel >= 0) && (_aiLevel < 0x000000ff));
                Assertion.Check((attackID >= 0) && (attackID < 0x000000ff));
                Assertion.Check((comboID >= 0) && (comboID < 0x000000ff));
                int packed = _aiLevel + (attackID << 8) + (comboID << 16);
                int targetID = -1;
                ActionController ac = null;
                if(_aiType == FC_AI_TYPE.PLAYER_WARRIOR)
                {
                    ac = ActionControllerManager.Instance.GetTargetByForward(_owner.ThisTransform,10f,_owner.Faction);
                }
                else
                {
                    ac = ActionControllerManager.Instance.GetEnemyTargetBySight(_owner.ThisTransform,20 ,0,_owner.Faction,60,120,true);
                }
                if(ac != null)
                {
                    targetID = (int)ac.ObjectID.NetworkId;
                }
                float ya = _owner.ThisTransform.rotation.eulerAngles.y;
                CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ATTACK_WITH_SPEC_CONTS,  
                    _owner.ObjectID,
                    packed,
                    FC_PARAM_TYPE.INT,
                    targetID,
                    FC_PARAM_TYPE.INT,
                    ya,
                    FC_PARAM_TYPE.FLOAT);

            }*/

            ab.Init(this);
            if (ab.IsFirstAttack && CurrentSkill.ComboHitValue <= 0)
            {
                _skillCount++;
            }
            if (ab._positionSyncLevel == FCConst.NET_POSITION_SYNC_LEVEL.LEVEL_0)
            {
                _owner.NetworkAgentUse.IsIgnoreSyncEnabled = false;
            }
            else
            {
                _owner.NetworkAgentUse.IsIgnoreSyncEnabled = true;
            }

            if (CurrentSkill != null && CurrentSkill._positionSync != FCConst.NET_POSITION_SYNC_LEVEL.LEVEL_0)
            {
                _owner.NetworkAgentUse.IsIgnoreSyncEnabled = true;
            }
            else
            {

            }

            _state._curState.SetSubAgent(ab as FCAgent);
            _currentAttack = ab;
            AniEventAniIsOver = _currentAttack.AniIsOver;
            EventIsAttackKeyEvent = _currentAttack.AttackKeyEvent;
            EventDirectionKeyEvent = _currentAttack.DirectionKeyEvent;
            EventIsStopAtPoint = _currentAttack.IsStopAtPoint;

            _owner.CurrentWeaponHitType = ab._hitType;
            //this must be false,when attack start
            _isOnParry = FC_PARRY_EFFECT.NONE;
            if (_owner.IsPlayerSelf && ab._showSkillTipWhenStart
                && CurrentSkill != null
                && ab.IsFirstAttack)
            {
                UIBattleSkillTips.Instance.ShowTips(CurrentSkill._skillName);
            }
            ab.AttackEnter();

            /*if(_owner.IsPlayerSelf && ab._endSkillTipWhenStart)
            {
                UIBattleSkillTips.Instance.FinishTips();
            }*/
            if (ab.IsFirstAttack && _owner._onSkillEnter != null)
            {
                _owner._onSkillEnter(CurrentSkillID);

            }
            SendAttackCommandToOthers(ab, attackID, comboID);
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            _isInAttackEndDelay = false;
            bool needDecreaseRage = true;
            if (_currentAttack.IsInDefy)
            {
                needDecreaseRage = false;
            }
            if (_state._nextState.CurrentStateID != AIAgent.STATE.ATTACK)
            {
                _owner.ACEndCurrentAttackEffect(true);
            }
            else
            {
                _owner.ACEndCurrentAttackEffect(false);
            }
            if (_owner.SelfAniSpeedAgent != null)
            {
                _owner.SelfAniSpeedAgent.StopEffect(true);
            }
            int sid = CurrentSkillID;
            _owner.ACActiveWeapon(FC_EQUIPMENTS_TYPE.MAX, false);
            _currentAttack.AttackQuit();
            int nextAttackID = _currentAttack.NextAttackIdx;
            AniEventAniIsOver = AniIsOver;
            if (_currentAttack.MakeSkillCD)
            {
                SetSkillINCoolDown(_attackCountAgent.CurrentSkillID, false);
            }
            if (_owner.IsPlayerSelf)
            {
                if (_state._nextState.CurrentStateID != AIAgent.STATE.ATTACK)
                {
                    _keyAgent.UpdateKeyState(true);
                }
                else
                {
                    if (_attackCountAgent.NextSkillID != _attackCountAgent.CurrentSkillID
                        && _attackCountAgent.NextSkillID != -1 && _attackCountAgent.CurrentSkillID != -1)
                    {
                        _keyAgent.UpdateKeyState(true);
                    }
                }
            }
            _ignoreTicketOnce = false;
            _currentAttack = null;
            EventIsAttackKeyEvent = null;
            EventDirectionKeyEvent = null;
            EventIsStopAtPoint = null;
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);

            if (_attackCountAgent.AttackLevel != _aiLevel)
            {
                _attackCountAgent.CurrentSkillID = -1;
                _attackCountAgent.NextSkillID = -1;
                if (_haveLevelUpAction && !_levelUpAtAnyTime)
                {
                    SetNextState(AIAgent.STATE.LEVEL_UP);
                }
                else if (!_haveLevelUpAction)
                {
                    SetNextState(AIAgent.STATE.IDLE);
                }
            }

            if (_state._nextState.CurrentStateID != AIAgent.STATE.RUN)
            {
                _owner.ACStop();
                _owner.ACRevertToDefalutMoveParams();
            }
            if (_state._nextState.CurrentStateID == AIAgent.STATE.IDLE || _state._nextState.CurrentStateID == AIAgent.STATE.RUN
                || _state._nextState.CurrentStateID == AIAgent.STATE.DEFY)
            {
                if (CurrentSkill != null
                    && CurrentSkill.SkillModule._canCacheCombo && _attackComboLastTime > 0 && (_owner.IsPlayerSelf || !_owner.IsPlayer))
                {
                    _attackCountAgent.NextSkillID = -1;
                    _owner.ACIncreaseAttackCount(_attackCountAgent.CurrentSkillID, _aiLevel, _currentAttack);
                }
                else
                {
                    _attackCountAgent.CurrentSkillID = -1;
                    _attackCountAgent.NextSkillID = -1;
                }
            }
            if (_state._nextState.CurrentStateID == AIAgent.STATE.HURT)
            {
                _attackCountAgent.CurrentSkillID = -1;
                _attackCountAgent.NextSkillID = -1;
            }
            _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);

            _isInAttackDistance = false;

            _owner.ACClearPositionSync();
            _owner.CurrentWeaponHitType = AttackHitType.None;

            //this must be false,when attack start
            _isOnParry = FC_PARRY_EFFECT.NONE;
            if (_state._nextState.CurrentStateID == AIAgent.STATE.ATTACK)
            {
                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            }
            else
            {
                _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
            }
            bool isEnd = false;
            if (_state._nextState.CurrentStateID != AIAgent.STATE.ATTACK)
            {
                isEnd = true;
            }
            else if (_attackCountAgent.CurrentSkillID != _attackCountAgent.NextSkillID
                && _attackCountAgent.NextSkillID != -1)
            {
                isEnd = true;
            }
            else if (_attackCountAgent.CurrentSkillID != -1
                && _attackCountAgent.NextCurSkillComboID == 0
                && nextAttackID > 255)
            {
                isEnd = true;
            }
            if (isEnd)
            {
                if (_rageSystemIsActive && needDecreaseRage)
                {
                    _rageAgent.TryEvent(RageAgent.RageEvent.ATTACK_OVER_WITH_NOHIT, FCWeapon.WEAPON_HIT_TYPE.ALL);
                }
                if (_owner.IsPlayerSelf)
                {
                    CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ACTION_CANCEL, _owner.ObjectID,
                        _owner.ThisTransform.localPosition,
                        null,
                        FC_PARAM_TYPE.NONE,
                        null,
                        FC_PARAM_TYPE.NONE,
                        null,
                        FC_PARAM_TYPE.NONE);

                    UIBattleSkillTips.Instance.FinishTips();
                }
                if (_owner._onSkillQuit != null)
                {
                    _owner._onSkillQuit(sid);
                }
            }

        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {

        }
    }

    public override void DefyTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            _owner.ACStop();
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            if (!_owner.IsPlayer)
            {
                FaceToTarget(_targetAC);
            }
        }
        else if (cmd == FCCommand.CMD.STATE_DONE)
        {
            GoToAttack();
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {

        }
    }
    //after attack ,we need set skill in cooldown
    public void SetSkillINCoolDown(int skillID, bool toEnable)
    {
        //if(_owner.IsPlayerSelf)
        {
            if (_state._nextState.CurrentStateID != AIAgent.STATE.ATTACK)
            {
                _attackCountAgent.SetSkillINCoolDown(skillID, false);
            }
            else
            {
                if (_attackCountAgent.NextSkillID != _attackCountAgent.CurrentSkillID
                    && _attackCountAgent.NextSkillID != -1 && _attackCountAgent.CurrentSkillID != -1)
                {
                    _attackCountAgent.SetSkillINCoolDown(skillID, false);
                }
            }
        }

    }

    //in run state ,I will set CurrentSubStateID after enter.
    public override void HurtTaskChange(FCCommand.CMD cmd)
    {
        if (cmd == FCCommand.CMD.STATE_FINISH)
        {
            _owner.ACRestoreToDefaultSpeed();
            _owner.ACStop();
            if (_owner.IsAlived)
            {
                if (_hurtAgent.StateWantToGo != AIAgent.STATE.NONE)
                {
                    SetNextState(_hurtAgent.StateWantToGo);
                }
                else if (_owner.IsPlayerSelf
                    && _keyAgent.ActiveKey <= 0
                    && _keyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
                {
                    MoveByDirection(_keyAgent._directionWanted, _owner.Data.TotalMoveSpeed, 9999f);
                    SetNextState(AIAgent.STATE.RUN);
                }
                else
                {
                    if (_owner.IsPlayerSelf)
                    {
                        if (_keyAgent.ActiveKey > FC_KEY_BIND.DIRECTION)
                        {
                            if (!TryToAttack())
                            {
                                SetNextState(AIAgent.STATE.IDLE);
                            }
                        }
                        else
                        {
                            SetNextState(AIAgent.STATE.IDLE);
                        }
                    }
                    else
                    {
                        bool goToAttack = false;
                        /*if(RageIsFull)
                        {
                            float ret = Random.Range(0f,1f);
                            if(ret <= _chanceToAttackWhenRageFull)
                            {
                                int skillID = _attackCountAgent.GetCounterSkill();
                                if(skillID != -1)
                                {
                                    _attackCountAgent.NextSkillID = skillID;
                                    _ignoreTicketOnce = true;
                                    GoToAttack();
                                    goToAttack = true;
                                }
                            }
                        }*/
                        //try to use rage Skill
                        if (!goToAttack)
                        {
                            SetNextState(AIAgent.STATE.IDLE);
                        }
                    }
                }
            }

        }
        else if (cmd == FCCommand.CMD.STATE_ENTER)
        {
            Debug.LogWarning("Go into hurt");
            if (_owner.IsPlayerSelf)
            {
                _keyAgent.SetToHurtState(true);
                ComboCounter.Instance.CurrentCount = 0;
            }
            if (_needHideWeaponWhenRun)
            {
                _owner.SwitchWeaponTo(EnumEquipSlot.MAX, _defaultWeaponType);
            }
            _owner.ACSetRaduis(_bodyRadiusHurt);
            _hurtAgent.CurrentHitType = _hurtAgent.NextHitType;
            _hurtAgent.NextHitType = AttackHitType.None;
            _hurtAgent.AnimationIsOver = false;
            _eotAgent.PauseEffect(Eot.EOT_TYPE.EOT_SPEED, true);
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
            _state._curState.CurrentSubStateID = _hurtAgent.CountHurtResult();
            _state._curState.LifeTime = _hurtAgent.EffectTime;
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            EventIsStopAtPoint = _hurtAgent.ACIsStopAtPoint;
            AniEventAniIsOver = _hurtAgent.AniIsOver;
            _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.AGAINST_TO_SELF_SPEED);
            if (!_owner.IsPlayer && _bodyMass < 50f)
            {
                //_owner.ACSetColliderAsTrigger(false);
            }
            _hurtCount++;
        }
        else if (cmd == FCCommand.CMD.STATE_QUIT)
        {
            _owner.ClearEffect();
            if (_owner.IsClientPlayer && GameManager.Instance.IsPVPMode)
            {
                UpdateNetCommand(AIAgent.COMMAND_DONE_FLAG.OUT_HURT_STATE);
            }
            Debug.LogWarning("Go outof hurt");
            if (_owner.IsPlayerSelf)
            {
                _keyAgent.SetToHurtState(false);
            }
            _owner.ACSetRaduis(_bodyRadius);
            if (!_owner.IsPlayer && _bodyMass < 50f)
            {
                //_owner.ACSetColliderAsTrigger(true);
            }
            _eotAgent.PauseEffect(Eot.EOT_TYPE.EOT_SPEED, false);
            _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
            _owner.ACRestoreToDefaultSpeed();
            EventIsStopAtPoint = null;
            AniEventAniIsOver = AniIsOver;
            if (!_owner.IsAlived)
            {
                GotoDead();
            }

            if (_state._nextState.CurrentStateID != AIAgent.STATE.HURT
                && _state._nextState.CurrentStateID != AIAgent.STATE.DEAD)
            {
                _hurtAgent.ClearHurtState();
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
                _owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
                if (_owner.IsPlayer && _keyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
                {
                    _owner.ACRotateTo(_keyAgent._directionWanted, -1, true);
                }
                else
                {
                    _owner.ACRotateTo(_owner.ThisTransform.forward, -1, true);
                }
            }

            _hurtAgent.HurtStateQuit();
        }
    }

    protected bool CountHurtResult(AttackUnit aut)
    {
        Debug.LogWarning("Begin CountHurtResult");
        bool ret = false;
        int realDamage = 0;
        AttackHitType eht = AttackHitType.None;
        bool isCriticalHit = false;

        if (!_owner.IsPlayer && aut.GetOwner() == _owner)
        {
            ret = false;
        }
        else if (_hurtAgent.CanBeHit(aut) && aut.CanHit(_owner))
        {
            if (_isHitBySomeone != null && _isHitBySomeone(aut.GetOwner()))
            {
                if (_owner._onHit != null)
                {
                    _owner._onHit((int)LevelManager.VarNeedCount.BLOCK_PLAYER);
                }
            }
            else
            {
                //if(aut.GetOwner().Data._characterId )
                realDamage = aut.GetFinalDamage(_owner.DefenseAllData, out isCriticalHit, _owner);

                if (aut.GetOwner() != null && aut.GetOwner().ACInSUPERSAIYAJIN2())
                {
                    realDamage = _owner.HitPoint + 100;
                }

                _owner.ACIncreaseThreat(realDamage, true, aut.GetOwner());

                ret = true;
                if (_currentAttack != null && _owner.IsAlived
                    && _currentAttack.HandleHitByTarget(aut.GetOwner(), aut.GetObjType() == FC_OBJECT_TYPE.OBJ_BULLET))
                {
                    ret = false;
                    if (_owner._onHit != null)
                    {
                        _owner._onHit((int)LevelManager.VarNeedCount.BLOCK_PLAYER);
                    }
                    if (CheatManager.showAttackInfo)
                    {
                        DamageCounter.attacklog = DamageCounter.attacklog + "Attack is blocked by some attack";
                        DamageCounter.attacklog = DamageCounter.attacklog + "------------------------------------------------------\n";
                        Debug.Log(DamageCounter.attacklog);
                    }
                }

                if (ret && (!_owner.IsClientPlayer || !GameManager.Instance.IsPVPMode))
                {
                    realDamage += _eotAgent.AddEot(aut.GetEots(_owner.DefenseAllData));

                    if (_rageSystemIsActive)
                    {
                        if (_currentAttack != null && _currentAttack.IsInDefy)
                        {
                            _rageAgent.TryEvent(RageAgent.RageEvent.BE_HIT, aut.GetAttackerType(), true);
                        }
                        else
                        {
                            _rageAgent.TryEvent(RageAgent.RageEvent.BE_HIT, aut.GetAttackerType());
                        }
                    }

                    eht = aut.GetAttackInfo()._hitType;
                    int armorIsBroken = -1;
                    armorIsBroken = _superArmor.TryToHit(realDamage, realDamage, eht, ref realDamage);
                    if (armorIsBroken == 0 && eht != AttackHitType.None
                        || (armorIsBroken == 1 && !HasState(AIAgent.STATE.ARMOR1_BROKEN))
                        || (armorIsBroken == 2 && !HasState(AIAgent.STATE.ARMOR2_BROKEN)))
                    {
                        if (_isOnParry != FC_PARRY_EFFECT.SUCCESS)
                        {
                            eht = _hurtAgent.GetHitType(aut, realDamage, _owner.HitPoint);
                        }
                    }
                    else
                    {
                        eht = AttackHitType.None;
                    }
                    if (_isOnParry == FC_PARRY_EFFECT.PARRY)
                    {
                        if (realDamage > 0)
                        {
                            if (GameManager.Instance.IsPVPMode)
                                realDamage = Mathf.Max(1, (int)((float)realDamage * _parryDamagePercent));
                        }
                    }
                    if (!_owner.IsPlayer && !aut.GetOwner().IsPlayer)
                    {
                        realDamage = 0;
                    }
                    _owner.ACReduceHP(realDamage, true, aut.IsFrom2P(), isCriticalHit, aut.GetAttackInfo()._isFromSkill);
                    if (CheatManager.showAttackInfo)
                    {
                        if (aut.GetObjType() == FC_OBJECT_TYPE.OBJ_BULLET)
                        {
                            DamageCounter.attacklog = DamageCounter.attacklog + "from bullet\n";
                        }
                        DamageCounter.attacklog = DamageCounter.attacklog + _owner.Data.id + "current hitpoint is " + _owner.HitPoint + "\n";
                        DamageCounter.attacklog = DamageCounter.attacklog + "------------------------------------------------------\n";
                        Debug.Log(DamageCounter.attacklog);
                    }
                    if (!_owner.IsAlived)
                    {
                        GotoDead();
                    }
                    else
                    {
                        if (armorIsBroken == -1 && _superArmor.ActiveArmor() == FCConst.SUPER_ARMOR_LVL2)
                        {
                            HurtBlend(aut.GetAttackInfo()._hitType);
                        }
                        else if (armorIsBroken == 2 && SetNextState(AIAgent.STATE.ARMOR2_BROKEN))
                        {
                        }
                        else if (armorIsBroken == 1 && SetNextState(AIAgent.STATE.ARMOR1_BROKEN))
                        {

                        }
                        else if (eht != AttackHitType.None)
                        {
                            SetNextState(AIAgent.STATE.HURT);
                        }
                        else
                        {
                            //means monster can have tiny hurt and didnt break its action
                            HurtBlend(aut.GetAttackInfo()._hitType);
                        }
                    }
                }
                if (_owner.IsAlived)
                {
                    int pushStrength = aut.GetAttackInfo()._pushStrength;
                    float pushTime = aut.GetAttackInfo()._pushTime;
                    AttackHitType ehttmp = aut.GetAttackInfo()._hitType;
                    if (pushStrength <= 0 && ehttmp >= AttackHitType.HurtNormal && ret && eht == AttackHitType.None)
                    {
#if xingtianbo
                        //if(ehttmp > AttackHitType.BLEND_HURT)
                        //{
                        //    ehttmp = ehttmp - AttackHitType.BLEND_HURT + AttackHitType.Normal;
                        //}
                        //else if(ehttmp > AttackHitType.NORMAL_HURT)
                        //{
                        //    ehttmp = ehttmp - AttackHitType.NORMAL_HURT + AttackHitType.Normal;
                        //}
#endif
                        if (ehttmp < AttackHitType.KnockBack)
                        {
                            pushStrength = FCHurtAgent._hitHurtStrength;
                            pushTime = FCHurtAgent._hitHurtTime;
                        }
                        else
                        {
                            pushStrength = FCHurtAgent._hitKnockStrength;
                            pushTime = FCHurtAgent._hitKnockTime;
                            if (aut.GetAttackInfo()._hitType == AttackHitType.KnockBack)
                            {
                                pushTime *= 2;
                            }
                        }
                    }
                    if (pushStrength > 0 && !GameManager.Instance.IsPVPMode)
                    {
                        Vector3 v1 = _owner.ThisTransform.localPosition - aut.GetAttackerTransform().position;
                        v1.y = 0;
                        v1.Normalize();
                        float angle = Vector3.Angle(aut.GetAttackerTransform().forward, v1);
                        if (angle < aut.GetAttackInfo()._pushAngle)
                        {
                            if (_bodyMass > pushStrength + FCConst.WEIGHT_MAX_FOR_G_EFFECT)
                            {
                                pushStrength = 0;
                            }
                            else if (_bodyMass > pushStrength + FCConst.WEIGHT_FOR_G_EFFECT)
                            {
                                pushStrength = ((FCConst.WEIGHT_MAX_FOR_G_EFFECT - (_bodyMass - (pushStrength + FCConst.WEIGHT_FOR_G_EFFECT)))
                                    * pushStrength
                                    / FCConst.WEIGHT_MAX_FOR_G_EFFECT);
                            }
                            _owner.ACEffectByGravity(pushStrength / (float)10, aut.GetAttackerTransform(),
                                    pushTime, aut.GetAttackInfo()._pushByPoint, false);
                            if (ret)
                            {
                                _hurtAgent.HitDirection = aut.GetAttackerTransform().forward;
                            }
                        }
                    }
                }
            }
            if (!_owner.IsPlayer && !aut.GetOwner().IsPlayer)
            {
                ret = false;
            }
            Debug.LogWarning("Have hurt action");
        }
        else
        {
            ret = false;
            Debug.LogWarning("Have no hurt action");
        }

        if (GameManager.Instance.IsPVPMode && ret && !_owner.IsClientPlayer)
        {
            _hurtAgent._hurtInfoForClient.x = (float)eht;
            _hurtAgent._hurtInfoForClient.y = (isCriticalHit == true) ? 1.0f : 0.0f;
            _hurtAgent._hurtInfoForClient.z = (float)aut.GetOwner().ObjectID.NetworkId;
            _hurtAgent._hurtInfoForClient.w = (float)aut.GetOwner().CharacterStrength;
            //_hurtAgent._hurtInfoForClient = new Quaternion((float)eht , (isCriticalHit == true) ? 1.0f : 0.0f , 
            //								(float)aut.GetOwner().ObjectID.NetworkId , (float)aut.GetOwner().CharacterStrength);						
        }

        return ret;
    }

    protected void HurtBlend(AttackHitType eht)
    {
        AttackHitType ehts = eht;
#if xingtianbo
        //if(eht > AttackHitType.BLEND_HURT)
        //{
        //    eht = eht - AttackHitType.BLEND_HURT + AttackHitType.Normal;
        //}
        //else if(eht > AttackHitType.NORMAL_HURT)
        //{
        //    eht = eht - AttackHitType.NORMAL_HURT + AttackHitType.Normal;
        //}
#endif
        if (_activeHurtBlend)
        {
            if (_owner.IsPlayer)
            {
                if (_currentAttack != null)
                {
                    _owner.ACSlowDownAttackAnimation(10, 100, 0, 0);
                }
                else
                {
                    _owner.SelfMoveAgent.PauseMove(true, 0.35f);
                    if (eht > AttackHitType.HurtFlash)
                    {
                        _owner._avatarController.SetAnimation("SubState", 2);
                    }
                    else
                    {
                        _owner._avatarController.SetAnimation("SubState", 1);
                    }
                }
            }
            else
            {
                _owner.ACSlowDownAttackAnimation(1, 100, 0, 0);
                //_owner.SelfMoveAgent.PauseMove(true,0.1f);
                if (eht == AttackHitType.ParrySuccess || ehts > AttackHitType.HurtTiny)
                {
                    _owner._avatarController.SetAnimation("SubState", 2);
                }

                /*if(eht > FC_HIT_TYPE.HURT_FLASH)
                {
                    _owner._avatarController.SetAnimation("SubState",2);
                }
                else
                {
                    _owner._avatarController.SetAnimation("SubState",1);
                }*/
            }
        }
    }
    protected bool TryToAttack()
    {
        bool ret = false;
        _attackComboLastTime = -1;
        _attackCountAgent.NextSkillID = _keyAgent.GetNextAttackID();
        if (_attackCountAgent.NextSkillID != -1 &&
            _attackCountAgent.NextSkillID != _attackCountAgent.CurrentSkillID)
        {
            //_currentAttack != null && _attackCountAgent.CurrentSkillID == -1
            //means player in revive or stand
            if (_attackCountAgent.CurrentSkillID == -1)
            {
                ret = SetNextState(AIAgent.STATE.ATTACK);
            }
            //means will change to 0 combo of next attack
            else if (_currentAttack != null
                && (((_currentAttack.AttackCanSwitch || _currentAttack.CanSwitchToOtherState) && _currentAttack.ShouldGotoNextHit)
                || _attackCountAgent.CompareTwoAttackPriority(_attackCountAgent.CurrentSkillID, _attackCountAgent.NextSkillID)))
            {
                ret = SetNextState(AIAgent.STATE.ATTACK);
            }
            //means ac first attack in game
            else if (_currentAttack == null)
            {
                ret = SetNextState(AIAgent.STATE.ATTACK);
            }
        }
        else
        {
            if (_state.CurrentStateID != AIAgent.STATE.ATTACK && _attackCountAgent.CurrentSkillID != -1
                && (_state._nextState == null
                || (_state._nextState != null && _state._nextState.CurrentStateID != AIAgent.STATE.ATTACK)))
            {
                ret = SetNextState(AIAgent.STATE.ATTACK);
            }
        }
        return ret;
    }
    public override void HandleKeyPress(int key, Vector3 direction)
    {
        if (_keyAgent.KeyIsEnabled(key) && !GameManager.Instance.GamePaused)
        {
            _keyAgent.PressKey((FC_KEY_BIND)key);
            if (key == FCConst.FC_KEY_DIRECTION)
            {
                _keyAgent._directionWanted = direction;
            }
            if (!_owner.IsAlived || _state.CurrentStateID == AIAgent.STATE.HURT
                || (_state._nextState != null && _state._nextState.CurrentStateID == AIAgent.STATE.HURT))
            {
                return;
            }
            if (key != FCConst.FC_KEY_DIRECTION)
            {
                if (EventIsAttackKeyEvent != null && EventIsAttackKeyEvent((FC_KEY_BIND)key, true))
                {

                }
                else
                {
                    TryToAttack();
                }
            }
            else
            {

                if (EventDirectionKeyEvent != null && EventDirectionKeyEvent(direction, true))
                { }
                else
                {
                    if (_currentAttack != null && !_currentAttack.AttackCanSwitch && !_currentAttack.CanSwitchToOtherState)
                    {

                    }
                    else
                    {
                        if (!HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE)
                        && !HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE))
                        {
                            if (_state != null
                                && _state.CurrentStateID == AIAgent.STATE.IDLE)
                            {
                                SetNextState(AIAgent.STATE.RUN);
                            }
                            if (HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2))
                            {
                                MoveByDirection(direction, _owner.Data.TotalMoveSpeed * 1.5f, 9999f, true);
                            }
                            else
                            {
                                if (_owner.IsPlayer && GameManager.Instance.IsPVPMode)
                                {
                                    MoveByDirection(direction, _owner.Data.TotalMoveSpeed, 9999f, true);
                                }
                                else
                                {
                                    MoveByDirection(direction, _owner.Data.TotalMoveSpeed, 9999f);
                                }
                            }

                        }
                        else if (!HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE))
                        {
                            _owner.ACRotateTo(_keyAgent._directionWanted, -1, false);
                        }
                    }


                }
            }
        }
    }

    public override void HandleKeyRelease(int key)
    {
        if (_keyAgent.KeyIsEnabled(key))
        {
            _keyAgent.ReleaseKey((FC_KEY_BIND)key);
            if (key == FCConst.FC_KEY_DIRECTION)
            {
                //_keyAgent._directionWanted = _owner.ThisTransform.forward;
                if (EventIsStopAtPoint != null && EventIsStopAtPoint())
                { }
                else
                {
                    ACStopAtPoint();
                }
                if (EventDirectionKeyEvent != null)
                {
                    EventDirectionKeyEvent(_owner.ThisTransform.forward, false);
                }
            }
            else
            {
                if (EventIsAttackKeyEvent != null && EventIsAttackKeyEvent((FC_KEY_BIND)key, false))
                {

                }
                else
                {
                    if (_keyAgent.ActiveKey > FC_KEY_BIND.DIRECTION)
                    {
                        _attackCountAgent.NextSkillID = _keyAgent.GetNextAttackID();
                    }
                }
            }
        }
    }

    protected void ACStopAtPoint()
    {
        _owner.ACStop();
        _isRunToActionPoint = true;
        if (_state.CurrentStateID == AIAgent.STATE.RUN)
        {
            if (_state._curState.CurrentSubStateID == (int)FCStateRun_meleeNormal.STATE.PATROL)
            {
                GotoNextPathPoint();
            }
            else
            {
                STATE state = AIAgent.STATE.MAX;
                if (_brainAgent != null)
                {
                    state = _brainAgent.GetNextStateByRun(_isInAttackDistance, _state._curState.CurrentSubStateID == (int)FCStateRun_meleeNormal.STATE.SEEK);
                }
                if (state != AIAgent.STATE.MAX)
                {
                    if (_state.CurrentStateID == AIAgent.STATE.RUN
                        && state == AIAgent.STATE.IDLE && _owner.IsClientPlayer)
                    {
                    }
                    else
                    {
                        SetNextState(state);
                    }
                }
                else
                {
                    if (!_owner.IsClientPlayer)
                    {
                        SetNextState(AIAgent.STATE.IDLE);
                    }
                }
            }
        }
        else if (_state.CurrentStateID != AIAgent.STATE.DEAD
            && _state.CurrentStateID != AIAgent.STATE.REVIVE
            && _state.CurrentStateID != AIAgent.STATE.ATTACK
            && _state.CurrentStateID != AIAgent.STATE.HURT
            && _state.CurrentStateID != AIAgent.STATE.HESITATE
            && _state.CurrentStateID != AIAgent.STATE.LEAVE_AWAY
            && _state._nextState == null)
        {
            SetNextState(AIAgent.STATE.IDLE);
        }
    }

    protected override void UpdateLevelUpConditions()
    {
        if (_isInTown)
        {
            return;
        }
        //if true, means damage is enough to make a counter
        string skillName = _skillForCounter;
        if (_skillsForCounter != null && _skillsForCounter.Length > _attackCountAgent.AttackLevel)
        {
            skillName = _skillsForCounter[_attackCountAgent.AttackLevel];
        }
        if (skillName != ""
            && (float)_damageAbsorbCurrent / _owner.Data.TotalHp > _damageValueToAction
            && _owner.IsAlived
            && _attackCountAgent.SkillIsInMap(skillName))
        {
            _damageAbsorbCurrent = 0;
            if (!_hasSuperArmor || (_hasSuperArmor && !_superArmor.ArmorIsBroken(FCConst.SUPER_ARMOR_LVL1)))
            {
                GoToAttackForce(skillName, -1);
            }
        }
    }
    #endregion
}
