using UnityEngine;
using System.Collections;

public class AttackSevenPunch : AttackBase {
	
	
	public float _start = 0;
	public float _end = 0.9f;
	public int _angleSpeed = 500;
	
	public string _startSound;
	public float _startSoundStartTime = 0.1f;
	private bool _startSoundFlag = false;
	
	public string _loopSound;
	public float _loopSoundStartTime = 0.2f;
	
	private float _timer = 0;
	private AudioSource _loopSoundAudio = null;
	private bool _loopSoundFlag = false;
	
	public override void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
	}
	
	public override void AttackEnter()
	{
		_loopSoundFlag = false;
		_startSoundFlag = false;
		
		base.AttackEnter();		
		_timer = SkillData.CurrentLevelData.skillTime;
	
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		if(_owner.ACOwner.IsClientPlayer)
		{
			_owner._updateAttackRotation = true;
		}
		
	}
	
	public override void AttackActive (bool beActive)
	{
		base.AttackActive (beActive);
		if(beActive)
		{
			_owner.ACOwner.ACFire(FirePortIdx);
		}
	}
	public override void AttackUpdate()
	{
		base.AttackUpdate();
	
		_timer -= Time.deltaTime;
		
		float animationPercent =  1.0f -  _timer / SkillData.CurrentLevelData.skillTime;
		
		if(!_startSoundFlag)
		{
			if(animationPercent >= _startSoundStartTime)
			{
				if(_startSound != null && _startSound != "")
				{
					SoundManager.Instance.PlaySoundEffect(_startSound);
				}
				_startSoundFlag = true;
			}
		}
		
		
		if(!_loopSoundFlag)
		{
			if(animationPercent >= _loopSoundStartTime)
			{
				if(_loopSound != null && _loopSound != "")
				{
					_loopSoundAudio = SoundManager.Instance.PlaySoundEffect(_loopSound, true);
				}
				_loopSoundFlag = true;
			}
		}
		
		if(_timer < 0)
		{
			if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
			{
				_currentState = AttackBase.ATTACK_STATE.STEP_2;
				AttackEnd();
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
		_shouldGotoNextHit = true;
		_attackCanSwitch = true;
		
		if(SkillData.CurrentLevelData.effect == 0)
		{
			_owner.ConditionValue = AttackConditions.CONDITION_VALUE.SEVEN_END1;
		}
		else if(SkillData.CurrentLevelData.effect == 1)
		{
			_owner.ConditionValue = AttackConditions.CONDITION_VALUE.SEVEN_END2;
		}
		else {
			_owner.ConditionValue = AttackConditions.CONDITION_VALUE.SEVEN_END3;
		}
		
		if(_loopSoundAudio != null)
		{
			_loopSoundAudio.Stop();
			_loopSoundAudio = null;
		}
		
		base.AttackEnd();
	}
	
	public override bool DirectionKeyEvent(Vector3 direction, bool isPress)
	{
		if(isPress)
		{
			float animationPercent =  _timer / SkillData.CurrentLevelData.skillTime;
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
		
		if(_loopSoundAudio != null)
		{
			_loopSoundAudio.Stop();
			_loopSoundAudio = null;
		}
	}
	
}
