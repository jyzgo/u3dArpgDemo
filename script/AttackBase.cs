using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class MMediaEffectInfo
{
	public EnumCameraEffect _cameraEffect = EnumCameraEffect.none;
	public bool _holdWhenEnd = false;
	public EnumShakeEffect _shakeLevel = EnumShakeEffect.none;
	public bool _needForceEnd = true;
	public float _lastTime = -1;
	
	public void PlayEffect()
	{
		CameraController.Instance.StartCameraEffect(_cameraEffect,_shakeLevel,_holdWhenEnd);
	}
	
	public void StopEffect()
	{
		if(_needForceEnd)
		{
			CameraController.Instance.StopCameraEffect();
		}
	}
}

[System.Serializable]
public class MMediaEffectInfoList
{
	public FC_EFFECT_EVENT_POS _effectPos;
	public string _firePortName = "";
	public MMediaEffectInfo[] _effectInfos;
}

[System.Serializable]
public class MMediaEffectInfoMap
{
	public MMediaEffectInfoList[] _mmEffectInfoList = null;
	public void PlayEffect(FC_EFFECT_EVENT_POS eeep, ActionController ac, object param1)
	{
		if(ac != null && ac.IsPlayer && !ac.IsPlayerSelf)
		{
			return;
		}
		string firePortName = "";
		if(eeep == FC_EFFECT_EVENT_POS.AT_FIREBULLET)
		{
			firePortName = (string)param1;
		}
		foreach(MMediaEffectInfoList meil in _mmEffectInfoList)
		{
			if(meil._effectPos == eeep &&
				(meil._effectPos != FC_EFFECT_EVENT_POS.AT_FIREBULLET
				|| (meil._effectPos == FC_EFFECT_EVENT_POS.AT_FIREBULLET
				&& (firePortName == meil._firePortName || meil._firePortName == ""))))
			{
				foreach(MMediaEffectInfo mei in meil._effectInfos)
				{
					
					mei.PlayEffect();
				}
			}
		}
	}
	public void StopEffect()
	{
		foreach(MMediaEffectInfoList meil in _mmEffectInfoList)
		{
			foreach(MMediaEffectInfo mei in meil._effectInfos)
			{
				mei.StopEffect();
			}
		}
	}
}

[System.Serializable]
public class BindEffectInfo
{
	public FC_CHARACTER_EFFECT _bindEffect = FC_CHARACTER_EFFECT.INVALID;
	public float _startPercent = 0.0f;
	public float _endPercent = 1.0f;
	
	private bool _isPlaying = false;
	public bool IsPlaying
	{
		get
		{
			return _isPlaying;
		}
		set
		{
			_isPlaying = value;
		}
	}	
	
	private bool _hasPlayed = false;
	public bool HasPlayed
	{
		get
		{
			return _hasPlayed;
		}
		set
		{
			_hasPlayed = value;
		}
	}	
	
	
}

[System.Serializable]
public class GlobalEffectInfo
{
    public FC_GLOBAL_EFFECT globalEffect = FC_GLOBAL_EFFECT.INVALID;
    public float startPercent = 0.0f;
    public float endPercent = 1.0f;

    private bool _isPlaying = false;
    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
        set
        {
            _isPlaying = value;
        }
    }

    private bool _hasPlayed = false;
    public bool HasPlayed
    {
        get
        {
            return _hasPlayed;
        }
        set
        {
            _hasPlayed = value;
        }
    }


}


[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/Attack/AttackBase")]
[System.Serializable]
public class AttackBase : FCObject,FCAgent {
	
	protected int _frameCount = 0;
	protected AIAgent _owner;
	//name of attack
	public string _name;
	public FCConst.NET_POSITION_SYNC_LEVEL _positionSyncLevel = FCConst.NET_POSITION_SYNC_LEVEL.LEVEL_0;
	//sound effect when attack enter
	public string _sfxName = "";
	//if _sfxName == "", we will use _sfxNames for play all the sound effects
	public string[] _sfxNames = null;
	//if we want not stop the sound when quit attack, fill the val
	public float _soundStopDelayTime = -1;
	
	protected bool _isInDefy = false;
	
	public bool IsInDefy
	{
		get
		{
			return _isInDefy;
		}
		set
		{
			_isInDefy = value;
		}
	}
	
	protected int _sfxNamesIdx = 0;
	#region //will not effect player self
	//when attack switch, or during attack, use this to decide how to seek target
	public SEEK_LEVEL _seekLevel = SEEK_LEVEL.NONE;
	//seek time when seek level = MANUAL
	public float _seekTimeForManual = 0.1f;
	//when enter attack, max angle rotate when attack enter
	// if > 180 means can rotate to any angle
	// if <= 0 , same as seek level = NEVER
	public float _angleRotateMaxWhenEnter = 181f;
	
	protected bool _isOnSeeking = false;
	#endregion //will not effect player self
	
	//if we want attack through target, enable it
	public bool _haveNoColliderWhenAttack = false;
	
	public enum EnergyCostTime
	{
		BEGIN,//cost energy when enter attack
		PROCESS,//cost energy during attack, have no effect now
		END//cost energy when attack end
	}
	
	//flag to decide what time for cost energy
	public EnergyCostTime _energyCostTime = EnergyCostTime.BEGIN;
	//if enable it , means energycost and _energyCostTime will have no use
	public bool _willNotCostEnergy = false;
	
	//when active weapon, the flags will decide whick weapon will active
	public FC_EQUIPMENTS_TYPE[] _weaponsToActive = null;
	protected int _weaponsToActiveIndex = 0;
	//when deactive weapon, the flags will decide whick weapon will active
	public FC_EQUIPMENTS_TYPE[] _weaponsToDeActive = null;
	protected int _weaponsToDeActiveIndex = 0;
	protected int _hitTargetCount = 0;
	
	public bool _hasSlashDumping = false;
	//if = 65535 , means use weapon sharpness ,otherwise use sharpnessWithAttack
	public int _sharpnessWithAttack = 65535;
	
	//if = 1, means when dumping of weapon will reduce when hit target
	//if = 0, means the dump will never reduced
	public float _dumpReduce = 1;
	
	//the attack cant be parried
	public bool _ignoredParry = false;
	
	//if you want to an attack have more count to hit target, fill it
	public int _activeWeaponTimes = 0;
	//the weapon will be reactive per activeWeaponTimeCD
	public float _activeWeaponTimeCD = 0.05f;
	//
	protected bool _firstActiveWeapon = false;
	protected int _currentWeaponTimes = 0;
	protected float _currentWeaponTimeCD = 0;
	protected int _attackActiveCount = 0;
	
	//if true , means weapon will hit target with hit id = 0
	//hurt will be happened every times when weapon hit some
	public bool _needNotHitID = false;
	
	#region //only used for player self
	//only used for player self
	//used to correct attack direction of player
	public bool _needAttackCorrect = false;
	public bool _needPushBack = false;
	public float _safeDistance = 20;
	public float _dangerDistance = 10;
	#endregion //only used for player self
	
	//animtion play count for one attack
	public int _animationCount = 1;
	//if true ,means attack will not switch until all animtion of attack is over
	public bool _endByAnimationOver = false;
	protected int _currentAnimationCount = 0;
	
	//means if true, even if _animationCount >0 ,attack will still end when current animation is over
	public bool _endByHitTarget = false; 
	protected bool _endByAnimationOverThisTime = false;
	
	//if _forceNextWhenHitTarget = true, means when hit target, attack will force switch to next attack at once
	public bool _forceNextWhenHitTarget = false;
	//when hit someone, after time force next, attack will switch
	public float _timeForceNext = 1f;
	protected float _timeForceNextCount = -1f;
	
	//if true, when attackCanSwitch = true, _shouldGotoNextHit = true
	public bool _needNotHoldAttackKey = false;
	
	//chance to show element hit effect
	public float _chanceToElemHit = 1;
	//what element effect will show when hit someone
	public FC_DAMAGE_TYPE _elemHitType = FC_DAMAGE_TYPE.NONE;
	protected FC_DAMAGE_TYPE _currentDamageType = FC_DAMAGE_TYPE.NONE;
	//effect of time for current attack
	public Eot[] _eots;
	
	//all sounds in loop will record in this array
	protected List<AudioSource> _soundsPlaying = new List<AudioSource>();
	
	public bool NeedAttackCorrect
	{
		set
		{
			_needAttackCorrect = value;
		}
		get
		{
			return _needAttackCorrect;
		}
	}
	
	// final attack for skills
	protected bool _isFinalAttack;
	
	//if true ,when attack end, attack will jump to attack of skill end
	protected bool _jumpToSkillEnd = false;
	
	//skill data from excel
	protected SkillData _skillData = null;
	public SkillData SkillData
	{
		get{return _skillData;}
		set{
			_skillData = value;
		}
	}
	
	//if true, means superarmor will not effect this attack
	public bool _needIgnoreSuperArmor = false;
	
	public enum SEEK_LEVEL
	{
		NONE,//only change direction when enter attack
		NORMAL,//will seek target before attack start,means weapon active,
		HALF,//means will seek target before the animation play half
		PRO,//will seek target before attack end, means weapon deactive
		All,// will seek target all attack
		NEVER,//will not trace target at any time, even not when enter attack
		MANUAL// by spec time
	}
	
	//record bullet fire port and sound info
	[System.Serializable]
	public class FireportInfo
	{
		public string _sfxWhenFireBullet = "";
		public string _firePortName = "";
		protected int _firePortIdx = -1;
		public int FirePortIdx
		{
			get
			{
				return _firePortIdx;
			}
			set
			{
				_firePortIdx = value;
			}
		}
	}

	public FireportInfo[] _fireInfos = null;
	
	//if true ,means loop way is a...z...a...z...
	//if false, means loop way is a....z..z.z...
	public bool _loopFireIndex = false;
	protected int _currentPortIdx = 0;
	
	//animation set for this attack
	//if = none, means no animation need play for the attack
	public FC_All_ANI_ENUM _attackAni = FC_All_ANI_ENUM.none;
	
	//camera effect for attack, only effect by player self
	public MMediaEffectInfoMap _mmEffectMap = null;
	
	//angle speed scale to default angle speed
	public float _angleSpeedScale = 1f;
	protected float _orgAngleSpeed;
	
	//damage scale to real damage points
	public float _damageScaleSource = 1f;
	protected float _damageScale = 1f;
	
	//pust strength to hit target
	public int _pushStrength = 0;
	//time for push enemy back
	public float _pushTime = 0.2f;
	//if push by weapon, need target in front of ac
	public float _pushAngle = 100;
	//if true, means push direction is Backward or toward to self position
	//if false, means push direction is backward of toward to self move direction
	public bool _pushByPoint = false;
	
	//if false, means this attack will not sync to other handset
	public bool _needSyncToOthers = true;
	
	//energy cost for current attack,scaled by skill data
	public int _energyCost = 0;
	protected int _energyCostOrg = 0;
	//if > 0, when enter attack, self will be in god mode for god time or quit attack
	public float _godTime = 0;
	
	//if self is in animation move mode, move length = move length * _aniMoveSpeedScale
	public float _aniMoveSpeedScale = 1f;
	
	//if true, means self in in manual move mode
	public bool _moveByManual = false;
	//speed curve for manual move mode
	public AnimationCurve _speedCurve =  new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
	//if in manual mode, base move speed
	public float _baseMoveSpeed = 10;
	//effect move speed of self
	protected float _speedScale = 1;
	
	//if true, count move length to target at start, try to stop near target when attack over
	public bool _tryToLockTargetAtStart = false;
	//if true, count move length to attack active, try to stop near target when attack over
	public bool _tryToLockTargetAtAttack = false;
	//suppose total move length of current attack
	public float _lengthPassThrough = 10;
	//max final move length of current attack
	public float _maxLengthPassThrough = 15;
	
	//hash code for name of current attack
	protected int _nameHashCode = 0;
	
	//first attack of skill
	protected bool _isFirstAttack = false;
	
	public float _distanceMin = 0;
	public float _distanceMax = 0;
	
	//esp for normal attack and bash
	public bool _cacheKeyPress = true;
	
	//if true, means this attack can gain energy from hit target
	public bool _hitGainEnergy = false;
	
	public bool _showSkillTipWhenStart = false;
	public bool _endSkillTipWhenStart = false;
	
	public bool IsFirstAttack
	{
		get
		{
			return _isFirstAttack;
		}
		set
		{
			_isFirstAttack = value;
		}
	}
	
	public bool IsFinalAttack
	{
		get
		{
			return _isFinalAttack;
		}
		set
		{
			_isFinalAttack = value;
		}
	}
	
	public int NameHashCode
	{
		get
		{
			return _nameHashCode;
		}
	}
	public int FirePortIdx
	{
		get
		{
			if(_fireInfos != null && _fireInfos.Length != 0 && _currentPortIdx >=0)
			{
				return _fireInfos[_currentPortIdx].FirePortIdx;
			}
			return -1;
			
		}
	}
	public float DamageScale
	{
		get
		{
			return _damageScale;
		}
	}
	
	// hit type of current attack
	public AttackHitType _hitType = AttackHitType.HurtNormal;
	
	//if attackCanSwitch = true && shouldGotoNextHit = true, attack should goto next
	protected bool _shouldGotoNextHit = false;
	
	//if _makeSkillCD = false, if we use this attack, when attack end , it will not cause skill to cool down
	protected bool _makeSkillCD = true;
	public bool MakeSkillCD
	{
		get
		{
			return _makeSkillCD;
		}
		set
		{
			_makeSkillCD = value;
		}
	}
	
	//the bind to current skill, only effect with player self
	protected FC_KEY_BIND _currentBindKey;
	
	//effect with attack normal, if _attackLastTime <0, attack can cache press key
	protected float _attackLastTime = 0.5f;
	
	public float AttackLastTime
	{
		get
		{
			return _attackLastTime;
		}
		set
		{
			_attackLastTime = value;
		}
	}
	
	public FC_KEY_BIND CurrentBindKey
	{
		get
		{
			return _currentBindKey;
		}
		set
		{
			_currentBindKey = value;
		}
	}
	
	public bool ShouldGotoNextHit
	{
		get
		{
			return _shouldGotoNextHit;
		}
	}
	
	public enum ATTACK_STATE
	{
		STEP_0,//0 mean attack not active
		STEP_1,
		STEP_2,
		STEP_3,
		ALL_DONE,//means attack is end normal, not be force quit
	}
	
	//if true means attack can be switch
	protected bool _attackCanSwitch = false;
	public bool AttackCanSwitch
	{
		get
		{
			return _attackCanSwitch;
		}
		set
		{
			_attackCanSwitch = value;
			if(_attackCanSwitch && _needNotHoldAttackKey)
			{
				_shouldGotoNextHit = true;
			}
			if(_attackCanSwitch)
			{
				_owner.UpdateNetCommand(AIAgent.COMMAND_DONE_FLAG.ATTACK_CAN_SWITCH);
			}
		}
	}
	
	//only used for player
	protected bool _canSwitchToOtherState = false;
	public bool CanSwitchToOtherState
	{
		get
		{
			return _canSwitchToOtherState;
		}
		set
		{
			_canSwitchToOtherState = value;
		}
	}
	
	//state flag
	protected ATTACK_STATE _currentState;
	public ATTACK_STATE CurrentState
	{
		get
		{
			return _currentState;
		}
	}
	
	//if true means attack cant break by any attack
	public bool _hasRigidBody2 = false;
	
	protected AttackHitType _currentAttackType;
	public AttackHitType CurrentAttackType
	{
		get
		{
			return _currentAttackType;
		}
		set
		{
			_currentAttackType = value;
		}
	}
	
	public BindEffectInfo[] _bindEffects = null;
	
	//use these params to modify the collision of weapon
	public int _weaponScaleX = 100;
	public int _weaponScaleY = 100;
	public int _weaponScaleZ = 100;
		
	//next attack id
	//-1 means attacktaskchange of ai change the next attack id
	//256 means current skill should go to end
	protected int _nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
	public int NextAttackIdx
	{
		get
		{
			return _nextAttackIdx;
		}
		set
		{
			_nextAttackIdx = value;
		}
	}
	
	protected AttackConditions[] _attackConditions;
	
	public AttackConditions[] AttCons
	{
		get
		{
			return _attackConditions;
		}
		set
		{
			_attackConditions = value;
		}
	}
	
	//used with can cache combo, only effect with player
	//FIXME  its not easy to know what val should used with player
	public float _attackDelayTime = 1f;
	
	//used with _isInAttackEndDelay, if true, means when attack end, attack will not switch to next at once, and will slow current animation
	public bool _attackEndDelay = false;
	public float _attackEndDelayTime = 1f;
	
	public float _aniReduceSpeed = 50;
	public float _aniReduceSpeedFinal = 0.1f;
	
	//if true, && time for attack > _timeCanBeInterrupt, means this skill can interrupt by other skill
	public bool _canInterruptByOther = true;
	public float _timeCanBeInterrupt = -1;
	
	//FIXME:  have no use now
	protected float _addDamageScale = -1;
	public float AddDamageScale
	{
		get
		{
			return _addDamageScale;
		}
	}
	
	protected bool _attackIsEnd = false;
	
	//if true, means this attack can hit both enemy and player
	public bool _canHitAll = false;
	
	//if true, means when when target hurt, only have animtion with motion
	public float _hitStrengthScale = 1;
	
	
	protected override void FirstInit()
	{
		base.FirstInit();
		_nameHashCode = _name.GetHashCode();
		_currentDamageType = FC_DAMAGE_TYPE.PHYSICAL;
		_energyCostOrg = _energyCost;
	}
	
	public FC_DAMAGE_TYPE GetDamageType()
	{
		return _currentDamageType;
	}
	
	//base.AttackEnter ,end, update, quit must be called in function override
	public virtual void AttackEnter()
	{
		if(GameManager.Instance.IsPVPMode){  
			_canHitAll = true;	
		}
		_attackIsEnd = false;
		_attackActiveCount = 0;
		_frameCount = 0;
		_addDamageScale = -1;
		_soundsPlaying.Clear();
		_currentState = AttackBase.ATTACK_STATE.STEP_0;
		_canSwitchToOtherState = false;
		_firstActiveWeapon = true;
		_currentAnimationCount = 0;
		_endByAnimationOverThisTime = _endByAnimationOver;
		_speedScale = 1;
		_jumpToSkillEnd = false;
		bool fromSkill = false;
		if(_owner.CurrentSkill != null && !_owner.CurrentSkill.SkillModule._isNormalSkill)
		{
			fromSkill = true;
		}
		if(fromSkill && _skillData != null && _isFirstAttack)
		{
			//add character name to skill name
//			string sn = _skillData._skillId + "_" + _owner.ACOwner.Data._characterId;
		}
		if(_fireInfos != null && _fireInfos.Length != 0)
		{
			foreach(FireportInfo fpi in _fireInfos)
			{
				_owner.ACOwner.ACGetFirePort(fpi.FirePortIdx).IsFromSkill = fromSkill;
			}
		}
		if(_moveByManual)
		{
			_owner.ACOwner.ACStop();
			_owner.ACOwner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		}
		if(_isFirstAttack)
		{
			//have super armor will make attak will not break and still can be hit
			if(_owner.CurrentSkill.SuperArmorTotal > 1)
			{
				_owner.SuperArmorSelf.CreateArmor(_owner.CurrentSkill.SuperArmorTotal
					, _owner.CurrentSkill.SuperArmorDamageAbsorb
					, _owner.CurrentSkill.SuperArmorLife
					, FCConst.SUPER_ARMOR_LVL2);
			}
			else
			{
				_owner.SuperArmorSelf.BreakArmor(FCConst.SUPER_ARMOR_LVL2);
			}
		}
		if(!_owner.ACOwner.IsPlayerSelf)
		{
			if(_seekLevel != SEEK_LEVEL.NEVER)
			{
				if( _owner.TargetAC != null)
				{
					_owner.FaceToTarget( _owner.TargetAC, _angleRotateMaxWhenEnter);
				}
			}
		}
		else
		{
			bool rotateByKey = false;
			if(_needAttackCorrect)
			{
				if(_owner._isRanger)
				{
					_owner.TargetAC = null;
					ActionController ac = ActionControllerManager.Instance.GetEnemyTargetBySight
					(_owner.ACOwner.ThisTransform,20 ,0,_owner.ACOwner.Faction,360,true);
					if(ac != null && ac.DangerLevel >= FCConst.ELITE_DANGER_LEVEL)
					{
						_owner.TargetAC = ac;
					}
				}
				else
				{
#if FC_AUTHENTIC
					if(_owner.TargetAC != null && _owner.TargetAC.IsAlived)
					{
						Vector3 v1 = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
						v1.y =0;
						v1.Normalize();
						if(v1 != Vector3.zero)
						{
							float angle = Vector3.Angle(_owner.ThisTransform.forward,v1);
							//FIXME  This is magic number
							//-1 may instead by some const val
							if(angle < _owner._attackCorrectAngleForMelee*3/2)
							{
								_owner.ACOwner.ACRotateTo(v1,-1,true,true);
							}
						}
					}
#endif //FC_AUTHENTIC
				}
				if(_owner._isRanger && _owner.TargetAC != null)
				{
					FC_DANGER_LEVEL edl = _owner.GetTargetDangerLevel(_safeDistance, _dangerDistance );
					if(edl == FC_DANGER_LEVEL.SAFE || edl == FC_DANGER_LEVEL.DANGER)
					{
						rotateByKey = true;
					}
					else
					{
						_owner.FaceToTarget( _owner.TargetAC, true);
					}
				}
			}

			if(rotateByKey && _owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
			{
				_owner.ACOwner.ACRotateTo( _owner.KeyAgent._directionWanted,-1,true,true);
			}
		}
		_orgAngleSpeed = _owner.ACOwner.CurrentAngleSpeed;
		_owner.ACOwner.CurrentAngleSpeed = _owner.ACOwner.CurrentAngleSpeed *_angleSpeedScale;
		_owner.StopMove();
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
		_attackCanSwitch = false;
		_shouldGotoNextHit = false;
		_currentBindKey = _owner.GetCurrentAttackKeyBind();
		if(_owner.ACOwner.IsClientPlayer)
		{
			_seekLevel = SEEK_LEVEL.NONE;
		}
		_currentPortIdx = 0;
		if(_mmEffectMap != null)
		{
			_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_BEGIN,_owner.ACOwner,null);
		}
		if(_hasRigidBody2)
		{
			_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
		}
		_attackLastTime = 1f;
		_nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
		if(
#if DEVELOPMENT_BUILD || UNITY_EDITOR
			!CheatManager.cheatForCostNoEnergy && 
#endif
			_energyCostTime == EnergyCostTime.BEGIN 
			&& !_willNotCostEnergy)
		{
			_owner.ACOwner.CostEnergy(-_energyCost, 1);
		}
		
		_owner.ACOwner.BulletsOfCurrentAttack.Clear();
		if(_godTime > 0.1f)
		{
			_owner.GodTime = _godTime;
		}
		_owner.IsOnParry = FC_PARRY_EFFECT.NONE;
		if(_tryToLockTargetAtStart && _owner.TargetAC != null)
		{
			Vector3 v3 = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
			v3.y = 0;
			float dis = v3.magnitude;
			dis = Mathf.Clamp(dis,0,_maxLengthPassThrough);
			_speedScale = dis/_lengthPassThrough;
		}
		_owner.ACOwner.SetAniMoveSpeedScale(_aniMoveSpeedScale*_speedScale);
		if(_seekLevel != SEEK_LEVEL.NONE && _seekLevel != SEEK_LEVEL.NEVER)
		{
			_isOnSeeking = true;
		}
		else
		{
			_isOnSeeking = false;
		}
		_owner.TryCostAllRage();
		
		_weaponsToActiveIndex = 0;
		_weaponsToDeActiveIndex = 0;
		_hitTargetCount = 0;
        IsFinalAttack = _owner.CurrentAttackIsFinalAttack();
		_owner.ACOwner.ACSetRaduis(_owner._bodyRadiusAttack);
		if(_haveNoColliderWhenAttack)
		{
			NoColliderWithOther();
		}
		//_owner.ACMoveByAnimator();
		
		//reset all effect infos
		foreach(BindEffectInfo effInfo in _bindEffects)
		{
			effInfo.IsPlaying = false;
			effInfo.HasPlayed = false;
		}
		_timeForceNextCount = -1;
		if(_elemHitType != FC_DAMAGE_TYPE.NONE && Random.Range(0,1f) < _chanceToElemHit)
		{
			_currentDamageType = _elemHitType;
		}
		else
		{
			_currentDamageType = FC_DAMAGE_TYPE.NONE;
		}
		if(_isInDefy)
		{
			_owner.IncreaseRage(RageAgent.RageEvent.ATTACK_OVER_WITH_NOHIT, FCWeapon.WEAPON_HIT_TYPE.ALL);
		}
		_owner.SetHitStrength(_hitStrengthScale);
	}
	
	public virtual void AttackActive(bool beActive)
	{
		if(beActive)
		{
			TutorialManager.Instance.ReadyTutorialDefense();
			_attackActiveCount++;
			
			if(_firstActiveWeapon)
			{
				_currentWeaponTimes = _activeWeaponTimes;
				_currentWeaponTimeCD = _activeWeaponTimeCD;
				_firstActiveWeapon = false;
			}
			if(_weaponsToActive != null && _weaponsToActive.Length>0 && _weaponsToActiveIndex < _weaponsToActive.Length)
			{
				_owner.ACOwner.ACActiveWeapon( _weaponsToActive[_weaponsToActiveIndex], true );
				_weaponsToActiveIndex++;
				if(_weaponsToActiveIndex >= _weaponsToActive.Length)
				{
					_weaponsToActiveIndex = 0;
				}
			}
			else
			{
				_owner.ACOwner.ACActiveWeapon( _owner._defaultWeaponType , true );
			}
			if(_seekLevel == SEEK_LEVEL.NORMAL)
			{
				_isOnSeeking = false;
			}
			if(_tryToLockTargetAtAttack && _owner.TargetAC != null && _speedScale == 1)
			{
				Vector3 v3 = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
				v3.y = 0;
				float dis = v3.magnitude;
				dis = Mathf.Clamp(dis,0,_maxLengthPassThrough);
				_speedScale = dis/_lengthPassThrough;
				if(!_moveByManual)
				{
					_owner.ACOwner.SetAniMoveSpeedScale(_aniMoveSpeedScale*_speedScale);
				}
			}
	
			if(_owner.CurrentSkill != null && _owner.ACOwner.IsPlayer)
			{
				if(_owner.CurrentSkill.SkillModule._isNormalSkill 
					&& _owner._randomSoundForNormalAttack != null
					&&  _owner._randomSoundForNormalAttack.Length > 0
					&& Random.Range(0,1f) > _owner._chanceToPlayNormalAttackSound)
				{
					int ret = Random.Range(0,_owner._randomSoundForNormalAttack.Length -1);
					SoundManager.Instance.PlaySoundEffect(_owner._randomSoundForNormalAttack[ret]);
				}
			}
			if(_sfxNames != null && _sfxNames.Length >0)
			{
				if(_sfxNames[_sfxNamesIdx].Contains("loop"))
				{
					AudioSource  audio = SoundManager.Instance.PlaySoundEffect(_sfxNames[_sfxNamesIdx], true);
					if(audio != null)
					{
						_soundsPlaying.Add(audio);
					}	
				}
				else
				{
					SoundManager.Instance.PlaySoundEffect(_sfxNames[_sfxNamesIdx]);
				}
				
			}
			else if(_sfxName != "")
			{
				if(_sfxName.Contains("loop"))
				{
					AudioSource  audio = SoundManager.Instance.PlaySoundEffect(_sfxName, true);
					if(audio != null)
					{
						_soundsPlaying.Add(audio);
					}	
				}
				else
				{
					SoundManager.Instance.PlaySoundEffect(_sfxName);
				}
				
			}
			if(_mmEffectMap != null)
			{
				_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_ATTACK_START,_owner.ACOwner,null);
			}
		}
		else
		{
			if(_seekLevel == SEEK_LEVEL.PRO)
			{
				_isOnSeeking = false;
			}
			if(_weaponsToDeActive != null && _weaponsToDeActive.Length>0 && _weaponsToDeActiveIndex < _weaponsToDeActive.Length)
			{
				_owner.ACOwner.ACActiveWeapon( _weaponsToDeActive[_weaponsToDeActiveIndex], false );
				_weaponsToDeActiveIndex++;
				if(_weaponsToDeActiveIndex >= _weaponsToDeActive.Length)
				{
					_weaponsToDeActiveIndex = 0;
				}
			}
			else
			{
				_owner.ACOwner.ACActiveWeapon( _owner._defaultWeaponType , false );
			}
		}
		//_owner.ACMoveByAnimator();

	}
	
	public virtual void AttackUpdate()
	{		
		_currentWeaponTimeCD -= Time.deltaTime;
		_frameCount ++;
		_owner.SuperArmorSelf.UpdateSuperArmor();
		if(_currentWeaponTimes >0 && _currentWeaponTimeCD <=0)
		{
			_currentWeaponTimes--;
			_currentWeaponTimeCD = _activeWeaponTimeCD;
			if(_currentWeaponTimes%2 != 0)
			{
				AttackActive(false);
				_currentWeaponTimeCD = 0.001f;
			}
			else
			{
				AttackActive(true);
			}
		}
		if(_owner.TargetAC != null && !_owner.TargetAC.IsAlived)
		{
			_isOnSeeking = false;
		}
		//update bind effect, play or stop
	
		if(_isOnSeeking)
		{
			if(_seekLevel == SEEK_LEVEL.HALF && _owner.ACOwner.AniGetAnimationNormalizedTime()>0.5f)
			{
				_isOnSeeking = false;
			}
			else if(_seekLevel == SEEK_LEVEL.MANUAL && _owner.ACOwner.AniGetAnimationNormalizedTime() > _seekTimeForManual)
			{
				_isOnSeeking = false;
			}
			if(_owner.TargetAC != null && !_owner.ACOwner.IsPlayer)
			{
				
					
				Vector3 dir =  _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
				dir.y =0;
				dir.Normalize();
				if(dir != Vector3.zero)
				{
					_owner.ACOwner.ACRotateToDirection(ref dir, false);
				}
			}
			else
			{
				if(_owner.KeyAgent._directionWanted != Vector3.zero)
				{
					_owner.ACOwner.ACRotateToDirection(ref _owner.KeyAgent._directionWanted, false);
				}
			}

			
		}
		if(_canSwitchToOtherState && _owner.ACOwner.IsPlayerSelf
			&& _owner.KeyAgent.ActiveKey != FC_KEY_BIND.NONE && _owner.KeyAgent.ActiveKey != _currentBindKey)
		{
			_attackCanSwitch = true;
			_shouldGotoNextHit = false;
		}
		if(_forceNextWhenHitTarget && _timeForceNextCount >0)
		{
			_timeForceNextCount -= Time.deltaTime;
			if(_timeForceNextCount <= 0)
			{
				_shouldGotoNextHit = true;
				_attackCanSwitch = true;
			}
		}
		UpdateBindEffects();
		if(_moveByManual)
		{
			if(_speedCurve.Evaluate(_owner.ACOwner.AniGetAnimationNormalizedTime())<=0)
			{
				_owner.ACOwner.ACStop();
			}
			else
			{
				_owner.ACOwner.CurrentSpeed = _speedCurve.Evaluate(_owner.ACOwner.AniGetAnimationNormalizedTime())*_baseMoveSpeed*_speedScale;
				_owner.ACOwner.ACMoveToDirection(ref _owner.ACOwner.SelfMoveAgent._rotateDirection,
					(int)_owner.ACOwner.CurrentAngleSpeed, 0.33f);
			}
			
		}
		if(_owner.ACOwner.IsPlayerSelf && _owner.TargetAC != null)
		{
			if(!_owner.TargetAC.IsAlived)
			{
				_owner.TargetAC = null;
			}
		}
	}
	
	//only called when attack was end by no break
	public virtual void AttackEnd()
	{
		if(GameManager.Instance.IsPVPMode){  
			_canHitAll = false;	
		}
		
		if(!_attackIsEnd)
		{
			_attackIsEnd = true;
			_owner.ACOwner.FireTarget = null;
			_currentDamageType = FC_DAMAGE_TYPE.NONE;
			_timeForceNextCount = -1;
			if(_owner.ACOwner.IsPlayer)
			{
				if( _currentBindKey == _owner.KeyAgent.ActiveKey && _currentBindKey != FC_KEY_BIND.NONE
					&& _isFinalAttack && !_attackCanSwitch)
				{
					_attackCanSwitch = true;
					_shouldGotoNextHit = true;
				}
			}
			if(_isFinalAttack && _owner.CurrentSkill != null && _owner.CurrentSkill.CoolDownTimeMax>0)
			{
				_shouldGotoNextHit = false;
			}
			if(!_owner.ACOwner.IsPlayer && _attackDelayTime >0.01f && _owner.CurrentSkill != null && _owner.CurrentSkill.SkillModule._canCacheCombo)
			{
				_shouldGotoNextHit = false;
			}
			if(_currentState != AttackBase.ATTACK_STATE.ALL_DONE)
			{
				_nextAttackIdx =  FCConst.UNVIABLE_ATTACK_INDEX;
				if(_attackConditions != null && _shouldGotoNextHit && _attackCanSwitch)
				{
					int ret = -1;
					foreach(AttackConditions acd in _attackConditions)
					{
						if(acd._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.ON_PARRY && _owner.IsOnParry != FC_PARRY_EFFECT.NONE)
						{
							//this means combo id 
							if(_owner.IsOnParry == (FC_PARRY_EFFECT) acd._conVal)
							{
								_nextAttackIdx = acd._jumpIdx;
								break;
							}
						}
						else if(acd._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.HIT_TARGET)
						{
							if(_hitTargetCount >0)
							{
								_nextAttackIdx = acd._jumpIdx;
								break;
							}
							
						}
						else if(acd._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.TO_END)
						{
							//this means we should end the combo hit
							ret = 256;
							break;
						}
						else if(acd._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.TO_SKILL_END
							&& _jumpToSkillEnd)
						{
							_jumpToSkillEnd = false;
							if(_owner.CurrentSkill != null)
							{
								_nextAttackIdx = _owner.CurrentSkill.ComboHitMax-1;
							}
							else
							{
								ret = FCConst.MAX_ATTACK_INDEX;
							}
							break;
						}
						else if(acd._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.COMMON_CONDITION
							&& _owner.ConditionValue != AttackConditions.CONDITION_VALUE.NONE)
						{
							if(_owner.ConditionValue == acd._value)
							{
								_nextAttackIdx = acd._jumpIdx;
								break;
							}
						}
						else if(acd._attCon == AttackConditions.ATTACK_JUMP_CONDITIONS.CHANCE_GO_NEXT)
						{
							int ir = Random.Range(0,100);
							if(ir <= acd._conVal)
							{
								_nextAttackIdx = acd._jumpIdx;
								break;
							}
						}
						
					}
					if(ret != FCConst.UNVIABLE_ATTACK_INDEX && _nextAttackIdx == FCConst.UNVIABLE_ATTACK_INDEX)
					{
						_nextAttackIdx = ret;
					}
				}
				if(
#if DEVELOPMENT_BUILD || UNITY_EDITOR					
					!CheatManager.cheatForCostNoEnergy && 
#endif					
					_energyCostTime == EnergyCostTime.END
					&& _attackCanSwitch
					&& _shouldGotoNextHit
					&& !_willNotCostEnergy)
				{
					_owner.ACOwner.CostEnergy(-_energyCost, 1);
				}
			
				_currentState = AttackBase.ATTACK_STATE.ALL_DONE;
				_owner.ACOwner.ACStop();
				_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
				_owner.ACOwner.ACRestoreToDefaultSpeed();
				_owner.AttackTaskChange(FCCommand.CMD.STATE_FINISH);
			}
		}
	}
	
	public virtual void ClearEffect()
	{
		
	}

	protected override void OnDestroy()
	{
		if(_soundsPlaying != null && SoundManager.Instance != null)
		{
			foreach(AudioSource ads in _soundsPlaying)
			{
				SoundManager.Instance.StopSoundEffect(ads, -1);
			}
		}
	}
	
	public virtual void AttackQuit()
	{
		_owner.ACOwner.FireTarget = null;
		ClearEffect();
		_owner.ClearHitBackList();
		foreach(AudioSource ads in _soundsPlaying)
		{
			SoundManager.Instance.StopSoundEffect(ads, _soundStopDelayTime);
		}
		_soundsPlaying.Clear();
		_currentDamageType = FC_DAMAGE_TYPE.NONE;
		//stop bind effect
		foreach(BindEffectInfo effInfo in _bindEffects)
		{
			if (effInfo.IsPlaying)
			{
				//is playing, stop it?
				CharacterEffectManager.Instance.StopEffect(effInfo._bindEffect,
					_owner.ACOwner._avatarController,
					-1);
				effInfo.IsPlaying = false;
			}
		}
		if(_moveByManual)
		{
			_owner.ACOwner.ACRevertToDefalutMoveParams();
			_owner.ACOwner.ACStop();
		}
		_attackConditions = null;
		_nextAttackIdx = FCConst.UNVIABLE_ATTACK_INDEX;
		if(_mmEffectMap != null)
		{
			_mmEffectMap.StopEffect();
		}
		_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);
		_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
		_owner.ACOwner.BulletsOfCurrentAttack.Clear();
		//this code must before _owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		_owner.ACOwner.SetAniMoveSpeedScale(1);
		_isOnSeeking = false;
		if(_haveNoColliderWhenAttack)
		{
			ResetColliderWithOther();
		}
		if(_hasRigidBody2)
		{
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
		}
		_owner.ACOwner.CurrentAngleSpeed = _orgAngleSpeed;
		if(_owner.ACOwner.IsPlayerSelf)
		{
			_owner.AttackComboLastTime = FCConst.ATTACK_COMBO_LAST_TIME;
		}
		else
		{
			if(!_isFinalAttack && _owner.CurrentSkill != null && _owner.CurrentSkill.SkillModule._canCacheCombo)
			{
				_owner.AttackComboLastTime = _attackDelayTime;
			}
			else
			{
				_owner.AttackComboLastTime = 0;
			}
		}
		_owner.ACOwner.ACSetRaduis(_owner._bodyRadius);
		if(_isFinalAttack || (_owner.AIStateAgent._nextState.CurrentStateID != AIAgent.STATE.ATTACK ))
		{
			_owner.SuperArmorSelf.BreakArmor(FCConst.SUPER_ARMOR_LVL2);
		}
		if(_owner.ACOwner.IsClientPlayer)
		{
			_owner._updateAttackRotation = false;
			_owner._updateAttackPos = false;
		}
		_owner.UpdateNetCommand(AIAgent.COMMAND_DONE_FLAG.ATTACK_IS_OVER);
	}

	public virtual bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		if(_owner.IsInAttackEndDelay && isPress)
		{
			return true;
		}
		return false;	
	}
	
	protected virtual bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return false;
	}
	//must override by sub class
	public virtual bool AttackKeyEvent(FC_KEY_BIND ekb, bool isPress)
	{
		if(_canInterruptByOther 
			&& _owner.ACOwner.IsPlayerSelf 
			&& ekb != FC_KEY_BIND.DIRECTION 
			&& ekb != FC_KEY_BIND.NONE)
		{
			if(_currentBindKey == FC_KEY_BIND.NONE &&_attackCanSwitch )
			{
				return false;
			}
			else
			{
#if FC_AUTHENTIC
                if((_owner.KeyAgent.CompareTwoSkillPriority(ekb, _currentBindKey) >0 && _owner.ACOwner.AniGetAnimationNormalizedTime() >= _timeCanBeInterrupt)
                || (_owner.KeyAgent.CompareTwoSkillPriority(ekb, _currentBindKey) <= 0 && _attackCanSwitch))
				{
					return false;
				}
#else
                if (ekb != _currentBindKey)
				{
					return false;
				}
#endif
			}
			
		}
		return AKEvent(ekb, isPress);	
	}
	public virtual void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
		if(_fireInfos != null && _fireInfos.Length != 0)
		{
			foreach(FireportInfo fpi in _fireInfos)
			{
				if(fpi.FirePortIdx<0 && fpi._firePortName != "")
				{
					fpi.FirePortIdx = _owner.ACOwner.ACGetFirePortIndex(fpi._firePortName);
				}
			}
		}

	}
	
	public virtual void AniBulletIsFire()
	{
		if(_needAttackCorrect && _owner.TargetAC != null && _owner._isRanger)
		{
			FC_DANGER_LEVEL edl = _owner.GetTargetDangerLevel(_safeDistance, _dangerDistance);
			if(edl == FC_DANGER_LEVEL.VERY_DANGER)
			{
				_owner.FaceToTarget( _owner.TargetAC, true );
			}
			else if(edl == FC_DANGER_LEVEL.DANGER)
			{
				ActionController ac = ActionControllerManager.Instance.GetEnemyTargetBySight
				(_owner.ACOwner.ThisTransform,20 ,0,_owner.ACOwner.Faction
						,_owner._attackCorrectAngleForRangerMin,_owner._attackCorrectAngleForRangerMax,true);
				if(ac == null)
				{
					_owner.FaceToTarget( _owner.TargetAC, true );
				}
			}
		}
		if(_mmEffectMap != null)
		{
			_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_FIREBULLET,_owner.ACOwner, _fireInfos[_currentPortIdx ]._firePortName);
		}
		if(_fireInfos !=null && _fireInfos[_currentPortIdx ]._sfxWhenFireBullet != "")
		{
			SoundManager.Instance.PlaySoundEffect(_fireInfos[_currentPortIdx ]._sfxWhenFireBullet);
		}
		if(_currentPortIdx < _fireInfos.Length-1)
		{
			_currentPortIdx++;
		}
		else if(_loopFireIndex)
		{
			_currentPortIdx = 0;
		}
	}
	
	public virtual void AniBeforeBulletFire()
	{
		
	}
	
	public virtual void IsHitTarget(ActionController ac,int sharpness)
	{
		if(ac != null && ac.CanShakeWhenBeHit)
		{
			if(_mmEffectMap != null)
			{
				_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_HIT_TARGET,_owner.ACOwner,null);
			}
		}
		if(_forceNextWhenHitTarget)
		{
			_timeForceNextCount = _timeForceNext;
		}
		_hitTargetCount++;
		if(_hasSlashDumping)
		{
			if(_sharpnessWithAttack != 65535)
			{
				sharpness = _sharpnessWithAttack;
			}
			_owner.ACOwner.ACSlowDownAttackAnimation(sharpness,ac.BodyHardness,_hitTargetCount,_dumpReduce);
		}
		if(_needAttackCorrect && !_owner._isRanger)
		{
#if FC_AUTHENTIC
			Vector3 v1 = ac.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
			v1.y =0;
			v1.Normalize();
			if(v1 != Vector3.zero)
			{
				float angle = Vector3.Angle(_owner.ThisTransform.forward,v1);
				if(angle < _owner._attackCorrectAngleForMelee*2/3)
				{
					_owner.ACOwner.ACRotateTo(v1,-1,true,true);
				}
			}
#endif //FC_AUTHENTIC
			if(_owner.ACOwner.IsPlayerSelf)
			{
				if(_owner.TargetAC == null)
				{
					_owner.TargetAC = ac;
				}
			}
		}
	}

    public virtual void HitTargetIsCrit(ActionController ac)
    {
        if (ac != null && ac.CanShakeWhenBeHit)
        {
            if (_mmEffectMap != null)
            {
                _mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_HIT_CRIT, _owner.ACOwner, null);
            }
        }
    }
	
	public virtual bool HandleHitByTarget(ActionController ac, bool isBullet)
	{
		_owner.IsOnParry = FC_PARRY_EFFECT.NONE;
		return false;
	}
	public void AniIsOver()
	{
		if(_frameCount>=4)
		{
			_frameCount = 0;
			_currentAnimationCount++;
			if(!_owner.IsInAttackEndDelay)
			{
				AniOver();
			}
		}
	}
	
	//should override by sub class
	protected virtual void AniOver()
	{
		
	}
	public virtual bool IsStopAtPoint()
	{
		return false;
	}
	
	protected virtual void OverrideFirePort(RangerAgent.FirePort firePort, SkillData skillData) {
		firePort._rangeInfo._param1 = skillData.CurrentLevelData.attackRange;
		if(firePort._rangeInfo._param1<=1.1f)
		{
			firePort._rangeInfo._param1 = 5;
		}
		firePort._rangeInfo._effectTime = skillData.CurrentLevelData.attackEffectTime;
		firePort.DamageScale = firePort.DamageScaleSource * skillData.CurrentLevelData.damageScale;
		firePort._fireCount = _skillData.CurrentLevelData.bulletCount;
		firePort.DotDamageTime = _skillData.CurrentLevelData.dotDamageTime;
		firePort._attribute1 = skillData.CurrentLevelData.attribute1;
		firePort._attribute2 = skillData.CurrentLevelData.attribute2;
		firePort.AttackCount = skillData.CurrentLevelData.attackNumber;
	}
	
	//can override by sub class, 
	public virtual bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = true;
		if(skillData != null)
		{
			_owner = owner as AIAgent;
			if(_skillData != skillData && skillData != null)
			{
				_skillData = skillData;
			}
			_damageScale = _damageScaleSource *skillData.CurrentLevelData.damageScale;
			if(_fireInfos != null && _fireInfos.Length != 0)
			{
				foreach(FireportInfo fpi in _fireInfos)
				{
					if(/*fpi.FirePortIdx < 0 && */fpi._firePortName != "")
					{ 
						fpi.FirePortIdx = _owner.ACOwner.ACGetFirePortIndex(fpi._firePortName);
						RangerAgent.FirePort fp = _owner.ACOwner.ACGetFirePort(fpi.FirePortIdx);
						//if(!fp.IsOverride)
						{
							OverrideFirePort(fp, skillData);
							fp.IsOverride = true;
						}
					}
				}
			}
			InitEnergyCost(_owner.ACOwner.Data.TotalReduceEnergy);
		}
		else
		{
			ret = false;
			_damageScale = _damageScaleSource;
		}
		return ret;
	}
	
	
	protected void InitEnergyCost(float reduce_energy)
	{
		_energyCost = _energyCostOrg;
		if(_skillData != null)
		{
			int retCost = (int)(_energyCost * _skillData.CurrentLevelData.cost);
			if(reduce_energy >= retCost &&  retCost > 0)
			{
				_energyCost = (int)(_energyCost *(1.0f) *_skillData.CurrentLevelData.cost - reduce_energy);
				if(_energyCost <= 0)
				{
					_energyCost = 1;
				}
			}
			else if(retCost > 0)
			{
				_energyCost = (int)(_energyCost *(1.0f) *_skillData.CurrentLevelData.cost - reduce_energy);
			}
		}
		
	}
	private int _layer = 0;
	public void NoColliderWithOther()
	{
		_layer = _owner.ACOwner.ThisObject.layer;
		_owner.ACOwner.ThisObject.layer = LayerMask.NameToLayer("FLASH");
		_owner.ACOwner.ACSetRaduis(0);
	}
	
	public void ResetColliderWithOther()
	{
		_owner.ACOwner.ThisObject.layer = _layer;
	}

	protected virtual void UpdateBindEffects()
	{
		//play and stop effect depends on the time
		foreach(BindEffectInfo effInfo in _bindEffects)
		{
			
			float currentAnimPercent = _owner.ACOwner.AniGetAnimationNormalizedTime();
			if (_attackActiveCount > 0)
			{
				currentAnimPercent += _attackActiveCount -1 ;
			}
			
			if (effInfo.IsPlaying)
			{
				//is playing, stop it?
				if (currentAnimPercent > effInfo._endPercent)
				{
					CharacterEffectManager.Instance.StopEffect(effInfo._bindEffect,
						_owner.ACOwner._avatarController,
						-1);
					effInfo.IsPlaying = false;
				}
			}
			else if (!effInfo.HasPlayed)
			{
				//not playing, if I have not played it before, play it?
				if ((currentAnimPercent >= effInfo._startPercent) && (currentAnimPercent < effInfo._endPercent))
				{
					CharacterEffectManager.Instance.PlayEffect(effInfo._bindEffect,
						_owner.ACOwner._avatarController,
						-1);
					effInfo.IsPlaying = true;
					effInfo.HasPlayed = true;
				}
			}
		}		
	}
	
}
