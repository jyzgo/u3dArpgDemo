using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RageCell
{
	// >0 means increase rage, instead decrease
	public float _rageValue = 0;
	// if true, means rage = rage * timedelta
	public bool _modifyByTime = false;
	//event that will active this event
	public RageAgent.RageEvent _rageEvent;
	//what type will active this event
	public FCWeapon.WEAPON_HIT_TYPE[] _weaponHitType;
	
	protected RageAgent _owner;
	
	public void Init(RageAgent owner)
	{
		_owner = owner;
	}
	
	public bool TryActive(RageAgent.RageEvent rr, FCWeapon.WEAPON_HIT_TYPE wht)
	{
		if(rr == _rageEvent && ContainWeaponHitType(wht))
		{
			ModifyRage(_rageValue);
			return true;
		}
		return false;
	}
	
	protected void ModifyRage(float rage)
	{
		if(_modifyByTime)
		{
			_owner.CurrentRage += rage * Time.deltaTime;
		}
		else
		{
			_owner.CurrentRage += rage;
		}
		Debug.Log(_owner.CurrentRage);
		if(_owner.RageIsFull)
		{
			//Debug.Log("Rage is full");
			_owner.FireRage();
		}
	}
	
	protected bool ContainWeaponHitType(FCWeapon.WEAPON_HIT_TYPE wht)
	{
		bool ret = false;
		if(wht == FCWeapon.WEAPON_HIT_TYPE.ALL)
		{
			ret = true;
		}
		else if(wht == FCWeapon.WEAPON_HIT_TYPE.NONE)
		{
			ret = false;
		}
		else
		{
			foreach(FCWeapon.WEAPON_HIT_TYPE tmp in _weaponHitType)
			{
				if(tmp == wht)
				{
					ret = true;
					break;
				}
			}
		}
		return ret;
	}
}

public class RageAgent : FCObject ,FCAgent  {
	
	//max rage for ac
	public bool _isActive = false;
	public float _rageMax;
	public float _distanceNearForRage = 3;
	public float _distanceFarForRage = 10;
	protected float _disSqrtFar;
	protected float _disSqrtNear;
	public FC_AI_TYPE _aiTypeForDistance;
	public float _chanceToAttackWhenRageFull = 1;
	
	public List<FCSkillConfig> _skillsForRage = new List<FCSkillConfig>();
	protected float _rageOffset = 0;
	
	public RageCell[] _rageEventAll;
	
	public AIAgent.STATE[] _statesCanFireRageAtOnce;
	public AIAgent.STATE[] _statesCanGetRage;
	
	protected AIAgent _owner;
	protected float _currentRage = 0;
	
	public float CurrentRage
	{
		get
		{
			return _currentRage;
		}
		set
		{
			_currentRage = Mathf.Clamp(value , 0, _rageMax);
		}
	}
	
	public bool RageIsFull
	{
		get
		{
			return _currentRage >= _rageMax;
		}
	}
	
	public enum RageEvent
	{
		HIT_TARGET,
		HURT,
		BE_HIT,
		NEAR,
		FAR,
		ATTACK_OVER_WITH_NOHIT,
		MAX
	}
	public void Init(FCObject owner)
	{
		_owner = owner as AIAgent;
		_currentRage = 0;
		_skillsForRage.Clear();
		_owner.AttackCountAgent.GetRageSkills(ref _skillsForRage);
		if(_distanceFarForRage <= 0)
		{
			_disSqrtFar = -1;
		}
		else
		{
			_disSqrtFar = _distanceFarForRage * _distanceFarForRage;
		}
		if(_distanceNearForRage <= 0)
		{
			_disSqrtNear = -1;
		}
		else
		{
			_disSqrtNear = _distanceNearForRage * _distanceNearForRage;
		}
		foreach(RageCell rc in _rageEventAll)
		{
			rc.Init(this);
		}
	}
	
	public void TryEvent(RageEvent ret,  FCWeapon.WEAPON_HIT_TYPE weaponHitType, bool beForce = false)
	{
		bool canTry = false;
		if(ret == RageEvent.HIT_TARGET
			|| ret == RageEvent.ATTACK_OVER_WITH_NOHIT
			|| beForce)
		{
			canTry = true;
		}
		foreach(AIAgent.STATE ass in _statesCanGetRage)
		{
			if(_owner.AIStateAgent.CurrentStateID == ass
				&& _owner.AIStateAgent.NextStateID != AIAgent.STATE.DEAD
				&& _owner.TargetAC != null
				&& _owner.TargetAC.IsAlived)
			{
				canTry = true;
				break;
			}
		}
		if(canTry)
		{
			foreach(RageCell rc in _rageEventAll)
			{
				if(rc.TryActive(ret, weaponHitType))
				{
					break;
				}
			}
		}
	}
	
	public void UpdateTargetDistance(ActionController target)
	{
		if(target != null 
			&& target.IsAlived
			&& target.AIUse._aiType == _aiTypeForDistance)
		{
			float disSqrt = (target.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition).sqrMagnitude;
			if(_disSqrtFar > 0 && disSqrt > _disSqrtFar)
			{
				TryEvent(RageEvent.FAR,FCWeapon.WEAPON_HIT_TYPE.ALL);
			}
			else if(disSqrt < _disSqrtNear)
			{
				TryEvent(RageEvent.NEAR,FCWeapon.WEAPON_HIT_TYPE.ALL);
			}
			
		}
	}
	
	public void ClearRage()
	{
		_currentRage = 0;
	}
	public void OnLevelUp()
	{
		_skillsForRage.Clear();
		_owner.AttackCountAgent.GetRageSkills(ref _skillsForRage);
	}
	// when rage full, will do call back
	public void FireRage()
	{
		foreach(AIAgent.STATE ass in _statesCanFireRageAtOnce)
		{
			if(_owner.AIStateAgent.CurrentStateID == ass
				&& (_owner.AIStateAgent.NextStateID != AIAgent.STATE.HURT
				&& _owner.AIStateAgent.NextStateID != AIAgent.STATE.DEAD)
				&& _owner.TargetAC != null
				&& _owner.TargetAC.IsAlived)
				
			{
				int skillID = _owner.AttackCountAgent.GetRageSkillID(_owner.TargetAC.ThisTransform.localPosition, _skillsForRage);
				if(skillID == -1 && _skillsForRage.Count > 0)
				{
					skillID = 0;
				}
				Debug.Log(_skillsForRage[skillID]._skillName);
				_owner.GoToAttack(_skillsForRage[skillID]._skillName, true);
				ClearRage();
			}
		}
	}
}
