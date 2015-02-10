using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using InJoy.RuntimeDataProtection;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/ActionController")]
public class ActionController : FCObject {
	
	#region component
	public GameObject _agent = null;
	//every object should a ID to sign self
	public int _instanceID = -1;
	
	//per attack from ac should have a attack id
	//0 means can always hit target
	private int _attackID = -1;
	//handle ac move function
	protected MoveAgent _moveAgent = null;
	//handle ac attack function
	protected AttackAgent _attackAgent = null;
	//AI center
	protected AIAgent _aiAgent = null;
	//handle armor and weapon
	protected EquipmentsAgent _equipsAgent = null;
	//function as monster eyes
	protected DiscoverAgent _discoverAgent = null;
	//manager bullet and bullet fire
	protected RangerAgent _rangerAgent = null;
	protected FCCommand _fastCommand = null;
	protected NetworkAgent _networkAgent = null;
	//manager threat of monsters
	protected ThreatAgent _threatAgent = null;
	//FIXME  I use it to manager all effect of ac, such as particle, camera,,, But now 
	//we have not a common manager to manage all effect
	protected EffectAgent _effectAgent = null;
	protected LootAgent _lootAgent = null;
	//manage animation speed of ac, such as slash dumping
	protected AniSpeedAgent _aniSpeedAgent = null;
	//this agent can slow timescale of system
	protected TimeScaleListener _timeScaleListener = null;
	//if player or monster need passive skill, should have this agent
	protected PassiveSkillAgent _passiveSkillAgent = null;

	//if true, means ac is active on map
	protected bool _isSpawned = false;
	
#if NOPROTECT	
	protected int _hitPoint = -1;
	protected int _energy = -1;
#else	
    //protected Encrypted<int> _hitPoint = -1;
    //protected Encrypted<int> _energy = -1;
    //protected Encrypted<float> _critRate = -1;
    //protected Encrypted<float> _critDamage = -1;
    //protected Encrypted<float> _skillTriggerRate = -1;
    //protected Encrypted<float> _skillAttackDamage = -1;
    protected EncryptInt _hitPoint = -1;
    protected EncryptInt _energy = -1;
    protected EncryptFloat _critRate = -1;
    protected EncryptFloat _critDamage = -1;
    protected EncryptFloat _skillTriggerRate = -1;
    protected EncryptFloat _skillAttackDamage = -1;
#endif	
	//playerself = 1P
	protected bool _isPlayerSelf = false;
	
	//FIXME  buffer should be buff, its a spell error
	//effect by ice eot
	protected float _bufferSpeedPercent = 1;
	protected float _bufferAniMoveSpeedPercent = 1;
	protected float _bufferAniPlaySpeedPercent = 1;
	protected Color _bufferColor = new Color(0,0,0);

    protected float _eotPhysicalAttack = 1; //the effect time of attack;
    protected float _eotPhysicalDefense = 1;//the effect time of defence;
    protected float _eotElementalAttack = 1;
    protected float _eotElementalResistance = 1; //the effect time of resist;

	//if we have buff potion in future, we should keep these
	protected float _bufferFinalDamageChangePercent = 0;
	protected float _bufferFinalDefenseChangePercent = 0;
	
	//if true, means weapon of ac is on back, not in hand
	protected bool _hangWeaponBack = false;
	//if true, means weapon is flying as a bullet
	protected bool _weaponInFly = false;
	
	//FIXME  I want to use it enable or disable animtion event for ac, but have no use now
	protected bool _canAcceptEvent = true;
	
	protected bool _deadAtOnce = false; //dead at once, do not go to dead state
	
	//Manager animtion and shader
	public AvatarController _avatarController = null;
	
	protected CirclePlayer _cameraAnchor = null;
	
	//if not null, when ac fire bullet, bullet will fly to the target
	protected ActionController _fireTarget = null;
	
	protected CountAgent _countAgent = null;
	
	public CountAgent SelfCountAgent
	{
		get
		{
			return _countAgent;
		}
	}
	public bool HangWeaponBack
	{
		get
		{
			return _hangWeaponBack;
		}
	}
	//means monk attack
	public bool HaveAttackCorrectMelee
	{
		get
		{
			return (!_aiAgent._isRanger 
				&& _aiAgent.CurrentAttack != null 
				&& _aiAgent.CurrentAttack._needAttackCorrect);
		}
	}
	
	public bool HaveAttackPushBack
	{
		get
		{
			return (!_aiAgent._isRanger 
				&& _aiAgent.CurrentAttack != null 
				&& _aiAgent.CurrentAttack._needPushBack);
		}
	}
	public AniSpeedAgent SelfAniSpeedAgent
	{
		get
		{
			return _aniSpeedAgent;
		}
	}
	
	public bool HasBloodEffect
	{
		get
		{
			return _aiAgent._hasBloodEffect;
		}
	}
	public float AttackCorrectAngleForMelee
	{
		get
		{
			return _aiAgent._attackCorrectAngleForMelee;
		}
	}
	
	public int DangerLevel
	{
		get
		{
			if(_aiAgent != null)
			{
				return _aiAgent.DangerLevel;
			}
			return 0;
		}
	}
	public CirclePlayer CameraAnchor
	{
		get
		{
			return _cameraAnchor;
		}
		set
		{
			_cameraAnchor = value;
			if(_moveAgent._circlePlayer == null)
			{
				_moveAgent._circlePlayer = _cameraAnchor;
				_moveAgent._circlePlayer.transform.position = ThisTransform.localPosition;
			}
		}
	}
	
	public FCTicket MonsterTicket
	{
		get
		{
			return _aiAgent._monsterTicket;
		}
	}
	public bool IsRanger
	{
		get
		{
			return _aiAgent._isRanger;
		}
	}
	
	//if true, means ac is in slash dumping
	public bool IsInSlowDump
	{
		get
		{
			if(_aniSpeedAgent != null)
			{
				return _aniSpeedAgent.InState;
			}
			return false;
		}
	}
	
	public float GForce
	{
		get
		{
			return _moveAgent._gForce;
		}
	}
	
	public float GEffectTime
	{
		get
		{
			return _moveAgent._gEffectTime;
		}
	}
	
	public Transform GCenter
	{
		get
		{
			return _moveAgent._gCenter;
		}
	}
	
	public bool GEnabled
	{
		get
		{
			return _moveAgent._gEnabled;
		}
	}
	
	public bool EnergyIsFull
	{
		get
		{
			return _energy >= _data.TotalEnergy;
		}
	}
	
	public bool HPIsFull
	{
		get
		{
			return _hitPoint >= _data.TotalHp;
		}
	}
	
	//if true, ac will know, his target is hit by someone now
	protected bool _wasHit = false;
	public bool WasHit
	{
		get
		{
			return _wasHit;
		}
	}
	
	//move function is pause?
	public bool MoveIsPause
	{
		get
		{
			return _moveAgent.IsInPause;
		}
	}
	public bool PauseMotionOneFrame
	{
		get
		{
			return _moveAgent._pauseMotion;
		}
		set
		{
			_moveAgent._pauseMotion = value;
		}
	}
	
	public Vector3 MotionWanted
	{
		get
		{
			return  _moveAgent._motion;
		}
	}
	
	//some times I need to kill the bullets that born by current attack, use this array to do it
	protected List<FCBullet> _bulletsOfCurrentAttack = new List<FCBullet>();
	
	//if true, means monster was critical hit this frame
	//only effect with player
	protected bool _isCriticalHit = false;
	
	public float BodyRadius
	{
		get
		{
			if(_aiAgent != null)
			{
				return _aiAgent._bodyRadius;
			}
			return 0.5f;
		}
	}
	
	public Vector3 _lastPosition = Vector3.zero;
	public Vector3 _currPosition = Vector3.zero;
	
	public bool ACIsMove
	{
		get
		{
			return _moveAgent.IsInMove();
		}
	}
	public bool IsCriticalHit
	{
		set
		{
			bool tmp = value;
			if(!_isCriticalHit && tmp)
			{
				
				CameraController.Instance.StartCameraEffect(EnumCameraEffect.none,EnumShakeEffect.shake1,false);
				//need change the function used here later
				_avatarController._uiHPController.BubbleUpHurtEffect("!", _isCriticalHit, 0, false);
			}
			_isCriticalHit = value;
		}
		get
		{
			return _isCriticalHit;
		}
	}
	
	public List<FCBullet> BulletsOfCurrentAttack
	{
		get
		{
			return _bulletsOfCurrentAttack;
		}
	}
	
	protected AttackHitType _currentWeaponHitType = AttackHitType.HurtNormal;
	
	public AttackHitType CurrentWeaponHitType
	{
		get
		{
			return _currentWeaponHitType;
		}
		set
		{
			_currentWeaponHitType = value;
		}
	}
	
	public void ACShowPickUpMoney(string money, bool symbol=false)
	{
		if(_avatarController != null)
		{
			if(_avatarController._uiHPController != null)
			{
				_avatarController._uiHPController.BubbleUpEffect(money, 1.0f, new Color(1.0f,1.0f,1.0f,1.0f), symbol);
			}
		}
	}
	
	public MoveAgent SelfMoveAgent
	{
		get
		{
			return _moveAgent;
		}
	}
	public bool MoveFollowForward
	{
		get
		{
			return _moveAgent.MoveFollowForward;
		}
		set
		{
			_moveAgent.MoveFollowForward = value;
		}
	}
	public Vector3 MoveDirection
	{
		get
		{
			return _moveAgent._moveDirection;
		}
		set
		{
			_moveAgent._moveDirection = value;
		}
	}
	//current speed before buff
	protected float _currentNormalSpeed = 0;
	
	public float CurrentNormalSpeed
	{
		get
		{
			return _currentNormalSpeed;
		}
	}
	public List<EquipmentIdx> EquipmentIds
	{
		get{return _data.equipList;}	
	}
	//record ac damage point
	private float[] _attackPoints = new float[FCConst.MAX_DAMAGE_TYPE];
	//private float[] _attackPercents = new float[FC_CONST.MAX_DAMAGE_TYPE];
	
	//record ac defense point and damage reduce
	private DefenseInfo _defenseInfo = new DefenseInfo();

	#region //call back
	public delegate void HpChangeMessage(float hpValue);
	public delegate void EnableInputKey(FC_KEY_BIND keyBind,bool beActive);
	public delegate void UpdateInputKeyState(FC_KEY_BIND keyBind, float timeLast, float timeLastPercent);
	public delegate void FUNCTION_AC(ActionController ac);
	public delegate void FUNCTION_AC_BOOL(ActionController ac, bool flag);
	public FUNCTION_AC_BOOL _onHitTarget;
	
	public delegate void FUNCTION_INT(int intValue);
	public FUNCTION_INT _onSkillEnter;
	public FUNCTION_INT _onAttackEnter;
	public FUNCTION_INT _onSkillQuit;
	public FUNCTION_INT _onHit;
	
	public delegate void FUNCTION_INT_INT(int intValue, int intValue1);
	public FUNCTION_INT_INT _onEnergyChange;
	
	public delegate void FUNCTION_FLOAT(float floatValue);
	public FUNCTION_FLOAT _onRunUpdate;
	
	public delegate void FUNCTION_BOOL(bool targ);
	public FUNCTION_BOOL _onShowBody;
	
	public delegate void FUNCTION_STATE(AIAgent.STATE ass);
	public FUNCTION_STATE _onStateQuit;
	
	public HpChangeMessage _hpChangeMessage = null;
	public HpChangeMessage _energyChangeMessage = null;
	public EnableInputKey _enableInputKey = null;
	public UpdateInputKeyState _updateInputKeyState = null;
	#endregion //call back
	public bool _isMonsterLeader = false;

    public FUNCTION_AC _onACDead;
	
	private ActionController _monsterLeader = null;
	
	//if true ,means this attack i belong to a skill
	public bool IsFromSkill
	{
		get
		{
			if(_aiAgent.CurrentSkillID != -1 && _aiAgent.CurrentSkill != null)
			{
				return !(_aiAgent.CurrentSkill.SkillModule._isNormalSkill);
			}
			return false;
		}
	}
	
	public ActionController MonsterLeader
	{
		get
		{
			return _monsterLeader;
		}
		set
		{
			_monsterLeader = value;
		}
	}
	
	public RangerAgent SelfRangerAgent
	{
		get
		{
			return _rangerAgent;
		}
	}
	public int BodyHardness
	{
		get
		{
			if( _aiAgent.HurtAgent != null)
			{
				return _aiAgent.HurtAgent._bodyHardness;
			}
			return 0;
		}
	}
	
	protected bool _canAcceptTimeScale = false;
	
	public bool CanAcceptTimeScale
	{
		get
		{
			return _canAcceptTimeScale && _isPlayerSelf;
		}
		set
		{
			_canAcceptTimeScale = value;
		}
	}
		
	public bool CanAcceptEvent
	{
		get
		{
			return _canAcceptEvent;
		}
		set
		{
			_canAcceptEvent = value;
		}
	}
	public float CurrentAngleSpeed
	{
		set
		{
			_moveAgent.CurrentAngleSpeed = value;
		}
		get
		{
			return _moveAgent.CurrentAngleSpeed;
		}
	}
	
	public float CurrentSpeed	
	{
		set
		{
			_currentNormalSpeed = value;
			_moveAgent.CurrentSpeed = _currentNormalSpeed*_bufferSpeedPercent;
		}
		get
		{
			return _moveAgent.CurrentSpeed;
		}
	}
	
	public float Acceleration
	{
		set
		{
			_moveAgent.Acceleration = value;
		}
		get
		{
			return _moveAgent.Acceleration;
		}
	}
	
	public int AttackID
	{
		get
		{
			return _attackID;
		}
		set
		{
			_attackID = value;
		}
	}
	
	public bool IsAlived
	{
		get
		{
			return _hitPoint > 0;
		}
	}
	
	public int Energy
	{
		get
		{
#if DEVELOPMENT_BUILD || UNITY_EDITOR			
			if(CheatManager.cheatForCostNoEnergy)
			{
				//means no cost
				return 99999;
			}
#endif
			return _energy;
		}
		set
		{
			_energy = value;
		}
	}
	public bool IsSpawned
	{
		get
		{
			return _isSpawned;
		}
	}
	
	public bool IsPlayer
	{
		get
		{
			return _data.isPlayer;
		}
		set
		{
			_data.isPlayer = value;
		}	
	}
	
	public bool IsPlayerSelf
	{
		get
		{
			return _isPlayerSelf;
		}
		set
		{
			_isPlayerSelf = value;
			if(_isPlayerSelf && _data.isPlayer)
			{
				_aiAgent.ControlByPlayer = true;
			}
		}
	}
	//means 2P, 3P, 4P
	public bool IsClientPlayer
	{
		get
		{
			return (!_isPlayerSelf && _data.isPlayer);
		}
	}
	public int HitPoint
	{
		get
		{
			return _hitPoint;
		}
		set
		{
			_hitPoint = value;
		}
	}	
	
	public float HitPointPercents
	{
		get
		{
			return _hitPoint/(float)_data.TotalHp;
		}
	}


    #region CritRate

    public float CritRate
    {
        get
        {
            return _critRate;
        }
        set
        {
            _critRate = value;
        }
    }


    public float CritDamage
    {
        get
        {
            return _critDamage;
        }
        set
        {
            _critDamage = value;
        }
    }

    #endregion

    #region Tattoo

    public float SkillTriggerRate
    {
        get
        {
            return _skillTriggerRate;
        }

        set
        {
            _skillTriggerRate = value;
        }
    }

    public float SkillAttackDamage
    {
        get
        {
            return _skillAttackDamage;
        }

        set
        {
            _skillAttackDamage = value;
        }
    }

    #endregion
    //this attribute does use now
	//FIXME  should use totalEnergy instead of (float)100
	public float EngergyPercents
	{
		get
		{
			return _energy/(float)_data.TotalEnergy;
		}
	}

	public AIAgent AIUse
	{
		get
		{
			return _aiAgent;
		}
	}
	
	public NetworkAgent NetworkAgentUse
	{
		get { return _networkAgent; }
	}
	
	public SuperArmor SuperArmorSelf
	{
		get
		{
			return _aiAgent.SuperArmorSelf;
		}
	}
	
	//normally, faction1 means player, faction2 means monster
	public FC_AC_FACTIOH_TYPE Faction
	{
		get
		{
			return _data.faction;
		}
	}


    protected bool _isInSkillGodDown = false;

    public bool SkillGodDown
    {
        get
        {
            return _isInSkillGodDown;
        }

        set
        {
            _isInSkillGodDown = value;
        }
    }


    protected bool _isSkillGodDownActive = false;

    public bool SkillGodDownActive
    {
        get
        { 
            return _isSkillGodDownActive;
        }

        set
        {
            _isSkillGodDownActive = value;
        }
    }

	
	public ActionController TargetAC
	{
		get
		{
			if(_aiAgent != null)
			{
				return _aiAgent.TargetAC;
			}
			return null;
		}
	}
	
	public float BufferSpeedPercent
	{
		get
		{
			return _bufferSpeedPercent;
		}
		set
		{
			_bufferSpeedPercent = value;
			_moveAgent.CurrentSpeed = _currentNormalSpeed*_bufferSpeedPercent;
		}
	}
	
	public float BufferAniMoveSpeedPercent
	{
		get
		{
			return _bufferAniMoveSpeedPercent;
		}
		set
		{
			_bufferAniMoveSpeedPercent = value;
			if(_aiAgent.CurrentAttack != null)
			{
				SetAniMoveSpeedScale(_aiAgent.CurrentAttack._aniMoveSpeedScale);	
			}
		}
	}
	
	public float BufferAniPlaySpeedPercent
	{
		get
		{
			return _bufferAniPlaySpeedPercent;
		}
		set
		{
			_bufferAniPlaySpeedPercent = value;
			if(_bufferAniPlaySpeedPercent>0.95f)
			{
				float speed = _avatarController.GetAnimationOrgSpeed();
				_avatarController.SetAnimationSpeed(speed);
			}
			else
			{
				float speed = _avatarController.GetAnimationOrgSpeed();
				_avatarController.SetAnimationSpeed(speed * _bufferAniPlaySpeedPercent);
			}
		}
	}


    public float EotPhysicalAttack 
    {
        set
        {
            _eotPhysicalAttack = value;
        }
    }

    public float EotPhysicalDefense
    {
        set
        {
            _eotPhysicalDefense = value;
        }
    }

    public float EotElementalAttack
    {
        set
        {
            _eotElementalAttack = value;
        }
    }

    public float EotElementalResistance
    {
        set
        {
            _eotElementalResistance = value;
        }
    }
	
	public float[] TotalAttackPoints
	{
		get
		{
            float[] _attackPointsCopy = new float[FCConst.MAX_DAMAGE_TYPE];

            for (int i = 0; i < _attackPoints.Length; i++ )
            {
                if (i == 0)
                {
                    _attackPointsCopy[i] = Mathf.RoundToInt(_attackPoints[i] * _eotPhysicalAttack);
                }
                else
                {
                    _attackPointsCopy[i] = Mathf.RoundToInt(_attackPoints[i] * _eotElementalResistance);
                }
            }

            return _attackPointsCopy;
		}
	}

    public float[] BaseAttackPoints
    {
        get
        {
            return _attackPoints;
        }
    }

    public DefenseInfo DefenseAllData
    {
        get
        {
            DefenseInfo _defenseInfoCopy = new DefenseInfo();
            _defenseInfoCopy._defensePercents = _defenseInfo._defensePercents;
            _defenseInfoCopy._defensePoints = new float[FCConst.MAX_DAMAGE_TYPE];

            for (int i = 0; i < _defenseInfoCopy._defensePoints.Length; i++)
            {
                if (i == 0)
                {
                    _defenseInfoCopy._defensePoints[i] = Mathf.RoundToInt(_defenseInfo._defensePoints[i] * _eotPhysicalDefense);
                }
                else
                {
                    _defenseInfoCopy._defensePoints[i] = Mathf.RoundToInt(_defenseInfo._defensePoints[i] * _eotElementalResistance);
                }
            }

            _defenseInfoCopy._criticalDamageResist = _defenseInfo._criticalDamageResist;

            return _defenseInfoCopy;
        }
    }


    public DefenseInfo BaseDefenseAllData
    {
        get
        {
            return _defenseInfo;
        }
    }
	
	public bool CanShakeWhenBeHit
	{
		get
		{
			//return false;
			return _aiAgent.CanShakeWhenBeHit;
		}
	}
	
		
	public float BaseFlyHeight
	{
		set
		{
			_moveAgent._baseFlyHeight = value;
			if(_moveAgent._navAgent != null)
			{
				_moveAgent._navAgent.baseOffset = value;
			}
		}
		get
		{
			if(_moveAgent._navAgent != null)
			{
				return _moveAgent._baseFlyHeight;
			}
			else
			{
				return 0;
			}
		}
	}
	
	public ActionController FireTarget
	{
		get
		{
			return _fireTarget;
		}
		set
		{
			_fireTarget = value;
		}
	}
	#endregion

    #region PassiveAgent

    public PassiveSkillAgent PassiveSkillAgent
    {
        get
        {
            return _passiveSkillAgent;
        }
    }

    #endregion

    #region data

    //data need sync to client
	public class DATA_SYNC
	{
		public Vector3 _position;
		public Vector3 _rotation;
		public int _hitPoint;
		protected ActionController _owner;
		//decide how often the data from host need be sync to self
		protected float[] _timeCounters;
		protected FCCommand _fastCommand;
		
		public float[] TimeCounters
		{
			get
			{
				return _timeCounters;
			}
		}
		public void Init(ActionController owner)
		{
			_timeCounters = new float[(int)FCConst.DATA_SYNC_TYPE.NUM];
			for(int i =0;i<_timeCounters.Length;i++)
			{
				_timeCounters[i] = 0;
			}
			_owner = owner;
			_fastCommand = new FCCommand();
			_fastCommand.Set(FCCommand.CMD.STOP,null,FCCommand.STATE.RIGHTNOW,true);
		}
		
		
		
		public void Update()
		{
			if(_owner.IsSpawned)
			{
				for(int i =0;i<_timeCounters.Length;i++)
				{
					if(_timeCounters[i]>=0)
					{
						_timeCounters[i] -= Time.deltaTime;
						if(_timeCounters[i]<=0)
						{
							if(GameManager.Instance.IsMultiplayMode){
								_timeCounters[i] = FCConst.DATA_REFRESH_TIME[i];
							}else{
								_timeCounters[i] = 0.0f;
							}
							
							_fastCommand._cmd = FCCommand.CMD.INVALID;
							
							switch((FCConst.DATA_SYNC_TYPE)i)
							{
								case FCConst.DATA_SYNC_TYPE.POSITION:
								{	
									if(_position != Vector3.zero)
									{
										if( (_owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.HURT 
											|| _owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.ATTACK
											//|| _owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.IDLE
											)
											&& !_owner.NetworkAgentUse.IsIgnoreSyncEnabled 
										)
										{
											//Debug.Log("distance pos = " + Vector3.Distance(_position , _owner._currPosition));
											if(Vector3.Distance(_position , _owner._currPosition) > 3.2f ){												
												//force to 
												_fastCommand._cmd = FCCommand.CMD.POSITION;
											}
											else{
												//lerp to 
												_fastCommand._cmd = FCCommand.CMD.POSITION_LERP_TO;
											}
										}
										else if(_owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.RUN){//move to
											_fastCommand._cmd = FCCommand.CMD.MOVE_TO_POINT;
										}
										
										_fastCommand._param1 = _position;
										_fastCommand._isHost = false;
										if( CommandManager.Instance.SendFast(ref _fastCommand,_owner))
										{
											_position = Vector3.zero;
										}
									}
								}break;
								case FCConst.DATA_SYNC_TYPE.ROTATION:
								{	
									if(_rotation != Vector3.zero)
									{
										if(_owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.HURT 
											|| _owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.ATTACK
											|| _owner.NetworkAgentUse.ClientAICurrStateID == AIAgent.STATE.IDLE)
										{	//force to 
											_fastCommand._cmd = FCCommand.CMD.ROTATE;
											 Quaternion rotation = Quaternion.Euler(_rotation);
											_fastCommand._param1 = rotation * Vector3.forward;
											_fastCommand._isHost = false;
											if(CommandManager.Instance.SendFast(ref _fastCommand,_owner))
											{
												_rotation = Vector3.zero;
											}
										}									
									}
								} break;
							}
						}
					}
	
				}
			}
		}
		
	}
	
	//all attributes original data
	private AcData _data;
	public AcData Data
	{
		get{return _data;}
		set{_data = value;}
	}
		
	public DATA_SYNC _dataSync;
	
	public DATA_SYNC DataSync
	{
		get
		{
			return _dataSync;
		}
	}
	#endregion	




    protected UnityEngine.Object InitAgent(string typeName)
	{
		UnityEngine.Object obj = _agent.GetComponent(typeName);
		if(obj!=null)
		{
			((FCAgent)obj).Init(this);
		}
		return obj;
	}
	protected override void Awake()
	{
		base.Awake();
		ObjectID.ObjectType = FC_OBJECT_TYPE.OBJ_AC;
		_fastCommand = new FCCommand();
		_fastCommand.Set(FCCommand.CMD.STOP,ObjectID,FCCommand.STATE.RIGHTNOW,true);
		_isSpawned = false;
		_dataSync = new DATA_SYNC();
		_dataSync.Init(this);
	}
	
	//init all agent, anget can be null
	public void Init()
	{
		_moveAgent = InitAgent(MoveAgent.GetTypeName()) as MoveAgent;
		_attackAgent = InitAgent(AttackAgent.GetTypeName()) as AttackAgent;
		_aiAgent = InitAgent(AIAgent.GetTypeName()) as AIAgent;
		_discoverAgent=  InitAgent(DiscoverAgent.GetTypeName()) as DiscoverAgent;
		_equipsAgent=  InitAgent(EquipmentsAgent.GetTypeName()) as EquipmentsAgent;
		_rangerAgent = InitAgent(RangerAgent.GetTypeName()) as RangerAgent;
		_networkAgent = InitAgent(NetworkAgent.GetTypeName()) as NetworkAgent;
		_avatarController = ThisObject.GetComponent<AvatarController>();
		_threatAgent = InitAgent(ThreatAgent.GetTypeName()) as ThreatAgent;
		_effectAgent = InitAgent(EffectAgent.GetTypeName()) as EffectAgent;
		_lootAgent = InitAgent(LootAgent.GetTypeName()) as LootAgent;
		_aniSpeedAgent = InitAgent(AniSpeedAgent.GetTypeName()) as AniSpeedAgent;
		_passiveSkillAgent = InitAgent(PassiveSkillAgent.GetTypeName()) as PassiveSkillAgent;
		_avatarController.EnableAnimationPhysics();
	}
	
	public int CharacterStrength
	{
		get
		{
			return _aiAgent.FinalHitStrength;
		}
	}
	
	//high mass means self cant be pust by low mass object
	public void ACSetAvoidPriority(int mass)
	{
		_moveAgent.SetAvoidPriority(mass);
	}

	
	#region command function
	public virtual void ACSpawn(Vector3 point, float rotationY)
	{
		if(_isSpawned)
		{
			//one enemy show call this function once when he is alived
            Debug.LogError(ThisObject.name + " has spawned already!");
		}
		
		TimeScaleListener tsl = GetComponent<TimeScaleListener>();
		_timeScaleListener = tsl;
		if(tsl != null)
		{
			tsl.Owner = this;
		}

		ActionControllerManager.Instance.Register(this);
		ThisTransform.localPosition = point;

		ThisObject.layer = (int)_data.faction;
		
		//equip and refresh ports on weapons
        EquipAllEquipments();
		_avatarController.RefreshDemagePorts(ThisObject);
		
		int level = _data.Level; //enemy level = 0;
        EnumRole role = EnumRole.NotSelected;
		if (IsPlayer)
		{
			level = PlayerInfo.Instance.CurrentLevel;
			role = PlayerInfo.Instance.Role;
		}

        if (_passiveSkillAgent != null)
        {
            _passiveSkillAgent.InitSkillData();
        }

        UpdateDataFromLevelData(level, role, false);
	
		if(_moveAgent != null)
		{
			if(_data.angleSpeed == 0)
			{
				//FIXME  this is a exp num
				_data.angleSpeed = 720;
			}
			CurrentSpeed = _data.TotalMoveSpeed;
			_moveAgent.CurrentAngleSpeed = _data.angleSpeed;
			if(_moveAgent._navAgent != null)
			{
				_moveAgent._navAgent.enabled = true;
			}
			Vector3 direction = Quaternion.Euler(0, rotationY, 0) * ThisTransform.forward;
            ACRotateToDirection(ref direction, true);
        }
        
        _energy = _data.energy;
		if(_energy <=0)
		{
			//FIXME  this is a exp num
			//monster should also have energy attribute
            _energy = (int)DataManager.Instance.CurGlobalConfig.getConfig("mpMax"); ;
            _data.energy = (int)DataManager.Instance.CurGlobalConfig.getConfig("mpMax"); 
		}
		
		//must put code befroe init skill data
		if(_rangerAgent != null)
		{
			_rangerAgent.RefreshAllPorts();
		}
		
		if(_attackAgent != null)
		{
			_attackAgent.InitSkillData(_aiAgent);
			
			_fastCommand._cmd = FCCommand.CMD.INIT_ATTACK_INFO;
			_aiAgent.HandleCommand(ref _fastCommand);
		}
		if((_data.isPlayer &&_aiAgent != null) && _isPlayerSelf)
		{
			_aiAgent.ControlByPlayer = true;
		}
		if(_threatAgent != null)
		{
			_threatAgent.StartToRun();
		}
		_aiAgent.ActiveAI();
		ACSetAvoidPriority(_aiAgent._bodyMass);
		_avatarController.SetHPDisplay(1, 0, false, false);
		_avatarController.SetEnergyDisplay(1);

		//allocate network id and save into object manager
		//change from network mnanger
		if (_data.isPlayer)
		{
			//hero
			ObjectID.NetworkId = FCConst.k_network_id_hero_start + _instanceID;
			
			//set avatar name for players
			if(PhotonNetwork.room == null)
			{
				//single play
				_avatarController.SetTextDisplay(PlayerInfo.Instance.DisplayNickname);
			}
			else
			{
				//multi
				MatchPlayerProfile thisPlayerProfile = MatchPlayerManager.Instance.GetMatchPlayerProfile(_instanceID);
				if (thisPlayerProfile != null)
				{
					_avatarController.SetTextDisplay(thisPlayerProfile._playerInfo.DisplayNickname);
				}
			}
		}
		else
		{
			//enemy
			ObjectID.NetworkId = FCConst.k_network_id_enemy_start + _instanceID;
		}
		foreach(FCWeapon ewp in _equipsAgent.WeaponList)
		{
			if(ewp != null)
			{
				ewp.GetAttackInfo()._attackPoints = _attackPoints;
				//ewp.GetAttackInfo()._attackPercents = _attackPercents;

                ewp.GetAttackInfo()._criticalChance = CritRate;
                ewp.GetAttackInfo()._criticalDamage = CritDamage;
                ewp.GetAttackInfo()._skillTriggerChance = SkillTriggerRate;
                ewp.GetAttackInfo()._skillAttackDamage = SkillAttackDamage;
			}
		}
		
		InitDirectionIndicator();
		if(IsPlayer)
		{
			ACSetWalkLayer(FCConst.WALKMASK_ALL);
		}
		else
		{
			ACSetWalkLayer(FCConst.WALKMASK_NORMAL);
		}
		_moveAgent._onMoveUpdate = WhenMoveUpdate;
		//CalculateLoot();
		
		if(_isPlayerSelf)
		{
			_moveAgent._navAgent.autoBraking = false;
			_moveAgent._navAgent.autoRepath = false;
			_countAgent = GetComponentInChildren<CountAgent>();
			if(_countAgent != null)
			{
				_countAgent.Init(this);
				_onSkillEnter += _countAgent.OnSkillEnter;
				_onSkillQuit += _countAgent.OnSkillEnd;
				_onEnergyChange += _countAgent.EnergyCount;
				_onHit += _countAgent.OnHit;
			}
			
		}
		//this line must at the end of the function
		if(GameManager.Instance.IsPVPMode && IsClientPlayer)
		{
			ThisObject.layer = FCConst.LAYER_NEUTRAL_2;
		}
		_isSpawned = true;

	}
	public void ACMove(ref Vector3 point)
	{
		//move ac
		_moveAgent.GotoPoint(ref point,true);
	}
	
	public void ACMoveByDirection(float time)
	{
		//move ac
		_moveAgent.GoByDirection(time);
	}
	
	public void ACMoveToDirection(ref Vector3 direction,int angleSpeed ,float timeLast)
	{
		//move ac
		_moveAgent.GoTowardToDirection(ref direction, angleSpeed, timeLast);
	}
	
	public void ACMoveToDirection(ref Vector3 direction,int angleSpeed)
	{
		//move ac
		_moveAgent.GoTowardToDirection(ref direction,angleSpeed);
	}
	
	public void ACStop()
	{
		//stop ac
		_moveAgent.Stop();
	}
	

	
	public void ACActiveNavmeshPath(bool beActive)
	{
		if(!beActive)
		{
			_moveAgent._navAgent.Stop(true);
		}
	}
	
	public void ACClearPositionSync()
	{
		_dataSync._position = Vector3.zero;
		_dataSync._rotation = Vector3.zero;
	}
	
	public void ACSwtichWeapon()
	{
		
	}
	
	public void SetAniMoveSpeedScale(float ss)
	{
		_avatarController.DeltaSpeedScale = ss*_bufferAniMoveSpeedPercent;
	}
	
	//if 0, means from hurt
	//if 1, means from attack
	//if 2, means from potion
	//if -1, means others
	public bool CostEnergy(int delta, int engSource)
	{
		delta = 0;
#if DEVELOPMENT_BUILD || UNITY_EDITOR		
		if(CheatManager.cheatForCostNoEnergy)
		{
			return true;
		}
#endif	
		int eng = _energy;
		bool ret = false;
		if(_aiAgent.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2)
			|| _aiAgent.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.COST_NO_ENERGY))
		{
			if(delta <0)
			{
				delta = 0;
			}
		}
		if(_aiAgent.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_NO_ENERGY))
		{
			if(delta>0)
			{
				delta = 0;
			}
		}
		if((_energy + delta)<=0)
		{
			ret = true;
		}
		_energy = Mathf.Clamp(_energy + delta,0,_data.TotalEnergy);
		
		_avatarController.SetEnergyDisplay((float)(_energy)/(float)_data.TotalEnergy);
		if(_isPlayerSelf)
		{
			if (delta < 0)
			{
				BattleSummary.Instance.RageUnleashed-=delta;
			}
			
			if (_energyChangeMessage != null)
			{
				_energyChangeMessage((float)(_energy)/(float)_data.TotalEnergy);
			}
			_aiAgent.KeyAgent.UpdateKeyState();
		}
		else
		{
			ret = true;
		}
		if(_onEnergyChange != null)
		{
			if(engSource == 0
				|| engSource == -1)
			{
				_onEnergyChange(_energy-eng, -1);
			}
			else if(engSource == 1)
			{
				_onEnergyChange(_energy-eng, _aiAgent.CurrentSkillID);
			}
			else if(engSource == 2)
			{
				_onEnergyChange(_energy-eng, -2);
			}
		}
		return ret;
	}
	
	public void ACPlayAnimation(AniSwitch ast)
	{
		int idxValue = (int)ast._aniIdx;
		if(idxValue < 0)
		{
			return;
		}

		idxValue = idxValue%1000;
		switch(idxValue/10)
		{
		case 0:
			ast._type = AvatarController.AnimType.AnimType_Normal;
			ast._parameter = idxValue;
			break;
		case 1:
			ast._type = AvatarController.AnimType.AnimType_Move;
			ast._parameter = idxValue - 10;
			break;
		case 2:
		case 3:
			ast._type = AvatarController.AnimType.AnimType_Attack;
			ast._parameter = idxValue - 20;
			break;
		case 4:
		case 5:
			ast._type = AvatarController.AnimType.AnimType_SpecialAttack;
			ast._parameter = idxValue - 40;
			break;
		case 6:
		case 7:
			ast._parameter = idxValue - 60;
			ast._type = AvatarController.AnimType.AnimType_Charge;
			break;
		case 8:
		case 9:
		case 10:
			ast._parameter = idxValue - 80;
			ast._type = AvatarController.AnimType.AnimType_Hurt;
			break;
		default:
			ast._type = AvatarController.AnimType.AnimType_Normal;
			ast._parameter = 0;
            Debug.LogError("wrong ani " + ast._aniIdx.ToString());
			break;
		}
		_avatarController.SetAnimation(ast);

		if(_bufferAniPlaySpeedPercent<0.95f)
		{
			float speed = _avatarController.GetAnimationOrgSpeed();
			_avatarController.SetAnimationSpeed(speed*_bufferAniPlaySpeedPercent);
		}
		
	}
	
	public void EquipAllEquipments()
	{
		if(_equipsAgent != null)
		{
			_equipsAgent.EquipAllModule();
            //_equipsAgent.EquipmentAllData();
			_equipsAgent.RefreshWeaponList();
		}
	}
	public void ACActiveWeapon(FC_EQUIPMENTS_TYPE flag,bool toActive)
	{
		//Debug.LogError("ACActiveWeapon"+flag+" bool+"+toActive);
		if(toActive == false || _aiAgent.CurrentAttack != null)
		{
			foreach(FCWeapon ewn in _equipsAgent.WeaponList)
			{
				FCWeapon.ReleaseBlade(ewn,toActive,flag,_aiAgent.CurrentAttack);
			}
		}
	}
	
	public void ACApplyWeaponTo(Transform slot,int weaponIndx = 0)
	{
		if(slot == null)
		{
			_weaponInFly = false;
		}
		else
		{
			_weaponInFly = true;
		}
		
		FC_EQUIPMENTS_TYPE eet = _aiAgent._defaultWeaponType;
		if(eet == FC_EQUIPMENTS_TYPE.WEAPON_LIGHT)
		{
			eet = (FC_EQUIPMENTS_TYPE)((int)eet + weaponIndx);
		}
		foreach(FCWeapon ewn in _equipsAgent.WeaponList)
		{
			ewn.SwitchWeaponTo(slot, eet);
		}
	}
	
	public void SwitchWeaponTo(EnumEquipSlot slot, FC_EQUIPMENTS_TYPE flag)
	{
		_weaponInFly = false;
		if(slot == EnumEquipSlot.weapon_hang)
		{
			_hangWeaponBack = true;
		}
		else
		{
			_hangWeaponBack = false;
		}
		foreach(FCWeapon ewn in _equipsAgent.WeaponList)
		{
			ewn.SwitchWeaponTo(slot, flag);
		}
	}
	
	public void ACAttack()
	{
		//ac prepare to attack
		_attackAgent.StartAttack();
	}
	
	public void ACAttackEnd()
	{
		//stop ac attack will
		_attackAgent.StopAttack();
	}
	
	public void ACHurt(FCObject ewt)
	{
		_fastCommand._param1 = ewt;
		_fastCommand._cmd = FCCommand.CMD.HURT;
		_fastCommand._param2 = this;
		CommandManager.Instance.SendFastToSelf(ref _fastCommand);
	}
	
	public void ACBeginToSearch(bool useMaxPower)
	{
		if(_discoverAgent != null)
		{
			_discoverAgent.BeginToDiscover(useMaxPower);
		}
	}
	public void ACDead()
	{
		_isSpawned = false;
		//_moveAgent.CloseAll();
		ActionControllerManager.Instance.UnRegister(this);
		LevelManager.Singleton.DeadEnemy(_instanceID, !_aiAgent._deadWithExplode);
	}
	
	public FCSkillMap[] ACGetAttackTable()
	{
		if(_attackAgent != null)
		{
			return _attackAgent._skillMaps;
		}
		return null;
	}
	
	public AttackBase ACGetAttackByName(string attName) 
	{
		AttackBase ab = null;
		if(_attackAgent != null)
		{
			ab = _attackAgent.GetAttack(attName);
		}
		return ab;
	}
	
	public AttackBase ACGetAttack(int id,int aiLevel,int comboIdx)
	{
		AttackBase ab = null;
		if(_attackAgent != null)
		{
			if(comboIdx == -1)
			{
				if(_attackAgent.GetSkillConfig(id, aiLevel).WithSpecCombo)
				{
					comboIdx = _attackAgent.GetSkillConfig(id, aiLevel).ComboHitValue;
					_attackAgent.GetSkillConfig(id, aiLevel).WithSpecCombo = false;
				}
				else
				{
					ACClearAttackCount(id,aiLevel);
					comboIdx = 0;
				}
			}
			ab = _attackAgent.GetAttack(id,aiLevel,comboIdx);
			if(ab != null)
			{
				ab.AttCons = _attackAgent.GetAttackConditions(id,aiLevel,comboIdx);
			}
		}
		return ab;
	}
	
	public AttackBase ACGetCurrentAttack()
	{
		return _aiAgent.CurrentAttack;
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
		_hpChangeMessage = null;
		_onHitTarget = null;
		_onRunUpdate = null;
		_onShowBody = null;
		_onStateQuit = null;
		_onSkillEnter = null;
		_onSkillQuit = null;
		_onEnergyChange = null;
		_onHit = null;
	}
	public int ACIncreaseAttackCount(int attackID,int aiLevel,AttackBase ab)
	{
		int cid = -1;
		if(_attackAgent != null)
		{
			if(ab != null && ab.NextAttackIdx >=0)
			{
				if(ab.NextAttackIdx >255)
				{
					cid = -1;
				}
				else
				{
					_attackAgent.SetAttackCombo(attackID,aiLevel,ab.NextAttackIdx);
					cid = ab.NextAttackIdx;
					ab.NextAttackIdx = -1;
				}

			}
			else
			{
				cid = _attackAgent.IncreaseAttackCombo(attackID,aiLevel);
			}
		}
		return cid;
	}
	
	public int ACGetAttackComboCount(int attackID,int aiLevel)
	{
		if(_attackAgent != null)
		{
			return _attackAgent.GetAttackCombo(attackID,aiLevel);
		}
		return -1;
	}
	
	public void ACClearAttackCount(int attackID,int aiLevel)
	{
		if(_attackAgent != null)
		{
			_attackAgent.ClearAttackCombo(attackID,aiLevel);
		}
	}
	
	public void ACRefreshDataWithPassive()
	{
        _attackPoints[0] = Data.TotalAttack;
        _defenseInfo._defensePoints[0] = Data.TotalDefense;
	}
	
	public void ClearWeaponList()
	{
		if(_equipsAgent != null)
		{
			_equipsAgent.WeaponList.Clear();
		}
	}
	public bool RegisterToWeaponList(FCWeapon ewn)
	{
		if(_equipsAgent != null)
		{
			_equipsAgent.WeaponList.Add(ewn);
			return true;
		}
		return false;
	}
	
	public void ACIncreaseThreat(int increaseValue,bool isFromLocal,ActionController target)
	{
		if(!IsPlayer && target.IsPlayer)
		{
			if(isFromLocal)
			{
				//send threat change to others
				CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_THREAT,  
					ObjectID,
					ThisTransform.localPosition,
					increaseValue,
					FC_PARAM_TYPE.INT,
					target.ObjectID.NetworkId,
					FC_PARAM_TYPE.INT,
					null,
					FC_PARAM_TYPE.NONE);				
			}
			if(_threatAgent != null && increaseValue >0)
			{
				_threatAgent.Increase(increaseValue,target);
			}
		}

	}
	
	public void ACIncreaseHP(int increaseValue)
	{
		_hitPoint += increaseValue;
		if(_hitPoint >= _data.TotalHp)
		{
			_hitPoint = _data.TotalHp;
		}
		if(_hpChangeMessage != null)
		{
			_hpChangeMessage(HitPointPercents);
		}
		_hpChangeMessage((float)increaseValue/_data.TotalHp);	
	}
	
	public void ACHandleHurt(AttackHitType eht , bool isCritical , OBJECT_ID attackerObjID , Vector3 hitDirection , float effectTime,
							bool isFromLocal,bool isFrom2P,bool isCriticalHit , int attackerCharacterStrength)
	{
		if(!IsAlived) {
		//	Debug.Log("Object ID " + (int)ObjectID + "(network id:" + ObjectID.NetworkId + ")'s HP will reduced by " + reduceValue + ". but it's not alived");
		}
		Vector3 hd = Vector3.zero;
		hd.x = hitDirection.x;
		hd.z = hitDirection.y;
		_aiAgent.HurtAgent.HitDirection = hd;
		_aiAgent.HurtAgent.NextHitType = eht;
		_aiAgent.HurtAgent.HitStrength = attackerCharacterStrength;
		_aiAgent.HurtAgent.EffectTime = effectTime;
		if(eht != AttackHitType.None)
		{
			_aiAgent.SetNextState(AIAgent.STATE.HURT);
		}

	}
	
	public void ACReduceHP(int reduceValue , int currentHp ,bool isFromLocal,bool isFrom2P,bool isCriticalHit,bool isFromSkill)
	{
		if(!IsAlived) {
			Debug.Log("Object ID " + (int)ObjectID + "(network id:" + ObjectID.NetworkId + ")'s HP will reduced by " + reduceValue + ". but it's not alived");
		}
		
		if(HitPoint != currentHp ) _hitPoint = currentHp;
		
		ACReduceHP(reduceValue , isFromLocal ,isFrom2P ,isCriticalHit , isFromSkill);
	}
	
	public void ACReduceHP(int reduceValue ,bool isFromLocal,bool isFrom2P,bool isCriticalHit,bool isFromSkill, bool isFromDot = false)
	{
		//return;
		//reduceValue = 0;
		if(reduceValue == 0)
		{
			if(_onHit != null)
			{
				return;
				//_onHit((int)LevelManager.VarNeedCount.ATTACK_IGNORE);
			}
			
		}
		int chp = _hitPoint;
		if(!IsAlived) {
			Debug.Log("Object ID " + (int)ObjectID + "(network id:" + ObjectID.NetworkId + ")'s HP will reduced by " + reduceValue + ". but it's not alived");
		}
//		int ret = 0;
		
		//force: must reduce HP, else will determin from specific logic
		if(GameManager.Instance.IsMultiplayMode && isFromLocal && isFrom2P)
		{
			reduceValue = 0;
		}
		_wasHit = true;
		_avatarController.HurtColor(0.2f);
		if(_isPlayerSelf && TutorialManager.Instance.isTutorialHpOrEnergy)
		{
			if(reduceValue > 0)
			{
				reduceValue = 0;
				if(_onHit != null)
				{
					_onHit((int)LevelManager.VarNeedCount.ATTACK_IGNORE);
				}
			}
		}
		//todo: a logic branch should not be determined by a numerical option
		if(reduceValue != 0)
		{
			if(IsPlayerSelf 
				&& LevelManager.Singleton.IsTutorialLevel()
				&& HitPointPercents <= 0.3f
				&& reduceValue > 0 )
			{
				reduceValue = 0;
			}
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			if(!IsPlayer)
			{
				if(CheatManager.dpsCountEnabled)
				{
					CheatManager.damageTotal += reduceValue;
				}
			}
#endif
			if(_aiAgent.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_NAMEKIAN) && isFromLocal)
			{
				//should not change code here. NAMEKIAN need active in any mode
				//if(GameManager.Instance.IsMultiplayMode ) 
				reduceValue = 0;
				if(_onHit != null)
				{
					_onHit((int)LevelManager.VarNeedCount.ATTACK_IGNORE);
				}
			}
			//increase energy
			if(_aiAgent._haveWayToGetEnergy)
			{
				CostEnergy(_aiAgent.GetEnergyByHurt((float)reduceValue/_data.TotalHp), 0);
			}
			else
			{
				CostEnergy(EnergyGain.GetEnergyByHurt((float)reduceValue/_data.TotalHp), 0);
			}
			
			if(isFromLocal)
			{
				//send hp reduce to others

				CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_HURT_HP,  
					ObjectID,
					ThisTransform.localPosition,
					reduceValue,
					FC_PARAM_TYPE.INT,
					HitPoint,
					FC_PARAM_TYPE.INT,
					null,
					FC_PARAM_TYPE.NONE);				
			}
			_hitPoint -= reduceValue;
//			ret = reduceValue;
			if (_hpChangeMessage != null)
			{
				int rp = reduceValue;
				
				if(reduceValue>_hitPoint)
				{
					rp = _hitPoint;
				}
				_hpChangeMessage(-(float)rp/_data.TotalHp);
			}
			_aiAgent.HpIsChanged(reduceValue);
			_avatarController.SetHPDisplay((float)_hitPoint/_data.TotalHp, reduceValue, isCriticalHit, isFromSkill);
			if(!IsAlived)
			{
			
				if(!IsPlayer)
				{
					Loot();
				}
				
				if(isFromLocal)
				{
					//send die to others
					CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.DIE_NORMAL,  
						ObjectID,
						ThisTransform.localPosition,
						null,
						FC_PARAM_TYPE.NONE,
						null,
						FC_PARAM_TYPE.NONE,
						null,
						FC_PARAM_TYPE.NONE);				
				}

			}
		}
		
		if(!IsAlived && IsPlayerSelf)
		{
			//go to dead
	
			//clear effects such as dizzy, freeze
			ClearEffect();
			if(_aiAgent.AIStateAgent.CurrentStateID != AIAgent.STATE.HURT
				|| _aiAgent.AIStateAgent.CurrentStateID != AIAgent.STATE.DEAD)
			{
				_aiAgent.GotoDead();
			}
		}
		if(_onHit != null && chp - _hitPoint >0 && !isFromDot)
		{
			_onHit((int)LevelManager.VarNeedCount.ATTACK_EFFECT);
		}
		return;
	}
	
	
	public void ClearEffect()
	{
		CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DIZZY, 
			_avatarController, -1);
	}
	
	public void ACRevertToDefalutMoveParams()
	{
		CurrentSpeed = _data.TotalMoveSpeed;
		_moveAgent.CurrentAngleSpeed = _data.angleSpeed;
	}
	void OnTriggerEnter_Debug(Collider other)
	{
		if(_isSpawned)
		{
			MessageReciever msgr = other.GetComponent<MessageReciever>();
			if(msgr != null && msgr._parent != null)
			{
				ACHurt(msgr._parent);
			}
		}

	}

	public void ACRotateToDirection(ref Vector3 direction , bool needNow)
	{
		_moveAgent.RotateToDirection(ref direction,-1,true, needNow);
	}
	
	//when direction mode is unlock,ACSetDirection will not effect transform.forward
	public void ACSetDirection(ref Vector3 direction)
	{
		_moveAgent.SetDirection(ref direction);
	}
	
	public void ACFollowSpeedDirection(ref Vector3 direction,float timeLast)
	{
		if(timeLast<0)
		{
			_moveAgent.SetDirection(ref direction);
		}
		else
		{
			_moveAgent.RotateToDirection(ref direction,timeLast,false, false);
		}
	}
	
	public void ACKillBulletByFirePort(int firePortIdx)
	{
		RangerAgent.KillBulletWithFirePort(firePortIdx,_rangerAgent);
	}
	public void ACRestoreToDefaultSpeed()
	{
		CurrentSpeed = _data.TotalMoveSpeed;
	}
	public void SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG rf)
	{
		_moveAgent.RotateFlag = rf;
	}
	public override bool HandleCommand(ref FCCommand ewd)
	{
		bool ret = false;
		//Todo if command > 10000 ,it should be a network command
		switch(ewd._cmd)
		{
		case FCCommand.CMD.POTION_HP:
			if(IsAlived)
			{
				ACEatPotion((float)ewd._param1,(float)ewd._param2,FCConst.POTION_TIME,FCPotionType.Health);
				ret = true;
			}
			break;
		case FCCommand.CMD.POTION_ENERGY:
			if(IsAlived)
			{
				ACEatPotion((float)ewd._param1,(float)ewd._param2,FCConst.POTION_TIME,FCPotionType.Mana);
				ret = true;
			}
			break;
		case FCCommand.CMD.POSITION_LERP_TO:
			if(IsAlived)
			{
				_networkAgent.SetPosLerpTo((Vector3)ewd._param1);
				return true;
			}
			break;
		case FCCommand.CMD.POSITION:
			if(IsAlived)
			{
				this.ThisTransform.localPosition = (Vector3)ewd._param1;
				return true;
			}
			break;
		default:
			if( _aiAgent.HandleCommand(ref ewd))
			{
				ret = true;
			}
			else if (_networkAgent != null && _networkAgent.HandleCommand(ref ewd))
			{
				ret = true;
			}	
			break;
		}
		return ret;
	}
	
	public void ACFire(int portIdx,float lifeTime = -1)
	{
		if(portIdx>=0 && _rangerAgent._firePorts[portIdx]._fireCount>0)
		{
			_aiAgent.BulletIsFire();
			RangerAgent.FireBullet(_rangerAgent._firePorts, portIdx, lifeTime, this);
		}
	}
	
	public int ACGetFirePortIndex(string portName)
	{
		int ret = -1;
		if(_rangerAgent != null)
		{
			int i = 0;
			foreach(RangerAgent.FirePort fp in _rangerAgent._firePorts)
			{
				if(fp._portName == portName)
				{
					ret = i;
					break;
				}
				i++;
			}
		}
		return ret;
	}
	
	public RangerAgent.FirePort ACGetFirePort(int firePortIdx)
	{
		RangerAgent.FirePort ret = null;
		if(_rangerAgent != null && firePortIdx >=0 && firePortIdx < _rangerAgent._firePorts.Length)
		{
			ret = _rangerAgent._firePorts[firePortIdx];
		}
		return ret;
	}
	public void ACRotateTo(Vector3 direction,float time,bool beForce)
	{
		_moveAgent.RotateToDirection(ref direction,time,beForce, false);
	}
	
	public void ACSTopRotate()
	{
		_moveAgent._rotateDirection = ThisTransform.forward;
	}
	
	public void ACRotateTo(Vector3 direction,float time,bool beForce, bool needNow)
	{
		if(needNow && _aiAgent.KeyAgent != null)
		{
			_aiAgent.KeyAgent._directionWanted = direction;
		}
		_moveAgent.RotateToDirection(ref direction,time,beForce, needNow);
	}
	
	public void ACRecoverAll()
	{
		_hitPoint = _data.TotalHp;
		CostEnergy(_data.TotalEnergy, -1);
		_avatarController.SetHPDisplay(1, 0, false, false);
		_avatarController.SetEnergyDisplay(1);
		if(_hpChangeMessage != null)
		{
			_hpChangeMessage(1.0f);
		}
		if(_energyChangeMessage != null)
		{
			_energyChangeMessage(1.0f);
		}
	}
	
	public void ACRevive()
	{
		_fastCommand._cmd = FCCommand.CMD.REVIVE;
		CommandManager.Instance.SendFastToSelf(ref _fastCommand);
	}
	public void ACSlowDownAttackAnimation(int sharpness,int targetHardness,int hitTargetCount, float dumpReduce)
	{
		if(_aniSpeedAgent != null)
		{
			HardnessAndSharpness.SetAniSpeed(sharpness,_aiAgent.HurtAgent._bodyHardness,_aniSpeedAgent,hitTargetCount, dumpReduce);
		}
	}
	
	public void ACSlowDownTime(float timeLast, float timeScale)
	{
		if(_timeScaleListener != null)
		{
			_timeScaleListener.SlowDownTimeScale(timeLast, timeScale);
		}
	}
	public void ReturnBulletToPool(FCBullet eb)
	{
		_rangerAgent.ReturnToPool(eb);
	}
	
	public FCBullet GetBulletFromPool(string bulletName)
	{
		return _rangerAgent.GetFromPool(bulletName);
	}

	public void AniActiveWeapon()
	{
		if(_aiAgent.CurrentAttack != null)
		{
			_aiAgent.CurrentAttack.AttackActive(true);
		}
	}
	
	public void AniFireBullet(int idx)
	{
		if(_aiAgent.CurrentAttack != null)
		{
			_aiAgent.CurrentAttack.AniBeforeBulletFire();
			ACFire(_aiAgent.CurrentAttack.FirePortIdx);
			
		}
		//_rangerAgent.FireBullet(idx);
	}
	
	public void AniPlayEffect(int idx)
	{
		_avatarController.PlayEffect(idx);
	}
	
	public void ACIsHitTarget(ActionController ac, int sharpness, bool gainEnergy, bool fromSkill)
	{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
		if(IsPlayerSelf)
		{
			if(CheatManager.dpsCountEnabled)
			{
				CheatManager.hitCount++;
			}
			ComboCounter.Instance.CurrentCount ++;
		}
#endif

		/*if(_isCriticalHit)
		{
			_isCriticalHit = false;
			CameraController.Instance.StartCameraEffect(EnumCameraEffect.none,EnumShakeEffect.shake1,false);
		}*/
		if(_aiAgent.CurrentAttack != null)
		{
			if(gainEnergy || _aiAgent.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_ENERGY_BY_ATTACK))
			{
				if(_aiAgent._haveWayToGetEnergy)
				{
					CostEnergy(_aiAgent.GetEnergyByAttack(), 1);
				}
				else
				{
					// > 0 ,means energy will increase
                    CostEnergy(EnergyGain.GetEnergyByAttack(ComboCounter.Instance.CurrentCount), 1);
				}
	
			}
			_aiAgent.CurrentAttack.IsHitTarget(ac, sharpness);
		}
		if(_onHitTarget != null)
		{
			_onHitTarget(ac, fromSkill);
		}
		if(_aiAgent.RageSystemIsActive && ac != null && ac.Faction != Faction)
		{
			_aiAgent.IncreaseRage(RageAgent.RageEvent.HIT_TARGET, FCWeapon.WEAPON_HIT_TYPE.ALL);
		}
	}


    public void ACHitTargetIsCrit(ActionController ac )
    {
        if (!IsPlayerSelf)
        {
            return;
        }

        if (_aiAgent.CurrentAttack != null)
        {
            _aiAgent.CurrentAttack.HitTargetIsCrit(ac);
        }
    }


	//stop all effect
	public void ACEndCurrentAttackEffect(bool force)
	{
		
	}
	
	public void AniStopEffect(int idx)
	{
		
	}
		
	public void AniDeActiveWeapon()
	{
		if(_aiAgent.CurrentAttack != null)
		{
			_aiAgent.CurrentAttack.AttackActive(false);
		}
		else
		{
			ACActiveWeapon(FC_EQUIPMENTS_TYPE.MAX,false);
		}
	}
	
	public void AniSetAnimationSpeed(float speed)
	{
		_avatarController.SetAnimationSpeed(speed);
	}
	
	public void ACAniSpeedRecoverToNormal()
	{
		_avatarController.RecoverToNormalSpeed();
	}
	
	public float AniGetAnimationSpeed()
	{
		return _avatarController.GetAnimationSpeed();
	}
	
	//get curretn animation play time percent
	public float AniGetAnimationNormalizedTime()
	{
		return _avatarController.GetAnimationNormalizedTime();
	}
	
	public float AniGetAnimationLength()
	{
		return _avatarController.GetAnimationLength();
	}
	
	public float AniGetAnimationOrgSpeed()
	{
		return _avatarController.GetAnimationOrgSpeed();
	}
	
	public void AttackCanSwitch()
	{
		if(_aiAgent.CurrentAttack != null)
		{
			//if is final attack with defy ,we should change it to defy
			if( _aiAgent.CurrentSkill != null
				&& _aiAgent.CurrentAttack.IsFinalAttack 
				&& !_aiAgent.CurrentSkill.SkillModule._isInLoop
				&& !_aiAgent.CurrentSkill.WithDefy )
			{
				_aiAgent.CurrentAttack.CanSwitchToOtherState = true;
			}
			else
			{
				_aiAgent.CurrentAttack.AttackCanSwitch = true;
			}
		}
	}
	
	public void AniIsStart()
	{
		float speed = _avatarController.GetAnimationOrgSpeed();
		if(_bufferAniPlaySpeedPercent<0.95f)
		{
			_avatarController.SetAnimationSpeed(speed*_bufferAniPlaySpeedPercent);
		}
		else
		{
			if(_avatarController.GetAnimationSpeed() != speed)
			{
				_avatarController.SetAnimationSpeed(speed);
			}
		}
	}
	
	
	// if isPoint == false, means direction = - center.forward
	public void ACEffectByGravity(float force, Transform gCenter, Vector3 gDirection, float gTime, bool isPoint,bool realG)
	{
		if(realG)
		{
			if(force >0)
			{
				_moveAgent.IsOnGround = false;
			}
			_moveAgent._gBeReal = true;
			if(_moveAgent._gEnabled)
			{
				_moveAgent._speedY = force;
				
			}
			else
			{
				_moveAgent._gEnabled = true;
				_moveAgent._speedY = force;
				_moveAgent._gForce = 0;
				_moveAgent._gDirection = Vector3.zero;
				_moveAgent._gIsPoint = false;
			}
		}
		else
		{
			if(gCenter != null)
			{
				_moveAgent._gEnabled = true;
				_moveAgent._gForce = force;
				_moveAgent._gEffectTime = gTime;
				_moveAgent._gCenter = gCenter;
				_moveAgent._gIsPoint = isPoint;
			}
			else
			{
				if(!_moveAgent._gBeReal)
				{
					_moveAgent._gEnabled = false;
				}
			}
		}
	}
	
	public void ACEffectByGravity(float force, Transform gCenter, float gTime, bool isPoint,bool realG)
	{
		Vector3 dir = Vector3.up;
		if(gCenter != null)
		{
			dir = gCenter.forward;
		}
		ACEffectByGravity(force, gCenter, dir, gTime, isPoint, realG);
	}
		
	public void AniIsOver(int loopCount)
	{
		_aiAgent.CurrentAniLoopCount = loopCount;
		if(_aiAgent.AniEventAniIsOver != null)
		{
			_aiAgent.AniEventAniIsOver();
		}
		
	}
	
	public void ACClearThreat(ActionController ac)
	{
		if(_threatAgent != null)
		{
			_threatAgent.ClearThreat(ac);
		}
	}
	public void ACSomePlayerInThreatIsDead()
	{
		if(_threatAgent != null)
		{
			_threatAgent.SomeTargetDead();
		}
	}
	// means other collsion box can collide with me
	public void ACEnableCollisionWithOtherACS(bool enableCollision)
	{
		if(enableCollision)
		{
			_moveAgent._navAgent.radius = BodyRadius;
		}
		else
		{
			_moveAgent._navAgent.radius = 0;
		}
		//_moveAgent.ThisRigidBody.isKinematic = (!enableCollision);
	}
	
	//means no one can collide with me ,false means enable
	public void ACDisableCollisionsWithOther(bool disableCollision)
	{
		//for air wall , we cant disable the collision
		_avatarController.collider.enabled = !disableCollision;
	}
	
	public void ACSetColliderAsTrigger(bool setToTrigger)
	{
		if(_avatarController.collider != null)
		{
			_avatarController.collider.isTrigger = setToTrigger;
		}
	}
	
	public void ACSetMoveMode(MoveAgent.MOVE_MODE mm)
	{
		_moveAgent.SetMoveMode(mm);
	}
	
	public void ACSetRaduis(float raduis)
	{
		_moveAgent.SetRaduis(raduis);
	}
	
	public Transform ACGetTransformByName(EnumEquipSlot name)
	{
		return _avatarController.GetSlotNode(name);
	}
	
	public void MoveByAnimator(bool beEnable)
	{
		if(_avatarController != null)
		{
			_avatarController.ControlByAni = beEnable;
		}

	}
	public void ACAddTargetToThreatList(ActionController ac)
	{
		if(_threatAgent != null)
		{
			_threatAgent.AddTargetToList(ac);
		}
	}
	
	public void ACBindKeyToAttack(int key,int attSetListIdx,int aiLevel)
	{
		_attackAgent._skillMaps[aiLevel]._skillConfigs[attSetListIdx].KeyBind = key;
	}
	
	public AttackAgent ACGetAttackAgent()
	{
		return _attackAgent;
	}
	
	public void ACAddBulletToAttack(FCBullet eb)
	{
		_bulletsOfCurrentAttack.Add(eb);
	}
	
	public void ACSetCheatFlag(FC_ACTION_SWITCH_FLAG asf,bool toSet)
	{
		if(toSet)
		{
			_aiAgent.SetActionSwitchFlag(asf);
		}
		else
		{
			_aiAgent.ClearActionSwitchFlag(asf);
		}
	}
	// true means player can kill any one
	public bool ACInSUPERSAIYAJIN2()
	{
		return _aiAgent.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2);
	}
	
	public void ACSetWalkLayer(int walkableMask)
	{
		_moveAgent.SetNavLayer(walkableMask);
	}
	protected void LateUpdate()
	{
		//dead at once
		if (_deadAtOnce)
			ACDead();
		
		//handle data syncs
		_dataSync.Update();
#if WQ_CODE_WIP
		Test();
#endif
		//UpdateDirectionIndicator();
		_lastPosition = _currPosition;
		_currPosition = ThisTransform.localPosition;
		_wasHit = false;
		
		UpdateCamera();
		_aiAgent.HandleLateUpdate();

	}
	
	public void UpdateCamera()
	{
		if(_isPlayerSelf)
		{
			CameraController.Instance.CameraUpdate();
		}
		
	}
	
	private void Loot()
	{
		if(_lootAgent != null)
		{
			_lootAgent.Loot();
		}


		TutorialManager.Instance.ReceiveFinishTutorialEvent(EnumTutorial.Battle_KillAnEnemy);
	}
	
	public void ACShowCharge(float percent)
	{
		if(IsPlayerSelf)
		{
			_avatarController.SetChargeDisplay(percent);
		}
	}
		
	public void WhenMoveUpdate(Vector3 motion)
	{
		_aiAgent.UpdadeHitBackList(motion);
	}
	#endregion
	
	
	public void CalculateLoot(List<LootObjData> list)
	{
		if(_lootAgent != null)
		{
			_lootAgent.StartCalculateAtOnce(list);
		}
	}
	
	public void ACEatPotion(float percentFirst, float percentPertime, float totalTime, FCPotionType potionType)
	{
		if(potionType == FCPotionType.Health)
		{
			//percentFirst = 0.1f;
			//percentPertime = 0.1f;
            int deltaHp = Mathf.RoundToInt(_data.TotalHp * (percentFirst + _data.TotalHPReply));

			if(IsPlayerSelf)
			{
			}
			if(deltaHp >=0)
			{
				ACIncreaseHP(deltaHp);
                //_aiAgent.EotAgentSelf.AddEot(Eot.EOT_TYPE.EOT_HP, totalTime, percentPertime, 0);
				CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_HP_ONCE, _avatarController, -1);
				//_avatarController.SetHPDisplay((float)_hitPoint/_data.TotalHp, -deltaHp, false, false);
			}
		}
		else if(potionType == FCPotionType.Mana)
		{
			int deltaEp = (int)(_data.TotalEnergy * percentFirst);
			if(IsPlayerSelf)
			{
			}
			if(deltaEp >=0)
			{
				int eng = _energy;
				_energy += deltaEp;
				if(_energy >= _data.energy)
				{
					_energy = _data.energy;
				}
				if (_energyChangeMessage != null)
				{
					_energyChangeMessage((float)(_energy)/(float)_data.TotalEnergy);
				}
				if(_onEnergyChange != null)
				{
					_onEnergyChange(_energy - eng, -2);
				}
				_aiAgent.KeyAgent.UpdateKeyState();
                //_aiAgent.EotAgentSelf.AddEot(Eot.EOT_TYPE.EOT_MANA, totalTime, percentPertime, 0);
				CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.RESTORE_MP_ONCE, _avatarController, -1);
			}
		}
	}
	
	public void OnLevelUp(int level)
	{
		//Player effect  is RPC
		CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.LEVEL_UP, 
					_avatarController, 
					-1);
		ACRecoverAll();
		if(_isPlayerSelf)
		{
			//my player
			CameraController.Instance.StartCameraEffect(EnumCameraEffect.effect_15);

            UpdateDataFromLevelData(level, true);
			
			//send cmd to others
			CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_LEVELUP,  
				ObjectID,
				ThisTransform.localPosition,
				level,
				FC_PARAM_TYPE.INT,
				null,
				FC_PARAM_TYPE.NONE,
				null,
				FC_PARAM_TYPE.NONE);			
		
		}

	}
	
	//receive level up from net
	public void OnLevelUp_FromNet(int level)
	{
		//Player effect  is RPC
		CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.LEVEL_UP, 
					_avatarController, 
					-1);


        UpdateDataFromLevelData(level, true);
	
	}



    private void UpdateDataFromLevelData(int level, bool isLevelUp)  
	{
        UpdateDataFromLevelData(level, PlayerInfo.Instance.Role, isLevelUp);
	}

    private void UpdateDataFromLevelData(int level, EnumRole role, bool isLevelUp)
    {
        _data.Level = level;
        _data.pLevelData = DataManager.Instance.GetCurrentClassLevelData(level, role);

        UpdateData();
    }

    public void UpdateData()
    {
        if (null == _data)
        {
            Debug.LogError("error data");
            return;
        }

        _data.ClearHitParamsData();

        _data.InitBaseHitParamsData();

        if (null != _data.pLevelData)
        {
            _data.DealPlayerLevelData();
        }

        if (_passiveSkillAgent != null)
        {
            _passiveSkillAgent.PassiveEffect(0, _data);
        }

        if (_equipsAgent != null)
        {
            _equipsAgent.EquipmentAllData();
        }

        bool IsCheckData = IsPlayerSelf && (GameManager.Instance.GameState == EnumGameState.InTown);

        _attackPoints[0] = IsCheckData ? CheckUseData(AIHitParams.Attack, Data.TotalAttack, Mathf.FloorToInt(PlayerInfo.Instance.Attack)) : Data.TotalAttack;
        _attackPoints[1] = IsCheckData ? CheckUseData(AIHitParams.IceDamage, Data.TotalIceAttack, Mathf.FloorToInt(PlayerInfo.Instance.IceDmg)) : Data.TotalIceAttack;
        _attackPoints[2] = IsCheckData ? CheckUseData(AIHitParams.FireDamage, Data.TotalFireAttack, Mathf.FloorToInt(PlayerInfo.Instance.FireDmg)) : Data.TotalFireAttack;
        _attackPoints[3] = IsCheckData ? CheckUseData(AIHitParams.LightningDamage, Data.TotalLightningAttack, Mathf.FloorToInt(PlayerInfo.Instance.LightningDmg)) : Data.TotalLightningAttack;
        _attackPoints[4] = IsCheckData ? CheckUseData(AIHitParams.PoisonDamage, Data.TotalPoisonAttack, Mathf.FloorToInt(PlayerInfo.Instance.PosisonDmg)) : Data.TotalPoisonAttack;

        _defenseInfo._defensePoints[0] = IsCheckData ? CheckUseData(AIHitParams.Defence, Data.TotalDefense, Mathf.FloorToInt(PlayerInfo.Instance.Defense)) : Data.TotalDefense;
        _defenseInfo._defensePoints[1] = IsCheckData ? CheckUseData(AIHitParams.IceResist, Data.TotalIceDefense, Mathf.FloorToInt(PlayerInfo.Instance.IceRes)) : Data.TotalIceDefense;
        _defenseInfo._defensePoints[2] = IsCheckData ? CheckUseData(AIHitParams.FireResist, Data.TotalFireDefense, Mathf.FloorToInt(PlayerInfo.Instance.FireRes)) : Data.TotalFireDefense;
        _defenseInfo._defensePoints[3] = IsCheckData ? CheckUseData(AIHitParams.LightningResist, Data.TotalLightningDefense, Mathf.FloorToInt(PlayerInfo.Instance.LightningRes)) : Data.TotalLightningDefense;
        _defenseInfo._defensePoints[4] = IsCheckData ? CheckUseData(AIHitParams.PoisonResist, Data.TotalPoisonDefense, Mathf.FloorToInt(PlayerInfo.Instance.PosisonRes)) : Data.TotalPoisonDefense;

        _defenseInfo._criticalDamageResist = IsCheckData ? CheckUseData(AIHitParams.CriticalDamageResist, Data.TotalCritDamageResist, PlayerInfo.Instance.CritDamageRes) : Data.TotalCritDamageResist;//Data.TotalCritDamageResist;


        //add hp. armor, energy, damage from passive skill

        /*_attackAgent.PassiveEffect(ref Data._passiveHp, tHp, ref Data._passiveArmor, tArmor,
            ref Data._passiveEnergy, tEnergy);*/
        HitPoint = IsCheckData ? (int)CheckUseData(AIHitParams.Hp, Data.TotalHp, PlayerInfo.Instance.HP) : Data.TotalHp;
        CritRate = IsCheckData ? CheckUseData(AIHitParams.CriticalChance, Data.TotalCritRate, PlayerInfo.Instance.Critical) : Data.TotalCritRate;
        CritDamage = IsCheckData ? CheckUseData(AIHitParams.CriticalDamage, Data.TotalCritDamage, PlayerInfo.Instance.CritDamage) : Data.TotalCritDamage;
        SkillTriggerRate = Data.TotalSkillTriggerRate;
        SkillAttackDamage = Data.TotalSkillAttackDamage;
        //DefenseAllData._defensePoints[0] = Data._physicalResist + Data._pLevelData._defense + ;	
    }

    public float CheckUseData(AIHitParams hitParams, float clientValue, float serverValue)
    {
        Debug.Log(string.Format("{0}  ClientValue:{1}  ServerValue:{2}", hitParams.ToString(), clientValue, serverValue));

        int value1 = Mathf.RoundToInt(clientValue * 10000);
		int value2 = Mathf.RoundToInt(serverValue * 10000);

        if (value1 == value2)
        {
            return clientValue;
        }
        else
        {
            Debug.LogError("The client and the server computing data are different");
            return serverValue;
        }
    }

    public void CheatJump()
    {
        rigidbody.isKinematic = false;

        rigidbody.useGravity = true;

        rigidbody.velocity = new Vector3(5, 8, 5);

        _moveAgent.CheatJump();
    }
	
	//private FrameAnimation _directionIndicator = null;
	/*
	private float _checkIndicatorTimer = 0;
	private static float CHECK_RATE = 0.5f;
	public void UpdateDirectionIndicator()
	{
		if(_aiAgent._aiType == FC_AI_TYPE.PLAYER_WARRIOR
			|| _aiAgent._aiType == FC_AI_TYPE.PLAYER_MAGE 
			|| _aiAgent._aiType == FC_AI_TYPE.PLAYER_MONK)
		{
			
			_checkIndicatorTimer += Time.deltaTime;
			if(_checkIndicatorTimer > CHECK_RATE)
			{
				_checkIndicatorTimer -= CHECK_RATE;
				
				ActionController ac = ActionControllerManager.Instance.GetEnemyTargetBySight(ThisTransform, 8.0f,5.0f,Faction, 150, false);
			
				if(ac != null)
				{
					ChangeDirectionIndicatorColor(Color.red);
				}else{
					ChangeDirectionIndicatorColor(Color.green);
				}
			}
		}
	}
	
	
	public void ChangeDirectionIndicatorColor(Color color)
	{
		if(_directionIndicator != null)
		{
			_directionIndicator.ChaneColor(color);
		}
	}
	
	*/
	
	public void ACHide()
	{
		_avatarController.ChangeMeshRenderers(false);
		ShowIndicator(false);
		if(_onShowBody != null)
		{
			_onShowBody(false);
		}
	}
	
	public void ACShow()
	{
		_avatarController.ChangeMeshRenderers(true);
		ShowIndicator(true);
		if(_onShowBody != null)
		{
			_onShowBody(true);
		}
	}
	
	public void InitDirectionIndicator()
	{
		if(_aiAgent._aiType == FC_AI_TYPE.PLAYER_WARRIOR
			|| _aiAgent._aiType == FC_AI_TYPE.PLAYER_MAGE 
			|| _aiAgent._aiType == FC_AI_TYPE.PLAYER_MONK)
		{
			
			FC_CHARACTER_EFFECT effid = FC_CHARACTER_EFFECT.DIRECTION_INDICATOR;
			if(GameManager.Instance.IsPVPMode || GameManager.Instance.IsMultiplayMode)
			{
				effid = FC_CHARACTER_EFFECT.DIRECTION_INDICATOR_MULTIPLAYER;
			}
			ModelAnimInstance effectInst = (ModelAnimInstance)CharacterEffectManager.Instance.PlayEffect(effid ,_avatarController, -1);
			if(GameManager.Instance.IsPVPMode || GameManager.Instance.IsMultiplayMode){
				Color color = MultiplayerDataManager.Instance.PlayerIndicatorColors[_instanceID];
				foreach(MeshRenderer render in effectInst._renderers){
					render.sharedMaterial.SetColor("_Color" , color);
				}
			}
		}
	}

	
	public void ShowIndicator(bool show)
	{
		if (show)
		{
			if(_aiAgent._aiType == FC_AI_TYPE.PLAYER_WARRIOR
				|| _aiAgent._aiType == FC_AI_TYPE.PLAYER_MAGE 
				|| _aiAgent._aiType == FC_AI_TYPE.PLAYER_MONK)
			{
				//CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DIRECTION_INDICATOR ,_avatarController, -1);
				InitDirectionIndicator();
			}
		}
		else
		{
			CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DIRECTION_INDICATOR ,_avatarController, 0.001f);
		}
		
	}


}

