using UnityEngine;
using System.Collections;

public class AiNormalMonster : AINormalWarrior 
{
	//this is local position to player
	protected Vector3 _targetPosition;
	
	public bool _usedForFly = false;
	public float _angleSpeedForFlyWander = 180f;
	//if _timeForWander < 0 ,means should goto attack
	public float _timeForWanderMin = 5f;
	public float _timeForWanderMax= 10f;
	
	protected float _timeForWander = 0;
	//when player hurt, try to make 
	public float _chanceToAttack_PlayerHurt = 0.5f;
	
	//if in the region, will increase rage
	public float _dangerousRaduis = 3f;
	protected float _dangerousRaduisSqrt = 3f;
	public float _ragePerSecDanger = 50f;
	protected float _timeTotalForWander = 0;
	
	public float _timeForWanderAtLeast = 0.5f;
	
	protected enum SEEK_MODE
	{
		NONE,
		FOLLOW_LEADER,
		COPY_PLAYER,
		WANDER_TO_PLAYER,
		WANDER_BY_SIMPLE_PATH,
		FEINT_ATTACK,
		RUN_AWAY_AFTER_ATTACK,
		RUN_AWAY_AFTER_HURT,
		IDLE
	}
	
	protected SEEK_MODE _currentSeekMode = SEEK_MODE.NONE;
	protected SEEK_MODE _nextSeekMode = SEEK_MODE.NONE;
	
	//if true, means a logic module of seeking is active now
	protected bool _inSeek = false;
	protected int _seekTargetCount = 0;
	public float _safeDistance = 2;
	protected bool _wantToAttack = false;
	
	protected float _realSafeDistance = 0;
	protected float _safeDistanceSqrt = 0;
	protected float _seekTime = 0;
	
	//used for circle path
	public float _hesitateJitterMin = 3f;
	public float _hesitateJitterMax = 4f;
	protected float _hesitateJitter = 12f;
	
	public float _chanceToAwayAfterHurt = 0.8f;
	public float _chanceToAwayAfterAttack = 0.8f;
	
	protected Vector3 _seekWanderOffset = new Vector3(0,0,12);
	
	//used for wander to player 
	public float _timeToRefreshTarget = 1f;
	
	//1 means to left, -1 means to right
	protected int _wanderToDirection = -1;
	
	public float _copyPlayerTimeMin = 2f;
	public float _copyPlayerTimeMax = 4f;
	public float _copyPlayerAttackChanceT = 0.3f;
	public float _copyPlayerAttackChanceAttackT = 1.5f;
	
	public float _wanderToPlayerTimeMin = 2f;
	public float _wanderToPlayerTimeMax = 5f;
	public float _chanceToFakeAttackT = 3;
	public float _chanceToAttackT = 15;
	public float _chanceToCopyPlayer = 0.3f;
	public float _chanceToSimplePath = 0.6f;
	public int _angleMinToClosePlayer = 80;
	public int _angleMaxToClosePlayer = 85;
	public int _angleMinToAwayPlayer = 90;
	public int _angleMaxToAwayPlayer = 95;
	public float _simplePathTimeMin = 2f;
	public float _simplePathTimeMax = 4f;
	public float _simplePathChancePlayerFrontT = 0.3f;
	public float _simplePathChancePlayerBackT = 6f;
	public float _awayHurtTimeMin = 2;
	public float _awayHurtTimeMax = 4;
	public float _awayAttackTimeMin = 2;
	public float _awayAttackTimeMax = 4;
	public float _awayAttackChanceToWanderSimple = 0.9f;
	public float _wanderSpeedPercents = 0.5f;
	protected float _wanderSpeed = 0;
	public enum AWAY_MODE
	{
		NONE,
		AWAY_AFTER_ATTACK,
		AWAY_AFTER_HURT
	}
	
	protected AWAY_MODE _awayFromTarget = AWAY_MODE.NONE;
	
	protected AniSwitch _aniSwitch = new AniSwitch();
	
	protected enum HES_DIRECTION
	{
		NONE,
		LEFT,
		RIGHT,
		FORWARD,
		BACK
	}
	
	protected HES_DIRECTION _hDirection = HES_DIRECTION.NONE;
	
	protected void ChangeToSeekMode()
	{
		_currentSeekMode = _nextSeekMode;
		_nextSeekMode = SEEK_MODE.NONE;
		switch(_currentSeekMode)
		{
		case SEEK_MODE.FOLLOW_LEADER:
			StartCoroutine(FOLLOW_LEADER());
			break;
		case SEEK_MODE.COPY_PLAYER:
			StartCoroutine(COPY_PLAYER());
			break;
		case SEEK_MODE.FEINT_ATTACK:
			StartCoroutine(FEINT_ATTACK());
			break;
		case SEEK_MODE.IDLE:
			StartCoroutine(IDLE());
			break;
		case SEEK_MODE.RUN_AWAY_AFTER_ATTACK:
			StartCoroutine(RUN_AWAY_AFTER_ATTACK());
			break;
		case SEEK_MODE.RUN_AWAY_AFTER_HURT:
			StartCoroutine(RUN_AWAY_AFTER_HURT());
			break;
		case SEEK_MODE.WANDER_BY_SIMPLE_PATH:
			StartCoroutine(WANDER_BY_SIMPLE_PATH());
			break;
		case SEEK_MODE.WANDER_TO_PLAYER:
			StartCoroutine(WANDER_TO_PLAYER());
			break;
		}
	}
	public override void IdleTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_QUIT)
		{	
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		}
		else if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			FaceToTarget(_targetAC);
			HandleInnerCmd(FCCommand.CMD.DIRECTION_FOLLOW_FORWARD,null);
			if(_state._preState.CurrentStateID == AIAgent.STATE.ATTACK)
			{
				_awayFromTarget = AWAY_MODE.AWAY_AFTER_ATTACK;
				_timeForIdleThisTime = 0.1f;
			}
			else if(_state._preState.CurrentStateID == AIAgent.STATE.HURT)
			{
				_awayFromTarget = AWAY_MODE.AWAY_AFTER_HURT;
				_timeForIdleThisTime = 0.3f;
			}
			else
			{
				_awayFromTarget = AWAY_MODE.NONE;
				_timeForIdleThisTime = 0;
			}
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
		}
		else if(cmd == FCCommand.CMD.STATE_DONE)
		{
			SetNextState(AIAgent.STATE.HESITATE);
		}
	}
	
	public override void HesitateTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			_timeTotalForWander = 0;
			_dangerousRaduisSqrt = _dangerousRaduis*_dangerousRaduis;
			_wanderSpeed = _owner.Data.TotalMoveSpeed * _wanderSpeedPercents;
			_hDirection = HES_DIRECTION.NONE;
			_realSafeDistance = _safeDistance+_bodyRadius+_targetAC.BodyRadius;
			_safeDistanceSqrt = _realSafeDistance * _realSafeDistance;
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			_wantToAttack = false;
			_timeForWander = Random.Range(_timeForWanderMin,_timeForWanderMax);
			//CountTargetToSetWay();
			float ret = Random.Range(0,1f);
			if(_awayFromTarget == AWAY_MODE.AWAY_AFTER_HURT)
			{
				if(RageIsFull)
				{
					if(CanAttackOthers)
					{
						GoToAttack();
					}
					else
					{
						if(!_haveActionTicket)
						{
							FCTicketManager.Instance.ApplyTicket(_owner);
						}
					}
				}
				else if(ret<_chanceToAwayAfterHurt)
				{
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_HURT);
				}
				else
				{
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
				//have chance,
				//RageFromHurt >50
				//rage max = 100.as
				//hurt run away time min and max
				
			}
			else if(_awayFromTarget == AWAY_MODE.AWAY_AFTER_ATTACK)
			{
				//have chance to hit again
				//or run away
				if(ret < _chanceToAwayAfterAttack)
				{
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_ATTACK);
				}
				else
				{
					GoToAttack();
				}
			}
			else
			{
				//
				FC_DANGER_LEVEL dl = GetTargetDangerLevel(3,8);
				if(dl == FC_DANGER_LEVEL.SAFE)
				{
					_hesitateJitter = Random.Range(_hesitateJitterMin,_hesitateJitterMax);
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
				else if(dl == FC_DANGER_LEVEL.DANGER)
				{
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_ATTACK);
					//SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_ATTACK);
				}
				else
				{
					
					SetNextSeekMode(SEEK_MODE.RUN_AWAY_AFTER_HURT);
				}
				
			}
			if(_usedForFly)
			{
				_owner.MoveFollowForward = true;
				_owner.CurrentAngleSpeed = _angleSpeedForFlyWander;
			}
			ChangeToSeekMode();
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			 UpdateHesitate();
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			StopAllCoroutines();
			_owner.CurrentAngleSpeed = _owner.Data.angleSpeed;
			_owner.ACAniSpeedRecoverToNormal();
			_owner.ACRestoreToDefaultSpeed();
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
			//_owner.MoveFollowForward = false;
		}
	}
	
	//get a random post near player
	public void CountTargetToSetWay()
	{
		if(_targetAC != null)
		{
			//means first time to seek player
			Vector2 rp = Random.insideUnitCircle*_realSafeDistance;
			_targetPosition = Vector3.zero;
			_targetPosition.x += rp.x;
			_targetPosition.z += rp.y;
		}
		
	}
	
	protected override void TargetFinded(ActionController ac)
	{
		base.TargetFinded(ac);
		if(_state.CurrentStateID == AIAgent.STATE.BORN)
		{
			FaceToTarget(_targetAC);
		}
	}
	
	//only face to target ,not move direction
	protected void UpdateFaceTarget()
	{
		if(!_usedForFly)
		{
			Vector3 dir = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
			dir.y =0;
			dir.Normalize();
			if(dir != Vector3.zero)
			{
				_owner.ACRotateTo(dir,-1,false);
			}
		}
	}
	
	protected void SetNextSeekMode(SEEK_MODE seekMode)
	{
		_inSeek = false;
		_nextSeekMode = seekMode;
	}
	//every seek mode has its own logic module
	private IEnumerator FOLLOW_LEADER()
	{
		_inSeek = true;
		//set destination near player
		CountTargetToSetWay();
		while(_inSeek)
		{
			UpdateFaceTarget();
			//v3 is destination that ai want to go
			Vector3 v3 = _targetAC.ThisTransform.localPosition +_targetPosition - _owner.ThisTransform.localPosition;
			Vector3 v3t = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
			
			// means ai is closed to player enough
			if(_safeDistanceSqrt < v3.sqrMagnitude && _safeDistanceSqrt <v3t.sqrMagnitude)
			{
				float angle = Vector3.Angle(_owner.MonsterLeader.ThisTransform.forward,v3);
				v3.Normalize();
				//if angle = 0 ,means leader and me have same move direction ,so my action will follow leader, otherwise instead
				v3t = _owner.MonsterLeader.ThisTransform.forward*((180-angle)/180)+v3*(angle/180);
				v3t.y =0;
				v3t.Normalize();
				if(v3t == Vector3.zero)
				{
					v3t = v3;
				}
				MoveByDirection(v3t,_wanderSpeed, 0.1f);
			}
			else
			{
				StopMove();
				_wantToAttack = false;
				SetNextSeekMode(SEEK_MODE.COPY_PLAYER);
			}
			UpdateAnimation();
			yield return null;
			
		}
		ChangeToSeekMode();
	}
	
	private IEnumerator COPY_PLAYER()
	{
		_inSeek = true;
		// if true , means monster will find a way to attack player
		_wantToAttack = false;
		bool awayFromPlayer = false;
		
		//show to GD ,min and max
		_seekTime = Random.Range(_copyPlayerTimeMin,_copyPlayerTimeMax);
		if(_usedForFly)
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
		else
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		}
		while(_inSeek)
		{
			UpdateFaceTarget();
			_seekTime -= Time.deltaTime;
			if(awayFromPlayer)
			{
				if((_owner.ThisTransform.localPosition-_targetPosition).sqrMagnitude < 1f)
				{
					GoToAttack();
				}
			}
			else
			{
				if(!_wantToAttack)
				{
					if(_seekTime >=0)
					{
						//direction to player
						Vector3 v3 = _targetAC.ThisTransform.localPosition- _owner.ThisTransform.localPosition;
						//get player motion last frame
						Vector3 v31 = _targetAC._currPosition - _targetAC._lastPosition;
						//means player is attack
						if( _targetAC.ACGetCurrentAttack() == null)
						{
							//show attack chance to GD
							float ret = Random.Range(0,1f);
							if(ret < _copyPlayerAttackChanceT*Time.deltaTime)
							{
								_wantToAttack = true;
							}
							else
							{
								//will copy player move way
								if(v31 != Vector3.zero)
								{
									v31.Normalize();
									//if mosnter is near player enough .the main direction of player is try to copy player motion
									if( v3.sqrMagnitude <_safeDistanceSqrt )
									{
										v3 = v31*0.9f+v3.normalized*0.1f;
										v3.Normalize();
										//when copy player motion ,will need monster move faster
										MoveByDirection(v3,_wanderSpeed*1.5f, 0.2f);
										_owner.AniSetAnimationSpeed(1.5f);
									}
									else
									{
										//otherwise far from player, will try to close and copy player move way at the same time
										ret =  v3.sqrMagnitude/_safeDistanceSqrt;
										ret -= 1;
										ret = Mathf.Clamp(ret,0,1f);
										v3 = v31*(1-ret)+v3.normalized*ret;
										v3.Normalize();
										MoveByDirection(v3,_wanderSpeed*1.5f, 0.2f);
										_owner.AniSetAnimationSpeed(1.5f);
									}
								}
							}
	
						}
						else
						{
							//if player is in attack, monster will have more chance to attack player
							if(Random.Range(0,1f) < _copyPlayerAttackChanceAttackT*Time.deltaTime)
							{
								_wantToAttack = true;
							}
						}
					}
					else
					{
						_wantToAttack = true;
					}
				}
				else
				{
					Vector3 v3 = _targetAC.ThisTransform.localPosition- _owner.ThisTransform.localPosition;
					
					//get a suitable skill to attack player
					FCSkillConfig ews = _attackCountAgent.GetSkillNear(v3);
					if(ews._distanceMinSqrt>v3.sqrMagnitude)
					{
						//means enemy should away from player to find a right position to attack player
						v3 = -v3;
						v3.y =0;
						v3.Normalize();
						v3.x = v3.x + Random.Range(-0.1f,0.1f);
						v3.y = v3.y + Random.Range(-0.1f,0.1f);
						v3.Normalize();
						_targetPosition = _targetAC.ThisTransform.localPosition+v3*ews.GetSkillActiveDistanceMin();
						MoveTo(ref _targetPosition);
						_seekTime = 0.5f;
						_wantToAttack = false;
						awayFromPlayer = true;
					}
					else
					{
						GoToAttack();
					}
				}
			}
			
			UpdateAnimation();
			yield return null;
			
		}
		ChangeToSeekMode();
	}
	
	protected void GenerateTargetPositionForWander(float radius)
	{
		float theta = Random.Range(-0.01f,0.01f) * Mathf.PI*2;
  		//create a vector to a target position on the wander circle
		 _targetPosition.x = radius * Mathf.Sin(theta);
		 _targetPosition.z = radius * Mathf.Cos(theta);
	}
	private IEnumerator WANDER_TO_PLAYER()
	{
		//_hesitateJitter need to show to player
		//time to refresh to show 
		//fake attack chance
		//true attack chance
		_inSeek = true;
		if(_usedForFly)
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
		else
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		}
		Vector3 v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		v3.y =0;
		v3.Normalize();
		if(v3 == Vector3.zero)
		{
			v3 = _owner.ThisTransform.forward;
		}
		float wanderRadius = 10;
		GenerateTargetPositionForWander(wanderRadius);
		
		_owner.ACRotateToDirection(ref v3, false);
		//MoveByDirection(ThisTransform.forward,_wanderSpeed,0.1f);

		_seekTime = Random.Range(_wanderToPlayerTimeMin,_wanderToPlayerTimeMax);
		_timeToRefreshTarget = 1f;
		while(_inSeek)
		{
			UpdateFaceTarget();
			 float hesitateJitter =  _hesitateJitter * Time.deltaTime;
  			//first, add a small random vector to the target's position
  			_targetPosition += new Vector3(Random.Range(-1,1f) * hesitateJitter,0,
                Random.Range(-1,1f) * hesitateJitter);
			_targetPosition.Normalize();
			_targetPosition*=wanderRadius;
 			 //move the target into a position wanderOffset in front of the agent
  			Vector3 targetPos = _targetPosition + _seekWanderOffset;
			Quaternion qt = Quaternion.FromToRotation(Vector3.forward,targetPos);
			targetPos = qt * _owner.MoveDirection * targetPos.magnitude;
			Vector3 dir1 = _owner.MoveDirection;
			dir1 = dir1 +targetPos.normalized;
			dir1.Normalize();
			MoveByDirection(dir1,_wanderSpeed,0.1f);
			_seekTime -= Time.deltaTime;
			
			//try change wander mode when  _timeToRefreshTarget is zero
			if(_timeToRefreshTarget >0)
			{
				_timeToRefreshTarget -=  Time.deltaTime;
				if(_timeToRefreshTarget <=0)
				{
					//means monster is far from player, we need way to close player
					if((_targetAC.ThisTransform.localPosition-_owner.ThisTransform.position).sqrMagnitude > _safeDistanceSqrt*6)
					{
						GenerateTargetPositionForWander(wanderRadius);
					}
					else if((_targetAC.ThisTransform.localPosition-_owner.ThisTransform.position).sqrMagnitude < _safeDistanceSqrt*4
						&& (_targetAC.ThisTransform.localPosition-_owner.ThisTransform.position).sqrMagnitude > _safeDistanceSqrt)
					{
						//need a feint attack
						float ret = Random.Range(0,1f);
						{
							if(ret < _chanceToFakeAttackT*Time.deltaTime)
							{
								SetNextSeekMode(SEEK_MODE.FEINT_ATTACK);
							}
						}
					}
					else if((_targetAC.ThisTransform.localPosition-_owner.ThisTransform.position).sqrMagnitude <= _safeDistanceSqrt)
					{
						float ret = Random.Range(0,1f);
						{
							//means close player enough, need go to attack
							if(ret < _chanceToAttackT*Time.deltaTime)
							{
								GoToAttack();
							}
						}
					}
					_timeToRefreshTarget = 1.5f;
				}
				
			}
			if(_seekTime <= 0)
			{
				_owner.ACStop();
				
				float ret = Random.Range(0,1f);
				//after wander action, we can go to copy player or cirtical move
				if(ret < _chanceToCopyPlayer)
				{
					SetNextSeekMode(SEEK_MODE.COPY_PLAYER);
				}
				else if(ret < _chanceToCopyPlayer + _chanceToSimplePath)
				{
					SetNextSeekMode(SEEK_MODE.WANDER_BY_SIMPLE_PATH);
				}
				else
				{
					GoToAttack();
				}
				_wantToAttack = false;
			}
			UpdateAnimation();
			yield return null;
		}
		ChangeToSeekMode();
	}
	
	private IEnumerator WANDER_BY_SIMPLE_PATH()
	{
		float angle = 0;
		if(_hDirection == HES_DIRECTION.LEFT)
		{
			angle = 90;
		}
		else
		{
			angle = -90;
		}
		if(_usedForFly)
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
		else
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		}
		Vector3 v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		//means 
		if(v3.sqrMagnitude >_realSafeDistance)
		{
			//show direction to GD ,80 -100,max and  min
			if(angle <0)
			{
				_wanderToDirection = - Random.Range(_angleMinToClosePlayer,_angleMaxToClosePlayer);
			}
			else
			{
				_wanderToDirection = Random.Range(_angleMinToClosePlayer,_angleMaxToClosePlayer);
			}
		}
		else
		{
			if(angle <0)
			{
				_wanderToDirection = - Random.Range(_angleMinToAwayPlayer,_angleMaxToAwayPlayer);
			}
			else
			{
				_wanderToDirection = Random.Range(_angleMinToAwayPlayer,_angleMaxToAwayPlayer);
			}
		}
		//min and max to GD
		_seekTime = Random.Range(_simplePathTimeMin,_simplePathTimeMax);
		_inSeek = true;
		while(_inSeek)
		{
			_seekTime -= Time.deltaTime;;
			UpdateFaceTarget();
			if(!_usedForFly)
			{
				v3 = _owner.ThisTransform.forward;
			}
			else
			{
				v3 = _targetAC.ThisTransform.localPosition -  _owner.ThisTransform.localPosition;
				v3.y =0;
				v3.Normalize();
				if(v3 == Vector3.zero)
				{
					v3 = _owner.ThisTransform.forward;
				}
			}
			Quaternion qt = Quaternion.Euler(0,_wanderToDirection,0);
			v3 = qt * v3;
			v3.Normalize();
			MoveByDirection(v3,_wanderSpeed,0.1f);
			float ret = Random.Range(0,10f);
			angle = Vector3.Angle(_owner.ThisTransform.forward,_targetAC.ThisTransform.forward);
			float tmp = _simplePathChancePlayerFrontT*Time.deltaTime;
			//chance per second
			if(angle < 60)
			{
				tmp = (180-angle)/90* _simplePathChancePlayerBackT*Time.deltaTime+tmp;
			}
			// if player is back to monster, monster will have more chance to attack
			if(ret < tmp || _seekTime <=0)
			{
				GoToAttack();
			}
			UpdateAnimation();
			yield return null;
		}
		ChangeToSeekMode();
		//_seekTargetCount = 17;
	}
	
	private IEnumerator FEINT_ATTACK()
	{
		_inSeek = true;
		int frameCount = 0;
		
		_owner.ACStop();
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		
		//play fake attack
		_hDirection = HES_DIRECTION.NONE;
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.fakeAttack0;
		_owner.ACPlayAnimation(_aniSwitch);
		
		while(_inSeek)
		{
			UpdateFaceTarget();
			frameCount++;
			if(frameCount > 10 && _owner.AniGetAnimationNormalizedTime()>0.35f)
			{
				Vector3 v3 = _targetAC.ThisTransform.localPosition- _owner.ThisTransform.localPosition;
				FCSkillConfig ews = _attackCountAgent.GetSkillAt(v3);
				if(ews != null)
				{
					SetNextSeekMode(SEEK_MODE.COPY_PLAYER);
				}
				else
				{
					GoToAttack();
				}
			}
			UpdateAnimation();
			yield return null;
		}
		_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
		ChangeToSeekMode();
	}
	
	//when after hurt ,enemy try to far from player
	private IEnumerator RUN_AWAY_AFTER_HURT()
	{
		_inSeek = true;
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		Vector3 v3 = _owner.ThisTransform.localPosition -_targetAC.ThisTransform.localPosition;
		v3.y = 0;
		v3.Normalize();
		if(v3 == Vector3.zero)
		{
			v3 = - _owner.ThisTransform.forward;
		}
		float angle = Random.Range(-10,10);
		if(angle <0)
		{
			angle -= 90;
		}
		else
		{
			angle += 90;
		}
		Quaternion qt = Quaternion.Euler(0,angle,0);
		
		_owner.ACRotateTo(v3,-1,false);
		v3 = qt * v3;
		_seekTime = Random.Range(_awayHurtTimeMin,_awayHurtTimeMax);
		v3.Normalize();
		
		//monster will not rotate to final direction right now, instead with angle speed
		_owner.CurrentSpeed = _wanderSpeed;
		_owner.ACMoveToDirection (ref v3, 180, _seekTime);
		//play run animations
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.run;
		_hDirection = HES_DIRECTION.NONE;
		_owner.ACPlayAnimation(_aniSwitch);
		
		while(_inSeek)
		{
			if(_seekTime >0)				
			{
				_seekTime -= Time.deltaTime;
				if(_seekTime<=0)
				{
					_owner.ACStop();
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
				//at end of this action, monster need try to face to player
				else if(_seekTime<=1)
				{
					v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
					v3.y =0;
					v3.Normalize();
					angle = Vector3.Angle(_owner.ThisTransform.forward,v3);
					//16 is a magic num , means monster face near to player
					if(angle <=16)
					{
						_seekTime = Random.Range(0.2f,0.4f);
						SetNextSeekMode(SEEK_MODE.IDLE);
						_owner.ACStop();
					}
					else
					{
						_owner.CurrentSpeed = _wanderSpeed/2;
						_owner.ACMoveToDirection (ref v3, 360);
					}


				}
			}
			UpdateAnimation();
			yield return null;
		}
		ChangeToSeekMode();
	}
	
	public override void BornTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_QUIT)
		{
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);
            ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);

			CameraController.Instance.AddCharacterForShadow(ACOwner._avatarController);
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			if(!_owner.IsPlayer && _targetAC != null)
			{
				FaceToTarget(_targetAC, true);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_ENTER)
		{
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_MOVE);
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.CANT_ROTATE);
            SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_SAIYAJIN);

			_runOnPath = false;
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
			if(!_owner.IsPlayer)
			{
				// to find a nearest player as target
				_owner.ACBeginToSearch(true);
			}
			else
			{
				if(_owner.IsClientPlayer)
				{
					SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_SUPER_NAMEKIAN);
				}
                SetNextState(AIAgent.STATE.IDLE);
			}
			if(!ACOwner.IsPlayer) {
				GlobalEffectManager.Instance.PlayEffect(FC_GLOBAL_EFFECT.BORN, ACOwner.ThisTransform.position);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_DONE)
		{
            if (GameManager.Instance.GameState == EnumGameState.InBattleCinematic)
            {
                SetNextState(STATE.DUMMY);
            }
            else if (GameManager.Instance.GameState == EnumGameState.InBattle)
            {
                if (_targetAC != null)
                {
                    SetNextState(AIAgent.STATE.HESITATE);
                }
                else
                {
                    SetNextState(AIAgent.STATE.IDLE);
                }
            }
		}
	}
	
	private IEnumerator IDLE()
	{
		_owner.ACStop();
		if(_usedForFly)
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
		else
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		}
		
		///play idle animations
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		_owner.ACAniSpeedRecoverToNormal();
		_hDirection = HES_DIRECTION.NONE;
		
		//need idle min and max to GD
		_seekTime = Random.Range(0,1f);
		_inSeek = true;
		while(_inSeek)
		{
			UpdateFaceTarget();
			_seekTime -= Time.deltaTime;
			if(_seekTime<=0)
			{
				float ret = Random.Range(0,1f);
				//show chance to GD
				if(ret <0.9f)
				{
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
				else
				{
					SetNextSeekMode(SEEK_MODE.COPY_PLAYER);
				}
				_owner.ACStop();
			}
			UpdateAnimation();
			yield return null;
		}
		ChangeToSeekMode();
	}
	
	private IEnumerator RUN_AWAY_AFTER_ATTACK()
	{
		if(_usedForFly)
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
		else
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		}
		Vector3 v3 = _owner.ThisTransform.localPosition -_targetAC.ThisTransform.localPosition;
		v3.y = 0;
		v3.Normalize();
		if(v3 == Vector3.zero)
		{
			v3 = -_owner.ThisTransform.forward;
		}
		float angle = Random.Range(-10,10);
		if(angle <0)
		{
			angle -= 90;
		}
		else
		{
			angle += 90;
		}
		Quaternion qt = Quaternion.Euler(0,angle,0);
		_owner.ACRotateTo(v3,-1,false);
		v3 = qt * v3;
		//need to show min and max to GD
		_seekTime = Random.Range(_awayAttackTimeMin,_awayAttackTimeMax);
		v3.Normalize();
		_owner.CurrentSpeed = _wanderSpeed;
		
		MoveByDirection(v3, _wanderSpeed, _seekTime);
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleB1;
		_hDirection = HES_DIRECTION.BACK;
		_owner.ACPlayAnimation(_aniSwitch);
		_inSeek = true;
		while(_inSeek)
		{
			UpdateFaceTarget();
			_seekTime -= Time.deltaTime;
			if(_seekTime<=0)
			{
				float ret = Random.Range(0,1f);

				if(ret <1-_awayAttackChanceToWanderSimple)
				{
					SetNextSeekMode(SEEK_MODE.WANDER_TO_PLAYER);
				}
				else
				{
					SetNextSeekMode(SEEK_MODE.WANDER_BY_SIMPLE_PATH);
				}
			}
			UpdateAnimation();
			yield return null;
		}
		ChangeToSeekMode();
	}

	protected override void GoToAttack()
	{
		if(_targetAC.IsAlived)
		{
			_owner.ACStop();
			//we need close player and attack
			_currentSeekMode = _nextSeekMode = SEEK_MODE.NONE;
			//for wander use
			_inSeek = false;
			StopAllCoroutines();
			base.GoToAttack();
		}
		else
		{
			//SetNextState(AIAgent.STATE.IDLE);
		}
	}
	
	public void UpdateAnimation()
	{
		//11 l
		//12 r
		//13 f
		//14 b
		Vector3 d1 = _owner.ThisTransform.forward;
		float angle = Vector3.Angle(_owner.MoveDirection ,d1);
		if( _targetAC.MoveDirection == Vector3.zero && _currentSeekMode == SEEK_MODE.COPY_PLAYER && _owner.MoveDirection == Vector3.zero)
		{
			if(_aniSwitch._aniIdx != FC_All_ANI_ENUM.idle)
			{
				_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
				_owner.ACPlayAnimation(_aniSwitch);
				_owner.ACAniSpeedRecoverToNormal();
				_hDirection = HES_DIRECTION.NONE;
			}
		}
		else if(_currentSeekMode == SEEK_MODE.COPY_PLAYER 
			|| _currentSeekMode == SEEK_MODE.RUN_AWAY_AFTER_ATTACK
			|| _currentSeekMode == SEEK_MODE.WANDER_TO_PLAYER
			|| _currentSeekMode == SEEK_MODE.WANDER_BY_SIMPLE_PATH)
		{
			if(angle > 165)
			{
				_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleB1;
				if(_hDirection != HES_DIRECTION.BACK)
				{
					_hDirection = HES_DIRECTION.BACK;
					_owner.ACPlayAnimation(_aniSwitch);
				}
			}
			else if(angle < 15)
			{
				_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleF1;
				if(_hDirection != HES_DIRECTION.FORWARD)
				{
					_hDirection = HES_DIRECTION.FORWARD;
					_owner.ACPlayAnimation(_aniSwitch);
				}
			}
			else
			{
				d1.Normalize();
				float zz = d1.z*_owner.MoveDirection.x-d1.x*_owner.MoveDirection.z;
				if(zz <0)
				{
					_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleR1;
					if(_hDirection != HES_DIRECTION.RIGHT)
					{
						_hDirection = HES_DIRECTION.RIGHT;
						_owner.ACPlayAnimation(_aniSwitch);
					}
				}
				else
				{
					_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleL1;
					if(_hDirection != HES_DIRECTION.LEFT)
					{
						_hDirection = HES_DIRECTION.LEFT;
						_owner.ACPlayAnimation(_aniSwitch);
					}
				}
				
			}
		}
		
	}
	
	public override void ShouldGotoAttack()
	{
		if(_state.CurrentStateID == AIAgent.STATE.HESITATE)
		{
			GoToAttack();
		}
	}
	
	public void UpdateHesitate()
	{
		_timeTotalForWander += Time.deltaTime;
		if(_targetAC.WasHit && _timeTotalForWander > _timeForWanderAtLeast)
		{
			float ret = Random.Range(0,1f);
			if(ret < _chanceToAttack_PlayerHurt)
			{
				if(_timeForWander > 1f)
				{
					_timeForWander = Random.Range(0,1f);
				}
				else
				{
					_timeForWander -= 0.5f;
				}
			}
		}
		_timeForWander -= Time.deltaTime;
		if(_timeForWander <= 0)
		{
			GoToAttack();
		}
		else
		{
			if(_currentSeekMode != SEEK_MODE.RUN_AWAY_AFTER_ATTACK
				&& _currentSeekMode != SEEK_MODE.RUN_AWAY_AFTER_HURT)
			{
				float lensqrt = (_targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition).sqrMagnitude;
				if(lensqrt < _dangerousRaduisSqrt)
				{
					_rageCurrent += _ragePerSecDanger * Time.deltaTime;
				}
				if(RageIsFull)
				{
					if(CanAttackOthers)
					{
						GoToAttack();
					}
					else
					{
						if(!_haveActionTicket)
						{
							FCTicketManager.Instance.ApplyTicket(_owner);
						}
					}
				}
			}
		}

	}

}
