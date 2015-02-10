using UnityEngine;
using System.Collections;

public class AiNormalRanger : AINormalWarrior {
	
	public string _heavyShootSkill;
	public enum RANGER_MODE
	{
		NONE,
		AVOID,
		SHOOT,
		WAIT,
		RUN_AWAY
	}
	
	public float _safeDistance = 16;
	public float _dangerDistance = 8;
	
	public float _maxAttackDistance = 20;
	protected RANGER_MODE _rangerMode = RANGER_MODE.NONE;
	protected RANGER_MODE _nextRangerMode = RANGER_MODE.NONE;
	
	public float _avoidSpeed = 4;
	
	public float _waitTime = 0.5f;
	public float _timeMinForAvoid = 0.5f;
	public float _timeMaxForAvoid = 2f;
	
	protected bool _inRangerMode;
	
	protected Vector3 _shootDirection;
	protected AniSwitch _aniSwitch = new AniSwitch();
	
	//protected bool _inSlowMove = false;
	protected float _inSlowTime = 0;
	protected bool _inFastAvoid = false;

    public override void ActiveAI()
    {
        base.ActiveAI();
        _haveWayToGetEnergy = true;
        _owner.CostEnergy(-99, -1);
        SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_ENERGY_BY_ATTACK);
    }

	
	public override void IdleTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_QUIT)
		{	
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		}
		else if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			if(_targetAC.IsAlived)
			{
				SetNextState(AIAgent.STATE.AVOID_AND_SHOOT);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_UPDATE)
		{
			if(_targetAC.IsAlived)
			{
				SetNextState(AIAgent.STATE.AVOID_AND_SHOOT);
			}
		}
		else if(cmd == FCCommand.CMD.STATE_FINISH)
		{
			
		}
	}
	
	public override void AvoidAndShootTaskChange(FCCommand.CMD cmd)
	{
		if(cmd == FCCommand.CMD.STATE_ENTER)
		{
			if(_owner.Energy >= 100)
			{
				SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_NO_ENERGY);
				ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_ENERGY_BY_ATTACK);
			}
			else
			{
				ClearActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_NO_ENERGY);
				SetActionSwitchFlag(FC_ACTION_SWITCH_FLAG.GAIN_ENERGY_BY_ATTACK);
			}
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_MOVE_MANUAL);
			EventIsStopAtPoint = IsStopAtPoint;
			FC_DANGER_LEVEL edl = GetTargetDangerLevel(_safeDistance, _dangerDistance);
			if(edl == FC_DANGER_LEVEL.SAFE)
			{
				if(_attackCountAgent.CanUseSkill())
				{
					SetNextRangerMode(RANGER_MODE.SHOOT);
				}
				else
				{
					SetNextRangerMode(RANGER_MODE.WAIT);
				}
			}
			else
			{
				SetNextRangerMode(RANGER_MODE.AVOID);
			}
			ChangeToRangerMode();
		}
		else if(cmd == FCCommand.CMD.STATE_QUIT)
		{
			_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
			_owner.ACStop();
			StopAllCoroutines();
			EventIsStopAtPoint = null;
			AniEventAniIsOver = AniIsOver;
			_owner.ACSetMoveMode(MoveAgent.MOVE_MODE.BY_ANIMATION);
		}
	}
	
	public void SetNextRangerMode(RANGER_MODE rm)
	{
		_inRangerMode = false;
		_nextRangerMode = rm;
	}
	
	public void ChangeToRangerMode()
	{
		_rangerMode = _nextRangerMode;
		_nextRangerMode = RANGER_MODE.NONE;
		switch(_rangerMode)
		{
		case RANGER_MODE.AVOID:
			StartCoroutine(AVOID());
			break;
		case RANGER_MODE.SHOOT:
			StartCoroutine(SHOOT());
			break;
		case RANGER_MODE.WAIT:
			StartCoroutine(WAIT());
			break;
		}
	}
	protected override void GoToAttack ()
	{
		_owner.ACStop();
		StopAllCoroutines();
		EventIsStopAtPoint = null;
		AniEventAniIsOver = AniIsOver;
		base.GoToAttack ();
	}
	
	protected IEnumerator WAIT()
	{
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		_inRangerMode = true;
		_owner.ACStop();
		//we need this state at least 0.5 sec
		float timeCount = _waitTime;
		while(_inRangerMode)
		{
			FaceToTarget(_targetAC);
			timeCount -= Time.deltaTime;
			if(timeCount <=0)
			{
				FC_DANGER_LEVEL edl = GetTargetDangerLevel(_safeDistance, _dangerDistance);
				if(edl == FC_DANGER_LEVEL.DANGER)
				{
					SetNextRangerMode(RANGER_MODE.AVOID);
				}
				else if(edl == FC_DANGER_LEVEL.VERY_DANGER)
				{
					SetNextRangerMode(RANGER_MODE.AVOID);
				}
				if(_attackCountAgent.CanUseSkill())
				{
					SetNextRangerMode(RANGER_MODE.SHOOT);
				}
			}

			yield return null;
		}
		ChangeToRangerMode();
	}
	
	Vector3 GetDirectionToTarget()
	{
		Vector3 v3 = _targetAC.ThisTransform.localPosition - _owner.ThisTransform.localPosition;
		v3.y =0;
		v3.Normalize();
		return v3;
	}
	
	IEnumerator AVOID()
	{
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.runCircleB1;
		_owner.ACPlayAnimation(_aniSwitch);
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.UNLOCK);
		_inRangerMode = true;
		_inFastAvoid = false;
		//we need this state at least 0.5 sec
		float timeCount = _timeMinForAvoid;
		float timeMaxForAvoid = _timeMaxForAvoid;
		Vector3 dir = GetDirectionToTarget();
		_inSlowTime = 0;
		float angleOffset = Random.Range(-10,10);
		while(_inRangerMode)
		{
			FaceToTarget(_targetAC);
			if(timeCount >=0)
			{
				timeCount -= Time.deltaTime;
			}
			dir += GetDirectionToTarget();
			dir.Normalize();
			timeMaxForAvoid -= Time.deltaTime;
			if(_inSlowTime>0.1f)
			{
				if(angleOffset <= 0)
				{
					angleOffset = -45;
				}
				else
				{
					angleOffset = 45;
				}
				
			}
			else if(_inFastAvoid)
			{
				if(angleOffset <= 0)
				{
					angleOffset = -5;
				}
				else
				{
					angleOffset = 5;
				}
				Quaternion qt = Quaternion.Euler(0,angleOffset,0);
				dir = qt * dir;
			}
			
			MoveByDirection(-dir,_avoidSpeed,0.1f);

			if(timeCount <=0 && timeMaxForAvoid >0)
			{
				FC_DANGER_LEVEL edl = GetTargetDangerLevel(_safeDistance, _dangerDistance);
				if(edl != FC_DANGER_LEVEL.VERY_DANGER && _attackCountAgent.CanUseSkill())
				{
					SetNextRangerMode(RANGER_MODE.SHOOT);
				}
			}
			else if(timeMaxForAvoid <=0)
			{
				SetNextRangerMode(RANGER_MODE.SHOOT);
			}
			else if(_inSlowTime>0.5f)
			{
				SetNextRangerMode(RANGER_MODE.SHOOT);
			}
			yield return null;
		}
		_owner.SetShipForSpeedAndForward(MoveAgent.ROTATE_FLAG.FOLLOW_SPEED);
		ChangeToRangerMode();
	}
	
	IEnumerator SHOOT()
	{
		_aniSwitch._aniIdx = FC_All_ANI_ENUM.idle;
		_owner.ACPlayAnimation(_aniSwitch);
		
		_inRangerMode = true;
		_shootDirection = _owner.ThisTransform.forward;
		_owner.ACStop();
		//we need this state at least 0.5 sec
		float timeCount = 0.2f;
		while(_inRangerMode)
		{
			FaceToTarget(_targetAC);
			timeCount -= Time.deltaTime;
			if( !_haveActionTicket)
			{
				FCTicketManager.Instance.ApplyTicket(_owner);
			}
			if(timeCount <=0 && _targetAC.IsAlived && _haveActionTicket && _attackCountAgent.CanUseSkill())
			{
				GoToAttack();
			}
			yield return null;
		}
		SetNextState(AIAgent.STATE.IDLE);
	}
	
	protected bool IsStopAtPoint()
	{
		return true;
	}
	
	public override void HandleLateUpdate()
	{
		float len = (_owner._lastPosition-_owner._currPosition).magnitude;
		if(len < 0.5f*Time.deltaTime)
		{
			_inSlowTime += Time.deltaTime;
		}
		else if(len > _avoidSpeed*0.6f*Time.deltaTime)
		{
			_inFastAvoid = true;
			_inSlowTime = 0;
		}
		else
		{
			_inSlowTime = 0;
		}
	}
	
}
