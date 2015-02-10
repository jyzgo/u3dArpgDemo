#define FC_AUTHENTIC

using UnityEngine;
using System.Collections;

[AddComponentMenu("FC/Logic/FCObject/PlayAndEnemy/Agent/AC/MoveAgent")]
public class MoveAgent : MonoBehaviour ,FCAgent
{
	#region data

    public Transform _cameraTarget;
	
	//time count for move pausing
	protected float _pauseTimeCount = -1;
	//if true, means motion will be ignore
	protected bool _isInPause = false;
	
	public bool IsInPause
	{
		get
		{
			return _isInPause;
		}
	}

	public enum ROTATE_FLAG
	{
		FACE_TO_TARGET,//forward toward to target
		AGAINST_TO_SELF_SPEED,//forward backward to move speed
		FOLLOW_SPEED,//forward follow move direction
		UNLOCK//forward and move direction have no relationship
	};
	
	public enum MOVE_MODE
	{
		BY_ANIMATION,// move drived by animtion pos offset
		BY_MOVE_MANUAL
	};
	
	public NavMeshAgent _navAgent;
	private FCObject _owner;
	private FCCommand _fastCommand;
	
	public MOVE_MODE _moveMode;
	
	public CirclePlayer _circlePlayer = null;
	
	//effect by gravity
	public float _gForce = 0;
	public Transform _gCenter = null;
	public float _gEffectTime;
	public bool _gEnabled = false;
	public bool _gIsPoint = true;
	public Vector3 _gDirection = Vector3.zero;
	public float _speedY = 0;
	
	public float _springOfBody = 0.5f;
	public float _jumpCount = 0;
	//if be real, means use g accleleration instead of g speed
	public bool _gBeReal = false;
	
	public Vector3 _gReadAccel = new Vector3(0,-10,0);
	
	public float _baseFlyHeight = 1;
	
	//if speed < _dragDeadZone ,means we will increase drag
	public float _dragDeadZone = 5f;
	//if in fall height, mosnter should change ani
	public float _fallHeight = 1;
	
	protected bool _moveRotateRightNowMode = true;
	
    private bool _isOnGround = true;    //by default, player is always on ground unless being hit to fly
	
	
	public bool IsNearGround
	{
		get
		{
			if(_navAgent.baseOffset <= _baseFlyHeight + _fallHeight && _speedY <=0)
			{
				return true;
			}
			return false;
		}
	}
    public bool IsOnGround
    {
        get { return _isOnGround; }

        set
        {
            _isOnGround = value;
			if(_isOnGround && _speedY <=0)
			{
				if(_jumpCount >0)
				{
					_jumpCount--;
					//FIXME  this is a exp num
					Vector3 vpos = new Vector3(0,-(_speedY*Time.fixedDeltaTime*1.5f),0);
					_speedY = -(_speedY*_springOfBody);
					_owner.ThisTransform.position = _owner.ThisTransform.position + vpos;
					_isOnGround = false;
				}
				else
				{
					_speedY = 0;
				}
				
			}
        }
    }
	
	public bool _pauseMotion = false;
	
	public bool PauseAttack
	{
		get
		{
			return _pauseMotion;
		}
		set
		{
			_pauseMotion = value;
		}
	}
	#endregion
	
	#region counter
	private Vector3 _targetPoint;
	private float _lastTime;
	private float _currentSpeed;
	private float _currentAngleSpeed;
	
	//I need this only show in game run mode 
	[HideInInspector]
	public Vector3 _moveDirection;
	public Vector3 _moveFinalDirection;
	
	public Vector3 _rotateDirection;
	//if true, means self is seek to a point
	private bool _isInMoveS;
	//if true, means self is moving with direction
	private bool _isInMoveD;
	
	public bool IsInMoveD
	{
		get
		{
			return _isInMoveD;
		}
	}
	
	private bool _isInRotate;
	//FIXME  have no use now
	private bool _followSpeedDirection;
	//if true, means is in navmesh path finding
	private bool _isInNavPathFinding;
	//if true, means move direction will change by self forward
	private bool _moveFollowForward = true;
	
	//FIXME  this is for fixupdate mode, have no use now, may should disable it
	public Vector3 _motion = Vector3.zero;
	
	//if true, means self has moved this frame
	protected bool _hasMoved = false;
	
	public float _currentMoveLengthCount = 0;
	
	//total move length of self
	//FIXME  if use it to count the move length of dash, it is not exact
	public uint _currentMoveLength = 0;
	
	public delegate void funtion_Vector3(Vector3 motion);
	public funtion_Vector3 _onMoveUpdate;
	
	public bool MoveFollowForward
	{
		get
		{
			return _moveFollowForward;
		}
		set
		{
			_moveFollowForward = value;
		}
	}
	

	private ROTATE_FLAG _rotateFlag;
	
	protected Rigidbody _thisRigidBody = null;
	
	protected float _acceleration = 0;
	protected float _defaultRaduis;
	
	public float Acceleration
	{
		get
		{
			return _acceleration;
		}
		set
		{
			_acceleration = value;
		}
	}
	
	public ROTATE_FLAG RotateFlag
	{
		set
		{
			_rotateFlag = value;
		}
		get
		{
			return _rotateFlag;
		}
	}
	
	public float CurrentSpeed
	{
		get
		{
			return _currentSpeed;
		}
		set
		{
			_currentSpeed = value;
			if(_isInNavPathFinding)
			{
				_navAgent.speed = _currentSpeed;
			}
		}
	}
	
	public float CurrentAngleSpeed
	{
		get
		{
			return _currentAngleSpeed;
		}
		set
		{
			_currentAngleSpeed = value;
		}
	}
	
	
	private CharacterController _characterController = null;
	
	public Rigidbody ThisRigidBody
	{
		get
		{
			return _thisRigidBody;
		}
		set
		{
			_thisRigidBody = value;
		}
	}
	#endregion
	//this function only effect with ac
	public void SetMoveMode(MOVE_MODE mm)
	{
		_moveMode = mm;
		if(_moveMode == MOVE_MODE.BY_ANIMATION)
		{
			//_navAgent.Stop();
			if(_owner.ObjectID.getOnlyObjectType == FC_OBJECT_TYPE.OBJ_AC)
			{
				(_owner as ActionController).MoveByAnimator(true);
			}
		}
		else
		{
			if(_owner.ObjectID.getOnlyObjectType == FC_OBJECT_TYPE.OBJ_AC)
			{
				(_owner as ActionController).MoveByAnimator(false);
			}
			if(_isInNavPathFinding)
			{
				_navAgent.Resume();
			}
		}
	}
	
	void OnDestroy()
	{
		_onMoveUpdate = null;
	}
	
	public void Init(FCObject owner)
	{
		_owner = owner;
		_isInMoveS = false;
		_isInMoveD = false;
		_isInNavPathFinding = false;
		_currentSpeed = 0;
		_moveDirection = Vector3.zero;
		_fastCommand = new FCCommand();
		_thisRigidBody = _owner.rigidbody;
		if(_thisRigidBody != null)
		{
			_thisRigidBody.drag = 100;
			_thisRigidBody.isKinematic = true;
		}
		_currentMoveLengthCount = 0;
		_currentMoveLength = 0;
		
		_characterController = _owner.GetComponent<CharacterController>();
		
		_fastCommand.Set(FCCommand.CMD.STOP,owner.ObjectID,FCCommand.STATE.RIGHTNOW,true);
		if(_navAgent != null)
		{
			_navAgent.radius = 0;
			_navAgent.enabled = false;
			_defaultRaduis = _navAgent.radius;
		}
		_rotateDirection = _owner.ThisTransform.forward;
		if(_fallHeight <=0)
		{
			_fallHeight = 1;
		}
		//_gReadAccel.y =  FC_CONST.REAL_G;
	}
	
	public void SetNavLayer(int layIndex)
	{
		_navAgent.walkableMask = layIndex;
	}
	
	public static string GetTypeName()
	{
		return "MoveAgent";
	}
	//the 
	public float GetFlyHeight()
	{
		if(_navAgent != null)
		{
			return _navAgent.baseOffset;
		}
		return 0;
	}
    /// <summary>
    /// Custom collision detection, called by Action controller
    /// </summary>
    /// <param name="collision"></param>
    public void CustomOnCollision(Collision collision)
    {
        /*if (collision.gameObject.layer == FC_CONST.LAYER_GROUND)
        {
            _isOnGround = true;

            _navAgent.enabled = true;
        }*/
    }

    public void CheatJump() //for test
    {
        _isOnGround = false;

        _navAgent.enabled = false;
    }
	
	#region logic function
	public void GotoPoint(ref Vector3 point,bool needPathFinding)
	{
		_moveFollowForward = false;
		if(!needPathFinding || _moveMode == MOVE_MODE.BY_ANIMATION)
		{
			_moveDirection = point -_owner.ThisTransform.localPosition;
			_moveDirection.y = 0;
			if(_moveDirection.sqrMagnitude>0)
			{
				_moveDirection.Normalize();
				_targetPoint = point;
				_isInMoveS = true;
			}
		}
		else
		{
			
			if(!_isInNavPathFinding)
			{
				_isInNavPathFinding = true;
				_navAgent.speed = _currentSpeed;
				_targetPoint = point;
				//FIXME  this is a exp num
				_navAgent.acceleration = _currentSpeed*1.5f;
				_navAgent.angularSpeed = _currentAngleSpeed;
				if(float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsNaN(point.z)
					|| point.x >= Mathf.Infinity || point.x <= Mathf.NegativeInfinity
					|| point.y >= Mathf.Infinity || point.y <= Mathf.NegativeInfinity
					|| point.z >= Mathf.Infinity || point.z <= Mathf.NegativeInfinity) {
					Debug.LogError("Move agent error:" + _owner.gameObject.name + "'s position is " + point);
				}
				else {
					_navAgent.destination = point;
					_navAgent.Resume();
					_navAgent.velocity = (point -_owner.ThisTransform.localPosition).normalized*_currentSpeed;
				}
			}
			else
			{
				_navAgent.speed = _currentSpeed;
				_targetPoint = point;
				//FIXME  this is a exp num
				_navAgent.acceleration = _currentSpeed*3f;
				_navAgent.angularSpeed = _currentAngleSpeed;
				/*_moveDirection = point -_owner.ThisTransform.localPosition;
				_moveDirection.y = 0;
				_moveDirection.Normalize();
				if(_moveDirection != Vector3.zero)
				{
					_navAgent.velocity = _currentSpeed * _moveDirection;
				}*/
				
				if(float.IsNaN(point.x) || float.IsNaN(point.y) || float.IsNaN(point.z)
					|| point.x >= Mathf.Infinity || point.x <= Mathf.NegativeInfinity
					|| point.y >= Mathf.Infinity || point.y <= Mathf.NegativeInfinity
					|| point.z >= Mathf.Infinity || point.z <= Mathf.NegativeInfinity) {
					Debug.LogError("Move agent error:" + _owner.gameObject.name + "'s position is " + point);
				}
				else {
					_navAgent.destination = point;
				}
			}
		}
		
	}
	
	public float DistanceToTargetPoint()
	{
		if(_isInNavPathFinding)
		{
			return _navAgent.remainingDistance;
		}
		return -1;
	}
	
	//FIXME  there is still some error with 
	public Vector3 GetGOffset()
	{
		Vector3 v3 = Vector3.zero;
		if(_gEnabled)
		{
			{
				if(_gIsPoint)
				{
					v3 = _gCenter.position - _owner.ThisTransform.localPosition;
					v3.y =0;
				}
				else
				{
					if(_gCenter != null)
					{
						v3 = _gCenter.forward;
					}
					else
					{
						v3 = _gDirection;
					}
				}
			}
			
			v3.Normalize();
			if(v3 != Vector3.zero)
			{
				v3 = v3 * _gForce * Time.deltaTime;
			}
			if(_gBeReal)
			{
				if(!_isOnGround)
				{
					if(_speedY >0)
					{
						_speedY = _speedY + _gReadAccel.y* Time.deltaTime;
					}
					else
					{
						_speedY = _speedY + _gReadAccel.y* Time.deltaTime *1.1f;
					}
					if(_navAgent != null)
					{
						_navAgent.baseOffset += _speedY*Time.deltaTime;
						if(_navAgent.baseOffset <= _baseFlyHeight)
						{
							_navAgent.baseOffset = _baseFlyHeight;
							IsOnGround = true;
						}
					}
					else
					{
						v3.y = v3.y + _speedY*Time.deltaTime;
					}
				}
			}
		}
		return v3;
	}
	public void Move(Vector3 motion)
	{
		if(motion != Vector3.zero)
		{
			_hasMoved = true;
		}
		if(_onMoveUpdate != null)
		{
			_onMoveUpdate(motion);
		}
		if(_isInPause)
		{
			motion.x = 0;
			motion.z = 0;
		}
		if(!_pauseMotion)
		{
			//FIXME  this may be motion not -motion
			_currentMoveLengthCount += _motion.magnitude;
		}
		if(_currentMoveLengthCount>=1)
		{
			_currentMoveLength = _currentMoveLength + (uint)_currentMoveLengthCount;
			_currentMoveLengthCount = _currentMoveLengthCount - (uint)_currentMoveLengthCount;
		}
		motion += GetGOffset();
		if(_moveMode == MOVE_MODE.BY_MOVE_MANUAL)
		{
			if(_thisRigidBody != null)
			{
				_motion = motion;
				if(!_pauseMotion)
				{
					//Debug.Log(motion.magnitude);
					//_owner.ThisTransform.localPosition +=motion;
					//_navAgent.Move(motion);
					//_thisRigidBody.MovePosition(_thisRigidBody.position + motion);
					
					if(_characterController != null)
					{
						_characterController.Move(motion);
					}else{
						_owner.ThisTransform.localPosition +=motion;
					}
					
				}
				else
				{
					//FIXME  some temp code for make dash better
					_owner.ThisTransform.localPosition -=(motion/2);
				}
				
				//_navAgent.Move(motion);
			}
			else
			{
				_owner.ThisTransform.localPosition += motion;
                if (_cameraTarget)
                {
                    _cameraTarget.position = _owner.ThisTransform.position;
                }
			}
		}
		_pauseMotion = false;
	}
	
	/*void FixedUpdate () 
	{
   	 		if(_moveMode == MOVE_MODE.BY_MOVE_MANUAL)
			{
				if(_thisRigidBody != null)
				{
					_thisRigidBody.position +=_motion;
					_motion = Vector3.zero;
					//_navAgent.Move(motion);
	                if (_cameraTarget)
	                {
	                    _cameraTarget.position = _thisRigidBody.position;
	                }
				}
				else
				{
					_owner.ThisTransform.localPosition += _motion;
	                if (_cameraTarget)
	                {
	                    _cameraTarget.position = _owner.ThisTransform.position;
	                }
				}
			}
	}*/

	public void SetDirection(ref Vector3 direction)
	{
		_moveDirection = direction;
	}
	public void GoByDirection(float timeLast)
	{
		GoByDirection(ref _moveDirection, timeLast, true);
	}

	public void GoByDirection(ref Vector3 direction,float timeLast,bool rightNowMode)
	{
		_isInNavPathFinding = false;
		if(rightNowMode)
		{
			_moveDirection = direction;
			_moveFinalDirection = direction;


            _owner.ThisTransform.forward = direction;
		}
		else
		{
			_moveFinalDirection = direction;
			if(direction == Vector3.zero)
			{
				_moveFinalDirection = _moveDirection;
			}
			
			
		}
		if(_rotateFlag != ROTATE_FLAG.UNLOCK)
		{
			_rotateDirection = direction;
		}
		_lastTime = timeLast;
		_isInMoveD = true;
		//_rotateDirection = Vector3.zero;
		if(_navAgent != null && _navAgent.enabled)
		{
            _navAgent.Stop();
		}
		_moveFollowForward = false;
		_moveRotateRightNowMode = rightNowMode;
	}
	
	public void GoTowardToDirection(ref Vector3 direction,int angleSpeed)
	{
		_isInNavPathFinding = false;
		_rotateDirection = direction;
		_currentAngleSpeed = angleSpeed;
		_isInMoveD = true;
		if(_navAgent != null && _navAgent.enabled)
		{
			_navAgent.Stop();
		}
		_moveFollowForward = true;
	}
	
	public void GoTowardToDirection(ref Vector3 direction,int angleSpeed,float timeLast)
	{
		_isInNavPathFinding = false;
		_rotateDirection = direction;
		_currentAngleSpeed = angleSpeed;
		_isInMoveD = true;
		_lastTime = timeLast;
		if(_navAgent != null && _navAgent.enabled)
		{
			_navAgent.Stop();
		}
		_moveFollowForward = true;
	}
	public void RotateToDirection(ref Vector3 direction,float timeLast,bool beForce,bool needNow)
	{
		if( direction != Vector3.zero)
		{
            //if (CheatManager.isSideView)
            //{
            //    if (CheckToReverseDirection(direction))
            //    {
            //        direction = Quaternion.Euler(0, 180, 0) * direction;
            //    }
            //}
            //else
            {
                _rotateDirection = direction;
            }

			if(beForce)
			{
				_moveDirection = direction;
			}

			if(needNow)
			{
				_owner.ThisTransform.forward = direction;
			}
		}

		//_isInRotate = true;
	}
	
	public void PauseMove(bool bePause,float pauseTime)
	{
		_isInPause = bePause;
		_pauseTimeCount = pauseTime;
		if(!bePause)
		{
			_pauseTimeCount = -1;
		}
	}
	void Update()
	{
		if(_gEffectTime <= 0)
		{
			if(_gBeReal)
			{
				_gForce = 0;
				_gDirection = Vector3.zero;
				_gIsPoint = false;
			}
			else
			{
				_gEnabled = false;
			}
		}
		if(_gEffectTime > 0)
		{
			_gEffectTime -= Time.deltaTime;
		}
		if(_pauseTimeCount > 0)
		{
			_pauseTimeCount -= Time.deltaTime;
		}
		else
		{
			_isInPause = false;
		}
		if(_rotateFlag == ROTATE_FLAG.FOLLOW_SPEED && (!_isInNavPathFinding || _moveMode == MOVE_MODE.BY_ANIMATION))
		{
			if(_moveDirection != Vector3.zero)
			{
				if(_moveMode == MOVE_MODE.BY_ANIMATION && _isInMoveS && _currentSpeed>0)
				{
					Vector3 md = _targetPoint - _owner.ThisTransform.localPosition;
					md.y = 0;
					md.Normalize();
					if(md != Vector3.zero)
					{
						_rotateDirection = md;
					}
				}
				else
				{
					//_rotateDirection = _moveDirection;
				}
			}
		}
		else if(_rotateFlag == ROTATE_FLAG.AGAINST_TO_SELF_SPEED)
		{
			if(_moveDirection != Vector3.zero)
			{
				_rotateDirection = -_moveDirection;
			}
		}
		if(_rotateDirection != _owner.ThisTransform.forward && !_isInNavPathFinding)
		{
			float angleR = Vector3.Angle(_rotateDirection,_owner.ThisTransform.forward);
			if(angleR<_currentAngleSpeed*Time.deltaTime)
			{
			}
			else
			{
				//FIXME  this code should move to a function
				float zz = _owner.ThisTransform.forward.z*_rotateDirection.x-_owner.ThisTransform.forward.x*_rotateDirection.z;
				if(zz>0)
				{
					_owner.ThisTransform.Rotate(new Vector3(0,_currentAngleSpeed*Time.deltaTime,0));
				}
				else if(zz<0)
				{
					_owner.ThisTransform.Rotate(new Vector3(0,-_currentAngleSpeed*Time.deltaTime,0));
				}
				else
				{
					int ret = Random.Range(0,1);
					if(ret > 0)
					{
						_owner.ThisTransform.Rotate(new Vector3(0,_currentAngleSpeed*Time.deltaTime,0));
					}
					else
					{
						_owner.ThisTransform.Rotate(new Vector3(0,_currentAngleSpeed*Time.deltaTime,0));
					}
				}
			}
		}
		
		if(!_isInNavPathFinding || _moveMode == MOVE_MODE.BY_ANIMATION)
		{
			if(_currentSpeed >0)
			{
				if(_isInMoveS)
				{
	
					float lenSqrt = (_targetPoint - _owner.ThisTransform.localPosition).sqrMagnitude;
					float curlen = _currentSpeed*Time.deltaTime;
#if WQ_CODE_WIP
					float curlenSqrt = curlen*curlen;
#endif
					if(lenSqrt > 9f)
					{
						Move(_moveDirection * curlen);
						_isInMoveS = true;
					}
					else
					{
						Move(_targetPoint - _owner.ThisTransform.localPosition);
						Stop();
						_fastCommand._cmd = FCCommand.CMD.STOP_IS_ARRIVE_POINT;
						CommandManager.Instance.SendFastToSelf(ref _fastCommand);
					}
				}
				else if(_isInMoveD && _lastTime >=0 )
				{
					if(!_moveRotateRightNowMode && _moveDirection != _moveFinalDirection)
					{
						float angleR = Vector3.Angle(_moveDirection,_moveFinalDirection);
						if(angleR<_currentAngleSpeed*Time.deltaTime)
						{
							_moveDirection = _moveFinalDirection;
							_moveRotateRightNowMode = true;
						}
						else
						{
							float zz = _owner.ThisTransform.forward.z*_moveFinalDirection.x-_owner.ThisTransform.forward.x*_moveFinalDirection.z;
							if(zz>0)
							{
								_moveDirection = Quaternion.Euler(0, _currentAngleSpeed*Time.deltaTime, 0)*_moveDirection;
								_owner.ThisTransform.Rotate(new Vector3(0,_currentAngleSpeed*Time.deltaTime,0));
							}
							else if(zz<0)
							{
								_moveDirection = Quaternion.Euler(0, _currentAngleSpeed*Time.deltaTime, 0)*_moveDirection;
								_owner.ThisTransform.Rotate(new Vector3(0,-_currentAngleSpeed*Time.deltaTime,0));
							}
							else
							{
								_moveRotateRightNowMode = true;
							}
						}
					}
					_lastTime-=Time.deltaTime;
					if(_moveFollowForward)
					{
						_moveDirection = _owner.ThisTransform.forward;
					}
					else
					{
						//_moveDirection = _rotateDirection;
					}
					Move(_moveDirection*_currentSpeed*Time.deltaTime);
					if(_lastTime<0 )
					{
						Stop();
						_fastCommand._cmd = FCCommand.CMD.STOP_IS_ARRIVE_POINT;
						CommandManager.Instance.SendFastToSelf(ref _fastCommand);
					}
				}

                if (_gEnabled)
                {
                    if (_isOnGround)
                    {
                        if (_currentSpeed >= Mathf.Epsilon)
                        {
                            if (_currentSpeed <= _dragDeadZone && _acceleration < -0.01f)
                            {
                                _acceleration += (_currentSpeed - _dragDeadZone);
                            }
                            _currentSpeed += _acceleration * Time.deltaTime;
                            if (_currentSpeed <= Mathf.Epsilon)
                            {
                                _currentSpeed = 0;
                                Stop();
                            }
                        }
                    }
                    else
                    {
                        if (_gForce <= 0.01f)
                        {
                            Stop();
                        }

                    }
                }
                else
                {
                    if (_currentSpeed >= Mathf.Epsilon)
                    {
                        if (_currentSpeed <= _dragDeadZone && _acceleration < -0.01f)
                        {
                            _acceleration += (_currentSpeed - _dragDeadZone);
                        }
                        _currentSpeed += _acceleration * Time.deltaTime;
                        if (_currentSpeed <= Mathf.Epsilon)
                        {
                            _currentSpeed = 0;
                            Stop();
                        }
                    }
                }

                //if(_currentSpeed >= Mathf.Epsilon)
                //{
                //    if(_currentSpeed <= _dragDeadZone && _acceleration < -0.01f)
                //    {
                //        _acceleration += (_currentSpeed-_dragDeadZone);
                //    }
                //    _currentSpeed += _acceleration*Time.deltaTime;
                //    if(_currentSpeed <= Mathf.Epsilon)
                //    {
                //        _currentSpeed = 0;
                //        Stop();
                //    }
                //}
			}
			else
			{
				if(_isInMoveS || _isInMoveD)
				{
					Stop();
					_fastCommand._cmd = FCCommand.CMD.STOP_IS_ARRIVE_POINT;
					CommandManager.Instance.SendFastToSelf(ref _fastCommand);
				}

			}
		}
		else if(_isInNavPathFinding)
		{
			//FIXME  this is a exp num, should move it to const
			if(_navAgent.remainingDistance < 0.1f)
			{
				_fastCommand._cmd = FCCommand.CMD.STOP_IS_ARRIVE_POINT;
				Stop();
				CommandManager.Instance.SendFastToSelf(ref _fastCommand);
			}
			else
			{
				if(!_navAgent.hasPath && !_navAgent.pathPending)
				{
					Stop();
					_fastCommand._cmd = FCCommand.CMD.STOP_IS_ARRIVE_POINT;
					CommandManager.Instance.SendFastToSelf(ref _fastCommand);
				}
				else
				{
					//Move(_navAgent.desiredVelocity * Time.deltaTime);
				}
			}
		}
		
		if(_circlePlayer != null)
		{
			//(_owner as ActionController).UpdateCamera();
		}			
		if(!_hasMoved && _gEnabled)
		{
			Move(Vector3.zero);
		}
		_hasMoved = false;	
	}
	
	public void Stop()
	{
		Stop(false);
	}
	public void Stop(bool stopAll)
	{
		_isInMoveS = false;
		_isInMoveD = false;
		_lastTime = 0;
		_acceleration = 0;
		_moveDirection = Vector3.zero;
		if(stopAll)
		{
			_gEnabled = false;
		}
		if(_navAgent != null && _navAgent.enabled)
		{
			_navAgent.Stop();
		}
		_moveRotateRightNowMode = true;
		if(_isInNavPathFinding)
		{
			//_navAgent.Stop();
			_isInNavPathFinding = false;
		}
	}
	
	public void DisableNavAgent()
	{
		_isInNavPathFinding = false;
		// add null reference protection 
		if(_navAgent != null && _navAgent.enabled)
		{
			_navAgent.enabled = false;
		}
	}
	public bool IsInMove()
	{
		return (_isInMoveS || _isInMoveD || _isInNavPathFinding) && _moveMode != MOVE_MODE.BY_ANIMATION;
	}
	
	public void SetPosition(Vector3 v3)
	{
		_owner.ThisTransform.localPosition = v3;
	}
	
	public void SetRaduis(float raduis)
	{
		if(raduis <0)
		{
			_navAgent.radius = _defaultRaduis;
		}
		else
		{
			_navAgent.radius = raduis;
		}
	}
	
	//0, 50- 70 is kept for player or other things
	//we should change the priority to enum
	//FIXME  we should need other way to do this function
	public void SetAvoidPriority(int mass)
	{
		mass = Mathf.Clamp(mass,FCConst.MIN_WEIGHT_FOR_ACTOR,FCConst.MAX_WEIGHT_FOR_ACTOR);
		if(mass <=0)
		{
			_navAgent.avoidancePriority = 0;
		}
		if(mass < FCConst.MID_WEIGHT_FOR_ACTOR)
		{
			mass = 120-mass;
			mass = Mathf.Clamp(mass,70,99);
			_navAgent.avoidancePriority = mass;
		}
		else if(mass == FCConst.MID_WEIGHT_FOR_ACTOR)
		{
			_navAgent.avoidancePriority = mass;
			
		}
		else
		{
			// if true, means never can push by other object who less than HILL_WEIGHT
			if(mass >= FCConst.HILL_WEIGHT)
			{
				mass = 0;
			}
			else
			{
				mass -= 40;
				mass = mass/10;
				if(mass > 49)
				{
					mass = 49;
				}
				mass = 50 - mass;
			}
			_navAgent.avoidancePriority = mass;
		}
	}
	public bool IsStop()
	{
		return !(_isInMoveS || _isInMoveD || _isInNavPathFinding);
	}
	
	#endregion
	
}
