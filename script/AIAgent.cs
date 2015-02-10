using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/AI/AIAgent")]
public class AIAgent : FCObject, FCAgent
{
    #region data

    //100 is for warrior, push strength for self.
    public int _characterStrength = 100;

    protected int _finalHitStrength = 100;
    public int FinalHitStrength
    {
        get
        {
            return _finalHitStrength;
        }
        set
        {
            _finalHitStrength = value;
        }
    }

    public void SetHitStrength(float scale)
    {
        _finalHitStrength = (int)((float)_characterStrength * scale);
    }
    //if true, means when be hit, self should have slow action or animation
    public bool _activeHurtBlend = false;
    //FIXME  have no use now
    public bool _haveTinyHurtState = false;

    //use this to set navmesh base offset
    public float _baseFlyHeight = 0;

    //if true, means self is in town
    public bool _isInTown = false;

    //all states of self is on this object
    public GameObject _stateAgents;
    //other agent for ai should on this object
    public GameObject _behaviourAgents;

    //body radius of self for normal, hurt and attack state
    public float _bodyRadius = 0.5f;
    public float _bodyRadiusAttack = 0.5f;
    public float _bodyRadiusHurt = 0.5f;

    //if true ,means energy gain not from class EnergyGain 
    public bool _haveWayToGetEnergy = false;
    //these two val active by _haveWayToGetEnergy = true
    public float _energyGainByHpPer = 5f;
    public int _energyGainAttack = 5;

    //effect with attackbase->_needAttackCorrect or GetHurtDirection
    public float _attackCorrectAngleForMelee = 90;
    //FIXME  used to auto aim target, now have no use with no danger level
    public int _attackCorrectAngleForRangerMin = 90;
    public int _attackCorrectAngleForRangerMax = 180;

    //if true, means blood effect can be played when hurt
    public bool _hasBloodEffect = true;
    //FIXME  have no use 
    public bool _hasSelfBloodColor = false;

    //if true, means when ai level up, we should have levelup action and logic
    public bool _haveLevelUpAction = false;
    //if true, means levelUp will do at once when conditions is ok
    //FIXEME  there still some error when _levelUpAtAnyTime = false
    public bool _levelUpAtAnyTime = false;

    //Get random time for monster idle
    public float _timeForIdleMin = 1f;
    public float _timeForIdleMax = 2f;

    //god time for self born
    public float _timeGodForBorn = 0.7f;
    //some one may need diff born effect
    public FC_GLOBAL_EFFECT _effectForBorn = FC_GLOBAL_EFFECT.BORN;
    protected float _timeForIdleThisTime;

    //if true, super armor 1 will instead of super armor 0
    public bool _hasSuperArmor = false;
    //if have superarmor1break state, time last for SuperArmor1Break
    public float _timeForSuperArmor1Break = 5f;
    //if true, means when run. self will hang weapon back
    public bool _needHideWeaponWhenRun = false;
    protected float _timeCounterSuperArmor1Break = 0;
    //now only used for player
    //1 means 100%
    public float _chanceToPlayNormalAttackSound = 0.5f;
    public string[] _randomSoundForNormalAttack;

    //hitid will record recent 10 hit ids from other attacker, prevent self was hurt by one slash with more than 1 times
    protected int[] _hitIDs = new int[10];
    protected int _hitIDsIdx = -1;

    //used to count self's super armor
    protected SuperArmor _superArmor;

    //FIXME  this val have no use now
    protected bool _hasCounterSkill = false;

    //count total run time for self
    protected float _runStateTime = 0;

    protected bool _inDeadState = false;
    public float RunStateTime
    {
        get
        {
            return _runStateTime;
        }
        set
        {
            _runStateTime = value;
        }
    }

    public SuperArmor SuperArmorSelf
    {
        get
        {
            return _superArmor;
        }
    }
    //only used for monster
    public FCTicket _monsterTicket = null;
    //if _energyGain > 1, we will add it to monster energy
    protected float _energyGain = 0f;

    // if mass height, the object with low mass can push the big object

    //if < 50 , _bodyMass = 100-avoidPriority
    // if > 50, (550-_bodyMass )/10
    //  0 ~99 avoidPriority
    public int _bodyMass = 50;


    public FC_AI_TYPE _aiType;

    //show it to GD for current version, will disable it in futher
    //only active for monster
    //rage is another energy system for monster
    //rage and energy can use at same time
    //FIXME  may should push rage into energy system
    public int _rageForHurt = 50;
    public float _chanceToAttackWhenRageFull = 1;
    protected bool _ignoreTicketOnce = false;
    protected float _rageCurrent = 0;
    protected int _rageMax = 100;

    // < 10 means normal enemy
    // 10 -20 means elite
    // >20 means boss
    //Now all enemy danger level < 10
    public int _dangerLevel = 0;

    public int DangerLevel
    {
        get
        {
            return _dangerLevel;
        }
    }

    //means if _attackComboLastTime >0, when attack ,will not reset combo count for the skill
    //only used for normal attack,not for skill
    protected float _attackComboLastTime = 0;
    public float AttackComboLastTime
    {
        get
        {
            return _attackComboLastTime;
        }
        set
        {
            _attackComboLastTime = value;
        }
    }

    public bool RageIsFull
    {
        get
        {
            return ((_rageCurrent >= _rageMax) && _rageSystemIsActive);
        }
    }
    protected bool _rageSystemIsActive = false;

    public bool RageSystemIsActive
    {
        get
        {
            return _rageSystemIsActive;
        }
    }

    protected RageAgent _rageAgent;
    protected bool _enableRageSystem;

    //if true ,means knock animation istead of dead animation
    protected bool _deadWithFly = false;
    //if true ,means self next state should be dead, so setnextstate will have no use
    protected bool _prepareToDeadState = false;

    //default weapon type for self
    public FC_EQUIPMENTS_TYPE _defaultWeaponType = FC_EQUIPMENTS_TYPE.WEAPON_HEAVY;

    //if true, means enemy will disappear at dead and play explode effect
    public bool _deadWithExplode = false;
    //mark a enemy as leader

    //dead with slow time 
    protected bool _slowTimeWhenDead = false;
    public bool SlowTimeWhenDead
    {
        get
        {
            return _slowTimeWhenDead;
        }
        set
        {
            _slowTimeWhenDead = value;
        }
    }
    public float _slowTimeLast = 3f;
    public float _slowTimeScale = 0.2f;

    protected int _aiLevel = 0;
    public int AILevel
    {
        get
        {
            return _aiLevel;
        }
    }

    //FIXME  have no use now
    protected float _inCollisionTime = -1;
    //used for animtion play
    private AniSwitch _aniSwitch = new AniSwitch();

    protected AttackBase _currentAttack = null;

    public delegate void AniEventWithNoParam();
    public AniEventWithNoParam AniEventAniIsOver;

    //sub agent may need these event
    public delegate bool ACEventIsStopAtPoint();
    public ACEventIsStopAtPoint EventIsStopAtPoint;

    public delegate bool ACEventIsAttackKeyEvent(FC_KEY_BIND ekb, bool isPress);
    public ACEventIsAttackKeyEvent EventIsAttackKeyEvent;

    public delegate bool ACEventDirectionKeyEvent(Vector3 direction, bool isPress);
    public ACEventDirectionKeyEvent EventDirectionKeyEvent;

    public delegate bool FUNCTION_AC(ActionController target);
    public FUNCTION_AC _isHitBySomeone;

    public delegate void ACEventGodDownSkillEvent(float duration);

    //if true, means need play normal dead animations
    protected bool _needPlayDeadAnimation = true;

    //effect by passive skill or attack
    protected float _addDamageScale = 0;
    //FIXME  should have passive add damage scale and attack add damage

    public int _maxAiLevel = 0;

    public float _parryDamagePercent = 0.5f;
    public float _parryDamagePercentPvp = 0.5f;

    //override by born point
    protected bool _lockByCamera = false;
    protected bool _haveHpBar = false;
    protected bool _haveEnergyBar = false;

    public bool LockByCamera
    {
        get
        {
            return _lockByCamera;
        }
        set
        {
            _lockByCamera = value;
        }
    }

    public bool HaveHpBar
    {
        get
        {
            return _haveHpBar;
        }
        set
        {
            _haveHpBar = value;
        }
    }

    public bool HaveEnergyBar
    {
        get
        {
            return this._haveEnergyBar;
        }
        set
        {
            _haveEnergyBar = value;
        }
    }

    public float AddDamageScale
    {
        get
        {
            if (_currentAttack != null && _currentAttack.AddDamageScale > 0)
            {
                return _addDamageScale + _currentAttack.AddDamageScale;
            }
            else
            {
                return _addDamageScale;
            }
        }
        set
        {
            _addDamageScale = value;
        }
    }

    public bool NeedPlayDeadAnimation
    {
        get
        {
            return _needPlayDeadAnimation;
        }
        set
        {
            _needPlayDeadAnimation = value;
        }
    }

    public bool DeadWithFly
    {
        get
        {
            return _deadWithFly;
        }
        set
        {
            _deadWithFly = value;
        }
    }

    public AttackBase CurrentAttack
    {
        get
        {
            return _currentAttack;
        }
    }

    //FIXME  this code may cause debug crash
    //So Add code to try to fixed it
    public FCSkillConfig CurrentSkill
    {
        get
        {
            //old way
            /*if(_isInTown || CurrentSkillID < 0 || _owner.ACGetAttackAgent() == null)
            {
                return null;
            }
            return  _owner.ACGetAttackAgent()._skillMaps[_aiLevel ]._skillConfigs[CurrentSkillID];*/
            //new way
            if (_owner.IsSpawned && !_isInTown)
            {
                return _attackCountAgent.CurrentSkill;
            }
            return null;
        }
    }

    public AniSwitch AnimationSwitch
    {
        get
        {
            return _aniSwitch;
        }
        set
        {
            _aniSwitch = value;
        }
    }

    public int CurrentSkillID
    {
        get
        {
            //FIXME  default value should not be skill id can use
            int v = -1;
            if (_attackCountAgent != null)
            {
                v = _attackCountAgent.CurrentSkillID;
            }
            return v;
        }
    }

    public int NextCurSkillComboID
    {
        get
        {
            return _attackCountAgent.NextCurSkillComboID;
        }
    }

    //the data need sync to client for attack
    public class NetSkillInfo
    {
        public int _ailevel = 0;
        public int _skillID = 0;
        public int _comboID = 0;
    }

    //if self be shake && attack have shake camera in attackbase, then camera will shake when self was hit
    public bool CanShakeWhenBeHit
    {
        get
        {
            if (_hurtAgent != null)
            {
                return _hurtAgent._canBeShake;
            }
            return false;
        }
    }

    protected NetSkillInfo _netSkillInfo = new NetSkillInfo();
    #endregion

    #region data
    //must not insert any var to make the enum value be changed
    //FIXME  now aniswitch instead of sub state, such as IDLE_XX or ATTACK_XX
    public enum STATE
    {
        NONE = -1,
        IDLE = 0,
        IDLE_1,// for normal idle action
        IDLE_2,// for normal idle action
        RUN = 100,
        HESITATE = 110,
        LEAVE_AWAY = 120,
        AVOID_AND_SHOOT = 130,
        DEFY = 140,
        BORN = 200,

        ATTACK = 400,
        ATTACK_CHARGE = 410,
        ATTACK_FIRE_SPEAR = 420,
        ATTACK_MAGIC_BALL = 430,
        ATTACK_MAGIC_BALL_1,
        ATTACK_MAGIC_BALL_2,
        ATTACK_FLASH = 440,
        ATTACK_SLASH = 450,
        ATTACK_SLASH_1,
        ATTACK_SLASH_2,
        ATTACK_SLASH_3,
        ATTACK_SLASH_5 = 455,
        ATTACK_SMASH_1 = 456,
        ATTACK_SLASH_CLOSE_1 = 457,
        ATTACK_BASH = 470,

        ATTACK_FIRE_WIND_1 = 481,
        ATTACK_FIRE_WIND_2,

        ATTACK_PARRY_START = 491,
        ATTACK_PARRY_END,
        ATTACK_PARRY_ATTACK,
        ATTACK_PARRY_SUCCESS,

        ATTACK_ICE_ARMOR = 501,
        ATTACK_ICE_ARMOR_SUCCESS = 502,

        ATTACK_DASH_START = 515,
        ATTACK_DASH_END1 = 516,
        ATTACK_DASH_END2 = 517,
        ATTACK_DASH_ATTACK = 518,

        ATTACK_LIGHTNINGBALL = 520,

        ATTACK_DODGE_START = 530,
        ATTACK_DODGE_END_1 = 531,
        ATTACK_DODGE_END_2 = 532,

        ATTACK_SEVEN_START = 543,
        ATTACK_SEVEN_END_1 = 544,
        ATTACK_SEVEN_END_2 = 545,
        ATTACK_SEVEN_END_3 = 546,

        ATTACK_KIKOHO = 557,
        ATTACK_KIKOHO_END = 558,

        HURT = 700,
        LEVEL_UP = 800,
        ARMOR1_BROKEN = 810,
        ARMOR2_BROKEN = 820,
        DEAD = 1000,
        REVIVE = 1100,
        STAND = 1200,
        SUMMON = 1300,
        DUMMY = 1400,


        MAX = 10000
    };

    //this class use to record or change state of AI
    public class STATE_STRUCT
    {
        public FCStateAgent _nextState;
        public FCStateAgent _curState;
        public FCStateAgent _preState;

        public STATE CurrentStateID
        {
            get
            {
                return _curState.CurrentStateID;
            }
        }

        public STATE NextStateID
        {
            get
            {
                if (_nextState == null)
                {
                    return STATE.NONE;
                }
                return _nextState.CurrentStateID;
            }
        }

        public STATE PreStateID
        {
            get
            {
                if (_preState == null)
                {
                    return STATE.NONE;
                }
                return _preState.CurrentStateID;
            }
        }

        //this function is told the state need to stop and change to next, but not real change
        public bool SetNextState(FCStateAgent ewa)
        {
            _nextState = ewa;
            if (_curState != null)
            {
                _curState.StopRun();
            }
            else
            {
                ChangeState();
            }
            return true;
        }

        //change to next state
        public void ChangeState()
        {
            _preState = _curState;
            _curState = _nextState;
            _nextState = null;

            if (GameManager.Instance.IsPVPMode)
            {
                /*if(_curState.StateOwner.ACOwner.NetworkAgentUse != null)
                {
                    {
                        CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_CURRSTATE , _curState.StateOwner.ACOwner.ObjectID,
                            _curState.StateOwner.ACOwner.ThisTransform.localPosition,
                            (int)CurrentStateID ,FC_PARAM_TYPE.INT , 
                            null , FC_PARAM_TYPE.NONE,
                            null , FC_PARAM_TYPE.NONE);
                    }
					
                }*/
            }

            _curState.Run();
        }
    }

    //if you want to some command must be handle by current state ,use it
    //not recommend to use
    public delegate bool SubAgentHandleCmd(FCCommand.CMD cmd, object param0, object param1, object param2, object param3);
    protected SubAgentHandleCmd _sAHandleCmd;


    protected STATE_STRUCT _state;

    public STATE_STRUCT AIStateAgent
    {
        get
        {
            return _state;
        }
    }

    //record all states
    //FIXME  may I should move stateList into STATE_STRUCT, and make STATE_STRUCT as an agent
    protected FCStateAgent[] _stateList;

    //if have this, enemy can have patrol 
    protected FCPathAgent _pathway = null;
    //if true, means enemy must go on path
    //now only used for town
    //FIXEME  I am not satisfied with code for patrol or run path
    //patrol should have independent state, and onPath can use for any state with move or no move
    protected bool _runOnPath = false;
    protected bool _isOnSeek = false;

    //decide whether use skill and use which skill
    //record current skill and next skill
    protected FCAttackCountAgent _attackCountAgent = null;

    //handle hurt event
    protected FCHurtAgent _hurtAgent = null;
    //an assist agent for ai to decide next action of self
    //FIXME  this agent should give monster more action like human or execute action script
    //but now, I cant give all the power to brain, may change it in next project or GD need more fine action of monster or npc
    protected BrainAgent _brainAgent = null;

    //handle all effect of time for self
    protected EotAgent _eotAgent = null;

    //if have this, monster can more way to seek player or attack in a state
    //now only used for boss
    protected SpecicalActionMonsterAgent _spActAgent = null;

    //target of self
    protected ActionController _targetAC;
    protected ActionController _owner;
    //record all action switch of ai
    protected int _actionSwitchFlag;

    //if true, means monster may goto attack
    protected bool _isInAttackDistance;
    //if true, means self can accept key event
    protected bool _controlByPlayer = false;
    //if true, means self can only control by net command
    protected bool _inHostControl;
    protected FCCommand _netCommand = null;

    //FIXME  have no use now
    protected int _currentAbilityLevel = 0;
    //have no use now, but some one may need it
    protected int _currentAniLoopCount = 0;
    //agent to hanlde key event
    protected AgentKeyControl _keyAgent;

    //when parry attack,block will be true
    protected FC_PARRY_EFFECT _isOnParry = FC_PARRY_EFFECT.NONE;

    //FIXME  this code is not write by me, but I think this code should have better pos instead of current
    protected AttackConditions.CONDITION_VALUE _conditionValue = AttackConditions.CONDITION_VALUE.NONE;
    public AttackConditions.CONDITION_VALUE ConditionValue
    {
        get { return _conditionValue; }
        set { _conditionValue = value; }
    }

    //to count god time from attackbase
    protected float _godTime = 0.0f;

    //do remember target of parry
    protected ActionController _parryTarget = null;

    //if true , when monster want to attack, they should apply ticket from ticket system
    public bool _haveTicketLimit = true;
    //if true, monster can attack
    protected bool _haveActionTicket = false;

    public bool CanAttackOthers
    {
        get
        {
            return (_haveActionTicket && (_monsterTicket == null || (_monsterTicket != null && _monsterTicket.CanUse())));
        }
    }

    //wheather monster use bow to attack
    public bool _isRanger = false;

    public ActionController ParryTarget
    {
        set
        {
            _parryTarget = value;
        }
        get
        {
            return _parryTarget;
        }
    }
    public float GodTime
    {
        set
        {
            _godTime = value;
            if (_godTime > Mathf.Epsilon)
            {
                SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
            }
            else
            {
                ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
            }
        }
        get
        {
            return _godTime;
        }
    }

    public FC_PARRY_EFFECT IsOnParry
    {
        get
        {
            return _isOnParry;
        }
        set
        {
            _isOnParry = value;
        }
    }

    public FCAttackCountAgent AttackCountAgent
    {
        get
        {
            return _attackCountAgent;
        }
    }

    public AgentKeyControl KeyAgent
    {
        get
        {
            return _keyAgent;
        }
    }
    public int CurrentAniLoopCount
    {
        get
        {
            return _currentAniLoopCount;
        }
        set
        {
            _currentAniLoopCount = value;
        }
    }

    #endregion

    #region declare
    public bool IsOnSeek
    {
        get
        {
            return _isOnSeek;
        }
        set
        {
            _isOnSeek = value;
        }
    }
    public bool RunOnPath
    {
        get
        {
            return _runOnPath;
        }
    }

    public bool HasPathWay
    {
        get
        {
            return (_pathway != null && _pathway.HasPath);
        }
    }

    public ActionController TargetAC
    {
        get
        {
            return _targetAC;
        }
        set
        {
            _targetAC = value;
        }
    }

    public bool ControlByPlayer
    {
        set
        {
            if (_controlByPlayer != value)
            {
                _controlByPlayer = value;
                if (_controlByPlayer)
                {
                    InputManager.Instance.AddWatch(_owner);
                }
                else
                {
                    InputManager.Instance.RemoveWatch(_owner);
                }
            }
        }
    }

    public ActionController ACOwner
    {
        get
        {
            return _owner;
        }
    }

    public string PathName
    {
        get
        {
            return _owner.Data.patrolPath;
        }
    }

    public int ActionSwitchFlag
    {
        set
        {
            _actionSwitchFlag = value;
        }
    }

    public bool IsInAttackDistance
    {
        get
        {
            return _isInAttackDistance;
        }
    }

    public bool InHostControl
    {
        get
        {
            return _inHostControl;
        }
        set
        {
            _inHostControl = value;
        }
    }

    public FCHurtAgent HurtAgent
    {
        get
        {
            return _hurtAgent;
        }
    }

    public float TimeForIdleThisTime
    {
        get
        {
            return _timeForIdleThisTime;
        }
    }
    #endregion

    public static string GetTypeName()
    {
        return "AIAgent";
    }

    protected bool _isInAttackEndDelay = false;
    public bool IsInAttackEndDelay
    {
        get
        {
            return _isInAttackEndDelay;
        }
    }

    //>0 means increase
    public void IncreaseRage(RageAgent.RageEvent ret, FCWeapon.WEAPON_HIT_TYPE weaponHitType)
    {
        if (_rageSystemIsActive)
        {
            _rageAgent.TryEvent(ret, weaponHitType);
        }
    }

    //means try to cost rage max ,but if have more than max, will not clean to zero
    public void TryCostAllRage()
    {
        if (_rageSystemIsActive)
        {
            _rageCurrent -= _rageMax;
            if (_rageCurrent <= 0)
            {
                _rageCurrent = 0;
            }
        }
    }
    public void Init(FCObject owner)
    {
        _owner = owner as ActionController;
        _sAHandleCmd = null;
        if (_controlByPlayer)
        {
            InputManager.Instance.AddWatch(_owner);
        }
    }

    public EotAgent EotAgentSelf
    {
        get
        {
            return _eotAgent;
        }
    }

    public virtual void ActiveAI()
    {
        _aiLevel = 0;
        _inDeadState = false;
        _parryTarget = null;
        _pathway = _behaviourAgents.GetComponent<FCPathAgent>();
        _hurtAgent = _behaviourAgents.GetComponent<FCHurtAgent>();
        _brainAgent = _behaviourAgents.GetComponent<BrainAgent>();
        _keyAgent = _behaviourAgents.GetComponent<AgentKeyControl>();
        _eotAgent = _behaviourAgents.GetComponent<EotAgent>();
        _spActAgent = _behaviourAgents.GetComponent<SpecicalActionMonsterAgent>();
        _netSkillInfo._ailevel = 0;
        _netSkillInfo._skillID = 0;
        _netSkillInfo._comboID = 0;
        _owner.BaseFlyHeight = _baseFlyHeight;
        //avoid the aitype was added by tow times
        _owner.ObjectID.ObjectType = (FC_OBJECT_TYPE)((int)_owner.ObjectID.ObjectType | (int)_aiType);
        _stateList = _stateAgents.GetComponents<FCStateAgent>();
        foreach (FCStateAgent ewa in _stateList)
        {
            ewa.Init(this);
        }
        _state = new STATE_STRUCT();

        if (_pathway)
        {
            _pathway.Init(this);
        }
        if (_hurtAgent != null)
        {
            _hurtAgent.Init(this);
        }
        if (_brainAgent != null)
        {
            _brainAgent.Init(this);
        }
        if (_keyAgent != null)
        {
            _keyAgent.Init(this);
            if (_attackCountAgent != null)
            {
                _keyAgent.InitKeyMap(_owner.ACGetAttackAgent()._skillMaps[_aiLevel]._skillConfigs);
            }
            else
            {
                _keyAgent.InitKeyMap(null);
            }
        }
        AniEventAniIsOver = null;
        AniEventAniIsOver += AniIsOver;
        if (_eotAgent != null)
        {
            _eotAgent.Init(this);
        }
        if (_isInTown)
        {
            _owner.ACSetRaduis(0);
        }
        else
        {
            _owner.ACSetRaduis(_bodyRadius);
        }
        ChangeAILevel(0);
        if (_owner.IsPlayer || !_haveTicketLimit || _dangerLevel >= FCConst.BOSS_DANGER_LEVEL)
        {
            _haveActionTicket = true;
        }
        _monsterTicket = null;
        _superArmor = new SuperArmor();
        _superArmor.Init(this);
        if (_hasSuperArmor)
        {
            _superArmor.CreateArmor(_owner.Data.thresholdMax, 0, -1, FCConst.SUPER_ARMOR_LVL1);
            _superArmor.BreakArmor(FCConst.SUPER_ARMOR_LVL0);
        }
        else
        {
            _superArmor.CreateArmor(_owner.Data.thresholdMax, 0, -1, FCConst.SUPER_ARMOR_LVL0);
        }
        for (int i = 0; i < _hitIDs.Length; i++)
        {
            _hitIDs[i] = -1;
        }
        _hitIDsIdx = 0;
        _addDamageScale = 0;
        _rageAgent = _behaviourAgents.GetComponent<RageAgent>();
        if (_rageAgent != null && _rageAgent._isActive)
        {
            _rageSystemIsActive = true;
            _rageAgent.Init(this);
        }
        SetHpBarAndCamera();
    }

    public void SetHpBarAndCamera()
    {
        if (_haveHpBar && UIManager.Instance.SetBossHPDisplay != null)
        {
            UIManager.Instance.SetBossHPDisplay(true, _maxAiLevel + 1, Localization.instance.Get(_owner.Data.nameIds));
            Assertion.Check(UIManager.Instance.SetBossHP != null);
            UIManager.Instance.SetBossHP(1.0f);
            Assertion.Check(UIManager.Instance.SetBossShield != null);
            UIManager.Instance.SetBossShield(1.0f);
            _owner._avatarController._uiHPController.SetHPBarVisible(false);
        }
        if (_lockByCamera)
        {
            CameraController.Instance.SetTarget2(_owner.ThisTransform);
        }
    }

    //used for skill can use in keyborad
    public void CheatInitKeyMap()
    {
        if (_keyAgent != null)
        {
            if (_attackCountAgent != null)
            {
                _keyAgent.InitKeyMap(_owner.ACGetAttackAgent()._skillMaps[_aiLevel]._skillConfigs);
            }
        }
    }


    #region logic

    public void ChangeState()
    {
        _state.ChangeState();
    }

    public bool TestTicketAvailable(bool useTicket)
    {
        bool ret = true;
        if (_haveActionTicket || _ignoreTicketOnce)
        {
            if (_monsterTicket != null)
            {
                if (useTicket && !_monsterTicket.UseTicket())
                {
                    ret = false;
                }
                else if (!useTicket && !_monsterTicket.CanUse())
                {
                    ret = false;
                }

            }
        }
        else
        {
            if (_monsterTicket == null && !FCTicketManager.Instance.ApplyTicket(_owner))
            {
                ret = false;
            }
        }
        return ret;
    }

    public bool SetNextState(STATE state)
    {
        if (state == STATE.ATTACK && _state.CurrentStateID != STATE.ATTACK)
        {
            if (!TestTicketAvailable(true))
            {
                state = STATE.IDLE;
            }
        }
        return SetNextState(state, false);
    }

    //go to attack with skill and combo id right now
    public void GoToAttackForce(string skillName, int comboValue)
    {
        _owner.ACStop();
        _attackCountAgent.SetNextSkill(skillName, comboValue, true);
        SetNextState(AIAgent.STATE.ATTACK, true);
    }

    protected virtual void GoToAttack()
    {
        HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD, null);
        //for run state
        _isOnSeek = true;
        _runOnPath = false;
        _attackCountAgent.Run();
        SetNextState(AIAgent.STATE.RUN);

    }

    public virtual void GoToAttack(string skillName, bool needDetectDistance)
    {
        _owner.ACStop();
        EventIsStopAtPoint = null;
        AniEventAniIsOver = AniIsOver;
        _attackCountAgent.SetNextSkill(skillName, -1, false);
        _attackCountAgent.NeedDetectDistance = needDetectDistance;
        GoToAttack();
    }

    public bool SetNextState(STATE state, bool force)
    {
        bool ret = false;
        //FIXME  code with judge dead should have changed, too more code for dead judgment
        if (!_owner.IsAlived && state != STATE.DEAD && state != STATE.HURT && state != STATE.REVIVE)
        {
            state = STATE.DEAD;
            if (state == _state.CurrentStateID)
            {
                return false;
            }
        }
        if (_state._nextState != null && _state._nextState.CurrentStateID > state && !force)
        {
            return false;
        }
        if ((_state._curState != null && _state.CurrentStateID == STATE.DEAD
            || (_state._nextState != null && _state._nextState.CurrentStateID == STATE.DEAD)) && (state != STATE.REVIVE))
        {
            return false;
        }
        foreach (FCStateAgent ewa in _stateList)
        {
            if (ewa.CurrentStateID == state)
            {
                ret = _state.SetNextState(ewa);
                break;
            }
        }
        if (ret)
        {
            if (state != AIAgent.STATE.RUN)
            {
                _isOnSeek = false;
            }
            if (state == AIAgent.STATE.ATTACK)
            {
                _attackCountAgent.Pause(true);
            }
        }
        return ret;
    }

    public bool HasState(STATE state)
    {
        bool ret = false;
        foreach (FCStateAgent ewa in _stateList)
        {
            if (ewa.CurrentStateID == state)
            {
                ret = true;
                break;
            }
        }
        return ret;
    }

    //FIXME  this code should have no use now
    public bool StateCanBreakTo(STATE state)
    {
        bool result = false;
        if (((_state.CurrentStateID != STATE.IDLE && _state.CurrentStateID != STATE.RUN)
            && state == STATE.RUN) || _state.CurrentStateID == STATE.HESITATE)
        {
            result = false;
        }
        else
        {
            foreach (FCStateAgent ewa in _stateList)
            {
                if (ewa.CurrentStateID == state)
                {
                    result = ewa.CanGoto();
                    break;
                }
            }
        }

        return result;
    }

    public bool InState(STATE state)
    {
        bool ret = false;

        if (_state._curState.CurrentStateID == state)
        {
            ret = true;
        }
        return ret;
    }

    public virtual void ChangeAILevel(int level)
    {
        _aiLevel = level;
    }

    //move to means go to target point
    public void MoveTo(ref Vector3 point)
    {
        _owner.ACMove(ref point);
    }

    public void MoveTo(ref Vector3 point, float speed, float angleSpeed)
    {
        _owner.CurrentAngleSpeed = angleSpeed;
        _owner.CurrentSpeed = speed;
        _owner.ACMove(ref point);
    }

    public void MoveTo(ref Vector3 point, float speed)
    {
        _owner.CurrentSpeed = speed;
        _owner.ACMove(ref point);
    }

    //move by means move with direction
    public void MoveByDirection(Vector3 direction, float speed, float time)
    {
        _owner.CurrentSpeed = speed;
        _owner.ACSetDirection(ref direction);
        _owner.ACMoveByDirection(time);
    }

    public void MoveByDirection(Vector3 direction, float speed, float time, bool rightNowMode)
    {
        _owner.CurrentSpeed = speed;
        _owner.SelfMoveAgent.GoByDirection(ref direction, time, rightNowMode);
    }

    public void MoveByDirection(Vector3 direction, float speed, float time, float acceleration)
    {
        _owner.CurrentSpeed = speed;
        _owner.Acceleration = acceleration;
        _owner.ACSetDirection(ref direction);
        _owner.ACMoveByDirection(time);
    }

    public bool CurrentAttackIsFinalAttack()
    {
        return _owner.ACGetAttackAgent().IsFinalAttack(CurrentSkillID, _aiLevel);
    }

    public void StopMove()
    {
        _owner.ACStop();
    }

    public bool CanSeekTarget()
    {
        bool ret = false;
        if (_targetAC.IsAlived)
        {
            ret = true;
        }
        else
        {

            if ((_targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition).sqrMagnitude < Mathf.Max(_owner.CurrentSpeed * _owner.CurrentSpeed, 25))
            {
                ret = false;
            }
            else
            {
                ret = true;
            }
        }
        return ret;
    }

    #region //override by sub class
    public virtual bool HandleInnerCmd(FCCommand.CMD cmd, object param0)
    {
        return true;
    }

    public virtual bool HandleInnerCmd(FCCommand.CMD cmd, object param0, object param1, object param2, object param3)
    {
        return true;
    }

    public virtual void BornTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void IdleTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void AttackTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void HurtTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void RunTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void DeadTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void ReviveTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void StandTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void AwayTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void LevelUpTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void DefyTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void HesitateTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void AvoidAndShootTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void Armor1BrokenTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void Armor2BrokenTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void SummonTaskChange(FCCommand.CMD cmd)
    {

    }

    public virtual void DummyTaskChange(FCCommand.CMD cmd)
    { 

    }

    protected virtual void UpdateLevelUpConditions()
    {

    }

    public virtual void HandleLateUpdate()
    {

    }

    public virtual void HandleKeyPress(int key, Vector3 direction)
    {

    }

    public virtual void HandleKeyRelease(int key)
    {

    }

    //must override by sub class
    public virtual void HpIsChanged(int changeValue)
    {
        if (_haveHpBar && UIManager.Instance.SetBossHPDisplay != null)
        {
            Assertion.Check(UIManager.Instance.SetBossHP != null);
            UIManager.Instance.SetBossHP(_owner.HitPointPercents);
            if (!_owner.IsAlived)
            {
                UIManager.Instance.SetBossHPDisplay(false, 1, "");
            }
        }
        if (changeValue > 0)
        {
            if (!_owner.IsPlayer && !_owner.IsAlived && _slowTimeWhenDead)
            {
                _owner.ACSlowDownTime(_slowTimeLast, _slowTimeScale);
            }
        }
    }

    #endregion //override by sub class

    public void SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG eValue)
    {
        _actionSwitchFlag |= (1 << ((int)eValue));
        if (_owner.IsAlived
            && (eValue == FC_ACTION_SWITCH_FLAG.IN_GOD
            || eValue == FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN
            || eValue == FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2))
        {
            if (!_isInTown)
            {
                _owner._avatarController.UnDestoryableColor(true);
            }
        }
    }

    public void ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG eValue)
    {
        _actionSwitchFlag &= (~(1 << ((int)eValue)));
        if ((eValue == FC_ACTION_SWITCH_FLAG.IN_GOD
            || eValue == FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN
            || eValue == FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2)
            && (!HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD)
            && !HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN)
            && !HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2)))
        {
            _owner._avatarController.UnDestoryableColor(false);
        }
    }

    public bool HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG eValue)
    {
        return (_actionSwitchFlag & (1 << ((int)eValue))) != 0;
    }

    public int GetThresholdMax()
    {
        return _owner.Data.thresholdMax;
    }

    public void GotoNextPathPoint()
    {
        if (_pathway.HasPath)
        {
            _pathway.GotoNextPathPoint();
            Vector3 point = _pathway.CurrentPathPoint;
            _owner.ACMove(ref point);
        }
    }

    protected void AniIsOver()
    {
        if (_state.CurrentStateID == STATE.REVIVE)
        {
            ReviveTaskChange(FCCommand.CMD.STATE_DONE);
        }
        else if (_state.CurrentStateID == STATE.BORN)
        {
            BornTaskChange(FCCommand.CMD.STATE_DONE);
        }
        else if (_state.CurrentStateID == STATE.DEFY)
        {
            DefyTaskChange(FCCommand.CMD.STATE_DONE);
        }
        else if (_state.CurrentStateID == STATE.ARMOR2_BROKEN)
        {
            Armor2BrokenTaskChange(FCCommand.CMD.STATE_DONE);
        }
    }
    public void PlayAnimation()
    {
        if (_aniSwitch._aniIdx != FC_All_ANI_ENUM.none)
        {
            _owner.ACPlayAnimation(_aniSwitch);
        }
    }

    protected override void OnDestroy()
    {
        AniEventAniIsOver = null;
        _isHitBySomeone = null;
    }

    public FC_KEY_BIND GetCurrentAttackKeyBind()
    {
        return _keyAgent.GetAttackKeyBind(_attackCountAgent.CurrentSkillID);
    }


    public void ShowCharge(float percents)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (!CheatManager.chargeWithNoKeyHold)
        {
            _owner.ACShowCharge(percents);
        }
#endif
    }

    protected void CollisionWithOtherACS(bool enableCollision, float timeLast)
    {

    }

    protected virtual void TargetFinded(ActionController ac)
    {
        _targetAC = ac;
    }

    protected virtual void UpdateComboTime()
    {
        if (_attackComboLastTime > 0)
        {
            _attackComboLastTime -= Time.deltaTime;
            if (_attackComboLastTime <= 0)
            {
                if (_state.CurrentStateID != AIAgent.STATE.ATTACK)
                {
                    _attackCountAgent.CurrentSkillID = -1;
                }
            }
        }
    }

    public void FaceToTarget(Transform target, bool needNow)
    {
        if (target != null)
        {
            Vector3 dir = target.position - _owner.ThisTransform.localPosition;
            dir.y = 0;
            dir.Normalize();
            if (dir != Vector3.zero)
            {
                _owner.ACRotateTo(dir, -1, true, needNow);
            }
        }

    }

    public void FaceToTarget(ActionController target, bool needNow)
    {
        if (target != null)
        {
            Vector3 dir = target.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
            dir.y = 0;
            dir.Normalize();
            if (dir != Vector3.zero)
            {
                _owner.ACRotateTo(dir, -1, true, needNow);
            }
        }

    }

    public void SendAttackCommandToOthers(AttackBase ab, int skillID, int comboID)
    {
        if (_owner.IsPlayerSelf && ab._needSyncToOthers)
        {
            Assertion.Check((_aiLevel >= 0) && (_aiLevel < 0x000000ff));
            Assertion.Check((skillID >= 0) && (skillID < 0x000000ff));
            Assertion.Check((comboID >= 0) && (comboID < 0x000000ff));
            int packed = _aiLevel + (skillID << 8) + (comboID << 16);
            int targetID = -1;
            ActionController ac = null;
            if (_aiType == FC_AI_TYPE.PLAYER_WARRIOR)
            {
                ac = ActionControllerManager.Instance.GetTargetByForward(_owner.ThisTransform, 10f, _owner.Faction);
            }
            else
            {
                ac = ActionControllerManager.Instance.GetEnemyTargetBySight(_owner.ThisTransform, 20, 0, _owner.Faction, 60, 120, true);
            }
            if (ac != null)
            {
                targetID = (int)ac.ObjectID.NetworkId;
            }
            float ya = 0;
            if (_owner.SelfMoveAgent.RotateFlag == MoveAgent.ROTATE_FLAG.UNLOCK)
            {
                Quaternion qt = Quaternion.FromToRotation(Vector3.forward, _owner.SelfMoveAgent._rotateDirection);
                ya = qt.eulerAngles.y;
            }
            else if (_owner.SelfMoveAgent._moveFinalDirection != Vector3.zero && _owner.SelfMoveAgent.IsInMoveD)
            {
                Quaternion qt = Quaternion.FromToRotation(Vector3.forward, _owner.SelfMoveAgent._moveFinalDirection);
                ya = qt.eulerAngles.y;
            }
            else
            {
                ya = _owner.ThisTransform.eulerAngles.y;
            }

            CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ATTACK_WITH_SPEC_CONTS,
                _owner.ObjectID,
                _owner.ThisTransform.localPosition,
                packed,
                FC_PARAM_TYPE.INT,
                targetID,
                FC_PARAM_TYPE.INT,
                ya,
                FC_PARAM_TYPE.FLOAT);
        }
    }
    public void FaceToTarget(ActionController target, float angle = 181)
    {
        if (target != null && angle > 0)
        {
            Vector3 dir = target.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
            dir.y = 0;
            dir.Normalize();
            if (dir != Vector3.zero)
            {
                if (angle >= 180)
                {
                    //_owner.ACRotateTo(dir,-1,true);
                }
                else
                {
                    float angleV = Vector3.Angle(_owner.ThisTransform.forward, dir);
                    if (angleV <= angle)
                    {
                        //_owner.ACRotateTo(dir,-1,true);
                    }
                    else
                    {
                        float zz = _owner.ThisTransform.forward.z * target.ThisTransform.forward.x - _owner.ThisTransform.forward.x * target.ThisTransform.forward.z;
                        if (zz > 0)
                        {

                            dir = Quaternion.Euler(0, angle, 0) * _owner.ThisTransform.forward;
                        }
                        else if (zz < 0)
                        {
                            dir = Quaternion.Euler(0, -angle, 0) * _owner.ThisTransform.forward;
                        }
                        //Debug.Log(angleV);

                        //Vector3 dir1 = target.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
                        //dir1.y =0;
                        //dir1.Normalize();
                        //angleV = Vector3.Angle(dir1, dir);
                        //Debug.Log(angleV);
                        //_owner.ACRotateTo(dir,-1,true);
                    }
                }
                _owner.ACRotateTo(dir, -1, true);
            }
        }

    }
    #endregion


    public FC_DANGER_LEVEL GetTargetDangerLevel(float disSafe, float disDanger)
    {
        FC_DANGER_LEVEL ret = FC_DANGER_LEVEL.SAFE;
        if (_targetAC != null)
        {
            float safeLength = (disSafe - _targetAC.BodyRadius - _owner.BodyRadius);
            float dangerLength = (disDanger - _targetAC.BodyRadius - _owner.BodyRadius);
            Vector3 dis = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
            //means monster and player didnt stay at the same plane
            if (dis.y > 3 || dis.y < -3)
            {
                ret = FC_DANGER_LEVEL.SAFE;
            }
            else
            {
                dis.y = 0;
                float len = dis.magnitude;
                if (len > safeLength)
                {
                    ret = FC_DANGER_LEVEL.SAFE;
                }
                else if (len > dangerLength)
                {
                    ret = FC_DANGER_LEVEL.DANGER;
                }
                else
                {
                    ret = FC_DANGER_LEVEL.VERY_DANGER;
                }
            }
        }
        return ret;
    }

    public void AddHitBackList(ActionController ac)
    {
        if (_hurtAgent != null)
        {
            _hurtAgent.AddHitBackList(ac);
        }
    }

    public void RemoveHitBackList(ActionController ac)
    {
        if (_hurtAgent != null)
        {
            _hurtAgent.RemoveHitBackList(ac);
        }
    }

    public void ClearHitBackList()
    {
        if (_hurtAgent != null)
        {
            _hurtAgent.ClearHitBackList();
        }
    }


    public void UpdadeHitBackList(Vector3 motion)
    {
        if (_hurtAgent != null)
        {
            _hurtAgent.UpdadeHitBackList(ref motion);
        }
    }

    public int GetEnergyByHurt(float hpPercents)
    {
        int ret = 0;
        _energyGain += hpPercents * _energyGainByHpPer;
        if (_energyGain > 1)
        {
            ret = (int)_energyGain;
            _energyGain -= ret;
        }
        return ret;
    }

    public int GetEnergyByAttack()
    {
        return _energyGainAttack;
    }

    public void BulletIsFire()
    {
        if (_currentAttack != null)
        {
            _currentAttack.AniBulletIsFire();
        }
        _state._curState.OnFireBullet();
    }

    public void GotoDead()
    {
        if (!_prepareToDeadState)
        {
            _prepareToDeadState = true;
            SetNextState(STATE.DEAD, true);
        }
    }

    public void GoToRevive()
    {
        SetNextState(AIAgent.STATE.REVIVE);
    }

    public void SetKeyState(FC_KEY_BIND ekb, bool enableKey)
    {
        if (_keyAgent != null)
        {
            _keyAgent.SetKeyState(ekb, enableKey);
        }
    }

    //unlock player and goto state
    public void GotoUnControlState(AIAgent.STATE ass)
    {
        SetKeyState(FC_KEY_BIND.MAX, false);
        _owner.ACStop();
        SetNextState(ass, true);
    }

    public void GotoInControlState(AIAgent.STATE ass)
    {
        SetKeyState(FC_KEY_BIND.MAX, true);
        _owner.ACStop();
        SetNextState(ass, true);
    }

    public void UnlockPlayer()
    {
        SetKeyState(FC_KEY_BIND.MAX, true);
    }

    //FCCommand _currentCommand = null;
    FCCommand _nextCommand = null;

    Vector3 _currentActionPoint;
    protected bool _isRunToActionPoint = false;
    bool _runCurrentCommand = true;
    public enum COMMAND_DONE_FLAG
    {
        NONE,
        ATTACK_CAN_SWITCH,
        ATTACK_IS_OVER,
        IN_HURT_STATE,
        OUT_HURT_STATE,
    }

    void FaceToPoint(Vector3 vs, Vector3 vt)
    {
        Vector3 dis = vt - vs;
        dis.y = 0;
        dis.Normalize();
        if (dis != Vector3.zero)
        {
            _owner.ACRotateTo(dis, -1, true, true);
        }
    }
    void UpdateNextNetCommand(COMMAND_DONE_FLAG cdf = COMMAND_DONE_FLAG.NONE)
    {
        bool hurtState = false;
        bool attackState = false;
        if (_state.CurrentStateID == STATE.HURT
            || _state.NextStateID == STATE.HURT)
        {
            hurtState = true;
        }
        if ((_state.CurrentStateID == STATE.ATTACK
            || _state.NextStateID == STATE.ATTACK)
            && (cdf != COMMAND_DONE_FLAG.ATTACK_CAN_SWITCH
            && cdf != COMMAND_DONE_FLAG.ATTACK_IS_OVER))
        {
            attackState = true;
        }
        CommandManager.Instance.ReadCommandFromCache2((int)_owner.ObjectID, ref _nextCommand);
        if (_nextCommand != null && (!_nextCommand._isRun || cdf != COMMAND_DONE_FLAG.NONE))
        {

            if (_currCommand._cmd == FCCommand.CMD.CLIENT_HURT)
            {
                //Debug.LogWarning(_nextCommand._cmd);
            }
            _nextCommand._isRun = true;

            switch (_nextCommand._cmd)
            {
                case FCCommand.CMD.ACTION_NEW_WAY:
                    {
                        if (!_currCommand._isRun)
                        {
                            _nextCommand._isRun = false;
                        }
                        else if (hurtState || attackState)
                        {
                            _nextCommand._isRun = false;
                        }
                        else if (cdf == COMMAND_DONE_FLAG.ATTACK_CAN_SWITCH || cdf == COMMAND_DONE_FLAG.ATTACK_IS_OVER)
                        {
                            FaceToPoint(_currCommand._commandPosition, (Vector3)_nextCommand._param1);
                        }
                        else if (_currCommand._cmd == FCCommand.CMD.ACTION_TO_IDLE)
                        {
                            FaceToPoint((Vector3)_currCommand._param1, (Vector3)_nextCommand._param1);
                        }
                        if (_nextCommand._isRun)
                        {
                            _currentActionPoint = (Vector3)_nextCommand._param1;
                            MoveTo(ref _currentActionPoint);
                            _runCurrentCommand = false;
                            _isRunToActionPoint = true;
                            if (_state.CurrentStateID != STATE.RUN)
                            {
                                SetNextState(STATE.RUN);
                            }
                        }
                    } break;
                case FCCommand.CMD.ACTION_TO_IDLE:
                    {
                        if (_state.CurrentStateID == STATE.RUN)
                        {
                            _currentActionPoint = (Vector3)_nextCommand._param1;
                            MoveTo(ref _currentActionPoint);
                            _runCurrentCommand = false;
                            if (_state.CurrentStateID != STATE.RUN)
                            {
                                SetNextState(STATE.RUN);
                            }
                        }
                        else if (!hurtState && !attackState)
                        {
                            _isRunToActionPoint = true;
                        }
                        else
                        {
                            _nextCommand._isRun = false;
                        }
                    } break;
                case FCCommand.CMD.ACTION_CANCEL:
                    {
                        if (_state.CurrentStateID == STATE.ATTACK)
                        {
                            HandleCommand(ref _nextCommand);
                            ClearAllCommand();
                        }
                        else
                        {
                            _nextCommand = null;
                            UpdateNextNetCommand();
                        }
                    } break;
                case FCCommand.CMD.ATTACK_WITH_SPEC_CONTS:
                    {
                        if (_state.CurrentStateID == STATE.IDLE || _state.CurrentStateID == STATE.RUN)
                        {
                            if (_currCommand._cmd == FCCommand.CMD.ACTION_TO_IDLE)
                            {
                                FaceToPoint((Vector3)_currCommand._param1, _nextCommand._commandPosition);
                            }
                            _currentActionPoint = (Vector3)_nextCommand._commandPosition;
                            MoveTo(ref _currentActionPoint);
                            _runCurrentCommand = false;
                            if (_state.CurrentStateID != STATE.RUN)
                            {
                                SetNextState(STATE.RUN);
                            }
                        }
                        else if (_state.CurrentStateID == STATE.ATTACK)
                        {
                            GotoNextCommand(true);
                        }
                    } break;
                case FCCommand.CMD.ACTION_TO_ATTACK_POS_SYNC:
                    {
                        if (_updateAttackPos && _attackMoveSpeed != 0 && attackState)
                        {
                            _currentActionPoint = (Vector3)_nextCommand._param1;
                            _owner.CurrentSpeed = _attackMoveSpeed;
                            MoveTo(ref _currentActionPoint);
                        }
                        if (_updateAttackRotation && attackState)
                        {
                            Quaternion rot = Quaternion.Euler((Vector3)_nextCommand._param2);
                            Vector3 vRotation = rot * Vector3.forward;
                            _owner.ACRotateTo(vRotation, -1, false);
                        }
                        _nextCommand = null;
                    } break;
                case FCCommand.CMD.ACTION_TO_HURT_POS_SYNC:
                    {
                        if (_hurtAgent.CurrentHitType == AttackHitType.ForceBack && hurtState)
                        {
                            //	_owner.SelfMoveAgent.SetPosition((Vector3)_nextCommand._param1);
                        }
                        _nextCommand = null;
                        UpdateNextNetCommand();
                    } break;
                case FCCommand.CMD.CLIENT_HURT:
                    {
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.REVIVE:
                case FCCommand.CMD.DIE_NORMAL:
                    {
                        _owner.HandleCommand(ref _nextCommand);
                        _nextCommand = null;
                    } break;
                case FCCommand.CMD.ACTION_EOT:
                    {
                        Eot.EOT_TYPE eet = (Eot.EOT_TYPE)((Vector3)_nextCommand._param1).x;
                        float lastTime = ((Vector3)_nextCommand._param2).x;
                        float damageP = ((Vector3)_nextCommand._param2).y;
                        float damageV = ((Vector3)_nextCommand._param2).z;
                        EotAgentSelf.AddEot(eet, lastTime, damageP, damageV);
                        _nextCommand = null;
                        UpdateNextNetCommand();
                    }
                    break;

                case FCCommand.CMD.CLIENT_HURT_HP:
                case FCCommand.CMD.CLIENT_POTION_HP:
                case FCCommand.CMD.CLIENT_POTION_ENERGY:
                    {
                        _owner.HandleCommand(ref _nextCommand);
                        _nextCommand = null;
                        UpdateNextNetCommand();
                    }
                    break;
                case FCCommand.CMD.POTION_HP:
                    {

                    } break;
                case FCCommand.CMD.ACTION_TO_STAND:
                    {
                        _owner.SelfMoveAgent.SetPosition((Vector3)_nextCommand._param1);
                        _nextCommand = null;
                        UpdateNextNetCommand();
                    } break;
                case FCCommand.CMD.INVALID:
                    {
                        _nextCommand = null;
                    } break;
                default:
                    break;
            }
        }
    }


    bool GotoNextCommand(bool readNext)
    {
        bool ret = false;
        if ((_currCommand != null && _currCommand._canDrop) || readNext)
        {
            if (_currCommand != null)
            {
                _currCommand._isRun = false;
                _currCommand._canDrop = false;
                _currCommand._needRunPerFrame = false;

            }
            _currCommand = _nextCommand;
            if (_currCommand != null)
            {
                _currCommand._isRun = false;
                _currCommand._canDrop = false;
                _currCommand._needRunPerFrame = false;
            }
            _nextCommand = null;
            _isRunToActionPoint = false;
            _runCurrentCommand = true;
            ret = true;
        }
        return ret;
    }

    public bool _updateAttackPos = false;
    public bool _updateAttackRotation = false;

    public float _attackMoveSpeed = 0;
    void ClearAllCommand()
    {
        _currCommand = null;
        _nextCommand = null;
        _isRunToActionPoint = false;
        _runCurrentCommand = true;
    }
    public void UpdateNetCommand(COMMAND_DONE_FLAG cdf = COMMAND_DONE_FLAG.NONE)
    {
        if (_state.CurrentStateID == STATE.STAND)
        {
            //return;
        }

        if (_currCommand != null && _currCommand._cmd == FCCommand.CMD.CLIENT_HURT
            && _nextCommand != null)
        {
            Debug.Log(_nextCommand._cmd);
        }
        if (cdf == COMMAND_DONE_FLAG.IN_HURT_STATE)
        {
            if (_nextCommand != null
                && (_nextCommand._cmd == FCCommand.CMD.ACTION_TO_IDLE
                || _nextCommand._cmd == FCCommand.CMD.ACTION_NEW_WAY))
            {
                _nextCommand = null;
            }
        }
        if (cdf == COMMAND_DONE_FLAG.ATTACK_IS_OVER
            || cdf == COMMAND_DONE_FLAG.ATTACK_CAN_SWITCH)
        {
            if (_currCommand != null && _currCommand._cmd == FCCommand.CMD.ATTACK_WITH_SPEC_CONTS)
            {
                _currCommand._canDrop = true;
            }
        }
        _runCurrentCommand = true;
        if (_currCommand == null || _currCommand._cmd == FCCommand.CMD.INVALID)
        {
            _currCommand = CommandManager.Instance.ReadCommandFromCache2((int)_owner.ObjectID);

            while (_currCommand != null &&
                (_currCommand._cmd == FCCommand.CMD.CLIENT_HURT_HP
                || _currCommand._cmd == FCCommand.CMD.CLIENT_POTION_HP
                || _currCommand._cmd == FCCommand.CMD.CLIENT_POTION_ENERGY))
            {
                _owner.HandleCommand(ref _currCommand);
                _currCommand = CommandManager.Instance.ReadCommandFromCache2((int)_owner.ObjectID);
            }
        }
        UpdateNextNetCommand(cdf);

        if (_isRunToActionPoint)
        {
            GotoNextCommand(false);
        }
        bool hurtState = false;
        bool attackState = false;
        if (_state.CurrentStateID == STATE.HURT
            || _state.NextStateID == STATE.HURT)
        {
            hurtState = true;
        }
        if ((_state.CurrentStateID == STATE.ATTACK
            || _state.NextStateID == STATE.ATTACK)
            && (cdf != COMMAND_DONE_FLAG.ATTACK_CAN_SWITCH
            && cdf != COMMAND_DONE_FLAG.ATTACK_IS_OVER))
        {
            attackState = true;
        }
        if (_runCurrentCommand && _currCommand != null)
        {

            if (_currCommand._isRun)
            {
                if (_currCommand._cmd == FCCommand.CMD.CLIENT_HURT)
                {
                    if (_nextCommand != null)
                    {
                        if (_nextCommand._cmd == FCCommand.CMD.CLIENT_HURT)
                        {
                            GotoNextCommand(true);
                        }
                        else if (!hurtState)
                        {
                            GotoNextCommand(true);
                        }
                    }
                }
                return;
            }

            switch (_currCommand._cmd)
            {

                case FCCommand.CMD.ACTION_TO_HURT_POS_SYNC:
                    {
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.ACTION_TO_ATTACK_POS_SYNC:
                    {
                        if (_updateAttackPos && attackState)
                        {
                            if (_nextCommand != null
                                && _nextCommand._cmd == FCCommand.CMD.ACTION_TO_ATTACK_POS_SYNC)
                            {
                                Quaternion rotation = Quaternion.Euler((Vector3)_currCommand._param2);
                                Vector3 movement = rotation * Vector3.forward;
                                if (_updateAttackPos)
                                {
                                    if (_attackMoveSpeed > 0)
                                    {
                                        _isRunToActionPoint = false;
                                        MoveByDirection(movement, _owner.CurrentSpeed, 999f, true);
                                    }
                                }
                                if (_updateAttackRotation)
                                {
                                    _owner.ACRotateTo(movement, -1, false, true);
                                }
                            }
                            else
                            {
                                _owner.SelfMoveAgent.SetPosition((Vector3)_currCommand._param1);
                                _owner.CurrentSpeed = 0;
                            }
                        }
                        else if (_updateAttackRotation && attackState)
                        {
                            Quaternion rot = Quaternion.Euler((Vector3)_currCommand._param2);
                            Vector3 vRotation = rot * Vector3.forward;
                            _owner.ACRotateTo(vRotation, -1, true, true);
                        }
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.ACTION_NEW_WAY:
                    {
                        if (_isRunToActionPoint)
                        {

                        }
                        else
                        {
                            if (_state.CurrentStateID == STATE.IDLE || _state.CurrentStateID == STATE.RUN)
                            {
                                FaceToPoint(_owner.ThisTransform.localPosition, (Vector3)_currCommand._param1);
                                Quaternion rotation = Quaternion.Euler((Vector3)_currCommand._param2);
                                Vector3 movement = rotation * Vector3.forward;
                                _isRunToActionPoint = false;
                                _owner.ACRestoreToDefaultSpeed();

                                if (_owner.CurrentNormalSpeed > 0)
                                {
                                    MoveByDirection(movement, _owner.CurrentNormalSpeed, 999f, true);
                                }
                                else
                                {
                                    MoveByDirection(movement, _owner.Data.TotalMoveSpeed, 999f, true);
                                }
                                //SetNextState(STATE.RUN);
                                if (_state.CurrentStateID != STATE.RUN)
                                {
                                    //MoveByDirection(movement, _owner.Data.TotalMoveSpeed, 999f, true);
                                    SetNextState(STATE.RUN);
                                }
                                //Debug.Log("ACTION_NEW_WAY FCCommand.CMD.ACTION_NEW_WAY");
                            }
                        }
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.ACTION_TO_IDLE:
                    {
                        if (_isRunToActionPoint)
                        {

                        }
                        else
                        {
                            if (((Vector3)_currCommand._param1 - _owner.ThisTransform.localPosition).magnitude >= 0.3f)
                            {
                                _owner.SelfMoveAgent.SetPosition((Vector3)_currCommand._param1);
                            }
                            Quaternion rotation = Quaternion.Euler((Vector3)_currCommand._param2);
                            Vector3 movement = rotation * Vector3.forward;
                            _owner.CurrentSpeed = 0;
                            _owner.ACRotateTo(movement, -1, false, true);

                            if (_state.CurrentStateID != STATE.IDLE)
                            {
                                SetNextState(STATE.IDLE);
                            }
                            GotoNextCommand(false);
                        }
                        GotoNextCommand(true);
                    } break;

                case FCCommand.CMD.ATTACK_WITH_SPEC_CONTS:
                    {
                        _owner.SelfMoveAgent.SetPosition((Vector3)_currCommand._commandPosition);
                        HandleCommand(ref _currCommand);
                        _currCommand._isRun = true;
                        _currCommand._canDrop = false;
                    } break;
                case FCCommand.CMD.ACTION_CANCEL:
                    {
                        HandleCommand(ref _currCommand);
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.CLIENT_HURT:
                    {
                        Quaternion param1 = (Quaternion)_currCommand._param1;
                        AttackHitType eht = (AttackHitType)param1.x;
                        bool isCritical = (param1.y == 0.0 ? false : true);
                        if (eht != AttackHitType.ForceBack)
                        {
                            _owner.SelfMoveAgent.SetPosition(_currCommand._commandPosition);
                        }
                        int attackerObjNetID = (int)param1.z;
                        int attackerCharacterStrength = (int)param1.w;
                        OBJECT_ID attackerObjID = ObjectManager.Instance.GetObjectByNetworkID(attackerObjNetID);

                        Quaternion param2 = (Quaternion)_currCommand._param2;
                        Vector3 hitDirection = new Vector3(param2.x, param2.y, param2.z);
                        float effectTime = param2.z;
                        _owner.ACHandleHurt(eht, isCritical, attackerObjID, hitDirection, effectTime, true, true, false, attackerCharacterStrength);

                        _currCommand._isRun = true;
                        _currCommand._canDrop = false;
                    } break;

                case FCCommand.CMD.DIE_NORMAL:
                case FCCommand.CMD.REVIVE:
                    {
                        _owner.HandleCommand(ref _currCommand);
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.ACTION_EOT:
                    {
                        Eot.EOT_TYPE eet = (Eot.EOT_TYPE)((Vector3)_currCommand._param1).x;
                        float lastTime = ((Vector3)_currCommand._param2).x;
                        float damageP = ((Vector3)_currCommand._param2).y;
                        float damageV = ((Vector3)_currCommand._param2).z;
                        EotAgentSelf.AddEot(eet, lastTime, damageP, damageV);
                        GotoNextCommand(true);
                    } break;

                case FCCommand.CMD.CLIENT_HURT_HP:
                case FCCommand.CMD.CLIENT_POTION_HP:
                case FCCommand.CMD.CLIENT_POTION_ENERGY:
                    {
                        //_owner.HandleCommand(ref _currCommand);
                        GotoNextCommand(true);
                    } break;
                case FCCommand.CMD.ACTION_TO_STAND:
                    {
                        GotoNextCommand(true);
                    } break;
                default:
                    HandleCommand(ref _currCommand);
                    GotoNextCommand(true);
                    break;
            }

        }
    }
    //update net command and cache command from manager
    List<FCCommand> _commandList = new List<FCCommand>();
    FCCommand _currCommand = new FCCommand();

    public void UpdateNetCommand2()
    {
        if (_commandList.Count < 2)
        {

            FCCommand ewc = null;
            FC_COMMAND_NETSTREAM ecns = null;

            CommandManager.Instance.ReadCommandFromCache((int)_owner.ObjectID, out ewc, out ecns);

            //rpc command
            if (ewc != null)
            {
                _commandList.Add(ewc);
            }

            //net command stream
            if (ecns != null)
            {
                if (ecns._state == STATE.RUN)
                {
                    FCCommand _fastCmd = new FCCommand();
                    _fastCmd._cmd = FCCommand.CMD.MOVE_TO_POINT;

                    Quaternion rotation = Quaternion.Euler(ecns._currentRotation);
                    Vector3 movement = rotation * Vector3.forward;
                    movement.y = 0f;
                    movement.Normalize();
                    Vector3 destinationPos = ecns._currentPosition + movement * _owner.Data.TotalMoveSpeed * (float)(PhotonNetwork.time - ecns.timeStamp);
                    _fastCmd._param1 = destinationPos;

                    _fastCmd._objID = _owner.ObjectID;
                    _fastCmd._isHost = false;
                    _fastCmd._cmdIndex = ecns._commandIndex;

                    //compare rpc command index with net stream command index
                    if (_commandList.Count > 0 && ecns._commandIndex < _commandList[_commandList.Count - 1]._cmdIndex)
                    {
                        _commandList.Insert(_commandList.Count - 1, _fastCmd);
                    }
                    else
                    {
                        _commandList.Add(_fastCmd);
                    }
                }
            }
        }

        if (_commandList.Count > 0)
        {
            _currCommand = _commandList[0];
            CommandManager.Instance.SendFastToSelf(ref _currCommand);
            _commandList.RemoveAt(0);
        }


        //if(FCCommand.CMD == FCCommand.CMD.MOVE)
        {
            //...............
            //if state == run
            //next command from net
            // if nextCommand == null. run always
            // if nextCommand == stream && state == run
            // run to target position
            // if pos = target pos
            //go to next command
            //catch next -> next command

        }
    }

}
