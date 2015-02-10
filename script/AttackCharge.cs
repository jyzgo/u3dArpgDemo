using UnityEngine;
using System.Collections;

[System.Serializable]
public class ChargeEffect
{
	public FC_CHARACTER_EFFECT[] _chargeEffect = null;
}


public class AttackCharge : AttackBase {
	
	public float _timeCount = 0;
	public string _chargeSound = "";
	public string _chargeFullSound = "";
	protected int _currentLevel;
	protected float _timeCountCharge;
	protected float _timeToShowCharge = 0.1f;
	public int _chargeLvl = 2;
	protected int _currentChargeLvl = 0;
	protected float _timeChargeStep = 0;
	
	public ChargeEffect[] _chargeEffect = null;
	
	public bool _needAimHelp = false;
	
	
	public override void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
	}
	
	public override void AttackEnter()
	{
		//Debug.Log("AttackEnter");
		base.AttackEnter();
		_timeCountCharge = 0;
		_timeChargeStep = _timeCount/_chargeLvl;
		
		//_owner.ShowCharge(0.0f);
		_owner.ACOwner.CurrentSpeed = 0;
		_currentChargeLvl = 0;
		if(_timeCount < Mathf.Epsilon)
		{
			_timeCount = 2f;
		}
		if(_chargeSound != "")
		{
			SoundManager.Instance.PlaySoundEffect(_chargeSound,true);
		}
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		_makeSkillCD = false;
		if(_needAimHelp)
		{
			CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.HERO_AIMING, _owner.ACOwner._avatarController, -1);
		}
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
		
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			_timeCountCharge += Time.deltaTime;
			if(_timeCountCharge>=_timeChargeStep && _currentChargeLvl<_chargeLvl)
			{
				_timeCountCharge -= _timeChargeStep;
				if(_currentChargeLvl>0)
				{
					//stop last level effect
					ChargeEffect chargeEffectLastLevel = _chargeEffect[_currentChargeLvl-1];
					
					Assertion.Check(chargeEffectLastLevel != null);
					foreach(FC_CHARACTER_EFFECT effInfo in chargeEffectLastLevel._chargeEffect)
					{
						CharacterEffectManager.Instance.StopEffect(effInfo, 
							_owner.ACOwner._avatarController, 0.1f);
					}
				}
				_currentChargeLvl++;
				
				//play this level effect
				if(_chargeEffect != null && _chargeEffect.Length != 0)
				{
					ChargeEffect chargeEffectThisLevel = _chargeEffect[_currentChargeLvl-1];
					Assertion.Check(chargeEffectThisLevel != null);
					foreach(FC_CHARACTER_EFFECT effInfo in chargeEffectThisLevel._chargeEffect)
					{
						CharacterEffectManager.Instance.PlayEffect(effInfo, 
							_owner.ACOwner._avatarController, -1);
					}

				}

				if(_currentChargeLvl == _chargeLvl)
				{
					if(_chargeFullSound != "")
					{
						SoundManager.Instance.PlaySoundEffect(_chargeFullSound);
					}
					if(_mmEffectMap != null)
					{
						_mmEffectMap.PlayEffect(FC_EFFECT_EVENT_POS.AT_CHARGE_FULL,_owner.ACOwner,null);
					}
				}
			}
			_timeToShowCharge -= Time.deltaTime;
			if(_timeToShowCharge<=0)
			{
				if( _currentChargeLvl<_chargeLvl)
				{
					_owner.ShowCharge(Mathf.Min(1f,( _timeCountCharge+_currentChargeLvl*_timeChargeStep) /_timeCount));
					_timeToShowCharge = 0.1f;
				}
				else
				{
					//_owner.ShowCharge(1f);
				}

			}
			if(_needNotHoldAttackKey)
			{
				if(_currentChargeLvl >= _chargeLvl)
				{
					if(_owner.ACOwner.IsPlayerSelf )
					{
						//release charge auto
						_attackCanSwitch = true;
						_shouldGotoNextHit = true;
						AttackEnd();
					}
					else if(_owner.ACOwner.IsClientPlayer && GameManager.Instance.IsPVPMode)
					{
						_attackCanSwitch = true;
						_shouldGotoNextHit = false;
						AttackEnd();
					}
				}
				
			}
			else
			{
				if(_owner.ACOwner.IsPlayerSelf)
				{
					//need key event to release charge
					if(!_owner.KeyAgent.keyIsPress( _currentBindKey ))
					{
						CommandManager.Instance.SendCommandToOthers(FCCommand.CMD.ACTION_CANCEL, _owner.ACOwner.ObjectID,
							_owner.ACOwner.ThisTransform.localPosition,
							null,
							FC_PARAM_TYPE.NONE,
							null,
							FC_PARAM_TYPE.NONE,
							null,
							FC_PARAM_TYPE.NONE);
						_shouldGotoNextHit = false;
						AttackEnd();
					}
					else
					{
						if(_currentChargeLvl>0)
						{
							_attackCanSwitch = true;
							_shouldGotoNextHit = true;
							AttackEnd();
						}
					}
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
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			return true;
		}

		return false;	
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
		
	}
	
	public override void AttackQuit()
	{
		if(_needAimHelp)
		{
			CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.HERO_AIMING, _owner.ACOwner._avatarController, -1);
		}
		//_owner.ShowCharge(-1f);
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
		return true;
	}
}
