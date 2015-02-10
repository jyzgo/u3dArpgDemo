using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HardnessAndSharpness
{
	// when be hit, need to slow down target hit speed.
	//this is base for Hardness equal Sharpness
	static protected float _hitSpeedDown = 30;
	static protected float _hitSpeedUp = 70;
	static protected float _hitSpeedMin = 0.02f;
	static protected float _hitSpeedMinTime = 0.05f;
	static protected float _hitSpeedMinLastTime = 0.05f;
	static protected int _sharpnessIncreaseByHit = 80;
	
	static public void SetAniSpeed(int sharpness, int hardness, AniSpeedAgent asa ,int hitTargetCount, float dumpReduce)
	{
		sharpness = sharpness + (int)(hitTargetCount*_sharpnessIncreaseByHit*dumpReduce);
		if(sharpness == 9999)
		{
			
		}
		else if( hardness <= 0)
		{
			
		}
		else
		{
			bool antiBlade = false;
			if(sharpness <= 0 || sharpness - hardness <= -100)
			{
				sharpness = 1;
				
			}
			float ret = sharpness - hardness;
			ret = Mathf.Clamp(ret,-100,100f);
			ret = ret /100;
			float hitSpeedDown = _hitSpeedDown-ret*_hitSpeedDown;
			if(hitSpeedDown < _hitSpeedDown/2)
			{
				hitSpeedDown = _hitSpeedDown/2;
			}
			float hitSpeedUp = _hitSpeedUp + _hitSpeedUp*(ret/2);
			float hitSpeedMin = _hitSpeedMin;
			if(ret >0)
			{
				hitSpeedMin = _hitSpeedMin + (ret)/10;
			}
			float SpeedMinTime =  _hitSpeedMinTime - ret*_hitSpeedMinTime;
			float SpeedMinLastTime =  _hitSpeedMinLastTime - ret*_hitSpeedMinLastTime;
			if(antiBlade == true)
			{
				SpeedMinLastTime = 0.1f;
				hitSpeedDown = 1000;
				hitSpeedMin = 0.1f;
				hitSpeedUp = 1000;
			}
			asa.SlowDownAnimation(hitSpeedDown,hitSpeedUp,hitSpeedMin,SpeedMinTime,SpeedMinLastTime, dumpReduce);
		}	
	}
}


[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AI/FCHurtAgent")]
public class FCHurtAgent : MonoBehaviour , FCAgent {

	AIAgent _owner;
	Vector3 _hitDirection;
	protected AttackHitType _currentHitType;
	protected AttackHitType _nextHitType;
	protected float _effectTime;
	protected ActionController _target = null;
	public float _tinyHurtSpeed = 0;
	public float _normalHurtSpeed = 5f;
	public float _flashHurtSpeed = 3f;
	public float _hurtSpeedDrag = 5f;
	public float _hitBackSpeed = 9;
	public float _hitBackDrag = 3;
	public float _hitUpSpeed = 4;
	
	public float _hitFlyLastTime = 0.6f;
	public float _parryBackSpeed = 3;
	public float _parryFailSpeed = 9;
	public string _sfxParryBreak = "";
	
	public static int _hitHurtStrength = 30;
	public static float _hitHurtTime = 0.1f;
	
	public static int _hitKnockStrength = 50;
	public static float _hitKnockTime = 0.1f;
	
	protected bool _animationIsOver = true;
	
	protected int _frameCount = 0;
	
	//means when after this time, ac will stop move
	//1 means 100% of time for the animation
	public int _bodyHardness = 100;
	
	public bool _canBeHitFly = true;
	public bool _canBeHitUp = true;
	public bool _canBeHitDown = true;
	
	public bool _haveRandomNormalHurt = false;
	
	protected AIAgent.STATE _stateWantToGo = AIAgent.STATE.NONE;
	protected bool _isInFall = false;
	
	public AIAgent.STATE StateWantToGo
	{
		get
		{
			return _stateWantToGo;
		}
	}
	
	public Vector3 HitDirection
	{
		set
		{
			_hitDirection = value;
		}
		get
		{
			return _hitDirection;
		}
	}
	public bool AnimationIsOver
	{
		get
		{
			return _animationIsOver;
		}
		set
		{
			_animationIsOver = value;
		}
	}
	
	//if _insky = true , means knock animation is not over
	protected bool _inSky = false;
	
	//when hurt ,if attacker need shake, if true ,then the camera will shake
	public bool _canBeShake = false ;
	
	protected AttackHitType _hitTypeShouldGo = AttackHitType.None;
	
	int _currentStep =0;
	protected float _hitStrength = 100;
	public float HitStrength
	{
		get
		{
			return _hitStrength;
		}
		set
		{
			_hitStrength = value;
		}
	}
	protected FCWeapon.WEAPON_HIT_TYPE _attackerType;
	
	public float EffectTime
	{
		get
		{
			return _effectTime;
		}
		set
		{
			_effectTime = value;
		}
		
	}
	public AttackHitType CurrentHitType
	{
		get
		{
			return _currentHitType;
		}
		set
		{
			_currentHitType = value;
		}
	}
	
	public AttackHitType NextHitType
	{
		get
		{
			return _nextHitType;
		}
		set
		{
			_nextHitType = value;
		}
	}
	
	public Quaternion _hurtInfoForClient;
	public Quaternion _hurtInfoForClient2;
	
	
	public void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
		_currentHitType = AttackHitType.None;
		_attackerType = FCWeapon.WEAPON_HIT_TYPE.NONE;
		_hurtInfoForClient = Quaternion.identity;
		_hurtInfoForClient2 = Quaternion.identity;
	}
	
	public bool CanBeHit(AttackUnit aut)
	{
        bool hasInSuper = _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
        bool hasInSuper2 = _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN2);
        bool hasInGod = _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_GOD);

		if(_owner.ACOwner.IsAlived
            && !hasInSuper
            && !hasInSuper2
            && !hasInGod)
		{
			aut.GetHurtDirection(_owner.ACOwner.ThisTransform,ref _hitDirection);
			//Debug.Log("GetHurtDirection = " + _hitDirection);
			return true;
		}
		else
		{
			if(_owner.ACOwner._onHit != null)
			{
				_owner.ACOwner._onHit((int)LevelManager.VarNeedCount.ATTACK_IGNORE);
			}
		}

        Debug.Log(string.Format("InSuper = {0}, InSuper2 = {1}, InGod = {2}", hasInSuper, hasInSuper2, hasInGod));

		return false;
	}

    public AttackHitType GetHitType(AttackUnit aut, int realDamage, int currentHP)
    {
        AttackInfo aif = aut.GetAttackInfo();

        AttackHitType eht = aif._hitType;

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
        if (!_canBeHitDown)
        {
            eht = AttackHitType.KnockBack;
        }
        if (_owner.IsOnParry == FC_PARRY_EFFECT.FAIL)
        {
            eht = AttackHitType.ParryFail;
            _owner.IsOnParry = FC_PARRY_EFFECT.NONE;
        }

#if xingtianbo
        //if(eht == AttackHitType.Normal)
        //{
        //    eht = AttackHitType.HURT_NORMAL;
        //}
#endif
        AttackHitType expectHitType = _currentHitType;
        if (_nextHitType != AttackHitType.None)
        {
            expectHitType = _nextHitType;
        }
        if (eht >= AttackHitType.HurtNormal && realDamage >= currentHP)
        {
            if (currentHP > 0
                && expectHitType != AttackHitType.Dizzy
                && expectHitType != AttackHitType.KnockDown
                && !_owner.ACOwner.IsPlayer)
            {
                if (eht != AttackHitType.HitFly)
                {
                    eht = AttackHitType.KnockDown;
                }
            }
            else
            {
                eht = AttackHitType.None;
            }
        }
        else if (expectHitType == AttackHitType.HitFly)
        {

        }
        else
        {
            if (((eht == AttackHitType.None)
            || _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY) && eht == AttackHitType.HurtNormal)
            || _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2)
            || (eht < expectHitType && eht != AttackHitType.ParrySuccess && expectHitType != AttackHitType.ForceBack)
            || (eht == AttackHitType.Dizzy && expectHitType == AttackHitType.Dizzy)
            || (eht == AttackHitType.KnockDown && expectHitType == AttackHitType.KnockDown)
            || (eht != AttackHitType.KnockDown && eht != AttackHitType.HitFly && expectHitType == AttackHitType.KnockDown)
            || _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_DEEP_FREEZE)
            || (_owner.AIStateAgent.CurrentStateID == AIAgent.STATE.STAND && eht == AttackHitType.HitFly)
            || _owner.HasActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_EOT_GODDOWN))
            {
                eht = AttackHitType.None;
            }
            if (eht == AttackHitType.ParrySuccess &&
                (expectHitType == AttackHitType.HitFly
                || expectHitType == AttackHitType.KnockDown
                || expectHitType == AttackHitType.Dizzy))
            {
                eht = AttackHitType.None;
            }
        }
        if (eht != AttackHitType.None)
        {
            _nextHitType = eht;
            _attackerType = aut.GetAttackerType();
            _effectTime = aif._effectTime;
            _hitStrength = aut.GetOwner().CharacterStrength;
            //base value is 100
        }
        if (eht == AttackHitType.BlackHole)
        {
            _target = aut.GetOwner();
        }
        return eht;


    }







    public int CountHurtResult()
    {
        _currentStep = 0;
        _frameCount = 0;
        _stateWantToGo = AIAgent.STATE.NONE;
        float speedScale = ((float)_hitStrength) / 100;

#if PVP_ENABLED
        if (GameManager.Instance.IsPVPMode && _owner.ACOwner.IsClientPlayer)
        {
            //speedScale = 0.0f;
        }
#endif
        if (_owner.ACOwner.IsPlayerSelf && GameManager.Instance.IsPVPMode)
        {
            _hurtInfoForClient2.x = _hitDirection.x;
            _hurtInfoForClient2.y = _hitDirection.z;
            _hurtInfoForClient2.z = _effectTime;

            CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.CLIENT_HURT,
                            _owner.ACOwner.ObjectID, _owner.ACOwner.ThisTransform.localPosition,
                            _hurtInfoForClient, FC_PARAM_TYPE.QUATERNION,
                            _hurtInfoForClient2, FC_PARAM_TYPE.QUATERNION,
                            null, FC_PARAM_TYPE.NONE);
        }

        if (!_owner.ACOwner.SelfMoveAgent.IsOnGround)
        {
            speedScale += 0.2f;
        }
        if (_currentHitType == AttackHitType.KnockDown)
        {
            _inSky = true;
            _owner.MoveByDirection(_hitDirection, _hitBackSpeed * speedScale, 99f, _hitBackDrag);
            if (_canBeHitDown)
            {
                _stateWantToGo = AIAgent.STATE.STAND;
            }
        }
        else if (_currentHitType == AttackHitType.KnockBack)
        {
            _owner.MoveByDirection(_hitDirection, _hitBackSpeed * speedScale, 99f, _hitBackDrag);
            if (_canBeHitFly)
            {
                _stateWantToGo = AIAgent.STATE.IDLE;
            }
        }
        else if (_currentHitType == AttackHitType.HitFly)
        {
            _inSky = true;
            _isInFall = false;
            if (_canBeHitUp)
            {
                //Debug.LogError("FC_HIT_TYPE.UP" + Time.realtimeSinceStartup);
                //_owner.MoveByDirection(_hitDirection, _hitBackSpeed * speedScale, 1.2f, 0);
                //_owner.MoveByDirection(_hitDirection, 0.1f, 0.1f, 0);

                _owner.MoveByDirection(_hitDirection, _hitBackSpeed * speedScale, 99f, _hitBackDrag);

                _stateWantToGo = AIAgent.STATE.STAND;
                _owner.ACOwner.ACEffectByGravity(_hitUpSpeed, null, Vector3.up, 999, false, true);
            }
            else
            {
                _owner.MoveByDirection(_hitDirection, _hitBackSpeed * speedScale, 99f, _hitBackDrag);
            }
        }
        else if (_currentHitType == AttackHitType.HurtFlash)
        {
            _owner.MoveByDirection(_hitDirection, _flashHurtSpeed * speedScale, 1f, _hurtSpeedDrag);
        }
        else if (_currentHitType == AttackHitType.HurtNormal)
        {
            //Debug.Log("FC_HIT_TYPE.HURT_NORMAL _hitDirection = " + _hitDirection + ", _normalHurtSpeed :" +_normalHurtSpeed + ",speedScale : " + speedScale + " , _hurtSpeedDrag =" +_hurtSpeedDrag );
            _owner.MoveByDirection(_hitDirection, _normalHurtSpeed * speedScale, 1f, _hurtSpeedDrag);
        }
        else if (_currentHitType == AttackHitType.ParrySuccess)
        {
            _owner.MoveByDirection(_hitDirection, _parryBackSpeed * speedScale, 1f, _hurtSpeedDrag);
        }
        else if (_currentHitType == AttackHitType.ParryFail)
        {
            if (_sfxParryBreak != "")
            {
                SoundManager.Instance.PlaySoundEffect(_sfxParryBreak);
            }
            _owner.MoveByDirection(_hitDirection, _parryFailSpeed * speedScale, 0.3f, 6f);
        }
        else if (_currentHitType == AttackHitType.Dizzy)
        {
            _owner.StopMove();
        }
        else if (_currentHitType == AttackHitType.ForceBack)
        {
            //			if(_owner.ACOwner.IsClientPlayer)
            //			{
            //				_owner.MoveByDirection(_hitDirection,_normalHurtSpeed*speedScale,0.2f,_hurtSpeedDrag);
            //			}
            //			else
            //			{
            _owner.StopMove();
            //		}
        }
        else if (_currentHitType == AttackHitType.BlackHole)
        {
            _owner.StopMove();
        }
#if xingtianbo
        //else if(_currentHitType == AttackHitType.KnockDown)
        //{
        //    _owner.StopMove();
        //    _stateWantToGo = AIAgent.STATE.STAND;
        //}
#endif
        else
        {
            _owner.StopMove();
        }
        _currentStep = 1;
        _owner.IncreaseRage(RageAgent.RageEvent.HURT, _attackerType);

        return (int)_currentHitType;
    }
	
	public void HurtUpdate()
	{
		//1 means hurt logic can update
		_frameCount++;
		if(_currentStep == 1)
		{
			if(_currentHitType == AttackHitType.Dizzy
			|| _currentHitType == AttackHitType.BlackHole)
			{
				_effectTime -= Time.deltaTime;
				if(_effectTime <0)
				{
					LifeTimeIsOver();
				}
			}
			else
			{
				if(_currentHitType == AttackHitType.HitFly && _owner.ACOwner.SelfMoveAgent.IsNearGround && !_isInFall)
				{
					_owner.AnimationSwitch._aniIdx = FC_All_ANI_ENUM.hurt_flyup_end;
					_owner.ACOwner.ACPlayAnimation(_owner.AnimationSwitch);
					_isInFall = true;
					_animationIsOver = false;
					_frameCount = 0;
				}
                if (_animationIsOver)
                {
                    if (_currentHitType == AttackHitType.KnockDown
                        || _currentHitType == AttackHitType.KnockBack)
                    {
                        //if(!_inSky)
                        {
                            LifeTimeIsOver();
                        }
                    }
                    else if (_currentHitType == AttackHitType.HurtNormal
                        || _currentHitType == AttackHitType.HurtFlash)
                    {
                        if (_owner.ACOwner.SelfMoveAgent.IsStop())
                        {
                            LifeTimeIsOver();
                        }
                    }
                    else
                    {
                        LifeTimeIsOver();
                    }
                }
                else
                {
                    if (_currentHitType == AttackHitType.KnockDown)
                    {
                        if (_owner.ACOwner.AniGetAnimationNormalizedTime() > _hitFlyLastTime)
                        {
                            _owner.StopMove();
                        }
                    }
                }
			}
		}
	}
	
	public void ClearHurtState()
	{
		_currentHitType = AttackHitType.None;
		_nextHitType = AttackHitType.None;
	}
	
	public void LifeTimeIsOver()
	{
		_currentStep = 2;
		_owner.HurtTaskChange(FCCommand.CMD.STATE_FINISH);
	}
	public bool HandleInnerCmd(FCCommand.CMD cmd,object param0,object param1,object param2,object param3)
	{
		bool result = false;
		if(_currentStep>0 && _currentStep<2)
		{
			switch(cmd)
			{
			case FCCommand.CMD.STOP_IS_ARRIVE_POINT:
				
				break;
				
			}
		}
		return result;
	}

    public bool ACIsStopAtPoint()
    {
        if (_currentHitType == AttackHitType.KnockDown
            || (_currentHitType == AttackHitType.HitFly && _owner.ACOwner.SelfMoveAgent.IsOnGround))
        {
            _inSky = false;
            return true;
        }
        return false;
    }
	
	public void AniIsOver()
	{
		if(_currentHitType != AttackHitType.Dizzy 
			&& _currentHitType != AttackHitType.BlackHole
            && (_currentHitType != AttackHitType.HitFly
            || (_currentHitType == AttackHitType.HitFly
			&& (_isInFall || (!_isInFall && _owner.ACOwner.SelfMoveAgent.IsOnGround))))
			&& _frameCount >2)
		{
			_animationIsOver = true;
			
		}
	}
	
	
	
	List<ActionController>  _hitBackList = new List<ActionController>();
	
	public void AddHitBackList(ActionController ac)
	{
		if(!_hitBackList.Contains(ac))
		{
			_hitBackList.Add(ac);
			//Debug.Log("AddHitBackList");
		}
	}
	
	public Vector3 _finalPoint = Vector3.zero;
    //bool _hasFinalPoint = false;
	
	public void RemoveHitBackList(ActionController ac)
	{
		_hitBackList.Remove(ac);
		//Debug.Log("RemoveHitBackList");
	}
	
	public void ClearHitBackList()
	{
		_hitBackList.Clear();
		//Debug.Log("ClearHitBackList");
	}
	
	public void HurtStateQuit()
	{
		_animationIsOver = false;
	}
	
	public void UpdadeHitBackList(ref Vector3 motion)
	{
		foreach(ActionController ac in _hitBackList)
		{
			if(ac.IsAlived)
			{
				if(_owner.CurrentAttack != null)
				{
					if(GameManager.Instance.IsPVPMode){
						Vector3 v = _owner.ACOwner.ThisTransform.localPosition - ac.ThisTransform.localPosition;
						v.Normalize();
						ac.SelfMoveAgent.SetPosition(_owner.ACOwner.ThisTransform.localPosition +  v * 2);
					}else{
						ac.SelfMoveAgent.Move(motion);
					}
				}
				
			}
		}
	}
	
	
	
}
