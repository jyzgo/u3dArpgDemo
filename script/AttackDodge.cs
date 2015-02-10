using UnityEngine;
using System.Collections;

public class AttackDodge : AttackBase {
	
	private FC_CHARACTER_EFFECT _godEffect = FC_CHARACTER_EFFECT.INVALID;// FC_CHARACTER_EFFECT.DASH_MODEL2;
	
	public float _start = 0;
	public float _end = 0.9f;
	public int _angleSpeed = 500;
	private float _godTimer = 0;
	
	public override void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
	}
	
	public override void AttackEnter()
	{
		_godTime = SkillData.CurrentLevelData.godTime;
		_godTimer = _godTime;
		
		base.AttackEnter();
		if(GameManager.Instance.IsPVPMode)
		{
			_owner.EotAgentSelf.ClearEot(Eot.EOT_TYPE.EOT_SPEED);
		}
		_owner.ACOwner.ACSetRaduis(0);
		if(_sfxName != null && _sfxName != "")
		{
			SoundManager.Instance.PlaySoundEffect(_sfxName);	
		}
		if(_owner.ACOwner.IsClientPlayer)
		{
			_owner._updateAttackRotation = true;
		}
		
		_owner.ACOwner._avatarController.SuperManColor(_godTimer);
		
		_aniMoveSpeedScale = SkillData.CurrentLevelData.speed;
		
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
	
		CharacterEffectManager.Instance.PlayEffect(_godEffect ,_owner.ACOwner._avatarController, -1);		
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
				
		
		if(_godTimer > 0)
		{
			_godTimer -= Time.deltaTime;
			if(_godTimer <= 0)
			{
				CharacterEffectManager.Instance.StopEffect(_godEffect ,_owner.ACOwner._avatarController, -1);
			}
		}
		

		
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			if(_owner.ACOwner.AniGetAnimationNormalizedTime() > 0.95f)
			{
				_currentState = AttackBase.ATTACK_STATE.STEP_2;
				AttackEnd();
			}
			else{
				if(_attackCanSwitch && _currentBindKey != FC_KEY_BIND.NONE && _owner.KeyAgent.ActiveKey == FC_KEY_BIND.DIRECTION )
				{
					AttackEnd();
				}	
			}
		}
	}
	
	
	public override void IsHitTarget(ActionController ac,int sharpness)
	{
		base.IsHitTarget(ac,sharpness);
	}
	

	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		return true;
	}

	public override void AttackEnd()
	{
		CharacterEffectManager.Instance.StopEffect(_godEffect ,_owner.ACOwner._avatarController, -1);
		
		/*
		_shouldGotoNextHit = true;
		_attackCanSwitch = true;
		
		if(SkillData.CurrentLevelData._effect == 0)
		{
			_owner.ConditionValue = AttackConditions.CONDITION_VALUE.DODGE_END1;
		}else{
			_owner.ConditionValue = AttackConditions.CONDITION_VALUE.DODGE_END2;
		}
		*/
			
		base.AttackEnd();
	}
	
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		if(isPress)
		{
			float animationPercent =  _owner.ACOwner.AniGetAnimationNormalizedTime();
			if(animationPercent > _start
				&& animationPercent < _end)
			{
				_owner.ACOwner.ACMoveToDirection(ref direction,_angleSpeed);
			}
			return true;
		}
		return true;	
	}
	
	public override bool IsStopAtPoint()
	{
		return true;
	}
	
	public override void AttackQuit()	
	{
		base.AttackQuit();
		_owner.ACOwner.ACRevertToDefalutMoveParams();
	}
	

}
