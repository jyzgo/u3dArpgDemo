using UnityEngine;
using System.Collections;

public class AttackParry : AttackBase {
	
	public float _parryTime = 0;
	protected float _currentParryTime = 0;
	public int _parrySuccessCost = 3;
	public int _parryCost = 3;
	//if true ,mean in recoil.so cant rotate
	public bool _isRecoil = false;
	protected bool _canParry = true;
	
	public bool _canParryBullet = false;
	
	public FC_GLOBAL_EFFECT _attackEffect = FC_GLOBAL_EFFECT.INVALID;
	public FC_CHARACTER_EFFECT _parryEffect =  FC_CHARACTER_EFFECT.INVALID;
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		//we must name the module of parry attack to ParryFire and ParrySuccess and ParryEnd
		_parryCost = _owner.ACOwner.ACGetAttackByName("ParryFire")._energyCost;
		_parrySuccessCost = _owner.ACOwner.ACGetAttackByName("ParrySuccess")._energyCost;
	}
	
	protected override void AniOver()
	{
		if(_isRecoil)
		{
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
			if(_owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
			{
				_owner.ACOwner.ACRotateToDirection(ref _owner.KeyAgent._directionWanted, false);
			}
		}
		
	}
	public override void AttackEnter()
	{
		base.AttackEnter();
		if(!_isRecoil)
		{
			_owner.ParryTarget = null;
		}
		else
		{
			_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
			if(_owner.ParryTarget != null)
			{
				Vector3 dir = _owner.ParryTarget.ThisTransform.position - _owner.ACOwner.ThisTransform.position;
				dir.y = 0;
				dir.Normalize();
				if(dir != Vector3.zero)
				{
					_owner.ACOwner.ACRotateToDirection(ref dir, false);
				}
				_owner.ParryTarget = null;
			}
		}
		_canParry = true;
		_currentParryTime = _parryTime;
		
		if(_currentParryTime <= Mathf.Epsilon)
		{
			_canParry = false;
			_currentState = AttackBase.ATTACK_STATE.STEP_2;
		}
		//play effect
		
		Transform trans = Utils.FindTransformByNodeName(_owner.ACOwner.ThisTransform, "B");
		if (trans == null)
            Debug.LogError("PARRY_EFFECT_POS is null");
		
		Vector3 pos = trans.position;			
		GlobalEffectManager.Instance.PlayEffect(_attackEffect, pos);	
		
		CharacterEffectManager.Instance.PlayEffect(_parryEffect ,_owner.ACOwner._avatarController, -1);
		
		if(_sfxName != "")
		{
			SoundManager.Instance.PlaySoundEffect(_sfxName);
		}
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
	}
	
	public override void AttackUpdate()
	{
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1 || _currentState == AttackBase.ATTACK_STATE.STEP_2 )
		{
			base.AttackUpdate();
			if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
			{
				_currentParryTime -= Time.deltaTime;
				if(_currentParryTime <=0 && _owner.IsOnParry == FC_PARRY_EFFECT.NONE)
				{
					_currentState = AttackBase.ATTACK_STATE.STEP_2;
					_shouldGotoNextHit = true;
					
					CharacterEffectManager.Instance.StopEffect(_parryEffect ,_owner.ACOwner._avatarController, -1);
				}
			}
	
			if(_owner.IsOnParry != FC_PARRY_EFFECT.NONE)
			{
				_shouldGotoNextHit = true;
				AttackEnd();
			}
			else if(!_owner.KeyAgent.keyIsPress(_currentBindKey) && _currentParryTime <= 0)
			{
				if(_owner.ACOwner.IsPlayerSelf)
				{
					CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ACTION_CANCEL,_owner.ACOwner.ObjectID,
						_owner.ACOwner.ThisTransform.localPosition,
						null,
						FC_PARAM_TYPE.NONE,
						null,
						FC_PARAM_TYPE.NONE,
						null,
						FC_PARAM_TYPE.NONE);
					_attackCanSwitch = true;
					_shouldGotoNextHit = true;
					AttackEnd();
				}
			}
		}
		
	}
	
		
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		if(isPress)
		{
			_owner.ACOwner.ACRotateTo(direction,-1,true);	
		}
		return true;	
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1
			|| _currentState == AttackBase.ATTACK_STATE.STEP_2)
		{
			return true;
		}
		return false;
	}
	
	public override bool HandleHitByTarget(ActionController ac,bool isBullet)
	{
		bool ret = false;
		Vector3 dir = ac.ThisTransform.localPosition -  _owner.ACOwner.ThisTransform.localPosition;
		dir.y = 0;
		if((ac.ACGetCurrentAttack() != null && ac.ACGetCurrentAttack()._ignoredParry))
		{
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
		}
		else if(_owner.IsOnParry == FC_PARRY_EFFECT.PARRY 
			|| _owner.IsOnParry == FC_PARRY_EFFECT.PARRY_BULLET_FAIL 
			|| _owner.IsOnParry == FC_PARRY_EFFECT.SUCCESS)
		{
			ret = true;
		}
		else if(_currentState == AttackBase.ATTACK_STATE.STEP_1 
			&& _canParry && _parrySuccessCost <= _owner.ACOwner.Energy 
			&& _isFirstAttack
			&& (_canParryBullet
			|| (!_canParryBullet && !isBullet)))
		{
			_owner.IsOnParry = FC_PARRY_EFFECT.SUCCESS;
			dir.Normalize();
			if(dir != Vector3.zero)
			{
				_owner.ACOwner.ACRotateToDirection(ref dir, false);
				_owner.ParryTarget = ac;
				_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
			}
			ret = true;
		}
		else if(_currentState == AttackBase.ATTACK_STATE.STEP_2 && _parryCost <= _owner.ACOwner.Energy)
		{
			if((isBullet && !_canParryBullet))
			{
				_owner.IsOnParry = FC_PARRY_EFFECT.PARRY_BULLET_FAIL;
			}
			else
			{
				_owner.IsOnParry = FC_PARRY_EFFECT.PARRY;
			}
			dir.Normalize();
			if(dir != Vector3.zero)
			{
				_owner.ACOwner.ACRotateToDirection(ref dir, false);
				_owner.ParryTarget = ac;
				_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
			}
			_attackCanSwitch = true;
			ret = false;
			
		}
		else
		{
			_owner.ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
			_owner.IsOnParry = FC_PARRY_EFFECT.FAIL;
			
			Transform trans = Utils.FindTransformByNodeName(_owner.ACOwner.ThisTransform, "node_right_weapon");
			if (trans == null)
                Debug.LogError("PARRY_EFFECT_POS is null");
		
			Vector3 pos = trans.position;			
			GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.PARRY_FAIL, pos);	
			
		}
		if(ret)
		{
			_attackCanSwitch = true;
		}
		return ret;
	}
	
	public override bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = base.InitSkillData(skillData, owner);
		if(ret)
		{
			//> 0 means can block bullet
			if(skillData.CurrentLevelData.specialAttackType>0)
			{
				_canParryBullet = true;
			}
		}
		return ret;
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
		
		CharacterEffectManager.Instance.StopEffect(_parryEffect ,_owner.ACOwner._avatarController, -1);
	}
	
	public override void AttackQuit()
	{
		base.AttackQuit();
	}
}
