using UnityEngine;
using System.Collections;

//this is for player
public class AttackMagicStorm : AttackBase 
{
	public FC_CHARACTER_EFFECT _attackEffect =  FC_CHARACTER_EFFECT.INVALID;
	
	public int _maxBallCanHit;
	public float _maxCDTime = 0.3f;
	public float _minCDTime = 0.1f;
	public float _cdDecreaseSpeed = 10;
	public float _cdIncreaseSpeed = 10;
	public float _keyEffectTime = 0.1f;
	protected float _keyEffectTimeCounter = 0;
	protected float _currentCDTime = 0.3f;
	protected float _timeCounter = 0;
	
	protected int _currentFireCount = 0;
	protected bool _bulletIsFire = false;
	
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
	}
	
	protected override void AniOver()
	{
		
	}
	
	public override void AttackEnter()
	{
		base.AttackEnter();
		_frameCount = 0;
		CharacterEffectManager.Instance.PlayEffect(_attackEffect ,_owner.ACOwner._avatarController, -1);
		_currentState = AttackBase.ATTACK_STATE.STEP_1;
		_timeCounter = 0;
		_currentCDTime = _maxCDTime;
		_bulletIsFire = false;
		_keyEffectTimeCounter = 0;
		_currentFireCount = 0;
	}
	
	public override void AttackUpdate()
	{
		base.AttackUpdate();
		if(_currentState == AttackBase.ATTACK_STATE.STEP_1)
		{
			if(_bulletIsFire)
			{
				_timeCounter +=Time.deltaTime;
				if(_timeCounter >= _currentCDTime)
				{
					_timeCounter -= _currentCDTime;
					_owner.ACOwner.AniFireBullet(0);
				}
			}
			if(_currentFireCount >= _maxBallCanHit)
			{
				AttackEnd();
			}
			if(_keyEffectTimeCounter > 0)
			{
				_keyEffectTimeCounter -= Time.deltaTime;
				_currentCDTime -= Time.deltaTime * _cdDecreaseSpeed;
			}
			else
			{
				_currentCDTime += Time.deltaTime * _cdIncreaseSpeed;
			}
			_currentCDTime = Mathf.Clamp(_currentCDTime, _minCDTime, _maxCDTime);
		}
	}
	
	public override void IsHitTarget(ActionController ac,int sharpness)
	{
		base.IsHitTarget(ac,sharpness);
	}
	
	protected override bool AKEvent(FC_KEY_BIND ekb, bool isPress)
	{
		if(ekb == _currentBindKey)
		{
			_currentCDTime -= Time.deltaTime * _cdDecreaseSpeed;
			_keyEffectTimeCounter = _keyEffectTime;
			_currentCDTime = Mathf.Clamp(_currentCDTime, _minCDTime, _maxCDTime);
		}
		return true;
	}
	
	public override void AniBulletIsFire()
	{
		base.AniBulletIsFire();
		_currentFireCount++;
		if(!_bulletIsFire)
		{
			_bulletIsFire = true;
		}
	}
	
	public override void AttackEnd()
	{
		base.AttackEnd();
	}
	
	public override void AttackQuit()
	{
		base.AttackQuit();
	}
	
}
