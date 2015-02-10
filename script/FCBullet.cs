using UnityEngine;
using System.Collections;

[System.Serializable]
public class BulletHitInfo
{
	public string _name;
	
	//speed of bullet at begin
	public float _shotSpeed;
	//speed of bullet at final
	public float _finalSpeed;
	public float _accelerate;
	//
	public float _lifeTime;
	
	//if true, means bullet can penetrate target to hit more
	public bool _canPenetrate;
	
	//max targets can hit in one frame
	public int _maxHitPerTime;
	//max targets can penetrate
	public int _maxHitTotal;
	
	public AttackHitType _hitType;
	
	//collide with wall or ground
	public Collider _physicalCollider;
	
	
	public Eot[] _eots;
	//means bullet will enable hit target after fire
	public bool _enableDamageAfterFire = true;
}

[AddComponentMenu("FC/Logic/FCObject/Bullet/EWBullet")]
public class FCBullet : FCObject,AttackUnit {
	
	//fire points for bullets born with self
	protected Transform[] _firePoints = null;
	public enum SEEK_LEVEL
	{
		NONE,
		NORMAL,
		PRO
	}
	public enum BOMB_POSITION
	{
		AT_DEAD,
		AT_HIT_WALL,
		AT_HIT_GROUND,
		AT_HIT_TARGET,
	}
	public string _name;
	//if true, means bullet can be destroy by ac
	public bool _controlByAttack = true;
	
	//max distance for can seek target
	public float _maxTargetingDistance = 20.0f;
	
	//ranger bullet and normal bullet have diff fire function and diff behaviour
	protected bool _isRangerBullet = false;
	//public Render;
	public BulletHitInfo[] _hitInfo;
	//means bullet only effect the hurt type of enemy ,will not reduce their hp
	public bool _enableDamage = true;
	
	protected FC_AC_FACTIOH_TYPE _faction;

	public GameObject _agents;
	//default hit point on target
	protected EnumEquipSlot _targetSolt = EnumEquipSlot.belt;
	protected MoveAgent _moveAgent;
	protected ActionController _target;
	protected ActionController _owner;
	
	protected int _step;
	
	//org fire point
	protected Transform _firePoint;
	protected bool _inState;
	protected BulletDisplayer[] _bulletDisplayers = null;
	protected int _currentEffectIndex = -1;
	protected bool _isFrom2P;
	protected AttackInfo _attackInfo = new AttackInfo();
	
	protected float _deadTime = -1f;
	
	public FC_DAMAGE_TYPE _damageType = FC_DAMAGE_TYPE.PHYSICAL;
	
	//this can only changed by monster
	public float _damageScale = 1f;
	
	public string _sfxFireName = "";
	public SEEK_LEVEL _seekLevel = SEEK_LEVEL.NORMAL;
	
	public BOMB_POSITION _bombPosition = BOMB_POSITION.AT_DEAD;
	protected BOMB_POSITION _curBombPosition = BOMB_POSITION.AT_DEAD;
	public string _firePortLifeOver = "";
	public string _firePortLifeIn = "";
	
	public int _pushStrength = 0;
	public float _pushTime = 0.2f;
	
	public bool _pushByPoint = true;
	public float _pushAngle = 270;
	
	public bool _hasGEffect = false;
	public Vector3 _gDirection = -Vector3.up;
	public float _gSpeed = 5;
	public bool _needNotHitID = false;
	public int _jumpCount = 0;
	public bool _canHitAll = false;
	
	public MessageReciever _damageReceiver = null;
	
	//if push by weapon, need target in front of ac
	
	
	private int _firePortLifeOverIdx = -1;
	private int _firePortLifeInIdx = -1;
	public RangerAgent.FirePort[] _firePorts = null;
	public bool _deadByLifeOver = false;
	private float _lifeTime = 0;
	
	protected bool _isDead = false;
	
	protected string _firePortName;
	
	protected int _fireAngleY = 0;
	protected bool _enableSpeedY = false;
	
	public enum SHAKE_POS
	{
		NONE,
		AT_BEGIN,
		AT_HIT_TARGET,
		AT_HIT_GROUND,
		AT_DEAD,
	}
	
	public SHAKE_POS _shakePos = SHAKE_POS.NONE;
	protected bool _haveShakeLoop = false;
	
	public EnumShakeEffect _shakeLevel = EnumShakeEffect.none;
	
	public bool IsRangerBullet
	{
		get
		{
			return _isRangerBullet;
		}
	}
	
	public bool IsDead
	{
		get
		{
			return _isDead;
		}
	}
	//when set zero ,means need to explode,so dont set it to zero in child class
	public float LifeTime
	{
		set
		{
			_lifeTime = value;
		}
		get
		{
			return _lifeTime;
		}
	}
	
	public float DamageScale
	{
		get
		{
			return _damageScale;
		}
		set
		{
			_damageScale = value;
		}
	}
	
	public float DeadTime
	{
		get
		{
			return _deadTime;
		}
		set
		{
			_deadTime = value;
		}
	}
	
	public FC_AC_FACTIOH_TYPE Faction
	{
		get
		{
			return _faction;
		}
		set
		{
			_faction = value;
		}
	}
	
	public ActionController Owner
	{
		get
		{
			return _owner;
		}
	}
	
	protected override void Awake ()
	{
		base.Awake ();
		if(_firePorts != null)
		{
			_firePoints = new Transform[(int)EnumEquipSlot.MAX];
			for(int i = 0; i< _firePoints.Length ; i++)
			{
				string node = "a";
				char b = (char)('a'+i);
				node = node.Replace('a', b);
				GameObject gb = Utils.GetNode(node, ThisObject);
				if(gb != null)
				{
					_firePoints[i] = gb.transform;
				}
			}
		}
		ObjectID.ObjectType = FC_OBJECT_TYPE.OBJ_BULLET;
		_bulletDisplayers = ThisObject.GetComponents<BulletDisplayer>();
		if(_agents != null)
		{
			_moveAgent = _agents.GetComponent<MoveAgent>();
			if(_moveAgent != null)
			{
				_moveAgent.Init(this);
			}		
		}
		_attackInfo._hitType = AttackHitType.HurtTiny;
		_attackInfo._damageType = FC_DAMAGE_TYPE.NONE;
		_attackInfo._effectTime = 4f;
		if(_firePorts != null)
		{
			int i= 0;
			foreach(RangerAgent.FirePort fr in _firePorts)
			{
				fr.RefreshPorts(this);
				if(fr._portName == _firePortLifeOver)
				{
					_firePortLifeOverIdx = i;
				}
				if(fr._portName == _firePortLifeIn)
				{
					_firePortLifeInIdx = i;
				}
				i++;
				
			}
		}
	}
	
	public FCWeapon.WEAPON_HIT_TYPE GetAttackerType()
	{
		if(_owner.AIUse._aiType == FC_AI_TYPE.PLAYER_MAGE)
		{
			return FCWeapon.WEAPON_HIT_TYPE.ENERGY;
		}
		else if(_owner.AIUse._aiType == FC_AI_TYPE.PLAYER_MONK)
		{
			return FCWeapon.WEAPON_HIT_TYPE.BLUNT;
		}
		return FCWeapon.WEAPON_HIT_TYPE.SLASH;
	}
	
	public bool IsBelongToFirePort(string fpName)
	{
		return _firePortName == fpName;
	}
	public virtual void Init(FCObject owner)
	{
		_owner = owner as ActionController;
		_faction = _owner.Faction;
		if(_shakeLevel == EnumShakeEffect.shake3_loop)
		{
			_haveShakeLoop = true;
		}
		else
		{
			_haveShakeLoop = false;
		}
	}
	
	public virtual bool CanHit(ActionController ac)
	{
		return true;
	}
	
	// must override by bullet ranger
	public virtual void FireRanger(ActionController target,Transform firePoint, RangerAgent.FirePort rfp)
	{
		_isDead = false;
		_attackInfo._isFromSkill = rfp.IsFromSkill;
		_firePortName = rfp._portName;
	}
	
	//must not be override by any other child class		
	public void Fire(ActionController target,Transform firePoint,int angleOffset, int angleOffsetY,float lifeTime, RangerAgent.FirePort rfp)
	{		
		_isDead = false;
		_attackInfo._isFromSkill = rfp.IsFromSkill;
		
		_lifeTime = lifeTime;
		_deadTime = -1;
		_target = target;
		_isFrom2P = _owner.IsClientPlayer;
		ThisObject.layer = (int)_faction+1;
		_firePoint = firePoint;
		_step = 0;
		_attackInfo._attackPoints = _owner.TotalAttackPoints;
		_attackInfo._criticalChance = _owner.Data.TotalCritRate;
		_attackInfo._criticalDamage = _owner.Data.TotalCritDamage;
        _attackInfo._skillTriggerChance = _owner.Data.TotalSkillTriggerRate;
        _attackInfo._skillAttackDamage = _owner.Data.TotalSkillAttackDamage;
		_attackInfo._pushStrength = _pushStrength;
		_attackInfo._pushTime = _pushTime;
		_attackInfo._pushAngle = _pushAngle;
		_attackInfo._pushByPoint = _pushByPoint;
		RangerAgent.FireRangeInfo fri = rfp._rangeInfo;
		if(angleOffsetY != 0)
		{
			_enableSpeedY = true;
			_fireAngleY = angleOffsetY;
		}
		if(fri != null)
		{
			_attackInfo._effectTime = fri._effectTime;
		}
		//_damageScale = rfp.DamageScale;
		if(!_owner.IsPlayer &&  _owner.ACGetCurrentAttack() != null)
		{
			_attackInfo._damageScale = _damageScale*_owner.ACGetCurrentAttack().DamageScale;
		}
		else
		{
			_attackInfo._damageScale = _damageScale*rfp.DamageScale;
		}
		if(_firePorts != null && _firePorts.Length != 0)
		{
			foreach(RangerAgent.FirePort pt in _firePorts)
			{
				pt.IsFromSkill = rfp.IsFromSkill;
				pt.DamageScale = _attackInfo._damageScale;
			}
		}
		
		ThisTransform.localPosition = firePoint.position;
		if(rfp._shootByFirePointDirection)
		{
			ThisTransform.forward = firePoint.forward;
		}
		else if(target != null)
		{
			if(target.ThisTransform.localPosition != ThisTransform.localPosition)
			{
				Vector3 d3 = target.ThisTransform.localPosition - ThisTransform.localPosition;
				d3.y = 0;
				ThisTransform.forward = d3;
			}
			else
			{
				ThisTransform.forward = _owner.ThisTransform.forward;
			}
		}
		else
		{
			ThisTransform.forward = _owner.ThisTransform.forward;
		}
		if(angleOffset != 65535)
		{
			ThisTransform.Rotate(new Vector3(0,angleOffset,0));
		}
		if(rfp.IsOverride && _hitInfo != null && _hitInfo.Length>0)
		{
			foreach(Eot eot in _hitInfo[_step]._eots)
			{
				eot.lastTime = rfp.DotDamageTime;
			}
		}
		//add record to remeber ther port name
		// for if some one want to kill bullet
		_firePortName = rfp._portName;
		ActiveLogic(rfp);
	}
	
	
	protected virtual IEnumerator STATE()
	{
		_inState = true;
		while(_inState)
		{
			yield return null;
		}
	}
	
	//some kind of may want to do some thing before bullet fire
	//should be override by sub class
	protected virtual void ActiceLogicSelf(RangerAgent.FirePort rfp)
	{
		
	}
	
	public void EffectByGravity(float force, Transform gCenter, Vector3 gDirection, float gTime, bool isPoint,bool realG)
	{
		if(realG)
		{
			if(force >0)
			{
				_moveAgent.IsOnGround = false;
			}
			_moveAgent._gBeReal = true;
			_moveAgent._jumpCount = _jumpCount;
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
			_moveAgent._gEnabled = true;
			_moveAgent._gForce = force;
			_moveAgent._gEffectTime = gTime;
			_moveAgent._gCenter = gCenter;
			_moveAgent._gIsPoint = isPoint;
			_moveAgent._gDirection = gDirection;
		}
		
	}
	
	public void DisableGEffect()
	{
		_moveAgent._gEnabled = false;
	}
	
	protected void ActiveLogic(RangerAgent.FirePort rfp)
	{
		if(GameManager.Instance.IsPVPMode){  
			_canHitAll = true;	
		}
		
		_curBombPosition = BOMB_POSITION.AT_DEAD;
		if(_moveAgent != null)
		{
			_moveAgent.enabled = true;
		}
		if(_bulletDisplayers != null)
		{
			if(_bulletDisplayers.Length == 1)
			{
				_currentEffectIndex = 0;
			}
			else if(_bulletDisplayers.Length > 1)
			{
				_currentEffectIndex = Random.Range(0,_bulletDisplayers.Length-1);
			}
			if(_currentEffectIndex >=0)
			{
				_bulletDisplayers[_currentEffectIndex].StartEffect();
			}
		}
		if(_sfxFireName != "")
		{
			Debug.Log("bullet +" + _sfxFireName);
			SoundManager.Instance.PlaySoundEffect(_sfxFireName);
		}
		ActiceLogicSelf(rfp);
		if(_firePortLifeInIdx > -1)
		{
			FireByPort(_firePortLifeInIdx);
		}
		if(_hasGEffect)
		{
			EffectByGravity(_gSpeed,null,_gDirection,999,false,false);
		}
		if(_damageReceiver != null && !_needNotHitID)
		{
			_damageReceiver._hitID = GameManager.GainHitID();
		}
		if(_shakePos == SHAKE_POS.AT_BEGIN)
		{
			CameraController.Instance.StartCameraEffect(EnumCameraEffect.none,_shakeLevel,false);
		}
		StartCoroutine(STATE());
	}
	public virtual ActionController GetOwner()
	{
		return _owner;
	}

	public virtual void GetHurtDirection(Transform targetTransform,ref Vector3 direction)
	{
		direction = targetTransform.position -ThisTransform.localPosition;
		direction.y =0;
		direction.Normalize();
		if(direction == Vector3.zero)
		{
			direction = -targetTransform.forward;
		}
	}
	
	public override bool HandleCommand(ref FCCommand ewd)
	{
		switch(ewd._cmd)
		{
			case FCCommand.CMD.ATTACK_HIT_WALL:
				if(!_deadByLifeOver)
				{
					_curBombPosition = BOMB_POSITION.AT_HIT_WALL;
					Dead();
				}

				break;
			case FCCommand.CMD.ATTACK_HIT_GROUND:
				if(_shakePos == SHAKE_POS.AT_HIT_GROUND)
				{
					CameraController.Instance.StartCameraEffect(EnumCameraEffect.none,_shakeLevel,false);
				}
				if(_moveAgent != null)
				{
					_moveAgent.IsOnGround = true;
				}
				if(!_deadByLifeOver && _moveAgent.IsOnGround)
				{
					_curBombPosition = BOMB_POSITION.AT_HIT_GROUND;
					
					Dead();
				}
				
				break;
			case FCCommand.CMD.ATTACK_HIT_TARGET:
				{
					ActionController target = ewd._param1 as ActionController;
					IsHitSomeone(_owner, this, _damageType, target);
				}			
				break;
		}
		return true;
	}
	
	public void IsHitSomeone(ActionController owner, AttackUnit attackUnit, FC_DAMAGE_TYPE damageType, ActionController target)
	{
		HandleHitTarget.HandleHit(owner, attackUnit, damageType, target);
		_curBombPosition = BOMB_POSITION.AT_HIT_TARGET;
		if(_shakePos == SHAKE_POS.AT_HIT_TARGET)
		{
			CameraController.Instance.StartCameraEffect(EnumCameraEffect.none,_shakeLevel,false);
		}
	}
	//must override by all child class
	public virtual void Dead()
	{
		if(!_isDead)
		{
			if(_firePortLifeInIdx > -1 
				&& (_bombPosition == BOMB_POSITION.AT_DEAD
				|| _bombPosition == _curBombPosition))
			{
				StopFireByPort(_firePortLifeInIdx);
			}
			_isDead = true;
			_deadTime = -1;
			if(_bombPosition == _curBombPosition)
			{
				FireByPort(_firePortLifeOverIdx);
			}
			if(_bulletDisplayers != null && _currentEffectIndex >=0)
			{
				_deadTime = _bulletDisplayers[_currentEffectIndex].EndEffect();
			}
			if(_moveAgent != null)
			{
				DisableGEffect();
				_moveAgent.enabled = false;
			}
			_owner.ReturnBulletToPool(this);
			if(_shakePos == SHAKE_POS.AT_DEAD)
			{
				CameraController.Instance.StartCameraEffect(EnumCameraEffect.none,_shakeLevel,false);
			}
			if(_haveShakeLoop)
			{
				CameraController.Instance.StopCameraEffect();
			}
		}
		if(_damageReceiver != null)
		{
			_damageReceiver.DeActiveLogic();
		}

	}
	
	public bool IsFrom2P()
	{
		return _isFrom2P;
	}
	
	public Transform ACGetTransformByName(EnumEquipSlot pName)
	{
		if(_firePoints != null)
		{
			return _firePoints[(int)pName];
		}
		else
		{
			return ThisTransform;
		}
	}
	
	public AttackInfo GetAttackInfo()
	{
		return _attackInfo;
	}
	
	public void FireByPort(int portIdx)
	{
		if(_firePorts != null && _firePorts.Length != 0 && portIdx >=0)
		{
			RangerAgent.FireBullet(_firePorts, portIdx, -1, _owner);
		}
	}
	
	public void StopFireByPort(int portIdx)
	{
		if(_firePorts != null && _firePorts.Length != 0 && portIdx >=0)
		{
			RangerAgent.StopFire(_firePorts[portIdx]);
		}
	}
	public int GetFinalDamage(DefenseInfo di,out bool isCriticalHit, ActionController target)
	{
		isCriticalHit = false;
		if(_enableDamage)
		{
			int ret = 0;
			ret = DamageCounter.Result(_attackInfo, di, out isCriticalHit, _owner, target);
			if(isCriticalHit)
			{
				_owner.IsCriticalHit = isCriticalHit;
			}
			return ret;
		}
		return 0;
	}

    public Eot[] GetEots(DefenseInfo di)
    {
        if (_hitInfo != null && _hitInfo.Length > 0)
        {
            foreach (Eot eot in _hitInfo[_step]._eots)
            {
                eot.IsFrom2p = _isFrom2P;

                switch (eot.eotType)
                {
                    case Eot.EOT_TYPE.EOT_PHYSICAL:
                        eot.OwnerDamage = _attackInfo._attackPoints[0] - di._defensePoints[0];
                        break;
                    case Eot.EOT_TYPE.EOT_FIRE:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1]; 
                        break;
                    case Eot.EOT_TYPE.EOT_ICE:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    case Eot.EOT_TYPE.EOT_THUNDER:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    case Eot.EOT_TYPE.EOT_POISON:
                        eot.OwnerDamage = _attackInfo._attackPoints[1] - di._defensePoints[1];
                        break;
                    default:
                        eot.OwnerDamage = _attackInfo._attackPoints[0] - di._defensePoints[0];
                        break;
                }
            }
            return _hitInfo[_step]._eots;
        }
        return null;
    }
	
	public int GetSharpness()
	{
		return 0;
	}
	
	public FC_OBJECT_TYPE GetObjType()
	{
		return ObjectID.getOnlyObjectType;
	}
	
	public Transform GetAttackerTransform()
	{
		return ThisTransform;
	}
	
}
