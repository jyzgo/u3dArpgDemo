using UnityEngine;
using System.Collections;

public class BulletWeaponCarrier : FCBullet {

//    bool _isCarryWeapon = false;
	public Transform _weaponNode = null;
	
	public bool _getWeaponFormFireNode = false;
	
	public bool _seekTarget;
	public bool _seekPoint;
	public bool _lockTarget;
	public int _angleSpeed;
	public float _lockTime;
	
	protected Vector3 _seekTargetPoint = Vector3.zero;
	protected int _weaponIndex = 0;
	
	public override void Init(FCObject owner)
	{
		base.Init(owner);
		_isRangerBullet = false;
//		_isCarryWeapon = false;
	}
	
	protected void ApplyWeapon()
	{
		//Debug.Log(_firePoint.name);
		if(!_getWeaponFormFireNode)
		{
			_weaponIndex = 0;
		}
		else if(_firePoint.name.Contains("right"))
		{
			_weaponIndex = 1;
		}
		else if(_firePoint.name.Contains("left"))
		{
			_weaponIndex = 2;
		}
		_owner.ACApplyWeaponTo(_weaponNode, _weaponIndex);
	}
	
	protected void ReturnWeapon()
	{
		_owner.ACApplyWeaponTo(null,_weaponIndex);
	}
	
	protected override IEnumerator STATE()
	{
		_inState = true;
		ApplyWeapon();
		float speed = _hitInfo[_step]._shotSpeed;
		if(LifeTime < 0)
		{
			LifeTime = _hitInfo[_step]._lifeTime;
		}
		_attackInfo._hitType = _hitInfo[_step]._hitType;
		Vector3 d2 = ThisTransform.forward;
		if(_moveAgent != null)
		{
			_moveAgent.SetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		}
		
		if(_moveAgent != null)
		{
			_moveAgent.CurrentSpeed = speed;
			_moveAgent.GoByDirection(ref d2,99, true);
		}
		
		bool seekPoint = _seekPoint;
		
		float lockTime = _lockTime;
		if(seekPoint)
		{
			if(_target != null)
			{
				_seekTargetPoint = _target.ACGetTransformByName(_targetSolt).position;
			}
			else
			{
				_seekTargetPoint = _owner.ACGetTransformByName(_targetSolt).position + _owner.ThisTransform.forward * 20;
				_seekTargetPoint.y = _owner.ACGetTransformByName(_targetSolt).position.y;
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
					_moveAgent.CurrentSpeed = speed;
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
							d1.y = 0;
							d1.Normalize();
							_moveAgent.GoTowardToDirection(ref d1,_angleSpeed);
						}
					}
				}
				
				if(LifeTime<=0)
				{
					_inState = false;
				}
				
			}
			yield return null;
		}
		Dead();
		
	}
	
	public override void Dead()
	{
		//disable colliders
		ReturnWeapon();
		if(_moveAgent != null)
		{
			_moveAgent.Stop();
			StopAllCoroutines();
		}
		base.Dead();
	}
}
