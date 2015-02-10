using UnityEngine;
using System.Collections;

//fire parry attack
public class AttackParryFire : AttackBase 
{
#if WQ_CODE_WIP
	private float _timeCount;
#endif
	public FC_GLOBAL_EFFECT _attackEffect = FC_GLOBAL_EFFECT.INVALID;
	
	public float _slowTimeLast;
	protected float _beginTime;
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}
	
	protected override void AniOver()
	{
		AttackEnd();
	}
	public override void AttackEnter()
	{
		base.AttackEnter();
		if(_owner.ParryTarget != null && _owner.ACOwner.IsPlayerSelf)
		{
#if FC_authentic
			Vector3 dir = _owner.ParryTarget.ThisTransform.position - _owner.ACOwner.ThisTransform.position;
			dir.y = 0;
			dir.Normalize();
			if(dir != Vector3.zero)
			{
				_owner.ACOwner.ACRotateToDirection(ref dir, true);
				if(_currentBindKey != FC_KEY_BIND.NONE)
				{
					if(!_owner.KeyAgent.keyIsPress(FC_KEY_BIND.DIRECTION))
					{
						_owner.KeyAgent._directionWanted = dir;
					}
				}
			}
#endif //FC_authentic
			_owner.ParryTarget = null;

		}
		_owner.SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
#if WQ_CODE_WIP
		_timeCount = 0f;
#endif
		
		_owner.ACOwner.CanAcceptTimeScale = true;
		_beginTime = Time.realtimeSinceStartup;
		
		Transform trans = Utils.FindTransformByNodeName(_owner.ACOwner.ThisTransform, "B");
		if (trans == null)
            Debug.LogError("PARRY_EFFECT_POS is null");
		
		Vector3 pos = trans.position;			
		GlobalEffectManager.Instance.PlayEffect(_attackEffect, pos);	
		
		_owner.ACOwner.ACFire(FirePortIdx);
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		
	}
	
	public override bool InitSkillData(SkillData skillData, AIAgent owner)
	{
		bool ret = base.InitSkillData(skillData, owner);
		if(ret)
		{
			RangerAgent.FirePort fp = _owner.ACOwner.ACGetFirePort(_fireInfos[0].FirePortIdx);
			fp._fireCount = 1;
		}
		return ret;
	}

	
	public override void AttackUpdate()
	{
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			base.AttackUpdate();
		}
	}
	
	public override void IsHitTarget(ActionController ac,int sharpness)
	{
		//_owner.ACOwner.ACSlowDownAnimation();
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}
	
	public override void AttackQuit()
	{
		if(_owner.ACOwner.CanAcceptTimeScale)
		{
			_owner.ACOwner.CanAcceptTimeScale = false;
			
		}
		if(!GameManager.Instance.GamePaused)
		{
            Time.timeScale = GameSettings.Instance.TimeScale;
		}
		//stop effect
		
		_owner.ACOwner.ACAniSpeedRecoverToNormal();
		
		base.AttackQuit();
	}
	
}
