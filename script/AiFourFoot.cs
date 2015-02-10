using UnityEngine;
using System.Collections;

public class AiFourFoot : AINormalWarrior 
{
	//way points need for this monster
	public string _wayPointsName;
	//speed for wolf run away
	public float _runAwaySpeed = 12f;
	public float _runAwayAngleSpeed = 720;
	public float _restTime = 1f;
	public float _tiredTime = 4f;
	public float _runAwayTime = 4f;
	public float _rangerAttackRadius = 4f;
	public float _faceToTargetTime = 0.5f;
	public float _closeDistanceToPlayer = 3f;
	public float _faceToPlayerAngleSpeed = 720;
	
	bool _fromHurt = false;
	
	
	public enum AWAY_MODE
	{
		NONE,
		RUN_AWAY,
		HAVE_A_REST,
		FACE_TO_PLAYER,
		FACE_TO_PLAYER2,
		FACE_TO_PLAYER3,
		TRY_TO_CLOSE_PLAYER,
		FACE_TO_PLAYER4,
		TIRED
	}
	
	protected Vector3 _targetPoint;
	protected Vector3 _FinalPoint;
	protected bool _inAway = false;
	protected int _preTargetWayPoint = -1;
	protected int _currentTargetWayPoint = -1;
	protected AniSwitch _aniSwitch = new AniSwitch();
	
	//only used for RUN_AWAY
	protected float _timeCounterForAway = 0;
	
	protected AWAY_MODE _awayMode;
	protected AWAY_MODE _currentAwayMode = AWAY_MODE.NONE;
	protected AWAY_MODE _nextAwayMode = AWAY_MODE.NONE;
	
	
	public override void ActiveAI()
	{
		base.ActiveAI();
		_haveWayToGetEnergy = true;
		_owner.CostEnergy(-99, -1);
	}
	//in run state ,I will set CurrentSubStateID before enter.
	public override void RunTaskChange(FCCommand.CMD cmd)
	{
		base.RunTaskChange(cmd);

		//play dust in run state for wolf boss
		if(cmd == FCCommand.CMD.STATE_QUIT)
		{	
			CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DUST_LARGE, 
				_owner._avatarController, -1);
		}
		else if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DUST_LARGE, 
				_owner._avatarController, -1);
		}
	}	
	
	public override void ChangeAILevel(int level)
	{
		base.ChangeAILevel(level);
		if(level == 0)
		{
			_owner.Energy = 1;
			SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.COST_NO_ENERGY);
			SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_ENERGY_BY_ATTACK);
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_NO_ENERGY);
		}
		else if(level == 1)
		{
			SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_NO_ENERGY);
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.COST_NO_ENERGY);
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_ENERGY_BY_ATTACK);
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
			if(_state._preState.CurrentStateID == AIAgent.STATE.HURT)
			{
				_fromHurt = true;
			}
			else
			{
				_fromHurt = false;
			}
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			if(_targetAC.IsAlived)
			{
				SetNextState(AIAgent.STATE.LEAVE_AWAY);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			if(_targetAC.IsAlived)
			{
				SetNextState(AIAgent.STATE.LEAVE_AWAY);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_FINISH)
		{
			
		}
	}
	
	//we need wolf run way from player
	public override void AwayTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			//first go to rest
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			EventIsStopAtPoint = IsStopAtPoint;
			if(_owner.Energy >= 100 && _aiLevel == 0)
			{
				ChangeAILevel(1);
				SetNextAwayMode(AWAY_MODE.RUN_AWAY);
			}
			else if(_owner.Energy>0 && _aiLevel == 1)
			{
				SetNextAwayMode(AWAY_MODE.RUN_AWAY);
			}
			else if(_owner.Energy <= 0 && _aiLevel == 1)
			{
				ChangeAILevel(0);
				SetNextAwayMode(AWAY_MODE.TIRED);
			}
			else
			{
				if(!_fromHurt)
				{
					SetNextAwayMode(AWAY_MODE.HAVE_A_REST);
				}
				else
				{
					SetNextAwayMode(AWAY_MODE.RUN_AWAY);
				}
			}
			ChangeToAwayMode();
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			_owner.ACStop();
			StopAllCoroutines();
			ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
			EventIsStopAtPoint = null;
			AniEventAniIsOver = AniIsOver;
			//_owner.ACSetColliderAsTrigger(false);
			_owner.CurrentAngleSpeed = _owner.Data.angleSpeed;
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		}
	}
	
	private void SetNextAwayMode(AWAY_MODE awayMode)
	{
		_inAway = false;
		_nextAwayMode = awayMode;
	}
	
	protected bool IsStopAtPoint()
	{
		if(_awayMode == AWAY_MODE.RUN_AWAY)
		{
			SetNextAwayPoint();
		}
		return true;
	}
	protected void ChangeToAwayMode()
	{
		if(_nextAwayMode != AWAY_MODE.NONE)
		{
			_currentAwayMode = _nextAwayMode;
			_nextAwayMode = AWAY_MODE.NONE;
			switch(_currentAwayMode)
			{
			case AWAY_MODE.HAVE_A_REST:
				StartCoroutine(HAVE_A_REST());
				break;
			case AWAY_MODE.RUN_AWAY:
				StartCoroutine(RUN_AWAY());
				break;
			case AWAY_MODE.FACE_TO_PLAYER:
				StartCoroutine(FACE_TO_PLAYER());
				break;
			case AWAY_MODE.FACE_TO_PLAYER2:
				StartCoroutine(FACE_TO_PLAYER2());
				break;
			case AWAY_MODE.FACE_TO_PLAYER3:
				StartCoroutine(FACE_TO_PLAYER3());
				break;
			case AWAY_MODE.FACE_TO_PLAYER4:
				StartCoroutine(FACE_TO_PLAYER4());
				break;
			case AWAY_MODE.TRY_TO_CLOSE_PLAYER:
				StartCoroutine(TRY_TO_CLOSE_PLAYER());
				break;
			case AWAY_MODE.TIRED:
				StartCoroutine(TIRED());
				break;
			}
			if(_currentAwayMode == AWAY_MODE.NONE)
			{
                Debug.LogError("error");
			}
		}
		else
		{
            Debug.LogError("error");
		}
		
	}
	
	public void UpdateAnimation()
	{
	}
	
	protected IEnumerator HAVE_A_REST()
	{
		_owner.ACStop();
		
		//when rest, play idle animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		//we need wolf boss idle for second.and need not face to player
		_inAway = true;
		float timeCount = _restTime;
		while(_inAway)
		{
			timeCount -= Time.deltaTime;
			if(timeCount <= 0)
			{
				SetNextAwayMode(AWAY_MODE.RUN_AWAY);
			}
			yield return null;
		}
		ChangeToAwayMode();
	}
	
	
	protected IEnumerator TIRED()
	{
		_owner.ACStop();
		
		//when rest, play idle animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		//we need wolf boss idle for second.and need not face to player
		_inAway = true;
		float timeCount = _tiredTime;
		while(_inAway)
		{
			timeCount -= Time.deltaTime;
			if(timeCount <= 0)
			{
				SetNextAwayMode(AWAY_MODE.RUN_AWAY);
			}
			yield return null;
		}
		ChangeToAwayMode();
	}
	
	/// <summary>
	/// Gets the next away point. Used for awway mode RUN_AWAY
	/// </summary>
	/// <returns>
	/// if false ,means need enter attack;
	/// </returns>
	private bool SetNextAwayPoint()
	{
		bool ret = false;
		Vector3 targetPoint = Vector3.zero;
		
		int preWp = _currentTargetWayPoint;
		_currentTargetWayPoint = FCWayPointsManger.Instance.GetWayPointByForward("pathWolf",_targetAC, _currentTargetWayPoint, _preTargetWayPoint, out targetPoint);
		_preTargetWayPoint = preWp;
		if(_currentTargetWayPoint != -1)
		{
			_targetPoint = targetPoint;
			_targetPoint.y = _owner.ThisTransform.localPosition.y;
			MoveTo(ref _targetPoint, _runAwaySpeed, _runAwayAngleSpeed);
			_timeCounterForAway = (_targetPoint - _owner.ThisTransform.localPosition).magnitude/_runAwaySpeed+0.2f;
			ret = true;
		}
		if(!ret)
		{
			GotoBashOrFaceToPlayer();
		}
		return ret;
	}
	
	private void GotoBashOrFaceToPlayer()
	{
		if(ActionControllerManager.Instance.IsNearPlayers(_owner,_rangerAttackRadius))
		{
			//need attack player nearby
			GoToAttack("Smash",false);
		}
		else
		{
			//face to player
			SetNextAwayMode(AWAY_MODE.FACE_TO_PLAYER);
		}
	}
	
	float GetAngleBetweenPlayerAndSelfForward()
	{
		float angle = 0;
		Vector3 v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		Vector3 v31 = _owner.ThisTransform.forward;
		v31.y =0;
		v3.y = 0;
		if(v3 != Vector3.zero)
		{
			v3.Normalize();
			angle = Vector3.Angle(v3,v31);
		}
		return angle;
	}
	protected IEnumerator RUN_AWAY()
	{
		//Debug.Log("Go into run away");
		//when rest, play run animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.run;
		_owner.ACPlayAnimation(_aniSwitch);
		//if get no waypoint ID ,means player in front of monster, we should use pounce
		_currentTargetWayPoint = FCWayPointsManger.Instance.GetWayPoint(_owner, _targetAC, "pathWolf", 3);
		_currentTargetWayPoint = 2;
		_preTargetWayPoint = _currentTargetWayPoint;
		_timeCounterForAway = 0;
		float timeCount = _runAwayTime;
		Vector3 distance = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		distance.y = 0;
		float disFlag = distance.sqrMagnitude;
		if(!_attackCountAgent.SkillIsCD("Pounce"))
		{
			SetNextAwayMode(AWAY_MODE.FACE_TO_PLAYER4);
			if(_currentTargetWayPoint == -1)
			{
				SetNextAwayMode(AWAY_MODE.FACE_TO_PLAYER3);
			}
			else if(disFlag <4)
			{
				GoToAttack("Smash", false);
				_inAway = false;
				//enable collision with player body
				//_owner.ACSetColliderAsTrigger(false);
			}
			else if(disFlag <6)
			{
				//choose skill pounce
				SetNextAwayMode(AWAY_MODE.FACE_TO_PLAYER3);
				//enable collision with player body
				//_owner.ACSetColliderAsTrigger(false);
			}
			else
			{
				float angle = GetAngleBetweenPlayerAndSelfForward();
				if(angle > 60)
				{
					SetNextAwayMode(AWAY_MODE.FACE_TO_PLAYER4);
				}
				else
				{
					GoToAttack();
				}
			}
		}
		else
		{
			//Debug.Log("Go into find way point");
			//something wrong with way point system
			_owner.ACSetColliderAsTrigger(true);
			if(_currentTargetWayPoint == -1)
			{
				_currentTargetWayPoint = FCWayPointsManger.Instance.GetWayPointRandom("pathWolf");
			}
			FCWayPointsManger.Instance.GetWayPoint("pathWolf", _currentTargetWayPoint, out _targetPoint);
			//Debug.Log("Target pos is x: " + _targetPoint.x + " y: " + _targetPoint.y + " z: " + _targetPoint.z);
			//go to run away
			_inAway = true;
			//wolf should jump to this point
			
			MoveTo(ref _targetPoint, _runAwaySpeed, _runAwayAngleSpeed);
			//Theory time for run away
			Vector3 v3 = _targetPoint - _owner.ThisTransform.localPosition;
			v3.y = 0;
			_timeCounterForAway = v3.magnitude/_runAwaySpeed*2+0.1f;
			//Debug.Log("time Counter For Away = "  + _timeCounterForAway);
			EventIsStopAtPoint = IsStopAtPoint;
			SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
			timeCount = 2;
			//disable collision with player body
			
			//play dust effect
			CharacterEffectManager.Instance.PlayEffect(FC_CHARACTER_EFFECT.DUST_LARGE, 
				_owner._avatarController, -1);			
		}
		
		while(_inAway)
		{
			//if true, means away mode already changed by SetNextAwayPoint
			bool retB = false;
			Vector3 v3 = _targetPoint-_owner.ThisTransform.localPosition;
			v3.y = 0;
			_timeCounterForAway -= Time.deltaTime;
			if(v3.sqrMagnitude <1f || _timeCounterForAway <=0)
			{
				retB = SetNextAwayPoint();
				//Debug.Log(" go to attack");
			}
			timeCount -= Time.deltaTime;
			if(timeCount <= 0 && retB)
			{
				GotoBashOrFaceToPlayer();
			}
			yield return null;
		}
		
		//end dust effect
		CharacterEffectManager.Instance.StopEffect(FC_CHARACTER_EFFECT.DUST_LARGE, 
			_owner._avatarController, -1);		
	
		
		ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.IN_RIGIDBODY2);
		EventIsStopAtPoint = null;
		ChangeToAwayMode();
	}
	
	protected void AniIsOverOfAway()
	{
		//when face to player animation is over, wolf need begin to attack
		if(_currentAwayMode == AWAY_MODE.FACE_TO_PLAYER)
		{
			//GoToAttack("Pounce", false);
		}
	}
	protected IEnumerator FACE_TO_PLAYER()
	{
		_inAway = true;
		_owner.CurrentAngleSpeed = _faceToPlayerAngleSpeed;
		_owner.ACStop();
		//when FACE_TO_PLAYER, play jump animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.jumpLeave1;
		_owner.ACPlayAnimation(_aniSwitch);
		// this state last longest for 3secs
		float timeCount = _faceToTargetTime;
		AniEventAniIsOver = AniIsOverOfAway;
		while(_inAway)
		{
			FaceToTarget(_targetAC);
			timeCount -= Time.deltaTime;
			if(timeCount <= 0)
			{
				GoToAttack("Pounce", false);
			}
			yield return null;
		}
		AniEventAniIsOver = AniIsOver;
		ChangeToAwayMode();
	}
	
		//only face to target ,not move direction
	private void UpdateFaceTarget()
	{
		Vector3 dir = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		dir.y =0;
		dir.Normalize();
		if(dir != Vector3.zero)
		{
			_owner.ACRotateTo(dir,-1,false);
		}
	}
	
	//try to leave away by back
	protected IEnumerator FACE_TO_PLAYER2()
	{
		_inAway = true;
		_owner.ACSetColliderAsTrigger(true);
		_owner.CurrentAngleSpeed = _faceToPlayerAngleSpeed;
		_owner.ACStop();
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		Vector3 v3 = _owner.ThisTransform.localPosition - _targetAC.ThisTransform.localPosition;
		v3.y = 0;
		v3.Normalize();
		if(v3 == Vector3.zero)
		{
			v3 = -_owner.ThisTransform.forward;
		}
		MoveByDirection(v3,_runAwaySpeed, 0.1f);
		//when FACE_TO_PLAYER, play idle animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		// this state last longest for 3secs
		float timeCount = _faceToTargetTime;
		AniEventAniIsOver = AniIsOverOfAway;
		while(_inAway)
		{
			UpdateFaceTarget();
			timeCount -= Time.deltaTime;
			if(timeCount <= 0)
			{
				GoToAttack("Claw", true);
			}
			yield return null;
		}
		AniEventAniIsOver = AniIsOver;
		//_owner.ACSetColliderAsTrigger(false);
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		ChangeToAwayMode();
	}
	
	//try to leave away by goto circle
	protected IEnumerator FACE_TO_PLAYER4()
	{
		_inAway = true;
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		_owner.MoveFollowForward = true;
		Vector3 v3 = _owner.ThisTransform.forward;
		v3.y = 0;
		Vector3 v31 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		v31.y = 0;
		float zz = v3.z*v31.x-v3.x*v31.z;
		float angleOffset = 0;
		if(zz > 0 )
		{
			angleOffset = 1f;
		}
		else
		{
			angleOffset = -1f;
		}
		float timeCount = 2;
		//when FACE_TO_PLAYER, play run animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.run;
		_owner.ACPlayAnimation(_aniSwitch);
		MoveByDirection(v3, _runAwaySpeed,timeCount + 0.1f);
		// this state last longest for 3secs
		
		while(_inAway)
		{
			Quaternion rot = Quaternion.Euler(0,angleOffset*_owner.CurrentAngleSpeed*Time.deltaTime,0);
			Vector3 direction = ThisTransform.forward;
			direction = rot * direction;
			_owner.ACRotateTo(direction,-1, true);
			timeCount -= Time.deltaTime;
			
			if(timeCount <= 0 || GetAngleBetweenPlayerAndSelfForward()< 30)
			{
				GoToAttack();
			}
			yield return null;
		}
		//_owner.ACSetColliderAsTrigger(false);
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		_owner.MoveFollowForward = false;
		ChangeToAwayMode();
	}
	
	//face to player and try to close player
	protected IEnumerator FACE_TO_PLAYER3()
	{
		_inAway = true;
		_owner.CurrentAngleSpeed = _faceToPlayerAngleSpeed;
		_owner.ACStop();
		//when FACE_TO_PLAYER, play idle animation
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		// this state last longest for 3secs
		float timeCount = _faceToTargetTime;
		AniEventAniIsOver = AniIsOverOfAway;
		while(_inAway)
		{
			UpdateFaceTarget();
			timeCount -= Time.deltaTime;
			if(timeCount <= 0)
			{
				SetNextAwayMode(AWAY_MODE.TRY_TO_CLOSE_PLAYER);
			}
			yield return null;
		}
		AniEventAniIsOver = AniIsOver;
		ChangeToAwayMode();
	}
	
	protected IEnumerator TRY_TO_CLOSE_PLAYER()
	{
		_inAway = true;
		_owner.CurrentAngleSpeed = _owner.Data.angleSpeed/10;
		_owner.ACStop();
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.run;
		_owner.ACPlayAnimation(_aniSwitch);
		
		float closeDistanceToPlayerSqrt = _closeDistanceToPlayer*_closeDistanceToPlayer;
		float timeCount = 0;
		Vector3 v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		v3.y =0;
		bool gotoAttack = false;
		if(v3.sqrMagnitude <= closeDistanceToPlayerSqrt)
		{
			gotoAttack = true;
		}
		else
		{
			timeCount = v3.magnitude/_runAwaySpeed;
		}
		if(timeCount < 0.01f)
		{
			gotoAttack = true;
		}
		else
		{
			_owner.ACMoveToDirection(ref v3,(int)(_owner.Data.angleSpeed/10), timeCount);
		}
		while(_inAway)
		{
			timeCount -= Time.deltaTime;
			if(timeCount <= 0)
			{
				
				gotoAttack = true;
			}
			else if(!gotoAttack)
			{
				v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
				v3.y =0;
				if(v3.sqrMagnitude <= closeDistanceToPlayerSqrt)
				{
					gotoAttack = true;
				}
				else
				{
					//v3.Normalize();
					
				}
				
			}
			if(gotoAttack)
			{
				float ret = Random.Range(0,1f);
				if(ret < 0.5f)
				{
					GoToAttack("Bit", false);
				}
				else
				{
					GoToAttack("Claw", false);
				}
			}
			yield return null;
		}
		AniEventAniIsOver = AniIsOver;
		_owner.CurrentAngleSpeed = _owner.Data.angleSpeed;
		ChangeToAwayMode();
	}
	
	/// <summary>
	/// Gos to attack.
	/// </summary>
	/// <param name='skillName'>
	/// skill want to used by monster
	/// </param>
	public override void GoToAttack(string skillName, bool needDetectDistance)
	{
		StopAllCoroutines();
		base.GoToAttack(skillName, needDetectDistance);
	}
	
	
}
