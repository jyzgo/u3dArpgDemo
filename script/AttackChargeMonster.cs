using UnityEngine;
using System.Collections;

public class AttackChargeMonster : AttackBase {

	public float _timeForCharge = 0;
	public string _chargeSound = "";
	public string _chargeFullSound = "";
	protected float _timeCount;
	
	public ChargeEffect[] _chargeEffect = null;
	
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}
	
	public override void AttackEnter()
	{
		//Debug.Log("AttackEnter");
		base.AttackEnter();
		_timeCount = 0;

		if(_timeForCharge < Mathf.Epsilon)
		{
			_timeForCharge = 2f;
		}
		if(_chargeSound != "")
		{
			SoundManager.Instance.PlaySoundEffect(_chargeSound,true);
		}
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		_makeSkillCD = false;
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
		
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			_timeCount += Time.deltaTime;
			if(_timeCount>=_timeForCharge)
			{
				_attackCanSwitch = true;
				_shouldGotoNextHit = true;
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
		return true;
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
		
	}
	
	public override void AttackQuit()
	{
		if(_chargeSound != "")
		{
			SoundManager.Instance.StopSoundEffect(_chargeSound);
		}
		
		if(_chargeEffect != null)
		{
			for(int i =0;i<_chargeEffect.Length;i++ )
			{
				//stop effect
				ChargeEffect chargeEffectLastLevel = _chargeEffect[i];

				foreach(FC_CHARACTER_EFFECT effInfo in chargeEffectLastLevel._chargeEffect)
				{
					CharacterEffectManager.Instance.StopEffect(effInfo, 
						_owner.ACOwner._avatarController, -1.0f);
				}				
			}

		}
		base.AttackQuit();
	}
	public override bool IsStopAtPoint()
	{
		return false;
	}
}
