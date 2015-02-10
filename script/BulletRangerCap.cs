using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletRangerCap : FCBullet {

	public CapsuleCollider _rangerCap;
	protected float _fireRaduis;
	protected bool _followAnchor = false;
	
	protected List<ActionController> _targetList = new List<ActionController>();
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		_isRangerBullet = true;
	}
	protected override IEnumerator STATE()
	{
		_inState = true;
		_rangerCap.enabled = true;
		_targetList.Clear();
		float lifeTime = _hitInfo[_step]._lifeTime;
		float holeCD = 0.1f;
		while(_inState)
		{
			if(lifeTime>0)
			{
				lifeTime -=Time.deltaTime;
				if(_attackInfo._hitType == AttackHitType.BlackHole)
				{
					if(holeCD >0)
					{
						holeCD -= Time.deltaTime;
						if(holeCD<=0)
						{
							holeCD+=Time.deltaTime;
							_rangerCap.enabled = !_rangerCap.enabled;
						}
					}
					else
					{
						holeCD = 0.1f;
					}
				}

                if (_attackInfo._hitType == AttackHitType.BlackHole)
				{
					foreach(ActionController ac in _targetList)
					{
						if(ac != null && ac.IsAlived)
						{
							Vector3 point = _owner.ThisTransform.localPosition;
							ac.CurrentSpeed = 10;
							ac.ACMove(ref point);
						}
					}
				}
				else if(_attackInfo._hitType == AttackHitType.Lock)
				{
					
				}
				if(_followAnchor)
				{
					ThisTransform.localPosition = _firePoint.position;
				}
			}
			else
			{
				_inState = false;
			}
			yield return null;
		}
		Dead();
		
	}
	
	public override bool CanHit(ActionController ac)
	{
		return true;
	}
	
	
	public override bool HandleCommand(ref FCCommand ewd)
	{
		base.HandleCommand(ref ewd);
		switch(ewd._cmd)
		{
			case FCCommand.CMD.ATTACK_HIT_TARGET:
			{
                if (_attackInfo._hitType == AttackHitType.BlackHole)
				{
					_targetList.Add((ewd._param1 as ActionController));
				}
				else if(_attackInfo._hitType == AttackHitType.Lock)
				{
					_targetList.Add((ewd._param1 as ActionController));
				}
	
				break;
			}

		}
		return true;
	}
	
	public override void Dead()
	{
		StopAllCoroutines();
		_rangerCap.enabled = false;
		base.Dead();
	}
	
	public override void FireRanger(ActionController target,Transform firePoint, RangerAgent.FirePort rfp)
	{
		RangerAgent.FireRangeInfo fri = rfp._rangeInfo;
		base.FireRanger(target, firePoint, rfp);
		_attackInfo._attackPoints = _owner.TotalAttackPoints;
		if(fri != null)
		{
			_damageScale = rfp.DamageScale;
			_rangerCap.radius = 1f*fri._param1;
			_followAnchor = fri._needAnchor;
			_attackInfo._effectTime = fri._effectTime;
			_attackInfo._damageScale = _damageScale;
		}
		
		_deadTime = -1;
		_target = null;
		_isFrom2P = _owner.IsClientPlayer;
		ThisObject.layer = (int)_faction+1;
		_step = 0;
		
		ThisTransform.localPosition = firePoint.position;
		_attackInfo._hitType = _hitInfo[_step]._hitType;
		
		_firePoint = firePoint;

        if (_attackInfo._hitType == AttackHitType.BlackHole 
			|| _attackInfo._hitType == AttackHitType.ParrySuccess)
		{
			_enableDamage = false;
		}
		else
		{
			//_enableDamage = true;
		}
		ActiveLogic(rfp);
	}
	
}
