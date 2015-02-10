using UnityEngine;
using System.Collections;

public class AttackChargeFire : AttackBase {
#if WQ_CODE_WIP	
	private float _timeCount = 0;
#endif
	protected int _currentLevel;
	
	public bool _endByTime = false;
	public bool _canMoveOnAttack = false;
	
	//if true, means if the attack will last until ac has no energy
	public bool _holdUntilNoEnergy = false;
	// means if hold key ,energy will cost 10/s
	public float _energyCostForHold = 10f;
	
	public float _lastTime = 5f;
	public bool _aniSpeedChange = false;
	protected float _timeCounter = 5f;
	protected float energyCostTotal = 0f;
	
	public FC_CHARACTER_EFFECT _attackEffect = FC_CHARACTER_EFFECT.INVALID;
	
	
	protected float _aniSpeedBegin = 0.9f;
	protected float _aniSpeedCur = 0.9f;
	protected float _aniSpeedEnd = 1.6f;
	protected FCBullet _bhBullet = null;
	
	public override void AttackEnter()
	{
		//Debug.Log("AttackEnter");
		base.AttackEnter();
		if(!_canMoveOnAttack)
		{
			//means fire spear
			_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
		}
		else
		{
			//means fire wind
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
			_owner.ACOwner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			_owner.ACOwner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
			if(_owner.ACOwner.IsClientPlayer)
			{
				_owner._updateAttackPos = true;
				_owner._attackMoveSpeed = _owner.ACOwner.Data.TotalMoveSpeed * _speedScale;
			}
			
			
		}
		_timeCounter = _lastTime;
		_aniSpeedCur = _aniSpeedBegin;
		if(_attackEffect != FC_CHARACTER_EFFECT.INVALID)
		{
			//play effect
			CharacterEffectManager.Instance.PlayEffect(_attackEffect, 
				_owner.ACOwner._avatarController,
				-1);
		}
		energyCostTotal = 0;
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();

		
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			if(_timeCounter >0)
			{
				if(_aniSpeedChange )
				{
					if(_aniSpeedCur<_aniSpeedEnd)
					{
						_aniSpeedCur+= (Time.deltaTime)/3;
					}
					else
					{
						_aniSpeedCur = _aniSpeedEnd;
					}
				}
				if(!_owner.ACOwner.IsInSlowDump)
				{
					_owner.ACOwner.AniSetAnimationSpeed(_aniSpeedCur);
				}
				if(_canMoveOnAttack)
				{
					if(_owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
					{
						_owner.MoveByDirection(_owner.KeyAgent._directionWanted,5,9999f);
					}
					
				}
				
				_timeCounter -= Time.deltaTime;
				if(_owner.ACOwner.IsPlayerSelf)
				{
					if(_owner.KeyAgent.keyIsPress( _currentBindKey ) && _timeCounter <=0 && _holdUntilNoEnergy)
					{
						
						float energyCost = _energyCostForHold*Time.deltaTime;
						if(
#if DEVELOPMENT_BUILD || UNITY_EDITOR							
							!CheatManager.cheatForCostNoEnergy && 
#endif
							!_willNotCostEnergy)
						{
							energyCostTotal+=energyCost;
							if(energyCostTotal >= 1)
							{
								_owner.ACOwner.CostEnergy(-((int)energyCostTotal), 1);
								energyCostTotal -= (int)energyCostTotal;
							}
							
							if(_owner.ACOwner.Energy>0.01f)
							{
								_timeCounter = 0.01f;
							}
						}
						else
						{
							_timeCounter = 0.01f;
						}
					}
				}
				
			}
			else
			{
				_attackCanSwitch = true;
				if(_isFinalAttack)
				{
					_shouldGotoNextHit = false;
				}
				else
				{
					_shouldGotoNextHit = true;
				}
				AttackEnd();
			}
		}
	}
	
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		return true;	
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			if(isPress == true && ekb == _currentBindKey)
			{
				_owner.AttackCountAgent.NextSkillID = _owner.AttackCountAgent.CurrentSkillID;
			}
			return true;
		}

		return false;	
	}
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}
	
	protected override void AniOver()
	{
		if(!_endByTime)
		{
			AttackEnd();
		}	
	}
	
	public override bool IsStopAtPoint()
	{
		return false;
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
		_owner.ACOwner.ACEndCurrentAttackEffect(true);
	}
	
	public override bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = base.InitSkillData(skillData, owner);
		if(ret)
		{
			_lastTime = skillData.CurrentLevelData.skillTime;
		}
		return ret;
	}
	public override void AttackQuit()
	{
		if(_attackEffect != FC_CHARACTER_EFFECT.INVALID)
		{
			//stop effect
			CharacterEffectManager.Instance.StopEffect(_attackEffect, 
				_owner.ACOwner._avatarController, 1);
		}
		if(_bhBullet != null)
		{
			_bhBullet.Dead();
		}
		if(_owner.ACOwner.IsClientPlayer)
		{
			if(_owner._updateAttackPos)
			{
				_owner.ACOwner.ACStop();
			}
			//_owner._attackMoveSpeed = _owner.ACOwner.Data.TotalMoveSpeed * _speedScale;
		}
		_bhBullet = null;
		base.AttackQuit();
	}
	
}
