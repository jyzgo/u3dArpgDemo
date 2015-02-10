using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/Bullet/BulletMissile")]
public class BulletMissile : FCBullet {
	
	public bool _seekTarget;
	public bool _seekPoint;
	public bool _lockTarget;
	public int _angleSpeed;
	public float _lockTime;
	
	public CubicRNSpline _path;
	
	
	protected int _currentHitTotalCount = 0;
	protected int _currentPerHitCount = 0;
	protected int _seekPointMode = 0;
	
	protected Vector3 _seekTargetPoint = Vector3.zero;
	
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		_isRangerBullet = false;
		if(_path != null)
		{
			_seekPointMode = 1;
		}
	}
	
	protected override IEnumerator STATE()
	{
		_inState = true;
		float speed = _hitInfo[_step]._shotSpeed;
		float speedY = 0;
		if(_seekPointMode == 1)
		{
			_enableSpeedY = false;
			//_path.CreateSimplePath(ThisTransform.position, )
		}
		else if(_enableSpeedY && _seekPointMode == 0)
		{
			float angleP = (_fireAngleY*Mathf.PI)/180f;
			speedY = Mathf.Sin(angleP)*speed;
			speed = Mathf.Cos(angleP)*speed;
		}
		if(LifeTime < 0)
		{
			LifeTime = _hitInfo[_step]._lifeTime;
		}
		_currentHitTotalCount =  _hitInfo[_step]._maxHitTotal;
		_currentPerHitCount = _hitInfo[_step]._maxHitPerTime;

		//set damage and physical layer for bullets, enable colliders
		if(_hitInfo[_step]._enableDamageAfterFire)
		{
			switch(_faction)
			{
			case FC_AC_FACTIOH_TYPE.NEUTRAL_1:
				if (_hitInfo[_step]._physicalCollider != null)
				{
					_hitInfo[_step]._physicalCollider.enabled = true;
					_hitInfo[_step]._physicalCollider.gameObject.layer = FCConst.LAYER_NEUTRAL_WEAPON_PHYSICAL_1;
				}
				break;

			case FC_AC_FACTIOH_TYPE.NEUTRAL_2:
				if (_hitInfo[_step]._physicalCollider != null)
				{
					_hitInfo[_step]._physicalCollider.enabled = true;
					_hitInfo[_step]._physicalCollider.gameObject.layer = FCConst.LAYER_NEUTRAL_WEAPON_PHYSICAL_2;
				}				
				break;
			}
			if(_damageReceiver != null)
			{
				_damageReceiver.ActiveLogic();
			}
		}
		
		_attackInfo._hitType = _hitInfo[_step]._hitType;
		Vector3 d2 = ThisTransform.forward;
		if(_moveAgent != null)
		{
			_moveAgent.SetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		}
		
		if(_enableSpeedY && _hasGEffect)
		{
			DisableGEffect();
			EffectByGravity(speedY,null,Vector3.up,999,false,true);
		}
		else if(_enableSpeedY)
		{
			DisableGEffect();
			EffectByGravity(speedY,null,Vector3.up,999,false,false);
		}
		if(_moveAgent != null)
		{
			_moveAgent.CurrentSpeed = speed;
			_moveAgent.GoByDirection(ref d2,99, true);
		}
		
		bool seekPoint = _seekPoint;
		
		float lockTime = _lockTime;
		float trackTime = -1;
		if(seekPoint)
		{
			if(_target != null && _path == null)
			{
				_seekTargetPoint = _target.ACGetTransformByName(_targetSolt).position;
			}
			else
			{
				_seekTargetPoint = _owner.ACGetTransformByName(_targetSolt).position + _owner.ThisTransform.forward * _maxTargetingDistance;
				_seekTargetPoint.y = _owner.ACGetTransformByName(_targetSolt).position.y;
			}
			if(_seekPointMode == 1)
			{
				//ThisTransform.forward = Quaternion.Euler(0, _fireAngleY , 0)*ThisTransform.forward;
				//ThisTransform.forward = _owner.ThisTransform.forward;
				_path.CreateSimplePath(ThisTransform.position, _seekTargetPoint, 
					_hitInfo[_step]._shotSpeed * ThisTransform.forward, (_seekTargetPoint - ThisTransform.position).normalized * _hitInfo[_step]._finalSpeed);
				trackTime = _path.GetTotalLength()/((_hitInfo[_step]._finalSpeed+_hitInfo[_step]._shotSpeed)/2);
			}
		}
		
		while(_inState)
		{
			if(LifeTime >0)
			{
				if(_hitInfo[_step]._accelerate > 0)
				{
					if(speed < _hitInfo[_step]._finalSpeed)
					{
						speed += _hitInfo[_step]._accelerate;
						if(speed >= _hitInfo[_step]._finalSpeed)
						{
							speed = _hitInfo[_step]._finalSpeed;
						}
					}
					
				}
				else if(_hitInfo[_step]._accelerate < 0)
				{
					if(speed > _hitInfo[_step]._finalSpeed)
					{
						speed += _hitInfo[_step]._accelerate;
						if(speed <= _hitInfo[_step]._finalSpeed)
						{
							speed = _hitInfo[_step]._finalSpeed;
						}
					}
					
				}
				if(_moveAgent != null)
				{
					if(_seekPointMode == 0)
					{
						_moveAgent.CurrentSpeed = speed;
					}
					else
					{
						_moveAgent.CurrentSpeed = 0;
						_moveAgent.Stop();
					}
				}
				LifeTime-=Time.deltaTime;
				lockTime -= Time.deltaTime;
				if(_moveAgent != null)
				{
					if(_target != null && _seekTarget)
					{
						Vector3 d1 = _target.ThisTransform.localPosition - ThisTransform.localPosition;
						d1.y = 0;
						d1.Normalize();
						_moveAgent.GoTowardToDirection(ref d1,_angleSpeed);
					}
					else if(seekPoint && _seekPointMode == 1)
					{
						Vector3 posNow = _path.transform.position;
						Vector3 velNow = Vector3.forward;
						float rp = (_hitInfo[_step]._lifeTime -LifeTime) /trackTime;
						//Debug.Log(rp);
						if(rp <= 0.8f)
						{
							_path.GetPosByTotalTime(rp,
							ref posNow,ref velNow);
						}
						else
						{
							LifeTime = 0;
						}
						_moveAgent.SetPosition(posNow);
						ThisTransform.forward = velNow.normalized;
					}
					else if(seekPoint && _seekTargetPoint != Vector3.zero && lockTime <0)
					{
						Vector3 d1 = _seekTargetPoint - _owner.ACGetTransformByName(_targetSolt).position;
						Vector3 d3 =  ThisTransform.localPosition - _owner.ACGetTransformByName(_targetSolt).position;
						if(d1.sqrMagnitude < d3.sqrMagnitude)
						{
							seekPoint = false;
						}
						else
						{
							d1 = _seekTargetPoint - ThisTransform.localPosition;
							//d1.y = 0;
							d1.Normalize();
							_moveAgent.GoTowardToDirection(ref d1,_angleSpeed);
						}
					}
				}
				
				if(LifeTime<=0)
				{
					_inState = false;
				}
				else
				{
					if(_currentPerHitCount <_hitInfo[_step]._maxHitPerTime)
					{
						if( _hitInfo[_step]._canPenetrate)
						{
							if(_currentHitTotalCount <=0 )
							{
								_inState = false;
								if(_moveAgent != null)
								{
									_moveAgent.Stop(true);
								}
							}
							else
							{
								_currentHitTotalCount--;
								_currentPerHitCount = _hitInfo[_step]._maxHitPerTime;
							}
						}
						else
						{
							_inState = false;
							if(_moveAgent != null)
							{
								_moveAgent.Stop(true);
							}
						}
					}
				}
				
			}
			yield return null;
		}
		Dead();
		
	}
	
	public override bool CanHit(ActionController ac)
	{
		return (_currentPerHitCount >0);
	}
	public override bool HandleCommand(ref FCCommand ewd)
	{
		base.HandleCommand(ref ewd);
		switch(ewd._cmd)
		{
		case FCCommand.CMD.ATTACK_HIT_TARGET:
			if(!_deadByLifeOver)
			{
				if(_currentPerHitCount >0)
				{
					_currentPerHitCount--;
					if(_currentPerHitCount <=0 && ! _hitInfo[_step]._canPenetrate)
					{
						_inState = false;
						_moveAgent.Stop();
					}
				}
			}

			break;
		}
		return true;
	}

	public override void Dead()
	{
		//disable colliders
		foreach (BulletHitInfo info in _hitInfo)
		{
			if (info != null)
			{
				if (info._physicalCollider != null)
					info._physicalCollider.enabled = false;
			}
		}
		
		if(_moveAgent != null)
		{
			_moveAgent.Stop();
			StopAllCoroutines();
		}
		base.Dead();
	}
}
