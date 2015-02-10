using UnityEngine;
using System.Collections;

public class AttackEarthQuake : AttackBase {
	
	
	public float _timeToMaxPower = 0.1f;
	public int _keyCountToMaxPower = 3;
	public int _attackRepeatCount = 3;
	
	protected float _timeCount = 0;
	protected float _keyCount = 0;
	protected bool _beginToCountTime = false;
	protected int _currentRepeatCount = 0;
	
	public string _battleEffectPrefabName = null; //the name of passive effect prefab
    //private BattleCharEffect _battleCharEffect = null;
	
	public override void Init (FCObject owner)
	{
		base.Init (owner);
	}
	
	public override void AttackEnter ()
	{
		base.AttackEnter ();
		_keyCount = 0;
		_timeCount = 999;
		UIBattleSkillTips.Instance.NextStar();
		if(_isFirstAttack)
		{
			_currentRepeatCount = 0;
			_owner.CurrentSkill.PublicIntValue = 0;
			
			//_battleCharEffect.ShowStartEffect(true);
			//_battleCharEffect.ResetLocation(_owner.ACOwner.ThisTransform.position);
			//_battleCharEffect.LockLocation(true);
			//_owner.CurrentSkill.PublicObjectValue = _battleCharEffect;
		}
		else
		{
			//_battleCharEffect = _owner.CurrentSkill.PublicObjectValue as BattleCharEffect;
		}
		if(!_isFinalAttack)
		{
			_currentPortIdx = -1;
			if(!_isFirstAttack)
			{
				_owner.CurrentSkill.PublicIntValue += 1;
			}
		}
		_beginToCountTime = false;
		//_battleCharEffect.ShowEffect(_owner.CurrentSkill.PublicIntValue+1);
	}
	
	protected override void AniOver ()
	{
		base.AniOver ();
		if(_isFirstAttack)
		{
			_attackCanSwitch = true;
		}
		else if(_isFinalAttack)
		{
			_shouldGotoNextHit = false;
			AttackEnd();
		}
		else
		{
			_attackCanSwitch = true;
		}
	}
	
	protected override bool AKEvent (FC_KEY_BIND ekb, bool isPress)
	{
		if(!_isFinalAttack)
		{
			if(ekb > FC_KEY_BIND.DIRECTION && isPress)
			{
				ekb = _currentBindKey;
				if(!_beginToCountTime)
				{
					_timeCount = 0;
					_beginToCountTime = true;
				}
				_keyCount ++;
				if(_keyCount >= _keyCountToMaxPower)
				{
					_currentPortIdx = 0;
				}
			}
		}
		else
		{
			base.AKEvent(ekb,isPress);
		}
		return true;
	}
	
	public override void AniBulletIsFire ()
	{
		base.AniBulletIsFire ();
	}
	
	protected void GotoFinalAttack()
	{
		_shouldGotoNextHit = true;
		_jumpToSkillEnd = true;
		AttackEnd();
	}
	
	public override void AttackUpdate ()
	{
		base.AttackUpdate ();
		if(_beginToCountTime)
		{
			_timeCount+= Time.deltaTime;
		}
		if(_attackCanSwitch)
		{
			if(_isFirstAttack)
			{
				if(_currentPortIdx <0 )
				{
					GotoFinalAttack();
				}
				else
				{
					//UIBattleSkillTips.Instance.NextStar();
					_shouldGotoNextHit = true;
					_jumpToSkillEnd = false;
					AttackEnd();
				}
				
			}
			else if(_isFinalAttack)
			{}
			else
			{
				if (_currentRepeatCount >= _attackRepeatCount && _attackRepeatCount >0)
				{
					_shouldGotoNextHit = true;
					_jumpToSkillEnd = false;
					//UIBattleSkillTips.Instance.NextStar();
					AttackEnd();
					
				}
				else if(_currentPortIdx <0 ||  _attackRepeatCount <= 0)
				{
					//UIBattleSkillTips.Instance.NextStar();
					GotoFinalAttack();
				}
				else
				{
					//UIBattleSkillTips.Instance.NextStar();
					if( _owner.CurrentSkillID != -1)
					{
						_owner.SendAttackCommandToOthers(this, _owner.CurrentSkillID, _owner.CurrentSkill.ComboHitValue);
					}
					AttackEnter ();
					if(_owner.ACOwner.IsPlayerSelf)
					{
						
					}
					_currentRepeatCount++;
					_owner.AnimationSwitch._aniIdx = _attackAni;
					_owner.ACOwner.ACPlayAnimation(_owner.AnimationSwitch);
				}

			}
		}
	}
	
	public override void AttackEnd ()
	{
		if(_jumpToSkillEnd)
		{
			//_battleCharEffect.ShowEffect(0);
			//_battleCharEffect.ShowStartEffect(false);			
			//_battleCharEffect.ShowSpecialEndEffect(0);
		}
		base.AttackEnd ();
	}
	
	public override void AniBeforeBulletFire()
	{
		if(_timeCount <= _timeToMaxPower)
		{
			_currentPortIdx = 0;
		}
	}
	
	public override void AttackQuit ()
	{
		base.AttackQuit ();
		_currentRepeatCount = 0;
	}
}
