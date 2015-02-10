using UnityEngine;
using System.Collections;

public class SpecicalActionMonsterAgent : FCObject {

	protected enum HES_DIRECTION
	{
		NONE,
		LEFT,
		RIGHT,
		FORWARD,
		BACK
	}
	
	protected HES_DIRECTION _hDirection = HES_DIRECTION.NONE;
	
	protected bool _inState = false;
	
	protected AIAgent _owner = null;
	protected int _wanderAngleSign = -1;
	
	public bool _usedForFly = false;
	public float _circleInner = 3f;
	public float _circleOuter = 6f;
	
	public float _circleInnerSqrt = 3f;
	public float _circleOuterSqrt = 6f;
	
	public float _wanderTime = 5f;
	public float _timeToRefreshWander = 1f;
	public float _wanderSpeed = 5f;
	
	public float _timeToRefreshFace = 0.4f;
	
	public float _wanderAngleSpeed = 180f;
	
	public delegate void function_SAL_SAC(FC_SP_ACTION_LISTS sals,FC_SP_ACTION_CONDITONS sacs);
	
	public function_SAL_SAC _SpActionIsEnd;
	
	protected float _wanderTimeOverride = -1f;
	
	protected AniSwitch _aniSwitch = new AniSwitch();
	
	protected override void FirstInit()
	{
		_circleInnerSqrt = _circleInner*_circleInner;
		_circleOuterSqrt = _circleOuter*_circleOuter;
		_SpActionIsEnd = null;
	}
	
	public void StartToWanderAround(AIAgent owner, function_SAL_SAC fss, float timeForWander)
	{
		_SpActionIsEnd = fss;
		_owner = owner;
		_wanderTimeOverride = timeForWander;
		StartCoroutine(WANDER_AROUND_PLAYER());
	}
	
	public void StopWanderAround()
	{
		_SpActionIsEnd = null;
		StopAllCoroutines();
	}
	
	protected Vector3 GetWanderDirection(float lenSqrt,ref Vector3 fromSelfToT)
	{
		Vector3 direction = Vector3.zero;
		if(lenSqrt <= 0)
		{
			return (-_owner.ACOwner.ThisTransform.forward);
		}
		if(lenSqrt < _circleInnerSqrt)
		{
			direction = -fromSelfToT;
			Quaternion qt = Quaternion.Euler(0,Random.Range(0,35)*_wanderAngleSign,0);
			direction = qt * direction;
		}
		else if(lenSqrt < _circleOuterSqrt)
		{
			direction =  Quaternion.Euler(0,Random.Range(45,80)*_wanderAngleSign,0) * fromSelfToT;
		}
		else
		{
			direction =  Quaternion.Euler(0,Random.Range(0,45)*_wanderAngleSign,0) * fromSelfToT;
		}
		return direction;
	}
	
	private void UpdateFaceTarget()
	{
		if(!_usedForFly)
		{
			Vector3 dir = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
			dir.y =0;
			dir.Normalize();
			if(dir != Vector3.zero)
			{
				_owner.ACOwner.ACRotateTo(dir,-1,false);
			}
		}
	}
	
	public void UpdateAnimation()
	{
		//11 l
		//12 r
		//13 f
		//14 b
		Vector3 d1 = _owner.ACOwner.ThisTransform.forward;
		float angle = Vector3.Angle(_owner.ACOwner.MoveDirection ,d1);
		if(angle > 150)
		{
			_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleB1;
			if(_hDirection != HES_DIRECTION.BACK)
			{
				_hDirection = HES_DIRECTION.BACK;
				_owner.ACOwner.ACPlayAnimation(_aniSwitch);
			}
		}
		else if(angle < 30)
		{
			_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleF1;
			if(_hDirection != HES_DIRECTION.FORWARD)
			{
				_hDirection = HES_DIRECTION.FORWARD;
				_owner.ACOwner.ACPlayAnimation(_aniSwitch);
			}
		}
		else
		{
			d1.Normalize();
			float zz = d1.z*_owner.ACOwner.MoveDirection.x-d1.x*_owner.ACOwner.MoveDirection.z;
			if(zz <0)
			{
				_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleR1;
				if(_hDirection != HES_DIRECTION.RIGHT)
				{
					_hDirection = HES_DIRECTION.RIGHT;
					_owner.ACOwner.ACPlayAnimation(_aniSwitch);
				}
			}
			else
			{
				_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleL1;
				if(_hDirection != HES_DIRECTION.LEFT)
				{
					_hDirection = HES_DIRECTION.LEFT;
					_owner.ACOwner.ACPlayAnimation(_aniSwitch);
				}
			}
			
		}
		
	}
	
	private IEnumerator WANDER_AROUND_PLAYER()
	{
		_owner.ACOwner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		_owner.ACOwner.CurrentAngleSpeed = _wanderAngleSpeed;
		_hDirection = HES_DIRECTION.NONE;
		_inState = true;
		if(_usedForFly)
		{
			_owner.ACOwner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
		else
		{
			_owner.ACOwner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		}
		
		float timeCount = 0.001f;
		float timeCount1 =  _wanderTime;
		if(_wanderTimeOverride <= 0)
		{
			_wanderTimeOverride = _wanderTime;
		}
		timeCount1 = _wanderTimeOverride;
		float timeCount2 = 0.001f;
		while(_inState)
		{

			timeCount1 -= Time.deltaTime;
			if(timeCount1 <=0)
			{
				_inState = false;
			}
			if(timeCount >0)
			{
				timeCount -= Time.deltaTime;
				if(timeCount<=0)
				{
					Vector3 v3 = _owner.TargetAC.ThisTransform.localPosition - _owner.ACOwner.ThisTransform.localPosition;
					v3.y =0;
					float lenSqrt = v3.sqrMagnitude;
					v3.Normalize();
					Vector3 wd = GetWanderDirection(lenSqrt,ref v3);
					_owner.MoveByDirection(wd, _wanderSpeed,_wanderTimeOverride+1f, true);
					timeCount = _timeToRefreshWander;
					timeCount2 = 0.001f;
				}
			}
			if(timeCount2 >0)
			{
				timeCount2 -= Time.deltaTime;
				if(timeCount2 <= 0)
				{
					UpdateFaceTarget();
					timeCount2 = _timeToRefreshFace;
				}
			}
			UpdateAnimation();
			yield return null;
		}
		_owner.ACOwner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		_owner.ACOwner.ACRevertToDefalutMoveParams();
		_hDirection = HES_DIRECTION.NONE;
		if(_SpActionIsEnd != null)
		{
			_SpActionIsEnd(FC_SP_ACTION_LISTS.RUN_RANDOM,FC_SP_ACTION_CONDITONS.AFTER_IDLE);
		}
	}
	
	protected override void OnDestroy()
	{
		_SpActionIsEnd = null;
	}
}
