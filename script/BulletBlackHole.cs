using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletBlackHole : FCBullet {

	public Transform _gCenter = null;
	// if target ac = 50, means _gforce value = acceleration
	public float _gForce;
	public float _gForceMin = 4f;
	protected float _fireMaxRaduis;
	protected bool _followAnchor = false;
	
	public float _startRadus = 0.5f;
	public float _expandSpeed = 20f;
	
	protected List<ActionController> _targetList = new List<ActionController>();
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		_isRangerBullet = true;
	}
	protected override IEnumerator STATE()
	{
		_inState = true;
		_damageReceiver.ActiveLogic();
		_targetList.Clear();
		float lifeTime = _hitInfo[_step]._lifeTime;
		Transform centerTransform = _gCenter;
		if(centerTransform == null)
		{
			centerTransform = ThisTransform;
		}
		while(_inState)
		{
			if(lifeTime>0)
			{
				if(_damageReceiver.GetRadius() < _fireMaxRaduis)
				{
					float radius = _damageReceiver.GetRadius();
					radius += _expandSpeed*Time.deltaTime;
					radius = Mathf.Clamp(radius, 0, _fireMaxRaduis);
					_damageReceiver.SetRadius(radius);
				}
				lifeTime -=Time.deltaTime;
				foreach(ActionController ac in _targetList)
				{
					if(ac != null && ac.IsAlived)
					{
						float curlen = (ThisTransform.position-ac.ThisTransform.localPosition).magnitude;
						float gforce = _gForceMin + (_gForce - _gForceMin)*(1-(curlen-_owner.BodyRadius)/_fireMaxRaduis);
						gforce = Mathf.Clamp(gforce, _gForceMin, _gForce);
						ac.ACEffectByGravity(gforce, centerTransform, 0.05f, true, false);
					}
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
		switch(ewd._cmd)
		{
		case FCCommand.CMD.ATTACK_HIT_TARGET:
		{
			_targetList.Add((ewd._param1 as ActionController));
			break;
		}
		case FCCommand.CMD.ATTACK_OUT_OF_RANGE:
		{
			_targetList.Remove((ewd._param1 as ActionController));
			(ewd._param1 as ActionController).ACEffectByGravity(0, null, 0, true, false);
			break;
		}
		}
		return true;
	}
	
	public override void Dead()
	{
		foreach(ActionController ac in _targetList)
		{
			if(ac != null && ac.IsAlived)
			{
				ac.ACEffectByGravity(0, null, 0, true, false);
			}
		}
		StopAllCoroutines();
		base.Dead();
	}
	
	public override void FireRanger(ActionController target,Transform firePoint, RangerAgent.FirePort rfp)
	{
		RangerAgent.FireRangeInfo fri = rfp._rangeInfo;
		base.FireRanger(target, firePoint, rfp);
		_attackInfo._attackPoints = _owner.TotalAttackPoints;
		_step = 0;
		_damageReceiver.SetRadius(_startRadus);
		
		if(fri != null)
		{
			_damageScale = rfp.DamageScale;
			_fireMaxRaduis = 1f*fri._param1;
			_followAnchor = fri._needAnchor;
			_hitInfo[_step]._lifeTime = fri._effectTime;
			_attackInfo._damageScale = _damageScale;
		}
		_deadTime = -1;
		_target = null;
		_isFrom2P = _owner.IsClientPlayer;
		ThisObject.layer = (int)_faction+1;
		
		
		ThisTransform.localPosition = firePoint.position;
		ThisTransform.forward = _owner.ThisTransform.forward;
		_attackInfo._hitType = _hitInfo[_step]._hitType;
		
		_firePoint = firePoint;
		
		_enableDamage = false;
		ActiveLogic(rfp);
	}
}
